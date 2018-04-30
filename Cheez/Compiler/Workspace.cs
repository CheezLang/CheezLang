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

            var scopeCreator = new ScopeCreator(this, GlobalScope);
            foreach (var s in mStatements)
            {
                scopeCreator.CreateScopes(s, GlobalScope);
            }

            AllScopes = scopeCreator.AllScopes;

            foreach (var scope in scopeCreator.AllScopes)
            {
                // define types
                foreach (var typeDecl in scope.TypeDeclarations)
                {
                    foreach (var member in typeDecl.Members)
                    {
                        member.Type = scope.GetCheezType(member.ParseTreeNode.Type);
                        if (member.Type == null)
                        {
                            ReportError(member.ParseTreeNode.Type, $"Unknown type '{member.ParseTreeNode.Type}' in struct member");

                        }
                    }

                    if (!scope.DefineType(typeDecl))
                    {
                        ReportError(typeDecl.ParseTreeNode.Name, $"A type called '{typeDecl.Name}' already exists in current scope");
                        continue;
                    }
                }

                // define functions
                foreach (var function in scope.FunctionDeclarations)
                {
                    if (!scope.DefineFunction(function))
                    {
                        ReportError(function.ParseTreeNode.Name, $"A function called '{function.Name}' already exists in current scope");
                        continue;
                    }

                    // check return type
                    {
                        function.ReturnType = CheezType.Void;
                        if (function.ParseTreeNode.ReturnType != null)
                        {
                            function.ReturnType = scope.GetCheezType(function.ParseTreeNode.ReturnType);
                            if (function.ReturnType == null)
                            {
                                ReportError(function.ParseTreeNode.ReturnType, $"Unknown type '{function.ParseTreeNode.ReturnType}' in function return type");
                            }
                        }
                    }

                    // check parameter types
                    {
                        foreach (var p in function.Parameters)
                        {
                            p.VarType = scope.GetCheezType(p.ParseTreeNode.Type);
                            if (p.VarType == null)
                            {
                                ReportError(p.ParseTreeNode.Type, $"Unknown type '{p.ParseTreeNode.Type}' in function parameter list");
                            }
                        }
                    }
                }

            }

            TypeChecker typeChecker = new TypeChecker(this);
            // define global variables
            //foreach (var v in GlobalScope.VariableDeclarations)
            //{
            //    typeChecker.CheckTypes(v, GlobalScope);
            //}

            // compile functions
            foreach (var s in GlobalScope.FunctionDeclarations)
            {
                typeChecker.CheckTypes(s);
            }
        }

        public void ReportError(ILocation location, string errorMessage, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            var file = mFiles[location.Beginning.file];
            mCompiler.ErrorHandler.ReportError(file, location, errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }
    }
}
