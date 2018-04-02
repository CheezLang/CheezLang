using Cheez.Ast;
using System;
using System.Collections.Generic;

namespace Cheez
{
    public interface IScope
    {
        List<FunctionDeclaration> FunctionDeclarations { get; }
        List<VariableDeclaration> VariableDeclarations { get; }
        List<TypeDeclaration> TypeDeclarations { get; }

        CTypeFactory Types { get; }
    }

    public class Scope : IScope
    {
        public List<FunctionDeclaration> FunctionDeclarations { get; } = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
        public List<TypeDeclaration> TypeDeclarations { get; } = new List<TypeDeclaration>();

        public CTypeFactory Types { get; } = new CTypeFactory();

        public FunctionDeclaration GetFunction(string name, CType[] parameters)
        {
            throw new NotImplementedException();
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
    }
}
