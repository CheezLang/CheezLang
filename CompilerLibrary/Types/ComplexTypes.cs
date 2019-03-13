using Cheez.Ast.Statements;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Types.Complex
{
    public class TraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        public TraitType(AstTraitDeclaration decl)
        {
            Size = 2 * PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
            Declaration = decl;
        }

        public override string ToString() => Declaration.Name.Name;
    }

    public class SumType : CheezType
    {
        public override bool IsPolyType => Variations.Any(v => v.IsPolyType);
        public override bool IsErrorType => Variations.Any(v => v.IsErrorType);

        public CheezType[] Variations { get; }

        public SumType(params CheezType[] vars)
        {
            Variations = vars;
        }

        public override string ToString()
        {
            return string.Join(" | ", Variations.Select(v => v.ToString()));
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
    }

    public class StructType : CheezType
    {
        public AstStructDecl Declaration { get; }
        public CheezType[] Arguments { get; }
        public int[] MemberOffsets { get; private set; }
        public override bool IsErrorType => Arguments.Any(a => a.IsErrorType);
        public override bool IsPolyType => Arguments.Any(a => a.IsPolyType);

        public StructType(AstStructDecl decl)
        {
            Declaration = decl;
            Arguments = decl.Parameters.Select(p => p.Value as CheezType).ToArray();
        }

        public StructType(AstStructDecl decl, CheezType[] args)
        {
            Declaration = decl;
            Arguments = args;
        }

        public void CalculateSize()
        {
            Size = 0;
            MemberOffsets = new int[Declaration.Members.Count];
            for (int i = 0; i < Declaration.Members.Count; i++)
            {
                var m = Declaration.Members[i];
                MemberOffsets[i] = Size;


                Size += Math.Max(PointerType.PointerSize, m.Type.Size);
            }

            Alignment = Size;
            if (Alignment == 0)
                Alignment = 4;
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
    }

    public class EnumType : CheezType
    {
        public string Name { get; }
        public Dictionary<string, int> Members { get; }

        public CheezType MemberType { get; set; }
        public override bool IsErrorType => MemberType.IsErrorType;

        public EnumType(AstEnumDecl en, CheezType memberType = null)
        {
            if (memberType == null)
                memberType = IntType.DefaultType;

            Alignment = memberType.Alignment;

            Name = en.Name.Name;
            Members = new Dictionary<string, int>();
            MemberType = memberType;

            int value = 0;
            foreach (var m in en.Members)
            {
                Members.Add(m.Name.Name, value++);
            }
        }

        public override string ToString() => $"enum {Name}";
        public override bool IsPolyType => false;
    }

    public class FunctionType : CheezType
    {
        //private static List<FunctionType> sTypes = new List<FunctionType>();

        public override bool IsPolyType => ReturnType.IsPolyType || Parameters.Any(p => p.type.IsPolyType);
        public bool VarArgs { get; set; }
        public (string name, CheezType type)[] Parameters { get; private set; }
        public CheezType ReturnType { get; private set; }

        public AstFunctionDecl Declaration { get; set; } = null;
        public override bool IsErrorType => ReturnType.IsErrorType || Parameters.Any(p => p.type.IsErrorType);

        public FunctionType((string, CheezType)[] parameterTypes, CheezType returnType)
        {
            this.Parameters = parameterTypes;
            this.ReturnType = returnType;
        }

        public FunctionType(AstFunctionDecl func)
        {
            this.Declaration = func;
            this.ReturnType = func.ReturnValue?.Type ?? CheezType.Void;
            this.Parameters = func.Parameters.Select(p => (p.Name?.Name, p.Type)).ToArray();
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
    }
}
