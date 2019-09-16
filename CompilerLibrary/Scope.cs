using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public interface ISymbol
    {
        AstIdExpr Name { get; }
        //CheezType Type { get; }
        ILocation Location { get; }
    }

    public interface ITypedSymbol : ISymbol
    {
        CheezType Type { get; }
    }

    public class ConstSymbol : ITypedSymbol
    {
        private static int _id_counter = 0;

        public ILocation Location => null;
        public AstIdExpr Name { get; private set; }

        public CheezType Type { get; private set; }
        public object Value { get; private set; }
        public readonly int Id = _id_counter++;

        public ConstSymbol(string name, CheezType type, object value)
        {
            this.Name = new AstIdExpr(name, false);
            this.Type = type;
            this.Value = value;
        }
    }

    public class TypeSymbol : ITypedSymbol
    {
        public ILocation Location => null;
        public AstIdExpr Name { get; private set; }

        public CheezType Type { get; private set; }

        public TypeSymbol(string name, CheezType type)
        {
            this.Name = new AstIdExpr(name, false);
            this.Type = type;
        }
    }

    public class Using : ITypedSymbol
    {
        public CheezType Type => Expr.Type;
        public AstIdExpr Name => throw new NotImplementedException();

        public AstExpression Expr { get; }
        public bool IsConstant => true;

        public ILocation Location { get; set; }
        public bool Replace = false;


        [DebuggerStepThrough]
        public Using(AstExpression expr, bool replace)
        {
            this.Location = expr.Location;
            this.Expr = expr;
            this.Replace = replace;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return $"using {Expr}";
        }
    }



    public struct SymbolStatus
    {
        public enum Kind
        {
            initialized,
            uninitialized,
            moved
        }

        public readonly int order;
        public ISymbol symbol;
        public Kind kind;
        public ILocation location;

        public SymbolStatus(int order, ISymbol symbol, Kind kind, ILocation location)
        {
            this.order = order;
            this.symbol = symbol;
            this.kind = kind;
            this.location = location;
        }

        public override string ToString() => $"{symbol.Name}: {kind} @ {location} [{location.Beginning}]";
    }

    public class Scope
    {
        public string Name { get; set; }
        public Scope Parent { get; }
        public bool IsOrdered { get; set; } = true;
        private int nextPosition = 1;

        private Scope mLinkedScope;
        public Scope LinkedScope
        {
            set { mLinkedScope = value; }
            get => mLinkedScope ?? Parent?.LinkedScope;
        }

        private Dictionary<string, ISymbol> mSymbolTable = new Dictionary<string, ISymbol>();
        private Dictionary<string, List<INaryOperator>> mNaryOperatorTable = new Dictionary<string, List<INaryOperator>>();
        private Dictionary<string, List<IBinaryOperator>> mBinaryOperatorTable = new Dictionary<string, List<IBinaryOperator>>();
        private Dictionary<string, List<IUnaryOperator>> mUnaryOperatorTable = new Dictionary<string, List<IUnaryOperator>>();
        private Dictionary<AstImplBlock, List<AstFunctionDecl>> mImplTable = new Dictionary<AstImplBlock, List<AstFunctionDecl>>();
        private Dictionary<ISymbol, SymbolStatus> mSymbolStatus;
        private List<AstFunctionDecl> mForExtensions = null;
        private (string label, object loopOrAction)? mBreak = null;
        private (string label, object loopOrAction)? mContinue = null;

        public ISymbol[] SymbolStatuses => mSymbolStatus?.Keys?.ToArray();
        public IEnumerable<SymbolStatus> AllSymbolStatusesReverseOrdered => mSymbolStatus?.Values.OrderByDescending(s => s.order);
        public IEnumerable<SymbolStatus> SymbolStatusesReverseOrdered => mSymbolStatus.Values
                                .Where(v => mSymbolTable.ContainsValue(v.symbol))
                                .OrderByDescending(s => s.order);

        //
        public List<AstStructDecl> StructDeclarations = new List<AstStructDecl>();
        public List<AstTraitDeclaration> TraitDeclarations = new List<AstTraitDeclaration>();
        public List<AstEnumDecl> EnumDeclarations = new List<AstEnumDecl>();
        public List<AstTypeAliasDecl> Typedefs = new List<AstTypeAliasDecl>();
        public List<AstUsingStmt> Uses = new List<AstUsingStmt>();

        public List<AstVariableDecl> Variables = new List<AstVariableDecl>();
        public List<AstFunctionDecl> Functions = new List<AstFunctionDecl>();
        public List<AstImplBlock> Impls = new List<AstImplBlock>();

        public Queue<AstImplBlock> unresolvedImpls = new Queue<AstImplBlock>();
        //

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
                mSymbolTable = new Dictionary<string, ISymbol>(mSymbolTable),
                mBinaryOperatorTable = new Dictionary<string, List<IBinaryOperator>>(mBinaryOperatorTable),
                mUnaryOperatorTable = new Dictionary<string, List<IUnaryOperator>>(mUnaryOperatorTable)
                // TODO: mImplTable?, rest?
            };
        }

        public void InitSymbolStats()
        {
            mSymbolStatus = new Dictionary<ISymbol, SymbolStatus>();
            if (Parent?.mSymbolStatus != null)
            {
                foreach (var symbol in Parent.mSymbolStatus)
                    mSymbolStatus[symbol.Key] = symbol.Value;
            }
            if (LinkedScope?.mSymbolStatus != null)
            {
                foreach (var symbol in LinkedScope.mSymbolStatus)
                    mSymbolStatus[symbol.Key] = symbol.Value;
            }
        }

        public void SetSymbolStatus(ISymbol symbol, SymbolStatus.Kind holdsValue, ILocation location)
        {
            var order = mSymbolStatus.Count;
            if (mSymbolStatus.TryGetValue(symbol, out var stat))
                order = stat.order;

            mSymbolStatus[symbol] = new SymbolStatus(order, symbol, holdsValue, location);
        }

        public SymbolStatus GetSymbolStatus(ISymbol symbol) => mSymbolStatus[symbol];

        public bool TryGetSymbolStatus(ISymbol symbol, out SymbolStatus status)
        {
            if (mSymbolStatus.TryGetValue(symbol, out var s))
            {
                status = s;
                return true;
            }

            status = default;
            return false;
        }

        public void ApplyInitializedSymbolsToParent()
        {
            if (Parent == null)
                return;

            if (Parent.mSymbolStatus == null)
                Parent.InitSymbolStats();

            foreach (var s in Parent.mSymbolStatus.Keys.ToArray())
            {
                Parent.mSymbolStatus[s] = mSymbolStatus[s];
            }
        }

        public void ApplyInitializedSymbolsTo(Scope scope)
        {
            foreach (var s in scope.mSymbolStatus.Keys.ToArray())
            {
                scope.mSymbolStatus[s] = mSymbolStatus[s];
            }
        }

        public void AddForExtension(AstFunctionDecl func)
        {
            if (mForExtensions == null)
                mForExtensions = new List<AstFunctionDecl>();
            mForExtensions.Add(func);
        }

        public List<AstFunctionDecl> GetForExtensions(CheezType type)
        {
            return mForExtensions?.Where(func =>
            {
                var paramType = func.Parameters[0].Type;
                if (paramType is ReferenceType r)
                    paramType = r.TargetType;
                if (CheezType.TypesMatch(paramType, type))
                    return true;
                return false;
            })?.ToList() ?? Parent?.GetForExtensions(type) ?? new List<AstFunctionDecl>();
        }

        public List<INaryOperator> GetNaryOperators(string name, params CheezType[] types)
        {
            var result = new List<INaryOperator>();
            int level = int.MaxValue;
            GetOperator(name, result, ref level, types);
            return result;
        }

        private void GetOperator(string name, List<INaryOperator> result, ref int level, params CheezType[] types)
        {
            if (!mNaryOperatorTable.ContainsKey(name))
            {
                Parent?.GetOperator(name, result, ref level, types);
                return;
            }

            var ops = mNaryOperatorTable[name];

            foreach (var op in ops)
            {
                var l = op.Accepts(types);
                if (l == -1)
                    continue;

                if (l < level)
                {
                    level = l;
                    result.Clear();
                    result.Add(op);
                }
                else if (l == level)
                {
                    result.Add(op);
                }
            }

            Parent?.GetOperator(name, result, ref level, types);
        }

        public List<IBinaryOperator> GetBinaryOperators(string name, CheezType lhs, CheezType rhs)
        {
            var result = new List<IBinaryOperator>();
            int level = int.MaxValue;
            GetOperator(name, lhs, rhs, result, ref level);
            return result;
        }

        private void GetOperator(string name, CheezType lhs, CheezType rhs, List<IBinaryOperator> result, ref int level)
        {
            if (!mBinaryOperatorTable.ContainsKey(name))
            {
                Parent?.GetOperator(name, lhs, rhs, result, ref level);
                return;
            }

            var ops = mBinaryOperatorTable[name];

            foreach (var op in ops)
            {
                var l = op.Accepts(lhs, rhs);
                if (l == -1)
                    continue;

                if (l < level)
                {
                    level = l;
                    result.Clear();
                    result.Add(op);
                }
                else if (l == level)
                {
                    result.Add(op);
                }
            }

            Parent?.GetOperator(name, lhs, rhs, result, ref level);
        }

        public List<IUnaryOperator> GetUnaryOperators(string name, CheezType sub)
        {
            var result = new List<IUnaryOperator>();
            int level = int.MaxValue;
            GetOperator(name, sub, result, ref level);
            return result;
        }

        private void GetOperator(string name, CheezType sub, List<IUnaryOperator> result, ref int level)
        {
            if (!mUnaryOperatorTable.ContainsKey(name))
            {
                Parent?.GetOperator(name, sub, result, ref level);
                return;
            }

            var ops = mUnaryOperatorTable[name];

            foreach (var op in ops)
            {
                var l = op.Accepts(sub);
                if (l == -1)
                    continue;

                if (l < level)
                {
                    level = l;
                    result.Clear();
                    result.Add(op);
                }
                else if (l == level)
                {
                    result.Add(op);
                }
            }

            Parent?.GetOperator(name, sub, result, ref level);
        }

        public void DefineUnaryOperator(string op, AstFunctionDecl func)
        {
            DefineUnaryOperator(new UserDefinedUnaryOperator(op, func));
        }

        public void DefineBinaryOperator(string op, AstFunctionDecl func)
        {
            DefineBinaryOperator(new UserDefinedBinaryOperator(op, func));
        }

        public void DefineOperator(string op, AstFunctionDecl func)
        {
            DefineOperator(new UserDefinedNaryOperator(op, func));
        }

        internal void DefineBuiltInTypes()
        {
            DefineTypeSymbol("i8", IntType.GetIntType(1, true));
            DefineTypeSymbol("i16", IntType.GetIntType(2, true));
            DefineTypeSymbol("i32", IntType.GetIntType(4, true));
            DefineTypeSymbol("i64", IntType.GetIntType(8, true));

            DefineTypeSymbol("u8", IntType.GetIntType(1, false));
            DefineTypeSymbol("u16", IntType.GetIntType(2, false));
            DefineTypeSymbol("u32", IntType.GetIntType(4, false));
            DefineTypeSymbol("u64", IntType.GetIntType(8, false));

            DefineTypeSymbol("int", IntType.GetIntType(8, true));
            DefineTypeSymbol("uint", IntType.GetIntType(8, false));

            DefineTypeSymbol("f32", FloatType.GetFloatType(4));
            DefineTypeSymbol("f64", FloatType.GetFloatType(8));
            DefineTypeSymbol("float", FloatType.GetFloatType(4));
            DefineTypeSymbol("double", FloatType.GetFloatType(8));

            DefineTypeSymbol("char", CheezType.Char);
            DefineTypeSymbol("bool", CheezType.Bool);
            DefineTypeSymbol("c_string", CheezType.CString);
            DefineTypeSymbol("string", CheezType.String);
            DefineTypeSymbol("void", CheezType.Void);
            DefineTypeSymbol("any", CheezType.Any);
            DefineTypeSymbol("type", CheezType.Type);
            DefineTypeSymbol("Code", CheezType.Code);
        }

        internal void DefineBuiltInOperators()
        {
            DefineLiteralOperators();

            DefineLogicOperators(new CheezType[] { BoolType.Instance }, 
                ("and", (a, b) => (bool)a && (bool)b), 
                ("or", (a, b) => (bool)a || (bool)b),
                ("==", (a, b) => (bool)a == (bool)b),
                ("!=", (a, b) => (bool)a != (bool)b));
            
            DefinePointerOperators();

            DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, CheezType.Type, CheezType.Type, (a, b) => a == b));
            DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, CheezType.Type, CheezType.Type, (a, b) => a != b));

            DefineBinaryOperator(new BuiltInTraitNullOperator("=="));
            DefineBinaryOperator(new BuiltInTraitNullOperator("!="));
        }

        private void DefineUnaryOperator(string name, CheezType type, BuiltInUnaryOperator.ComptimeExecution exe)
        {
            DefineUnaryOperator(new BuiltInUnaryOperator(name, type, type, exe));
        }

        private void DefineUnaryOperator(IUnaryOperator op)
        {
            List<IUnaryOperator> list = null;
            if (mUnaryOperatorTable.ContainsKey(op.Name))
                list = mUnaryOperatorTable[op.Name];
            else
            {
                list = new List<IUnaryOperator>();
                mUnaryOperatorTable[op.Name] = list;
            }

            list.Add(op);
        }

        private void DefineLogicOperators(CheezType[] types, params (string name, BuiltInBinaryOperator.ComptimeExecution exe)[] ops)
        {
            foreach (var op in ops)
            {
                List<IBinaryOperator> list = null;
                if (mBinaryOperatorTable.ContainsKey(op.name))
                    list = mBinaryOperatorTable[op.name];
                else
                {
                    list = new List<IBinaryOperator>();
                    mBinaryOperatorTable[op.name] = list;
                }

                foreach (var t in types)
                {
                    list.Add(new BuiltInBinaryOperator(op.name, BoolType.Instance, t, t, op.exe));
                }
            }
        }

        private void DefinePointerOperators()
        {
            foreach (var op in new string[] { "==", "!=" })
            {
                List<IBinaryOperator> list = null;
                if (mBinaryOperatorTable.ContainsKey(op))
                    list = mBinaryOperatorTable[op];
                else
                {
                    list = new List<IBinaryOperator>();
                    mBinaryOperatorTable[op] = list;
                }
                
                list.Add(new BuiltInPointerOperator(op));
            }
        }

        private void DefineLiteralOperators()
        {
            // literal types
            DefineUnaryOperator("!", CheezType.Bool, b => !(bool)b);
            DefineUnaryOperator("-", IntType.LiteralType, a => ((NumberData)a).Negate());
            DefineUnaryOperator("-", FloatType.LiteralType, a => ((NumberData)a).Negate());

            DefineBinaryOperator(new BuiltInBinaryOperator("+", IntType.LiteralType, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a + (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("-", IntType.LiteralType, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a - (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("*", IntType.LiteralType, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a * (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("/", IntType.LiteralType, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a / (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("%", IntType.LiteralType, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a % (NumberData)b));

            DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a == (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a != (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("<", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a < (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("<=", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a <= (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator(">", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a > (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator(">=", CheezType.Bool, IntType.LiteralType, IntType.LiteralType, (a, b) => (NumberData)a >= (NumberData)b));


            DefineBinaryOperator(new BuiltInBinaryOperator("+", FloatType.LiteralType, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a + (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("-", FloatType.LiteralType, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a - (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("*", FloatType.LiteralType, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a * (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("/", FloatType.LiteralType, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a / (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("%", FloatType.LiteralType, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a % (NumberData)b));

            DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a == (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a != (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("<", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a < (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator("<=", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a <= (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator(">", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a > (NumberData)b));
            DefineBinaryOperator(new BuiltInBinaryOperator(">=", CheezType.Bool, FloatType.LiteralType, FloatType.LiteralType, (a, b) => (NumberData)a >= (NumberData)b));


            // basic types

            foreach (var type in new CheezType[]
            {
                IntType.GetIntType(1, true),
                IntType.GetIntType(2, true),
                IntType.GetIntType(4, true),
                IntType.GetIntType(8, true),
                IntType.GetIntType(1, false),
                IntType.GetIntType(2, false),
                IntType.GetIntType(4, false),
                IntType.GetIntType(8, false),
                CheezType.Char
            })
            {
                DefineUnaryOperator("-", type, a => ((NumberData)a).Negate());

                DefineBinaryOperator(new BuiltInBinaryOperator("+", type, type, type, (a, b) => (NumberData)a + (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("-", type, type, type, (a, b) => (NumberData)a - (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("*", type, type, type, (a, b) => (NumberData)a * (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("/", type, type, type, (a, b) => (NumberData)a / (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("%", type, type, type, (a, b) => (NumberData)a % (NumberData)b));

                DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, type, type, (a, b) => (NumberData)a == (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, type, type, (a, b) => (NumberData)a != (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("<", CheezType.Bool, type, type, (a, b) => (NumberData)a < (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("<=", CheezType.Bool, type, type, (a, b) => (NumberData)a <= (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator(">", CheezType.Bool, type, type, (a, b) => (NumberData)a > (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator(">=", CheezType.Bool, type, type, (a, b) => (NumberData)a >= (NumberData)b));
            }

            foreach (var type in new FloatType[] { FloatType.GetFloatType(4), FloatType.GetFloatType(8) })
            {
                DefineUnaryOperator("-", type, a => ((NumberData)a).Negate());

                DefineBinaryOperator(new BuiltInBinaryOperator("+", type, type, type, (a, b) => (NumberData)a + (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("-", type, type, type, (a, b) => (NumberData)a - (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("*", type, type, type, (a, b) => (NumberData)a * (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("/", type, type, type, (a, b) => (NumberData)a / (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("%", type, type, type, (a, b) => (NumberData)a % (NumberData)b));

                DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, type, type, (a, b) => (NumberData)a == (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, type, type, (a, b) => (NumberData)a != (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("<", CheezType.Bool, type, type, (a, b) => (NumberData)a < (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator("<=", CheezType.Bool, type, type, (a, b) => (NumberData)a <= (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator(">", CheezType.Bool, type, type, (a, b) => (NumberData)a > (NumberData)b));
                DefineBinaryOperator(new BuiltInBinaryOperator(">=", CheezType.Bool, type, type, (a, b) => (NumberData)a >= (NumberData)b));
            }
        }

        internal int NextPosition()
        {
            if (IsOrdered)
            {
                if (Parent != null && Parent.nextPosition > nextPosition)
                    nextPosition = Parent.nextPosition;
                return nextPosition++;
            }
            return 0;
        }

        private void DefineArithmeticOperators(CheezType[] types, params string[] ops)
        {
            foreach (var name in ops)
            {
                List<IBinaryOperator> list = null;
                if (mBinaryOperatorTable.ContainsKey(name))
                    list = mBinaryOperatorTable[name];
                else
                {
                    list = new List<IBinaryOperator>();
                    mBinaryOperatorTable[name] = list;
                }

                foreach (var t in types)
                {
                    list.Add(new BuiltInBinaryOperator(name, t, t, t));
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

        private void DefineBinaryOperator(IBinaryOperator op)
        {
            List<IBinaryOperator> list = null;
            if (mBinaryOperatorTable.ContainsKey(op.Name))
                list = mBinaryOperatorTable[op.Name];
            else
            {
                list = new List<IBinaryOperator>();
                mBinaryOperatorTable[op.Name] = list;
            }

            list.Add(op);
        }

        private void DefineOperator(INaryOperator op)
        {
            List<INaryOperator> list = null;
            if (mNaryOperatorTable.ContainsKey(op.Name))
                list = mNaryOperatorTable[op.Name];
            else
            {
                list = new List<INaryOperator>();
                mNaryOperatorTable[op.Name] = list;
            }

            list.Add(op);
        }

        public void DefineLoop(AstWhileStmt loop)
        {
            if (mBreak != null)
                throw new Exception("Well that's not supposed to happen...");

            var name = loop.Label?.Name;
            mBreak = (name, loop);
            mContinue = (name, loop);
        }

        public void OverrideBreakName(string label)
        {
            if (mBreak == null)
                throw new Exception("Well that's not supposed to happen...");

            if (mBreak != null)
                mBreak = (label, mBreak.Value.loopOrAction);
        }

        public void OverrideContinueName(string label)
        {
            if (mContinue == null)
                throw new Exception("Well that's not supposed to happen...");

            if (mContinue != null)
                mContinue = (label, mContinue.Value.loopOrAction);
        }

        public void DefineBreak(string name, AstExpression action)
        {
            if (mBreak != null)
                throw new Exception("Well that's not supposed to happen...");

            mBreak = (name, action);
        }

        public void DefineContinue(string name, AstExpression action)
        {
            if (mContinue != null)
                throw new Exception("Well that's not supposed to happen...");

            mContinue = (name, action);
        }

        public object GetBreak(string label = null)
        {
            if (mBreak != null && (label == null || mBreak.Value.label == label))
                return mBreak.Value.loopOrAction;

            return Parent?.GetBreak(label);
        }

        public object GetContinue(string label = null)
        {
            if (mContinue != null && (label == null || mContinue.Value.label == label))
                return mContinue.Value.loopOrAction;

            return Parent?.GetContinue(label);
        }

        public (bool ok, ILocation other) DefineSymbol(ISymbol symbol, string name = null)
        {
            name = name ?? symbol.Name.Name;
            if (mSymbolTable.TryGetValue(name, out var other))
                return (false, other.Location);

            mSymbolTable[name] = symbol;

            //switch (symbol)
            //{
            //    case AstSingleVariableDecl v when !v.GetFlag(StmtFlags.GlobalScope):
            //    case AstParameter p when p.IsReturnParam:
            //        SetSymbolStatus(symbol, SymbolStatus.Kind.uninitialized, symbol.Location);
            //        break;
            //}

            return (true, null);
        }

        public (bool ok, ILocation other) DefineUse(string name, AstExpression expr, bool replace, out Using use)
        {
            use = null;
            if (mSymbolTable.TryGetValue(name, out var other))
                return (false, other.Location);

            use = new Using(expr, replace);
            mSymbolTable[name] = use;
            return (true, null);
        }

        public (bool ok, ILocation other) DefineConstant(string name, CheezType type, object value)
        {
            if (mSymbolTable.TryGetValue(name, out var other))
                return (false, other.Location);

            mSymbolTable[name] = new ConstSymbol(name, type, value);
            return (true, null);
        }

        public bool DefineTypeSymbol(string name, CheezType symbol)
        {
            if (symbol is null)
                throw new ArgumentNullException(nameof(symbol));

            if (mSymbolTable.ContainsKey(name))
                return false;

            mSymbolTable[name] = new TypeSymbol(name, symbol);
            return true;
        }

        public (bool ok, ILocation other) DefineDeclaration(AstDecl decl)
        {
            string name = decl.Name.Name;
            if (mSymbolTable.TryGetValue(name, out var other))
                return (false, other.Location);

            mSymbolTable[name] = decl;
            return (true, null);
        }

        public ISymbol GetSymbol(string name)
        {
            if (mSymbolTable.ContainsKey(name))
            {
                var v = mSymbolTable[name];
                return v;
            }
            return Parent?.GetSymbol(name);
        }

        public IEnumerable<AstFunctionDecl> GetFunctionsWithDirective(string directive)
        {
            foreach (var f in Functions)
                if (f.HasDirective(directive))
                    yield return f;

            if (Parent != null)
                foreach (var f in Parent.GetFunctionsWithDirective(directive))
                    yield return f;
        }

        public bool DefineImplFunction(AstFunctionDecl f)
        {
            if (!mImplTable.TryGetValue(f.ImplBlock, out var list))
            {
                list = new List<AstFunctionDecl>();
                mImplTable[f.ImplBlock] = list;
            }

            if (list.Any(ff => ff.Name == f.Name))
                return false;

            list.Add(f);

            return true;
        }

        public List<AstFunctionDecl> GetImplFunction(CheezType targetType, string name)
        {
            var impls = mImplTable.Where(kv =>
            {
                var implType = kv.Key.TargetType;
                if (CheezType.TypesMatch(implType, targetType))
                    return true;
                return false;
            });

            var candidates = new List<AstFunctionDecl>();

            foreach (var impl in impls)
            {
                var list = impl.Value;
                
                var c = list?.FirstOrDefault(f => f.Name.Name == name);
                if (c != null)
                    candidates.Add(c);

            }

            if (candidates.Count == 0)
            {
                if (Parent != null)
                    return Parent.GetImplFunction(targetType, name);
                return candidates;
            }

            return candidates;
        }

        public AstFunctionDecl GetImplFunctionWithDirective(CheezType targetType, string attribute)
        {
            var impls = mImplTable.Where(kv =>
            {
                var implType = kv.Key.TargetType;
                if (CheezType.TypesMatch(implType, targetType))
                    return true;
                return false;
            });

            var candidates = new List<AstFunctionDecl>();

            foreach (var impl in impls)
            {
                var list = impl.Value;

                var c = list?.FirstOrDefault(f => f.HasDirective(attribute));
                if (c != null)
                    candidates.Add(c);

            }

            if (candidates.Count == 0)
                return Parent?.GetImplFunctionWithDirective(targetType, attribute);

            return candidates[0];
        }

        public override string ToString()
        {
            if (Parent != null)
                return $"{Parent}::{Name}";
            return $"::{Name}";
        }
    }
}
