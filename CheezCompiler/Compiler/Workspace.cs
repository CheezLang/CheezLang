using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public class Workspace
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

            foreach (var pts in file.Statements)
            {
                mStatements.Add(pts.CreateAst());
            }
        }

        public void RemoveFile(PTFile file)
        {
            mFiles.Remove(file.Name);

            mStatements.RemoveAll(s => s.GenericParseTreeNode.SourceFile == file);
            //GlobalScope.FunctionDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);
            //GlobalScope.TypeDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);
            //GlobalScope.VariableDeclarations.RemoveAll(fd => fd.GenericParseTreeNode.SourceFile == file);

            // @Todo: make that better, somewhere else
            //GlobalScope = new Scope("Global");
        }

        public void CompileAll()
        {
            GlobalScope = new Scope("Global");
            GlobalScope.DefineBuiltInTypes();
            GlobalScope.DefineBuiltInOperators();

            var semanticer = new Semanticer();
            semanticer.DoWork(this, mStatements, mCompiler.ErrorHandler);

            if (MainFunction == null)
            {
                mCompiler.ErrorHandler.ReportError("No main function was specified");
            }
        }
        
        public void ReportError(ILocation location, string errorMessage, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            var file = mFiles[location.Beginning.file];
            mCompiler.ErrorHandler.ReportError(file, location, errorMessage, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        public void ReportError(string errorMessage, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            mCompiler.ErrorHandler.ReportError(errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
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
