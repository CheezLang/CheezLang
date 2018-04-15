using Cheez.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public interface IScope
    {
        List<FunctionDeclaration> FunctionDeclarations { get; }
        List<VariableDeclaration> VariableDeclarations { get; }
        List<TypeDeclaration> TypeDeclarations { get; }

        CTypeFactory Types { get; }

        FunctionDeclaration GetFunction(string name, List<CType> parameters);
        bool DefineVariable(string name, VariableDeclaration variable, CType type);
        (CType type, VariableDeclaration ast)? GetVariable(string name);
    }

    public class Scope : IScope
    {
        public List<FunctionDeclaration> FunctionDeclarations { get; } = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
        public List<TypeDeclaration> TypeDeclarations { get; } = new List<TypeDeclaration>();

        public CTypeFactory Types { get; } = new CTypeFactory();

        private Dictionary<string, List<FunctionDeclaration>> mFunctionTable = new Dictionary<string, List<FunctionDeclaration>>();
        private Dictionary<string, (CType, VariableDeclaration)> mVariableTable = new Dictionary<string, (CType, VariableDeclaration)>();

        public FunctionDeclaration GetFunction(string name, List<CType> parameters)
        {
            if (!mFunctionTable.ContainsKey(name))
            {
                List<FunctionDeclaration> funcs = FunctionDeclarations.Where(f => f.Name == name).ToList();
                mFunctionTable[name] = funcs;
            }

            {
                var funcs = mFunctionTable[name];
                return BestFittingFunction(funcs, parameters);
            }
        }

        private FunctionDeclaration BestFittingFunction(List<FunctionDeclaration> funcs, List<CType> parameters)
        {
            foreach (var f in funcs)
            {
                return f;
            }
            return null;
        }

        public CType GetType(string name)
        {
            throw new NotImplementedException();
        }

        public bool DefineVariable(string name, VariableDeclaration variable, CType type)
        {
            if (mVariableTable.ContainsKey(name))
                return false;

            mVariableTable[name] = (type, variable);

            return true;
        }

        public (CType type, VariableDeclaration ast)? GetVariable(string name)
        {
            if (mVariableTable.ContainsKey(name))
                return mVariableTable[name];

            return null;
        }
    }

    public class ScopeRef : IScope
    {
        public Scope Scope { get; }
        public IScope Parent { get; }

        public List<FunctionDeclaration> FunctionDeclarations => Scope.FunctionDeclarations;
        public List<VariableDeclaration> VariableDeclarations => Scope.VariableDeclarations;
        public List<TypeDeclaration> TypeDeclarations => Scope.TypeDeclarations;

        public CTypeFactory Types => Scope.Types;

        public ScopeRef(Scope scope, IScope parent = null)
        {
            this.Scope = scope;
            this.Parent = parent;
        }

        public FunctionDeclaration GetFunction(string name, List<CType> parameters)
        {
            var func = Scope.GetFunction(name, parameters);
            if (func == null && Parent != null)
                func = Parent.GetFunction(name, parameters);
            return func;
        }

        public bool DefineVariable(string name, VariableDeclaration variable, CType type)
        {
            return Scope.DefineVariable(name, variable, type);
        }

        public (CType type, VariableDeclaration ast)? GetVariable(string name)
        {
            var v = Scope.GetVariable(name);
            if (v == null && Parent != null)
                v = Parent.GetVariable(name);
            return v;
        }
    }
}
