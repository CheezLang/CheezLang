using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using System;
using System.Collections.Generic;

namespace Cheez.Types
{
    public abstract class CheezType
    {
        public static CheezType Void => VoidType.Instance;
        public static CheezType Impl => ImplType.Instance;
        public static CheezType CString => PointerType.GetPointerType(CharType.GetCharType(1));
        public static CheezType String => StringType.Instance;
        public static CheezType StringLiteral => StringLiteralType.Instance;
        public static CheezType Bool => BoolType.Instance;
        public static CheezType Error => ErrorType.Instance;
        public static CheezType Type => CheezTypeType.Instance;
        public static CheezType Code => CodeType.Instance;
        public static CheezType Any => AnyType.Intance;
        public static CheezType Module => ModuleType.Instance;
        public static CheezType PolyValue => PolyValueType.Instance;

        public abstract bool IsPolyType { get; }

        public abstract bool IsErrorType { get; }

        public virtual bool IsCopy { get; } = true;
        public virtual bool IsComptimeOnly { get; } = false;

        private int Size = -1;
        private int Alignment = -1;
        private bool? IsDefaultConstructable;

        private static List<CheezType> typesWithMissingProperties = new List<CheezType>();
        public static IEnumerable<CheezType> TypesWithMissingProperties => typesWithMissingProperties;

        protected CheezType(int size, int align, bool hasDefault)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (align < 0)
                throw new ArgumentOutOfRangeException(nameof(align));
            Size = size;
            Alignment = align;
            IsDefaultConstructable = hasDefault;
        }

