using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.SemanticAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public class Workspace
    {
        public Scope GlobalScope { get; } = new Scope("Global");

        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Compiler mCompiler;

        private PriorityQueue<CompilationUnit> mCompilationQueue = new PriorityQueue<CompilationUnit>();
        private List<(CompilationUnit unit, object condition)> mWaitingQueue = new List<(CompilationUnit, object)>();

        private ErrorHandler mErrorHandler = new ErrorHandler();
        public bool HasErrors { get; private set; }

        private List<AstStatement> mStatements = new List<AstStatement>();

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

        public void CompileAll()
        {
            var scopeCreator = new ScopeCreator(this, GlobalScope);
            foreach (var s in mStatements)
            {
                scopeCreator.CreateScopes(s, GlobalScope);
            }

            // gather declarations
            //foreach (var file in mFiles.Values)
            //{
            //    GatherDeclarations(GlobalScope, file.Statements);
            //}

            foreach (var scope in scopeCreator.AllScopes)
            {
                // define types

                // define functions
                foreach (var function in scope.FunctionDeclarations)
                {
                    if (!scope.DefineFunction(function))
                    {
                        ReportError(function.ParseTreeNode.Name, $"A function called '{function.Name}' already exists in current scope");
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

        //private void EnqueueUnit(CompilationUnit unit)
        //{
        //    mCompilationQueue.Enqueue(0, unit);
        //}

        //private void CheckWaitingQueue()
        //{
        //    for (int i = mWaitingQueue.Count - 1; i >= 0; i--)
        //    {
        //        var v = mWaitingQueue[i];
        //        var unit = v.unit;
        //        var condition = v.condition;

        //        switch (condition)
        //        {
        //            case WaitForType t:
        //                if (unit.file.PrivateScope.Types.GetCType(t.TypeName) != null)
        //                {
        //                    EnqueueUnit(unit);
        //                    mWaitingQueue.RemoveAt(i);
        //                }
        //                break;
        //        }
        //    }
        //}

        //public void Compile()
        //{
        //    while (true)
        //    {
        //        if (mCompilationQueue.IsEmpty)
        //        {
        //            if (mWaitingQueue.Count == 0)
        //                return;

        //            CheckWaitingQueue();
        //            if (mCompilationQueue.IsEmpty && mWaitingQueue.Count > 0)
        //            {
        //                // compilation error: unresolved references
        //                throw new Exception("Compilation Error");
        //            }
        //        }

        //        var next = mCompilationQueue.Dequeue();

        //        try
        //        {
        //            Compile(next);
        //        }
        //        catch (WaitForType w)
        //        {
        //            mWaitingQueue.Add((next, w));
        //        }
        //    }
        //}

        //private void Compile(CompilationUnit unit)
        //{

        //}

        public void ReportError(ILocation location, string errorMessage, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            HasErrors = true;
            var file = mFiles[location.Beginning.file];
            mErrorHandler.ReportError(file, location, errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }
    }
}
