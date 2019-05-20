using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using System.Collections.Generic;

namespace Cheez.Types
{
    public abstract class CheezType
    {
        public static CheezType Void => VoidType.Intance;
        public static CheezType CString => PointerType.GetPointerType(CheezType.Char);
        public static CheezType String => SliceType.GetSliceType(CheezType.Char);
        public static CheezType StringLiteral => StringLiteralType.Instance;
        public static CheezType Char => CharType.Instance;
        public static CheezType Bool => BoolType.Instance;
        public static CheezType Error => ErrorType.Instance;
        public static CheezType Type => CheezTypeType.Instance;
        public static CheezType Any => AnyType.Intance;

        public abstract bool IsPolyType { get; }
        public virtual int Size { get; set; } = 0;
        public virtual int Alignment { get; set; } = 8;

        public abstract bool IsErrorType { get; }

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

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (this == concrete)
                return 0;
            return -1;
        }

        public static bool TypesMatch(CheezType a, CheezType b)
        {
            if (a == b)
                return true;

            if (a is PolyType || b is PolyType)
                return true;

            if (a is StructType sa && b is StructType sb)
            {
                if (sa.Declaration.Name.Name != sb.Declaration.Name.Name)
                    return false;
                if (sa.Arguments.Length != sb.Arguments.Length)
                    return false;
                for (int i = 0; i < sa.Arguments.Length; i++)
                {
                    if (!TypesMatch(sa.Arguments[i], sb.Arguments[i]))
                        return false;
                }

                return true;
            }
            else if (a is TraitType ta && b is TraitType tb)
            {
                if (ta.Declaration.Name.Name != tb.Declaration.Name.Name)
                    return false;
                if (ta.Arguments.Length != tb.Arguments.Length)
                    return false;
                for (int i = 0; i < ta.Arguments.Length; i++)
                {
                    if (!TypesMatch(ta.Arguments[i], tb.Arguments[i]))
                        return false;
                }

                return true;
            }
            else if (a is EnumType ea && b is EnumType eb)
            {
                if (ea.Declaration.Name.Name != eb.Declaration.Name.Name)
                    return false;
                if (ea.Arguments.Length != eb.Arguments.Length)
                    return false;
                for (int i = 0; i < ea.Arguments.Length; i++)
                {
                    if (!TypesMatch(ea.Arguments[i], eb.Arguments[i]))
                        return false;
                }

                return true;
            }

            return false;
        }
    }

    public class CheezTypeType : CheezType
    {
        public static CheezTypeType Instance { get; } = new CheezTypeType();
        public override bool IsPolyType => false;
        public override string ToString() => "type";

        public override bool IsErrorType => false;
    }
}
