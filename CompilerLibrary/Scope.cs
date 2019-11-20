#nullable enable

using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public interface ISymbol
    {
        string Name { get; }
        //CheezType Type { get; }
        ILocation Location { get; }
    }

    public interface ITypedSymbol : ISymbol
    {
        CheezType Type { get; }
    }

    public class AmbiguousSymol : ISymbol
    {
        public string Name => throw new NotImplementedException();
        public ILocation Location => throw new NotImplementedException();

        public List<ISymbol> Symbols { get; }

        public AmbiguousSymol(List<ISymbol> syms)
        {
            Symbols = syms;
        }
    }

    public class ModuleSymbol : ISymbol
    {
        public string Name { get; }

        public ILocation Location { get; }

        public Scope Scope { get; }

        public ModuleSymbol(Scope scope, string name, ILocation importLocation)
        {
            Scope = scope;
            Name = name;
            Location = importLocation;
        }
    }

    public class ConstSymbol : ITypedSymbol
    {
        public ILocation Location => throw new NotImplementedException();
        public string Name { get; private set; }

        public CheezType Type { get; private set; }
        public object Value { get; private set; }

        public ConstSymbol(string name, CheezType type, object value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }
    }

    public class TypeSymbol : ITypedSymbol
    {
        public ILocation Location => throw new NotImplementedException();
        public string Name { get; private set; }

        public CheezType Type { get; private set; }

        public TypeSymbol(string name, CheezType type)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    public class Using : ITypedSymbol
    {
        public CheezType Type => Expr.Type;
        public string Name => throw new NotImplementedException();

        public AstExpression Expr { get; }

        public ILocation Location { get; set; }
        public bool Replace { get; set; } = false;


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



    public class Scope
    {
        public string Name { get; set; }
        public Scope? Parent { get; }

        private Scope? mLinkedScope;
        public Scope? LinkedScope
        {
            set { mLinkedScope = value; }
            get => mLinkedScope ?? Parent?.LinkedScope;
        }

        public Scope? TransparentParent { get; }

        private List<Scope>? mUsedScopes = null;

        private Dictionary<string, ISymbol> mSymbolTable = new Dictionary<string, ISymbol>();
        private Dictionary<string, List<INaryOperator>> mNaryOperatorTable = new Dictionary<string, List<INaryOperator>>();
        private Dictionary<string, List<IBinaryOperator>> mBinaryOperatorTable = new Dictionary<string, List<IBinaryOperator>>();
        private Dictionary<string, List<IUnaryOperator>> mUnaryOperatorTable = new Dictionary<string, List<IUnaryOperator>>();
        private Dictionary<AstImplBlock, List<AstFuncExpr>> mImplTable = new Dictionary<AstImplBlock, List<AstFuncExpr>>();

        private List<AstFuncExpr>? mForExtensions = null;
        private (string? label, object loopOrAction)? mBreak = null;
        private (string? label, object loopOrAction)? mContinue = null;

        public IEnumerable<KeyValuePair<string, ISymbol>> Symbols => mSymbolTable.AsEnumerable();

        public Scope(string name, Scope? parent = null, Scope? transparentParent = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.TransparentParent = transparentParent;
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

        public void AddUsedScope(Scope scope)
        {
            if (mUsedScopes == null)
                mUsedScopes = new List<Scope>();
            mUsedScopes.Add(scope);
        }

        public void AddForExtension(AstFuncExpr func)
        {
            if (mForExtensions == null)
                mForExtensions = new List<AstFuncExpr>();
            mForExtensions.Add(func);
        }

        public List<AstFuncExpr> GetForExtensions(CheezType type)
        {
            var result = new List<AstFuncExpr>();
            GetForExtensions(type, result);
            return result;
        }

        public void GetForExtensions(CheezType type, List<AstFuncExpr> result, bool localOnly = false)
        {
            if (mForExtensions != null)
            {
                foreach (var func in mForExtensions)
                {
                    var paramType = func.Parameters[0].Type;
                    if (paramType is ReferenceType r)
                        paramType = r.TargetType;
                    if (CheezType.TypesMatch(paramType, type))
                        result.Add(func);
                }
            }

            if (localOnly)
                return;

            if (mUsedScopes != null)
            {
                foreach (var scope in mUsedScopes)
                    scope.GetForExtensions(type, result, true);
            }

            Parent?.GetForExtensions(type, result);
        }

        public List<INaryOperator> GetNaryOperators(string name, params CheezType[] types)
        {
            var result = new List<INaryOperator>();
            int level = int.MaxValue;
            GetOperator(name, result, ref level, false, types);
            return result;
        }

        private void GetOperator(string name, List<INaryOperator> result, ref int level, bool localOnly, params CheezType[] types)
        {
            if (mNaryOperatorTable.TryGetValue(name, out var ops))
            {
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
            }

            if (localOnly)
                return;

            if (mUsedScopes != null)
            {
                foreach (var scope in mUsedScopes)
                {
                    scope.GetOperator(name, result, ref level, true, types);
                }
            }

            Parent?.GetOperator(name, result, ref level, false, types);
        }

        public List<IBinaryOperator> GetBinaryOperators(string name, CheezType lhs, CheezType rhs)
        {
            var result = new List<IBinaryOperator>();
            int level = int.MaxValue;
            GetOperator(name, lhs, rhs, result, ref level);
            return result;
        }

        private void GetOperator(string name, CheezType lhs, CheezType rhs, List<IBinaryOperator> result, ref int level, bool localOnly = false)
        {
            if (mBinaryOperatorTable.TryGetValue(name, out var ops))
            {
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
            }

            if (localOnly)
                return;

            if (mUsedScopes != null && !localOnly)
            {
                foreach (var scope in mUsedScopes)
                {
                    scope.GetOperator(name, lhs, rhs, result, ref level, true);
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

        private void GetOperator(string name, CheezType sub, List<IUnaryOperator> result, ref int level, bool localOnly = false)
        {
            if (mUnaryOperatorTable.TryGetValue(name, out var ops))
            {
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
            }

            if (localOnly)
                return;

            if (mUsedScopes != null && !localOnly)
            {
                foreach (var scope in mUsedScopes)
                {
                    scope.GetOperator(name, sub, result, ref level, true);
                }
            }

            Parent?.GetOperator(name, sub, result, ref level);
        }

        public void DefineUnaryOperator(string op, AstFuncExpr func)
        {
            DefineUnaryOperator(new UserDefinedUnaryOperator(op, func));
        }

        public void DefineBinaryOperator(string op, AstFuncExpr func)
        {
            DefineBinaryOperator(new UserDefinedBinaryOperator(op, func));
        }

        public void DefineOperator(string op, AstFuncExpr func)
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
            DefineBinaryOperator(new BuiltInFunctionOperator("=="));
            DefineBinaryOperator(new BuiltInFunctionOperator("!="));
        }

        private void DefineUnaryOperator(string name, CheezType type, BuiltInUnaryOperator.ComptimeExecution exe)
        {
            DefineUnaryOperator(new BuiltInUnaryOperator(name, type, type, exe));
        }

        private void DefineUnaryOperator(IUnaryOperator op)
        {
            List<IUnaryOperator>? list = null;
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
                List<IBinaryOperator>? list = null;
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
                List<IBinaryOperator> list;
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

                DefineBinaryOperator(new BuiltInBinaryOperator("+", type, type, type, (a, b) => ((NumberData)a).AsDouble() + ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("-", type, type, type, (a, b) => ((NumberData)a).AsDouble() - ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("*", type, type, type, (a, b) => ((NumberData)a).AsDouble() * ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("/", type, type, type, (a, b) => ((NumberData)a).AsDouble() / ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("%", type, type, type, (a, b) => ((NumberData)a).AsDouble() % ((NumberData)b).AsDouble()));

                DefineBinaryOperator(new BuiltInBinaryOperator("==", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() == ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("!=", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() != ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("<", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() < ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator("<=", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() <= ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator(">", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() > ((NumberData)b).AsDouble()));
                DefineBinaryOperator(new BuiltInBinaryOperator(">=", CheezType.Bool, type, type, (a, b) => ((NumberData)a).AsDouble() >= ((NumberData)b).AsDouble()));
            }
        }

        private void DefineBinaryOperator(IBinaryOperator op)
        {
            List<IBinaryOperator> list;
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
            List<INaryOperator> list;
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
            {
                if (Parent != null)
                    Parent.OverrideBreakName(label);
                else
                    throw new Exception("Well that's not supposed to happen...");
            }


            if (mBreak != null)
                mBreak = (label, mBreak.Value.loopOrAction);
        }

        public void OverrideContinueName(string label)
        {
            if (mContinue == null)
            {
                if (Parent != null)
                    Parent.OverrideContinueName(label);
                else
                    throw new Exception("Well that's not supposed to happen...");
            }

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

        public object? GetBreak(string? label = null)
        {
            if (mBreak != null && (label == null || mBreak.Value.label == label))
                return mBreak.Value.loopOrAction;

            return Parent?.GetBreak(label);
        }

        public object? GetContinue(string? label = null)
        {
            if (mContinue != null && (label == null || mContinue.Value.label == label))
                return mContinue.Value.loopOrAction;

            return Parent?.GetContinue(label);
        }

        public (bool ok, ILocation? other) DefineLocalSymbol(ISymbol symbol, string? name = null)
        {
            name = name ?? symbol.Name;
            if (mSymbolTable.TryGetValue(name, out var other))
                return (false, other.Location);

            mSymbolTable[name] = symbol;
            return (true, null);
        }

        public (bool ok, ILocation? other) DefineSymbol(ISymbol symbol, string? name = null)
        {
            if (TransparentParent != null)
                return TransparentParent.DefineSymbol(symbol, name);

            return DefineLocalSymbol(symbol, name);
        }

        public (bool ok, ILocation? other) DefineUse(string name, AstExpression expr, bool replace, out Using use)
        {
            use = new Using(expr, replace);
            return DefineSymbol(use, name);
        }

        public (bool ok, ILocation? other) DefineConstant(string name, CheezType type, object value)
        {
            return DefineSymbol(new ConstSymbol(name, type, value));
        }

        public bool DefineTypeSymbol(string name, CheezType symbol)
        {
            return DefineSymbol(new TypeSymbol(name, symbol)).ok;
        }

        public (bool ok, ILocation? other) DefineDeclaration(AstDecl decl)
        {
            return DefineSymbol(decl, decl.Name.Name);
        }

        public ISymbol? GetSymbol(string name, bool searchUsedScopes = true, bool searchParentScope = true)
        {
            if (mSymbolTable.ContainsKey(name))
            {
                var v = mSymbolTable[name];
                return v;
            }

            if (mUsedScopes != null && searchUsedScopes)
            {
                List<ISymbol> found = new List<ISymbol>();
                foreach (var scope in mUsedScopes)
                {
                    var sym = scope.GetSymbol(name, false, false);
                    if (sym == null)
                        continue;
                    found.Add(sym);
                }

                if (found.Count == 1)
                    return found[0];
                if (found.Count > 1)
                    return new AmbiguousSymol(found);
            }

            if (searchParentScope)
                return Parent?.GetSymbol(name);
            return null;
        }

        public AstTraitTypeExpr? GetTrait(string name)
        {
            var sym = GetSymbol(name);
            if (sym is AstConstantDeclaration c && c.Initializer is AstTraitTypeExpr s)
                return s;
            return null;
        }

        public AstStructTypeExpr? GetStruct(string name)
        {
            var sym = GetSymbol(name);
            if (sym is AstConstantDeclaration c && c.Initializer is AstStructTypeExpr s)
                return s;
            return null;
        }
        
        public AstEnumTypeExpr? GetEnum(string name)
        {
            var sym = GetSymbol(name);
            if (sym is AstConstantDeclaration c && c.Initializer is AstEnumTypeExpr s)
                return s;
            return null;
        }

        public bool DefineImplFunction(AstFuncExpr f)
        {
            if (!mImplTable.TryGetValue(f.ImplBlock, out var list))
            {
                list = new List<AstFuncExpr>();
                mImplTable[f.ImplBlock] = list;
            }

            if (list.Any(ff => ff.Name == f.Name))
                return false;

            list.Add(f);

            return true;
        }

        public List<AstFuncExpr> GetImplFunction(CheezType targetType, string name)
        {
            var impls = mImplTable.Where(kv =>
            {
                var implType = kv.Key.TargetType;
                if (CheezType.TypesMatch(implType, targetType))
                    return true;
                return false;
            });

            var candidates = new List<AstFuncExpr>();

            foreach (var impl in impls)
            {
                var list = impl.Value;
                
                var c = list?.FirstOrDefault(f => f.Name == name);
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

        public AstFuncExpr? GetImplFunctionWithDirective(CheezType targetType, string attribute)
        {
            var impls = mImplTable.Where(kv =>
            {
                var implType = kv.Key.TargetType;
                if (CheezType.TypesMatch(implType, targetType))
                    return true;
                return false;
            });

            var candidates = new List<AstFuncExpr>();

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
