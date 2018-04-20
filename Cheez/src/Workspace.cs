using Cheez.Ast;
using Cheez.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cheez
{
    public class CompilationError
    {
        public ILocation Location { get; set; }
        public string Message { get; set; }

        public CompilationError(ILocation item, string message)
        {
            this.Location = item;
            this.Message = message;
        }
    }

    public class Workspace
    {
        public Scope GlobalScope { get; } = new Scope("Global");

        private Dictionary<string, CheezFile> mFiles = new Dictionary<string, CheezFile>();
        private Dictionary<object, CheezType> mTypeMap = new Dictionary<object, CheezType>();
        private Dictionary<object, Scope> mScopeMap = new Dictionary<object, Scope>();
        private Dictionary<FunctionDeclarationAst, Scope> mFunctionScopeMap = new Dictionary<FunctionDeclarationAst, Scope>();
        private Dictionary<IVariableDeclaration, VariableData> mVariableDataMap = new Dictionary<IVariableDeclaration, VariableData>();
        private Compiler mCompiler;

        private PriorityQueue<CompilationUnit> mCompilationQueue = new PriorityQueue<CompilationUnit>();
        private List<(CompilationUnit unit, object condition)> mWaitingQueue = new List<(CompilationUnit, object)>();

        private ErrorHandler mErrorHandler = new ErrorHandler();
        public bool HasErrors { get; private set; }

        public Workspace(Compiler comp)
        {
            mCompiler = comp;
        }

        public void AddFile(CheezFile file)
        {
            mFiles[file.Name] = file;
        }

        public void SetCheezType(object o, CheezType type)
        {
            if (o == null)
                return;
            mTypeMap[o] = type;
        }

        public CheezType GetCheezType(object o)
        {
            if (!mTypeMap.ContainsKey(o))
                return null;
            return mTypeMap[o];
        }

        public void SetFunctionScope(FunctionDeclarationAst o, Scope scope)
        {
            mFunctionScopeMap[o] = scope;
        }

        public void SetScope(object o, Scope scope)
        {
            mScopeMap[o] = scope;
        }

        public Scope GetScope(object o)
        {
            if (!mScopeMap.ContainsKey(o))
                return null;
            return mScopeMap[o];
        }

        public void SetVariableData(IVariableDeclaration declaration, VariableData data)
        {
            mVariableDataMap[declaration] = data;
        }

        public VariableData GetVariableData(IVariableDeclaration declaration)
        {
            if (!mVariableDataMap.ContainsKey(declaration))
                return null;
            return mVariableDataMap[declaration];
        }

        public Scope GetFunctionScope(FunctionDeclarationAst o)
        {
            if (!mFunctionScopeMap.ContainsKey(o))
                return null;
            return mFunctionScopeMap[o];
        }

        public void CompileAll()
        {
            var scopeCreator = new ScopeCreator(this);
            foreach (var file in mFiles.Values)
            {
                foreach (var s in file.Statements)
                {
                    scopeCreator.CreateScopes(s, GlobalScope);
                }
            }

            // gather declarations
            //foreach (var file in mFiles.Values)
            //{
            //    GatherDeclarations(GlobalScope, file.Statements);
            //}

            // define types

            // define functions
            foreach (var function in GlobalScope.FunctionDeclarations)
            {
                if (!GlobalScope.DefineFunction(function))
                {
                    ReportError(function.NameExpr, $"A function called '{function.Name}' already exists in current scope");
                }

                // check return type
                {
                    var returnType = CheezType.Void;
                    if (function.ReturnType != null)
                    {
                        returnType = GlobalScope.GetCheezType(function.ReturnType);
                        if (returnType == null)
                        {
                            ReportError(function.ReturnType, $"Unknown type '{function.ReturnType}' in function return type");
                        }
                    }
                    SetCheezType(function.ReturnType, returnType);
                }

                // check parameter types
                {
                    foreach (var p in function.Parameters)
                    {
                        var type = GlobalScope.GetCheezType(p.Type);
                        if (type == null)
                        {
                            ReportError(p.Type, $"Unknown type '{p.Type}' in function parameter list");
                        }
                        SetCheezType(p, type);
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
                typeChecker.CheckTypes(s, GlobalScope);
            }
        }

        private void GatherDeclarations(Scope scope, IEnumerable<Statement> statements)
        {
            foreach (var s in statements)
            {
                switch (s)
                {
                    case FunctionDeclarationAst f:
                        scope.FunctionDeclarations.Add(f);
                        SetScope(f, scope);
                        break;

                    case TypeDeclaration t:
                        scope.TypeDeclarations.Add(t);
                        SetScope(t, scope);
                        break;


                    case VariableDeclarationAst v:
                        scope.VariableDeclarations.Add(v);
                        SetScope(v, scope);
                        break;
                }
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
