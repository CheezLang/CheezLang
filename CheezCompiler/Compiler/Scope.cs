using Cheez.Compiler.Ast;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler
{
    public interface ISymbol : INamed
    {
        CheezType Type { get; }
        bool IsConstant { get; }
    }

    public class Using : ISymbol
    {
        public CheezType Type => Expr.Type;
        public AstIdentifierExpr Name { get; }

        public AstExpression Expr { get; }
        public bool IsConstant => true;

        [DebuggerStepThrough]
        public Using(AstIdentifierExpr name, AstExpression expr)
        {
            this.Name = name;
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return $"using {Expr}";
        }
    }

    public class CompTimeVariable : ISymbol
    {
        public AstIdentifierExpr Name { get; }
        public CheezType Type { get; }
        public bool IsConstant => true;
        public object Value { get; }

        public CompTimeVariable(string name, CheezType type, object value)
        {
            this.Name = new AstIdentifierExpr(null, name);
            this.Type = type;
            this.Value = value;
        }
    }

    //public class CheezTypeSymbol : ISymbol
    //{
    //    public AstIdentifierExpr Name { get; }
    //    public CheezType Type { get; }
    //    public CheezType Value { get; set; }

    //    public bool IsConstant => true;

    //    public CheezTypeSymbol(AstIdentifierExpr name, CheezType type)
    //    {
    //        this.Name = name;
    //        this.Value = type;
    //        this.Type = CheezType.Type;
    //    }
    //}

    public class Scope
    {
        public string Name { get; set; }

        public Scope Parent { get; }

        public List<AstFunctionDecl> FunctionDeclarations { get; } = new List<AstFunctionDecl>();
        public List<AstVariableDecl> VariableDeclarations { get; } = new List<AstVariableDecl>();
        public List<AstStatement> TypeDeclarations { get; } = new List<AstStatement>();
        public List<AstImplBlock> ImplBlocks { get; } = new List<AstImplBlock>();


        //private CTypeFactory types = new CTypeFactory();

        private Dictionary<string, ISymbol> mSymbolTable = new Dictionary<string, ISymbol>();
        private Dictionary<string, List<IOperator>> mOperatorTable = new Dictionary<string, List<IOperator>>();
        private Dictionary<string, List<IUnaryOperator>> mUnaryOperatorTable = new Dictionary<string, List<IUnaryOperator>>();
        private Dictionary<CheezType, List<AstFunctionDecl>> mImplTable = new Dictionary<CheezType, List<AstFunctionDecl>>();

        public IEnumerable<KeyValuePair<string, ISymbol>> Symbols => mSymbolTable.AsEnumerable();

        public Scope(string name, Scope parent = null)
        {
            this.Name = name;
            this.Parent = parent;
        }

        public Scope Clone()
        {
            return new Scope(Name, Parent)
            {
                // @Todo: clone everything?
            };
        }

        //public CheezType GetCheezType(PTTypeExpr expr)
        //{
        //    return types.GetCheezType(expr) ?? Parent?.GetCheezType(expr);
        //}

        //public CheezType GetCheezType(string name)
        //{
        //    return types.GetCheezType(name) ?? Parent?.GetCheezType(name);
        //}

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

        public List<IUnaryOperator> GetOperators(string name, CheezType sub)
        {
            var result = new List<IUnaryOperator>();
            int level = 0;
            GetOperator(name, sub, result, ref level);
            return result;
        }

        private void GetOperator(string name, CheezType sub, List<IUnaryOperator> result, ref int level)
        {
            if (!mOperatorTable.ContainsKey(name))
            {
                Parent?.GetOperator(name, sub, result, ref level);
                return;
            }

            var ops = mUnaryOperatorTable[name];

            foreach (var op in ops)
            {
                if (CheckType(op.SubExprType, sub))
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

            Parent?.GetOperator(name, sub, result, ref level);
        }

        internal void DefineBuiltInTypes()
        {
            DefineTypeSymbol("i8", IntType.GetIntType(1, true));
            DefineTypeSymbol("i16", IntType.GetIntType(2, true));
            DefineTypeSymbol("i32", IntType.GetIntType(4, true));
            DefineTypeSymbol("i64", IntType.GetIntType(8, true));

            DefineTypeSymbol("byte", IntType.GetIntType(1, true));
            DefineTypeSymbol("short", IntType.GetIntType(2, true));
            DefineTypeSymbol("int", IntType.GetIntType(4, true));
            DefineTypeSymbol("long", IntType.GetIntType(8, true));

            DefineTypeSymbol("u8", IntType.GetIntType(1, false));
            DefineTypeSymbol("u16", IntType.GetIntType(2, false));
            DefineTypeSymbol("u32", IntType.GetIntType(4, false));
            DefineTypeSymbol("u64", IntType.GetIntType(8, false));

            DefineTypeSymbol("ubyte", IntType.GetIntType(1, false));
            DefineTypeSymbol("ushort", IntType.GetIntType(2, false));
            DefineTypeSymbol("uint", IntType.GetIntType(4, false));
            DefineTypeSymbol("ulong", IntType.GetIntType(8, false));

            DefineTypeSymbol("f32", FloatType.GetFloatType(4));
            DefineTypeSymbol("float", FloatType.GetFloatType(4));
            DefineTypeSymbol("f64", FloatType.GetFloatType(8));
            DefineTypeSymbol("double", FloatType.GetFloatType(8));

            DefineTypeSymbol("bool", BoolType.Instance);
            DefineTypeSymbol("string", StringType.Instance);
            DefineTypeSymbol("void", VoidType.Intance);
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

            //
            DefineArithmeticUnaryOperators(intTypes, "-", "+");
            DefineArithmeticUnaryOperators(floatTypes, "-", "+");
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

        private void DefineArithmeticUnaryOperators(CheezType[] types, params string[] ops)
        {
            foreach (var name in ops)
            {
                List<IUnaryOperator> list = null;
                if (mUnaryOperatorTable.ContainsKey(name))
                    list = mUnaryOperatorTable[name];
                else
                {
                    list = new List<IUnaryOperator>();
                    mUnaryOperatorTable[name] = list;
                }

                foreach (var t in types)
                {
                    list.Add(new BuiltInUnaryOperator(name, t, t));
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
            if (mSymbolTable.ContainsKey(symbol.Name.Name))
                return false;

            mSymbolTable[symbol.Name.Name] = symbol;
            return true;
        }

        public bool DefineTypeSymbol(string name, CheezType symbol)
        {
            if (mSymbolTable.ContainsKey(name))
                return false;

            mSymbolTable[name] = new CompTimeVariable(name, CheezType.Type, symbol);
            return true;
        }

        public ISymbol GetSymbol(string name)
        {
            if (mSymbolTable.ContainsKey(name))
                return mSymbolTable[name];
            return Parent?.GetSymbol(name);
        }

        public bool DefineImplFunction(CheezType targetType, AstFunctionDecl f)
        {
            if (!mImplTable.TryGetValue(targetType, out var list))
            {
                list = new List<AstFunctionDecl>();
                mImplTable[targetType] = list;
            }

            if (list.Any(ff => ff.Name == f.Name))
                return false;

            list.Add(f);

            return true;
        }

        public AstFunctionDecl GetImplFunction(CheezType targetType, string name)
        {
            if (mImplTable.TryGetValue(targetType, out var list))
            {
                return list.FirstOrDefault(f => f.Name.Name == name) ?? Parent?.GetImplFunction(targetType, name);
            }

            return Parent?.GetImplFunction(targetType, name);
        }

        //public EnumType DefineType(AstEnumDecl en)
        //{
        //    if (types.GetCheezType(en.Name.Name) != null)
        //        return null;

        //    var type = new EnumType(en);
        //    types.CreateAlias(en.Name.Name, type);
        //    return type;
        //}

        //public bool DefineType(AstTypeDecl t)
        //{
        //    if (types.GetCheezType(t.Name.Name) != null)
        //        return false;

        //    types.CreateAlias(t.Name.Name, new StructType(t));
        //    return true;
        //}

        //public bool DefineType(string name, CheezType type)
        //{
        //    if (types.GetCheezType(name) != null)
        //        return false;

        //    types.CreateAlias(name, type);
        //    return true;
        //}

        public override string ToString()
        {
            if (Parent != null)
                return $"{Parent}::{Name}";
            return $"::{Name}";
        }
    }
}
