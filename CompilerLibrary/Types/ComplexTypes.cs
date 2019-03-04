﻿using Cheez.Ast.Statements;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Types.Complex
{
    public class TraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }
        public override bool IsPolyType => false;

        public TraitType(AstTraitDeclaration decl)
        {
            Size = 2 * PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
            Declaration = decl;
        }

        public override string ToString() => Declaration.Name.Name;
    }

    public class TupleType : CheezType
    {
        private static List<TupleType> sTypes = new List<TupleType>();

        public (string name, CheezType type)[] Members { get; }
        public override bool IsPolyType => false;

        private TupleType((string name, CheezType type)[] members)
        {
            Members = members;
        }

        public static TupleType GetTuple((string name, CheezType type)[] members)
        {
            return new TupleType(members);
            //var f = sTypes.FirstOrDefault(ft =>
            //{
            //    if (ft.Members.Length != members.Length)
            //        return false;

            //    return ft.Members.Zip(members, (a, b) => a.type == b.type).All(b => b);
            //});

            //if (f != null)
            //    return f;

            //var type = new TupleType(members);
            //sTypes.Add(type);
            //return type;
        }

        //public static TupleType GetTuple(CheezType[] types)
        //{
        //    var members = types.Select(t => (null as string, t)).ToArray();
        //    return GetTuple(members);
        //}

        public override string ToString()
        {
            var members = string.Join(", ", Members.Select(m =>
            {
                if (m.name != null) return m.name + ": " + m.type.ToString();
                return m.type.ToString();
            }));
            return "(" + members + ")";
        }
    }

    public class StructType : CheezType
    {
        public AstStructDecl Declaration { get; }
        public CheezType[] Arguments { get; }
        public int[] MemberOffsets { get; private set; }

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

        private void CalculateSize()
        {
            Size = 0;
            MemberOffsets = new int[Declaration.Members.Count];
            for (int i = 0; i < Declaration.Members.Count; i++)
            {
                var m = Declaration.Members[i];
                MemberOffsets[i] = Size;
                Size += m.Type.Size;
            }

            Alignment = Size;
            if (Alignment == 0)
                Alignment = 4;
        }

        public override string ToString()
        {
            if (Declaration.IsPolyInstance)
            {
                var args = string.Join(", ", Declaration.Parameters.Select(p => $"{p.Value}"));
                return $"{Declaration.Name.Name}({args})";
            }
            return $"{Declaration.Name.Name}";
        }

        public int GetIndexOfMember(string right)
        {
            return Declaration.Members.FindIndex(m => m.Name.Name == right);
        }

        public override bool IsPolyType => false;
    }

    public class EnumType : CheezType
    {
        public string Name { get; }
        public Dictionary<string, int> Members { get; }

        public CheezType MemberType { get; set; }

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

        public override bool IsPolyType => false;
        public bool VarArgs { get; set; }
        public (string name, CheezType type)[] Parameters { get; private set; }
        public CheezType ReturnType { get; private set; }

        public AstFunctionDecl Declaration { get; set; } = null;

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
