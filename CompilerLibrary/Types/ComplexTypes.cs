using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types.Primitive;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Types.Complex
{
    public class TraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }
        public AstTraitDeclaration DeclarationTemplate => Declaration.Template ?? Declaration;

        public override bool IsErrorType => Arguments.Any(a => a.IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => a.IsPolyType);

        public CheezType[] Arguments { get; }

        public TraitType(AstTraitDeclaration decl, CheezType[] args = null)
        {
            Size = 2 * PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
            Declaration = decl;
            Arguments = args ?? decl.Parameters.Select(p => p.Value as CheezType).ToArray();
        }
        
        public override string ToString()
        {
            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a.ToString()));
                return $"{Declaration.Name.Name}({args})";
            }
            return $"{Declaration.Name.Name}";
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is TraitType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = this.Arguments[i].Match(str.Arguments[i], polyTypes);
                    if (s == -1)
                        return -1;
                    score += s;
                }
                return score;
            }

            return -1;
        }
    }

    public abstract class SumType : CheezType
    {
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
        }
    }

    public class TupleType : CheezType
    {
        private static List<TupleType> sTypes = new List<TupleType>();

        public (string name, CheezType type)[] Members { get; }
        public override bool IsPolyType => Members.Any(m => m.type.IsPolyType);
        public override bool IsErrorType => Members.Any(m => m.type.IsErrorType);

        private TupleType((string name, CheezType type)[] members)
        {
            Members = members;
            CalculateSize();
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
            var hashCode = 309225798;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            return hashCode;
        }

        public void CalculateSize()
        {
            Alignment = 1;
            Size = 0;
            for (int i = 0; i < Members.Length; i++)
            {
                var m = Members[i];

                var ms = m.type.Size;
                var ma = m.type.Alignment;

                Alignment = Math.Max(Alignment, ma);
                Size += ms;
                Size = Utilities.GetNextAligned(Size, ma);
            }

            Size = Utilities.GetNextAligned(Size, Alignment);
        }
    }

    public class StructType : CheezType
    {
        public AstStructDecl Declaration { get; }
        public CheezType[] Arguments { get; }
        public override bool IsErrorType => Arguments.Any(a => a.IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => a.IsPolyType);

        public AstStructDecl DeclarationTemplate => Declaration.Template ?? Declaration;

        public StructType(AstStructDecl decl, CheezType[] args = null)
        {
            Size = -1;
            Declaration = decl;
            Arguments = args ?? decl.Parameters.Select(p => p.Value as CheezType).ToArray();
        }

        public void CalculateSize()
        {
            Alignment = 1;
            Size = 0;
            for (int i = 0; i < Declaration.Members.Count; i++)
            {
                var m = Declaration.Members[i];

                var ms = m.Type.Size;
                var ma = m.Type.Alignment;

                Alignment = Math.Max(Alignment, ma);
                Size += ms;
                Size = Utilities.GetNextAligned(Size, ma);
            }

            Size = Utilities.GetNextAligned(Size, Alignment);
        }

        public override string ToString()
        {
            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a.ToString()));
                return $"{Declaration.Name.Name}({args})";
            }
            return $"{Declaration.Name.Name}";
        }

        public int GetIndexOfMember(string right)
        {
            return Declaration.Members.FindIndex(m => m.Name.Name == right);
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

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is StructType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = this.Arguments[i].Match(str.Arguments[i], polyTypes);
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
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType[]>.Default.GetHashCode(Arguments);
            return hashCode;
        }
    }

    public class EnumType : CheezType
    {
        public AstEnumDecl Declaration { get; set; }

        public Dictionary<string, long> Members { get; private set; }
        public CheezType[] Arguments { get; }
        public IntType TagType { get; set; }
        public override bool IsErrorType => Arguments.Any(a => a.IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => a.IsPolyType);

        public AstEnumDecl DeclarationTemplate => Declaration.Template ?? Declaration;

        public EnumType(AstEnumDecl en, CheezType[] args = null)
        {
            Size = -1;
            Declaration = en;
            Arguments = args ?? en.Parameters.Select(p => p.Value as CheezType).ToArray();
        }

        public void CalculateSize()
        {
            Alignment = Declaration.TagType.Alignment;

            TagType = Declaration.TagType;
            Members = new Dictionary<string, long>();

            var maxMemberSize = 0;

            foreach (var m in Declaration.Members)
            {
                Members.Add(m.Name.Name, ((NumberData)m.Value.Value).ToLong());
                if (m.AssociatedTypeExpr != null)
                    maxMemberSize = Math.Max(maxMemberSize, ((CheezType)m.AssociatedTypeExpr.Value).Size);
            }

            Size = TagType.Size + maxMemberSize;
            Size = Utilities.GetNextAligned(Size, Alignment);
        }

        public override string ToString()
        {
            if (Arguments?.Length > 0)
            {
                var args = string.Join(", ", Arguments.Select(a => a.ToString()));
                return $"{Declaration.Name.Name}({args})";
            }
            return $"{Declaration.Name.Name}";
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is EnumType str)
            {
                if (this.DeclarationTemplate != str.DeclarationTemplate)
                    return -1;

                int score = 0;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    int s = this.Arguments[i].Match(str.Arguments[i], polyTypes);
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
        public (string name, CheezType type, AstExpression defaultValue)[] Parameters { get; private set; }
        public CheezType ReturnType { get; private set; }

        public AstFunctionDecl Declaration { get; set; } = null;
        public override bool IsErrorType => ReturnType.IsErrorType || Parameters.Any(p => p.type.IsErrorType);

        public CallingConvention CC = CallingConvention.Default;

        public FunctionType((string, CheezType, AstExpression defaultValue)[] parameterTypes, CheezType returnType, CallingConvention cc)
        {
            this.Parameters = parameterTypes;
            this.ReturnType = returnType;
            this.CC = cc;

            Size = PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
        }

        public FunctionType(AstFunctionDecl func)
        {
            this.Declaration = func;
            this.ReturnType = func.ReturnTypeExpr?.Type ?? CheezType.Void;
            this.Parameters = func.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue)).ToArray();

            if (func.TryGetDirective("stdcall", out var dir))
                this.CC = CallingConvention.Stdcall;

            Size = PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
        }

        public override string ToString()
        {
            var args = string.Join(", ", Parameters.Select(p =>
            {
                if (p.name != null)
                    return $"{p.name}: {p.type}";
                return p.type.ToString();
            }));
            if (ReturnType != CheezType.Void)
                return $"fn({args}) -> {ReturnType}";
            else
                return $"fn({args})";
        }

        public override bool Equals(object obj)
        {
            if (obj is FunctionType f)
            {
                if (ReturnType != f.ReturnType)
                    return false;

                if (Parameters.Length != f.Parameters.Length)
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
            var hashCode = -1451483643;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + VarArgs.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(ReturnType);
            return hashCode;
        }
    }
}
