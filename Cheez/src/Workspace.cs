using Cheez.Ast;
using Cheez.SemanticAnalysis;
using System;
using System.Collections.Generic;

namespace Cheez
{
    public class Workspace
    {
        public Scope GlobalScope { get; set; } = new Scope();

        private List<CheezFile> mFiles = new List<CheezFile>();
        private Dictionary<object, CType> mTypeMap = new Dictionary<object, CType>();
        private Dictionary<object, IScope> mScopeMap = new Dictionary<object, IScope>();
        private Dictionary<FunctionDeclaration, IScope> mFunctionScopeMap = new Dictionary<FunctionDeclaration, IScope>();
        private Compiler mCompiler;

        private PriorityQueue<CompilationUnit> mCompilationQueue = new PriorityQueue<CompilationUnit>();
        private List<(CompilationUnit unit, object condition)> mWaitingQueue = new List<(CompilationUnit, object)>();

        public Workspace(Compiler comp)
        {
            mCompiler = comp;
        }

        public void AddFile(CheezFile file)
        {
            mFiles.Add(file);
        }

        public void SetType(object o, CType type)
        {
            mTypeMap[o] = type;
        }

        public CType GetType(object o)
        {
            // @Todo
            //if (!mTypeMap.ContainsKey(o))
            //    return null;
            return mTypeMap[o];
        }

        public void SetScope(object o, IScope scope)
        {
            mScopeMap[o] = scope;
        }

        public IScope GetScope(object o)
        {
            // @Todo
            //if (!mTypeMap.ContainsKey(o))
            //    return null;
            return mScopeMap[o];
        }

        public void CompileAll()
        {
            // gather declarations
            foreach (var file in mFiles)
            {
                GatherDeclarations(GlobalScope, file.Statements);
            }

            // define types

            // define functions
            foreach (var f in GlobalScope.FunctionDeclarations)
            {

            }

            // define global variables

            // compile functions
            TypeChecker typeChecker = new TypeChecker(this);
            foreach (var s in GlobalScope.FunctionDeclarations)
            {
                typeChecker.CheckTypes(s);
            }
        }

        private void GatherDeclarations(IScope scope, IEnumerable<Statement> statements)
        {
            foreach (var s in statements)
            {
                switch (s)
                {
                    case FunctionDeclaration f:
                        scope.FunctionDeclarations.Add(f);
                        SetScope(f, scope);
                        {
                            var funcScope = new Scope();
                            var funcScopeRef = new ScopeRef(funcScope, scope);
                            mFunctionScopeMap[f] = funcScopeRef;
                            GatherDeclarations(scope, f.Statements);
                        }
                        break;

                    case TypeDeclaration t:
                        scope.TypeDeclarations.Add(t);
                        SetScope(t, scope);
                        break;


                    case VariableDeclaration v:
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
    }
}