        protected CheezType(int size, int align)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (align < 0)
                throw new ArgumentOutOfRangeException(nameof(align));
            Size = size;
            Alignment = align;
            typesWithMissingProperties.Add(this);
        }

        protected CheezType(bool hasDefault)
        {
            IsDefaultConstructable = hasDefault;
            typesWithMissingProperties.Add(this);
        }

        protected CheezType()
        {
            typesWithMissingProperties.Add(this);
        }

        public int GetSize() => Size;
        public int GetAlignment() => Alignment;

        public void SetSizeAndAlignment(int size, int align)
        {
            Size = size;
            Alignment = align;
        }

        internal bool IsDefaultConstructableComputed() => IsDefaultConstructable != null;
        internal bool GetIsDefaultConstructable() => IsDefaultConstructable.Value;

        internal void SetIsDefaultConstructable(bool isDefaultConstructable)
        {
            IsDefaultConstructable = isDefaultConstructable;
        }

        public static bool operator ==(CheezType a, CheezType b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(CheezType a, CheezType b)
        {
            return !(a == b);
        }

        internal static void ClearAllTypes()
        {
            typesWithMissingProperties = new List<CheezType>();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (this == concrete)
                return 0;
            return -1;
        }

        public static bool TypesMatch(CheezType a, CheezType b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            if (a == b)
                return true;

            if (a is PolyType || b is PolyType)
                return true;

            switch (a, b)
            {
                case (ReferenceType ra, ReferenceType rb): return TypesMatch(ra.TargetType, rb.TargetType);
                case (PointerType ra, PointerType rb): return TypesMatch(ra.TargetType, rb.TargetType);

                case (SliceType aa, SliceType bb): return TypesMatch(aa.TargetType, bb.TargetType);
                case (ArrayType aa, ArrayType bb): return Workspace.CheezValuesMatch((null, aa.Length), (null, bb.Length)) && TypesMatch(aa.TargetType, bb.TargetType);
                case (RangeType aa, RangeType bb): return TypesMatch(aa.TargetType, bb.TargetType);

                case (TupleType aa, TupleType bb):
                    {
                        if (aa.Members.Length != bb.Members.Length)
                            return false;

                        for (int i = 0; i < aa.Members.Length; i++)
                        {
                            if (!TypesMatch(aa.Members[i].type, bb.Members[i].type))
                                return false;
                        }

                        return true;
                    }

                case (GenericStructType aa, StructType bb):
                    {
                        if (aa.Declaration != bb.DeclarationTemplate)
                            return false;
                        if (aa.Arguments.Length != bb.Arguments.Length)
                            return false;
                        for (int i = 0; i < aa.Arguments.Length; i++)
                        {
                            if (!Workspace.CheezValuesMatch(aa.Arguments[i], (null, bb.Arguments[i])))
                                return false;
                        }

                        return true;
                    }

                case (GenericEnumType aa, EnumType bb):
                    {
                        if (aa.Declaration != bb.DeclarationTemplate)
                            return false;
                        if (aa.Arguments.Length != bb.Arguments.Length)
                            return false;
                        for (int i = 0; i < aa.Arguments.Length; i++)
                        {
                            if (!Workspace.CheezValuesMatch(aa.Arguments[i], (null, bb.Arguments[i])))
                                return false;
                        }

                        return true;
                    }
                case (GenericTraitType aa, TraitType bb):
                    {
                        if (aa.Declaration != bb.DeclarationTemplate)
                            return false;
                        if (aa.Arguments.Length != bb.Arguments.Length)
                            return false;
                        for (int i = 0; i < aa.Arguments.Length; i++)
                        {
                            if (!Workspace.CheezValuesMatch(aa.Arguments[i], (null, bb.Arguments[i])))
                                return false;
                        }

                        return true;
                    }

                case (StructType bb, GenericStructType aa):
                    return TypesMatch(aa, bb);
                case (EnumType bb, GenericEnumType aa):
                    return TypesMatch(aa, bb);
                case (TraitType bb, GenericTraitType aa):
                    return TypesMatch(aa, bb);
            }
            if (a is StructType sa && b is StructType sb)
            {
                if (sa.Name != sb.Name)
                    return false;
                if (sa.Arguments.Length != sb.Arguments.Length)
                    return false;
                for (int i = 0; i < sa.Arguments.Length; i++)
                {
                    if (!Workspace.CheezValuesMatch((null, sa.Arguments[i]), (null, sb.Arguments[i])))
                        return false;
                }

                return true;
            }
            else if (a is TraitType ta && b is TraitType tb)
            {
                if (ta.Declaration.Name != tb.Declaration.Name)
                    return false;
                if (ta.Arguments.Length != tb.Arguments.Length)
                    return false;
                for (int i = 0; i < ta.Arguments.Length; i++)
                {
                    if (!Workspace.CheezValuesMatch((null, ta.Arguments[i]), (null, tb.Arguments[i])))
                        return false;
                }

                return true;
            }
            else if (a is EnumType ea && b is EnumType eb)
            {
                if (ea.Declaration.Name != eb.Declaration.Name)
                    return false;
                if (ea.Arguments.Length != eb.Arguments.Length)
                    return false;
                for (int i = 0; i < ea.Arguments.Length; i++)
                {
                    if (!Workspace.CheezValuesMatch((null, ea.Arguments[i]), (null, eb.Arguments[i])))
                        return false;
                }

                return true;
            }

            return false;
        }
    }

    public class ImplType : CheezType
    {
        public static readonly ImplType Instance = new ImplType();

        public override bool IsPolyType => false;

        public override bool IsErrorType => false;

        public ImplType() : base(0, 1, false)
        { }
    }

    public class ModuleType : CheezType
    {
        public static readonly ModuleType Instance = new ModuleType();

        public override bool IsPolyType => false;

        public override bool IsErrorType => false;

        public ModuleType() : base(0, 1, false)
        { }
    }

    public class CheezTypeType : CheezType
    {
        public static CheezTypeType Instance { get; } = new CheezTypeType();
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
        public override bool IsComptimeOnly => true;

        public override string ToString() => "type";

        private CheezTypeType() : base(0, 1, false) { }
    }

    public class CodeType : CheezType
    {
        public static CodeType Instance { get; } = new CodeType();
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
        public override bool IsComptimeOnly => true;

        public override string ToString() => "Code";

        private CodeType() : base(0, 1, false) { }
    }
}
