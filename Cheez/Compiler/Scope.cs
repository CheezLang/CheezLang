using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using System;
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
        private Dictionary<string, List<IOperator>> mOperatorTable = new Dictionary<string, List<IOperator>>();

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

        public List<IOperator> GetOperators(string name, CheezType lhs, CheezType rhs)
        {
            var result = new List<IOperator>();
            int level = 0;
            GetOperator(name, lhs, rhs, result, ref level);
            return result;
        }

        private void GetOperator(string name, CheezType lhs, CheezType rhs, List<IOperator> result, ref int level)
        {
            if (!mOperatorTable.ContainsKey(name))
            {
                Parent?.GetOperator(name, lhs, rhs, result, ref level);
                return;
            }

            var ops = mOperatorTable[name];

            foreach (var op in ops)
            {
                if (CheckType(op.LhsType, lhs) && CheckType(op.RhsType, rhs))
                {
                    if (level < 2)
                        result.Clear();
                    result.Add(op);
                    level = 2;
                }
                //else if (level < 2 && CheckType(op.LhsType, lhs))
                //{
                //    if (level < 1)
                //        result.Clear();
                //    result.Add(op);
                //    level = 1;
                //}
                //else if (level < 2 && CheckType(op.RhsType, rhs))
                //{
                //    if (level < 1)
                //        result.Clear();
                //    result.Add(op);
                //    level = 1;
                //}
            }

            Parent?.GetOperator(name, lhs, rhs, result, ref level);
        }

        internal void DefineBuiltInOperators()
        {
            CheezType[] intTypes = new CheezType[]
            {
                IntType.GetIntType(1, true),
                IntType.GetIntType(2, true),
                IntType.GetIntType(4, true),
                IntType.GetIntType(8, true),
                IntType.GetIntType(1, false),
                IntType.GetIntType(2, false),
                IntType.GetIntType(4, false),
                IntType.GetIntType(8, false)
            };
            CheezType[] floatTypes = new CheezType[]
            {
                FloatType.GetFloatType(4),
                FloatType.GetFloatType(8)
            };

            DefineArithmeticOperators(intTypes, "+", "-", "*", "/", "%");
            DefineArithmeticOperators(floatTypes, "+", "-", "*", "/");

            // 
            DefineLogicOperators(intTypes, ">", ">=", "<", "<=", "==", "!=");
            DefineLogicOperators(floatTypes, ">", ">=", "<", "<=", "==", "!=");

            DefineLogicOperators(new CheezType[] { BoolType.Instance }, "and", "or");

        }

        private void DefineLogicOperators(CheezType[] types, params string[] ops)
        {
            foreach (var name in ops)
            {
                List<IOperator> list = null;
                if (mOperatorTable.ContainsKey(name))
                    list = mOperatorTable[name];
                else
                {
                    list = new List<IOperator>();
                    mOperatorTable[name] = list;
                }

                foreach (var t in types)
                {
                    list.Add(new BuiltInOperator(name, BoolType.Instance, t, t));
                }
            }
        }

        private void DefineArithmeticOperators(CheezType[] types, params string[] ops)
        {
            foreach (var name in ops)
            {
                List<IOperator> list = null;
                if (mOperatorTable.ContainsKey(name))
                    list = mOperatorTable[name];
                else
                {
                    list = new List<IOperator>();
                    mOperatorTable[name] = list;
                }
                
                foreach (var t in types)
                {
                    list.Add(new BuiltInOperator(name, t, t, t));
                }
            }
        }

        private bool CheckType(CheezType needed, CheezType got)
        {
            if (needed == got)
                return true;

            if (got == IntType.LiteralType)
            {
                return needed is IntType || needed is FloatType;
            }
            if (got == FloatType.LiteralType)
            {
                return needed is FloatType;
            }

            return false;
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
