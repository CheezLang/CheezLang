using Cheez.Compiler.Ast;
using Cheez.Compiler.SemanticAnalysis;
using Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Compiler mCompiler;

        public IEnumerable<PTFile> Files => mFiles.Select(kv => kv.Value);

        private List<AstStatement> mStatements = new List<AstStatement>();
        public IReadOnlyList<AstStatement> Statements => mStatements;

        public Scope GlobalScope { get; private set; }
        public List<Scope> AllScopes { get; private set; }

        public AstFunctionDecl MainFunction { get; set; }

        public Dictionary<CheezType, List<TraitType>> TypeTraitMap = new Dictionary<CheezType, List<TraitType>>();
        public Dictionary<CheezType, List<AstImplBlock>> Implementations = new Dictionary<CheezType, List<AstImplBlock>>();

        public Workspace(Compiler comp)
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

            //var semanticer = new Semanticer();
            //semanticer.DoWork(this, mStatements, mCompiler.ErrorHandler);

            var declarationAnalyzer = new DeclarationAnalyzer();
            declarationAnalyzer.CollectDeclarations(this);

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
            var (callingFunctionFile, callingFunctionName, callLineNumber) = Util.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        public void ReportError(string errorMessage, List<(string, ILocation)> details)
        {
            var (callingFunctionFile, callingFunctionName, callLineNumber) = Util.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mCompiler.ErrorHandler.ReportError(errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        public void ReportError(ILocation lc, string message, (string, ILocation)? detail = null)
        {
            var (callingFunctionFile, callingFunctionName, callLineNumber) = Util.GetCallingFunction().GetValueOrDefault(("", "", -1));
            var details = detail != null ? new List<(string, ILocation)> { detail.Value } : null;
            mCompiler.ErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details
            }, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        public IEnumerable<AstImplBlock> GetTraitImplementations(CheezType type)
        {
            if (Implementations.TryGetValue(type, out var impls))
            {
                return impls.Where(i => i.Trait != null);
            }

            return new AstImplBlock[0];
        }
    }
}
