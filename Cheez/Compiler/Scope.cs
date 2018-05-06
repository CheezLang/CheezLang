using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using System.Collections.Generic;

namespace Cheez.Compiler
{
    public interface ISymbol : INamed
    {
        CheezType Type { get; }
    }

    public class Scope
    {
        public string Name { get; set; }

        public Scope Parent { get; }

        public List<AstFunctionDecl> FunctionDeclarations { get; } = new List<AstFunctionDecl>();
        public List<AstVariableDecl> VariableDeclarations { get; } = new List<AstVariableDecl>();
        public List<AstTypeDecl> TypeDeclarations { get; } = new List<AstTypeDecl>();

        private CTypeFactory types = new CTypeFactory();

        private Dictionary<string, ISymbol> mSymbolTable = new Dictionary<string, ISymbol>();

        public Scope(string name, Scope parent = null)
        {
            this.Name = name;
            this.Parent = parent;
        }

        public CheezType GetCheezType(PTTypeExpr expr)
        {
            return types.GetCheezType(expr) ?? Parent?.GetCheezType(expr);
        }

        public CheezType GetCheezType(string name)
        {
            return types.GetCheezType(name) ?? Parent?.GetCheezType(name);
        }

        public bool DefineSymbol(ISymbol symbol)
        {
            if (mSymbolTable.ContainsKey(symbol.Name))
                return false;

            mSymbolTable[symbol.Name] = symbol;
            return true;
        }

        public ISymbol GetSymbol(string name)
        {
            if (mSymbolTable.ContainsKey(name))
                return mSymbolTable[name];
            return Parent?.GetSymbol(name);
        }

        public bool DefineType(AstTypeDecl t)
        {
            if (types.GetCheezType(t.Name) != null)
                return false;

            types.CreateAlias(t.Name, new StructType(t));
            return true;
        }

        public bool DefineType(string name, CheezType type)
        {
            if (types.GetCheezType(name) != null)
                return false;

            types.CreateAlias(name, type);
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
