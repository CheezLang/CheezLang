using Cheez.Types.Abstract;
using Cheez.Types.Primitive;
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
        public int Size { get; set; } = 0;
        public int Alignment { get; set; } = 1;

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
    }

    public class CheezTypeType : CheezType
    {
        public static CheezTypeType Instance { get; } = new CheezTypeType();
        public override bool IsPolyType => false;
        public override string ToString() => "type";

        public override bool IsErrorType => false;
    }
}
