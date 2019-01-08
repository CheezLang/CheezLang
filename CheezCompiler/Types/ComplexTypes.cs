using Cheez.Ast.Statements;
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
        private static List<FunctionType> sTypes = new List<FunctionType>();

        public CheezType[] ParameterTypes { get; private set; }
        public CheezType ReturnType { get; private set; }
        public bool VarArgs { get; set; } = false;

        private FunctionType(CheezType[] parameterTypes, CheezType returnType)
        {
            this.ParameterTypes = parameterTypes;
            this.ReturnType = returnType;
        }

        public static FunctionType GetFunctionType(AstFunctionDecl decl)
        {
            var pt = decl.Parameters.Select(p => p.Type).ToArray();
            return GetFunctionType(pt, decl.ReturnValue?.Type ?? CheezType.Void);
        }

        public static FunctionType GetFunctionType(CheezType[] parameterTypes, CheezType returnType)
        {
            var f = sTypes.FirstOrDefault(ft =>
            {
                if (ft.ReturnType != returnType)
                    return false;
                if (ft.ParameterTypes.Length != parameterTypes.Length)
                    return false;

                return ft.ParameterTypes.Zip(parameterTypes, (a, b) => a == b).All(b => b);
            });

            if (f != null)
                return f;

            var type = new FunctionType(parameterTypes, returnType);
            sTypes.Add(type);
            return type;
        }

        public override string ToString()
        {
            var args = string.Join(", ", ParameterTypes.ToList());
            if (ReturnType != CheezType.Void)
                return $"fn({args}) -> {ReturnType}";
            else
                return $"fn({args})";
        }

        public override bool IsPolyType => false;
    }
}
