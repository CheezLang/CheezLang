using Cheez.Ast;
using System.Collections.Generic;

namespace Cheez
{
    public interface IScope
    {
        List<FunctionDeclaration> Functions { get; }
        List<VariableDeclaration> VariableDeclarations { get; }
        List<object> TypeDeclarations { get; }

        CTypeFactory Types { get; }
    }

    public class Scope : IScope
    {
        public List<FunctionDeclaration> Functions { get; } = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
        public List<object> TypeDeclarations { get; }

        public CTypeFactory Types { get; } = new CTypeFactory();
    }

    public class ScopeRef : IScope
    {
        public Scope Scope { get; }
        public IScope Parent { get; }

        public List<FunctionDeclaration> Functions => Scope.Functions;
        public List<VariableDeclaration> VariableDeclarations => Scope.VariableDeclarations;
        public List<object> TypeDeclarations => Scope.TypeDeclarations;

        public CTypeFactory Types => Scope.Types;

        public ScopeRef(Scope scope, IScope parent = null)
        {
            this.Scope = scope;
            this.Parent = parent;
        }
    }
}
