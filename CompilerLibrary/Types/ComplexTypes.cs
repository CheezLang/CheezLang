using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types.Primitive;
using Cheez.Util;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Types.Complex
{
    public class TraitErrorType : TraitType
    {
        public override bool IsErrorType => true;
        public TraitErrorType() : base(null, Array.Empty<CheezType>())
        {
        }

        public override string ToString() {
            return "<Error Trait>";
        }
    }

    public class TraitType : CheezType
    {
        public AstTraitTypeExpr Declaration { get; }
        public AstTraitTypeExpr DeclarationTemplate => Declaration.Template ?? Declaration;

        public override bool IsErrorType => Arguments.Any(a => (a as CheezType).IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => (a as CheezType).IsPolyType);

        public object[] Arguments { get; }

        public TraitType(AstTraitTypeExpr decl, object[] args = null)
            : base(false)
        {
            Declaration = decl;
            Arguments = args ?? decl.Parameters?.Select(p => p.Value)?.ToArray() ?? Array.Empty<object>();
        }
        
        public override string ToString()
        {
            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a.ToString()));
                return $"{Declaration.Name}[{args}]";
            }
            return $"{Declaration.Name}";
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is TraitType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch((null, this.Arguments[i]), (null, str.Arguments[i]), polyTypes);
                    //int s = (this.Arguments[i] as CheezType).Match(str.Arguments[i] as CheezType, polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public class SumType : CheezType
    {
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        public CheezType[] Types { get; private set; }

        public static CheezType GetSumType(params CheezType[] types)
        {
            var unique = new HashSet<CheezType>(types);

            if (unique.Any(x => x.IsErrorType))
                return CheezType.Error;

            if (unique.Count == 1)
            {
                return unique.First();
            }

            return CheezType.Void;
            // return new SumType {Types = types};
        }

        override public string ToString() {
            return string.Join("|", Types.Select(c => c.ToString()));
        }
    }

    public class TupleType : CheezType
    {
        public static readonly TupleType UnitLiteral = GetTuple(Array.Empty<(string, CheezType)>());

        public (string name, CheezType type)[] Members { get; }
        public override bool IsPolyType => Members.Any(m => m.type.IsPolyType);
        public override bool IsErrorType => Members.Any(m => m.type.IsErrorType);
        public override bool IsCopy => Members.All(m => m.type.IsCopy);

        private TupleType((string name, CheezType type)[] members) : base()
        {
            Members = members;
        }

        public static TupleType GetTuple((string name, CheezType type)[] members)
        {
            return new TupleType(members);
        }
        
        public override string ToString()
        {
            var members = string.Join(", ", Members.Select(m =>
            {
                if (m.name != null) return m.name + ": " + m.type.ToString();
                return m.type.ToString();
            }));
            return "(" + members + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj is TupleType t)
            {
                if (Members.Length != t.Members.Length) return false;
                for (int i = 0; i < Members.Length; i++)
                    if (Members[i].type != t.Members[i].type) return false;

                return true;
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            var hash = new HashCode();
            //hash.Add(base.GetHashCode());
            foreach (var m in Members)
            {
                hash.Add(m.type.GetHashCode());
            }

            return hash.ToHashCode();
        }
    }

    public class StructType : CheezType
    {
        //public AstStructDecl Declaration { get; }
        public string Name { get; }
        public AstStructTypeExpr Declaration { get; }
        public object[] Arguments { get; }
        public override bool IsErrorType => Arguments.Any(a => a is CheezType t && t.IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => a is CheezType t && t.IsPolyType);
        public override bool IsCopy { get; }
        public AstStructTypeExpr DeclarationTemplate => Declaration.Template ?? Declaration;

        public StructType(AstStructTypeExpr decl, bool isCopy, string name, object[] args = null)
            : base()
        {
            if (decl.IsPolymorphic)
            {
                throw new Exception();
            }
            Declaration = decl;
            IsCopy = isCopy;
            Name = name;
            Arguments = args ?? decl.Parameters.Select(p => p.Value).ToArray();
        }

        public override string ToString()
        {
            if (Name == "#anonymous")
            {
                return Declaration.Accept(new AnalysedAstPrinter());
            }

            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a?.ToString()));
                return $"{Name}[{args}]";
            }
            return Name;
        }

        public int GetIndexOfMember(string right)
        {
            return Declaration.Members.FindIndex(m => m.Name == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is StructType s)
            {
                if (Declaration != s.Declaration)
                    return false;

                if (Arguments.Length != s.Arguments.Length)
                    return false;

                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (Arguments[i] != s.Arguments[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is StructType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch((null, this.Arguments[i]), (null, str.Arguments[i]), polyTypes);
                    //int s = (this.Arguments[i] as CheezType).Match(str.Arguments[i] as CheezType, polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }

        public override int GetHashCode()
        {
            var hashCode = 1624555593;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object[]>.Default.GetHashCode(Arguments);
            return hashCode;
        }
    }

    public class EnumType : CheezType
    {
        public AstEnumTypeExpr Declaration { get; set; }
        //public AstEnumDecl Declaration { get; set; }

        //public Dictionary<string, long> Members { get; private set; }
        public object[] Arguments { get; }
        public override bool IsErrorType => Arguments.Any(a => (a as CheezType).IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => (a as CheezType).IsPolyType);

        public AstEnumTypeExpr DeclarationTemplate => Declaration.Template ?? Declaration;

        public override bool IsCopy { get; }

        public EnumType(AstEnumTypeExpr en, bool isCopy, object[] args = null) : base(false)
        {
            Declaration = en;
            IsCopy = isCopy;
            Arguments = args ?? en.Parameters.Select(p => p.Value).ToArray();
        }

        public override string ToString()
        {
            if (Declaration.Name == "#anonymous")
            {
                return Declaration.Accept(new AnalysedAstPrinter());
            }

            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a.ToString()));
                return $"{Declaration.Name}[{args}]";
            }
            return $"{Declaration.Name}";
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is EnumType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = Workspace.PolyValuesMatch((null, this.Arguments[i]), (null, str.Arguments[i]), polyTypes);
                    //int s = (this.Arguments[i] as CheezType).Match(str.Arguments[i] as CheezType, polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public class FunctionType : CheezType
    {
        public enum CallingConvention
        {
            Default,
            Stdcall,
        }

        //private static List<FunctionType> sTypes = new List<FunctionType>();

        public override bool IsPolyType => ReturnType.IsPolyType || Parameters.Any(p => p.type.IsPolyType);
        public bool VarArgs { get; set; }
        public bool IsFatFunction { get; set; }
        public (string name, CheezType type, AstExpression defaultValue)[] Parameters { get; private set; }
        public CheezType ReturnType { get; private set; }

        private FunctionType _underlyingFuncType = null;
        public FunctionType UnderlyingFuncType {
            get {
                if (_underlyingFuncType == null && IsFatFunction) {
                    var parameterTypes = Enumerable.Prepend(
                            Parameters, 
                            ("_data", PointerType.GetPointerType(CheezType.Void, true), null))
                        .ToArray();
                    _underlyingFuncType = new FunctionType(parameterTypes, ReturnType, false, CC);
                }
                return _underlyingFuncType;
            }
        }

        public override bool IsErrorType => ReturnType.IsErrorType || Parameters.Any(p => p.type.IsErrorType);

        public CallingConvention CC { get; } = CallingConvention.Default;

        public FunctionType((string name, CheezType type, AstExpression defaultValue)[] parameterTypes, CheezType returnType, bool isFatFunc, CallingConvention cc)
            : base(isFatFunc ? 2 * PointerType.PointerSize : PointerType.PointerSize, PointerType.PointerAlignment, true)
        {
            if (parameterTypes.Any(p => p.type == null))
            {
                throw new ArgumentNullException(nameof(parameterTypes));
            }

            this.Parameters = parameterTypes;
            this.ReturnType = returnType;
            this.IsFatFunction = isFatFunc;
            this.CC = cc;
        }

        public FunctionType(AstFuncExpr func)
            : base(PointerType.PointerSize, PointerType.PointerAlignment, true)
        {
            this.ReturnType = func.ReturnTypeExpr?.Type ?? CheezType.Void;
            this.Parameters = func.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue)).ToArray();
            this.IsFatFunction = false;

            if (func.TryGetDirective("stdcall", out var dir))
                this.CC = CallingConvention.Stdcall;
        }

        public override string ToString()
        {
            var args = string.Join(", ", Parameters.Select(p =>
            {
                if (p.name != null)
                    return $"{p.name}: {p.type}";
                return p.type.ToString();
            }));
            var fn = IsFatFunction ? "Fn" : "fn";
            if (ReturnType != CheezType.Void)
                return $"{fn}({args}) -> {ReturnType}";
            else
                return $"{fn}({args})";
        }

        public override bool Equals(object obj)
        {
            if (obj is FunctionType f)
            {
                if (ReturnType != f.ReturnType)
                    return false;

                if (Parameters.Length != f.Parameters.Length)
                    return false;

                if (IsFatFunction != f.IsFatFunction)
                    return false;

                if (CC != f.CC)
                    return false;

                for (int i = 0; i < Parameters.Length; i++)
                    if (this.Parameters[i].type != f.Parameters[i].type)
                        return false;

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(IsPolyType);
            hash.Add(VarArgs);
            hash.Add(IsFatFunction);
            foreach (var p in Parameters)
                hash.Add(p.type);
            hash.Add(ReturnType);
            return hash.ToHashCode();
        }

        //public override int GetHashCode()
        //{
        //    var hashCode = -1451483643;
        //    hashCode = hashCode * -1521134295 + base.GetHashCode();
        //    hashCode = hashCode * -1521134295 + VarArgs.GetHashCode();
        //    hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(ReturnType);
        //    return hashCode;
        //}
    }
}
