using Cheez.Ast;
using System;
using System.Collections.Generic;

namespace Cheez
{
    public class Scope
    {
        public string Name { get; set; }

        public Scope Parent { get; }

        public List<FunctionDeclarationAst> FunctionDeclarations { get; } = new List<FunctionDeclarationAst>();
        public List<VariableDeclarationAst> VariableDeclarations { get; } = new List<VariableDeclarationAst>();
        public List<TypeDeclaration> TypeDeclarations { get; } = new List<TypeDeclaration>();

        private CTypeFactory types = new CTypeFactory();

        private Dictionary<string, FunctionDeclarationAst> mFunctionTable = new Dictionary<string, FunctionDeclarationAst>();
        private Dictionary<string, (CheezType, IVariableDeclaration)> mVariableTable = new Dictionary<string, (CheezType, IVariableDeclaration)>();

        public Scope(string name, Scope parent = null)
        {
            this.Name = name;
            this.Parent = parent;
        }

        public CheezType GetCheezType(TypeExpression expr)
        {
            return types.GetCType(expr) ?? Parent?.GetCheezType(expr);
        }

        public FunctionDeclarationAst GetFunction(string name, List<CheezType> parameters)
        {
            if (mFunctionTable.ContainsKey(name))
                return mFunctionTable[name];

            return Parent?.GetFunction(name, parameters);
        }

        public CheezType GetType(string name)
        {
            throw new NotImplementedException();
        }

        public bool DefineVariable(string name, IVariableDeclaration variable, CheezType type)
        {
            if (mVariableTable.ContainsKey(name))
                return false;

            mVariableTable[name] = (type, variable);

            return true;
        }

        public (CheezType type, IVariableDeclaration ast)? GetVariable(string name)
        {
            if (mVariableTable.ContainsKey(name))
                return mVariableTable[name];

            return Parent?.GetVariable(name);
        }

        public bool DefineFunction(FunctionDeclarationAst f)
        {
            if (mFunctionTable.ContainsKey(f.NameExpr.Name))
            {
                return false;
            }

            mFunctionTable[f.NameExpr.Name] = f;

            return true;
        }

        public override string ToString()
        {
            if (Parent != null)
                return $"{Parent}::{Name}";
            return $"::{Name}";
        }
    }
}
