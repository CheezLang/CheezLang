﻿using Cheez.Ast;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Util;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public enum TargetArchitecture
    {
        X86,
        X64
    }

    public partial class Workspace
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private CheezCompiler mCompiler;

        public IEnumerable<PTFile> Files => mFiles.Select(kv => kv.Value);

        private List<AstStatement> mStatements = new List<AstStatement>();
        public IReadOnlyList<AstStatement> Statements => mStatements;

        public Scope GlobalScope { get; private set; }
        public List<Scope> AllScopes { get; private set; }

        public AstFunctionDecl MainFunction { get; set; }

        public Dictionary<CheezType, List<AstImplBlock>> TypeTraitMap = new Dictionary<CheezType, List<AstImplBlock>>();

        public int MaxPolyStructResolveStepCount { get; set; } = 10;
        public int MaxPolyFuncResolveStepCount { get; set; } = 10;

        private AstFunctionDecl currentFunction = null;

        public TargetArchitecture TargetArch = TargetArchitecture.X64;

        public Workspace(CheezCompiler comp)
        {
            mCompiler = comp;
        }

        public void AddFile(PTFile file)
        {
            mFiles[file.Name] = file;
            mStatements.AddRange(file.Statements);
        }

        public PTFile GetFile(string file)
        {
            return mFiles[file];
        }

        public void RemoveFile(PTFile file)
        {
            mFiles.Remove(file.Name);

            mStatements.RemoveAll(s => s.SourceFile == file);
            //GlobalScope.FunctionDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);
            //GlobalScope.TypeDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);
            //GlobalScope.VariableDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);

            // @Todo: make that better, somewhere else
            //GlobalScope = new Scope("Global");
        }

        public void CompileAll()
        {
            var preludeScope = new Scope("prelude");
            preludeScope.DefineBuiltInTypes();
            preludeScope.DefineBuiltInOperators();

            GlobalScope = new Scope("Global", preludeScope);

            Pass1(); // collect declarations
            Pass2(); // resolve types

            // at this points all concrete types are known

            PassResolveGlobalTypeDeclarations();
            PassResolveImpls();

            PassResolveGlobalUses();

            Pass4(); // resolve function signatures
            // Pass5(); // impls
            // Pass6(); // match impl functions with trait functions
            Pass6(); // variables
            Pass7(); // function bodies


            MainFunction = GlobalScope.GetSymbol("Main") as AstFunctionDecl;
            if (MainFunction == null)
            {
                mCompiler.ErrorHandler.ReportError("No main function was specified");
            }
        }

        [SkipInStackFrame]
        public void ReportError(Error error)
        {
            mCompiler.ErrorHandler.ReportError(error);
        }

        [SkipInStackFrame]
        public void ReportError(string errorMessage)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        public void ReportError(string errorMessage, List<(string, ILocation)> details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(new Error
            {
                Message = errorMessage,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrame]
        public void ReportError(string errorMessage, (string, ILocation) details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(new Error
            {
                Message = errorMessage,
                Details = new List<(string, ILocation)> { details },
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrame]
        public void ReportError(ILocation lc, string message, (string, ILocation)? detail = null)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            var details = detail != null ? new List<(string, ILocation)> { detail.Value } : null;
            mCompiler.ErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrame]
        public void ReportError(ILocation lc, string message, IEnumerable<(string, ILocation)> details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        private void AddTraitForType(CheezType type, AstImplBlock impl)
        {
            List<AstImplBlock> traits = null;
            if (!TypeTraitMap.TryGetValue(type, out traits))
            {
                traits = new List<AstImplBlock>();
                TypeTraitMap.Add(type, traits);
            }

            traits.Add(impl);
        }
    }
}
