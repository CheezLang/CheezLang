using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.SemanticAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public class Workspace
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Compiler mCompiler;

        private List<AstStatement> mStatements = new List<AstStatement>();
        public IReadOnlyList<AstStatement> Statements => mStatements;

        public Scope GlobalScope { get; private set; }
        public List<Scope> AllScopes { get; private set; }

        public AstFunctionDecl MainFunction { get; set; }

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
        }
        
        public void ReportError(ILocation location, string errorMessage, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            var file = mFiles[location.Beginning.file];
            mCompiler.ErrorHandler.ReportError(file, location, errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }
    }
}
