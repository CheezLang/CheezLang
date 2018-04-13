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
    }

    public class Scope : IScope
    {
        public List<FunctionDeclaration> FunctionDeclarations { get; } = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
        public List<TypeDeclaration> TypeDeclarations { get; } = new List<TypeDeclaration>();

        public CTypeFactory Types { get; } = new CTypeFactory();

        private Dictionary<string, List<FunctionDeclaration>> mFunctionTable = new Dictionary<string, List<FunctionDeclaration>>();

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
    }
}
