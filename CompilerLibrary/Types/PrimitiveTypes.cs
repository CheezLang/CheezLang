using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Cheez.Types.Primitive
{
    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();
        public override string ToString() => "void";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        private VoidType() : base(0, 1, true) { }
    }

    public class AnyType : CheezType
    {
        public static AnyType Intance { get; } = new AnyType();

        private AnyType() : base(16, 8, false) { }

        public override string ToString() => "any";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance { get; } = new BoolType();
        public override string ToString() => "bool";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
        private BoolType() : base(1, 1, true) { }
    }

    public class IntType : CheezType
    {
        private static Dictionary<(int, bool), IntType> sTypes = new Dictionary<(int, bool), IntType>();
        public static IntType LiteralType { get; } = new IntType(0, false);
        public static IntType DefaultType => GetIntType(8, true);

        private IntType(int size, bool sign) : base(size, size, true)
        {
            Signed = sign;
        }

        public bool Signed { get; private set; }
        public override bool IsErrorType => false;
        public override bool IsPolyType => false;

        public static IntType GetIntType(int sizeInBytes, bool signed)
        {
            var key = (sizeInBytes, signed);

            if (sTypes.ContainsKey(key))
            {
                return sTypes[key];
            }

            var type = new IntType(sizeInBytes, signed);

            sTypes[key] = type;
            return type;
        }

        public override string ToString()
        {
            return (Signed ? "i" : "u") + (GetSize() * 8);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is IntType t)
            {
                if (t.Signed != this.Signed)
                    return -1;

                if (concrete.GetSize() != this.GetSize())
                    return -1;
                return 0;
            }
            return -1;
        }

        public BigInteger MinValue => (Signed, GetSize()) switch
        {
            (true, 1) => sbyte.MinValue,
            (true, 2) => short.MinValue,
            (true, 4) => int.MinValue,
            (true, 8) => long.MinValue,
            (false, 1) => byte.MinValue,
            (false, 2) => ushort.MinValue,
            (false, 4) => uint.MinValue,
            (false, 8) => ulong.MinValue,
            _ => throw new NotImplementedException()
        };

        public BigInteger MaxValue => (Signed, GetSize()) switch
        {
            (true, 1) => sbyte.MaxValue,
            (true, 2) => short.MaxValue,
            (true, 4) => int.MaxValue,
            (true, 8) => long.MaxValue,
            (false, 1) => byte.MaxValue,
            (false, 2) => ushort.MaxValue,
            (false, 4) => uint.MaxValue,
            (false, 8) => ulong.MaxValue,
            _ => throw new NotImplementedException()
        };
    }

    public class FloatType : CheezType
    {
        private static Dictionary<int, FloatType> sTypes = new Dictionary<int, FloatType>();
        public static FloatType LiteralType { get; } = new FloatType(0);
        public static FloatType DefaultType => GetFloatType(8);
        public override bool IsErrorType => false;
        public override bool IsPolyType => false;

        private FloatType(int size) : base(size, size, true) { }

        public static FloatType GetFloatType(int bytes)
        {
            if (sTypes.ContainsKey(bytes))
            {
                return sTypes[bytes];
            }

            var type = new FloatType(bytes);

            sTypes[bytes] = type;
            return type;
        }

        public override string ToString()
        {
            return "f" + (GetSize() * 8);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is FloatType t)
            {
                if (concrete.GetSize() > this.GetSize())
                    return -1;
                if (concrete.GetSize() < this.GetSize())
                    return 1;
                return 0;
            }
            return -1;
        }

        public double MinValue => GetSize() switch
        {
            4 => float.MinValue,
            8 => double.MinValue,
            _ => throw new NotImplementedException()
        };

        public double MaxValue => GetSize() switch
        {
            4 => float.MaxValue,
            8 => double.MaxValue,
            _ => throw new NotImplementedException()
        };
        public double NaN => GetSize() switch
        {
            4 => float.NaN,
            8 => double.NaN,
            _ => throw new NotImplementedException()
        };
        
        public double PosInf => GetSize() switch
        {
            4 => float.PositiveInfinity,
            8 => double.PositiveInfinity,
            _ => throw new InvalidOperationException()
        };
        
        public double NegInf => GetSize() switch
        {
            4 => float.NegativeInfinity,
            8 => double.NegativeInfinity,
            _ => throw new InvalidOperationException()
        };
    }

    public class PointerType : CheezType
    {
        public const int PointerSize = 8;
        public const int PointerAlignment = 8;

        private static Dictionary<CheezType, PointerType> sTypes = new Dictionary<CheezType, PointerType>();
        public static PointerType NullLiteralType { get; } = new PointerType(null);

        private PointerType(CheezType target) : base(PointerSize, PointerAlignment, true)
        {
            TargetType = target;
        }

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType?.IsErrorType ?? false;
        public override bool IsPolyType => TargetType?.IsPolyType ?? false;

        public static PointerType GetPointerType(CheezType targetType)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
            {
                return sTypes[targetType];
            }

            var type = new PointerType(targetType);

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"&{TargetType}";
        }

        public override bool Equals(object obj)
        {
            if (obj is PointerType p)
                return TargetType == p.TargetType;
            return false;
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is PointerType p)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override int GetHashCode()
        {
            var hashCode = -1663075914;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(TargetType);
            return hashCode;
        }
    }

    public class ReferenceType : CheezType
    {
        private static Dictionary<CheezType, ReferenceType> sTypes = new Dictionary<CheezType, ReferenceType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        private ReferenceType(CheezType target) : base(PointerType.PointerSize, PointerType.PointerAlignment, false)
        {
            TargetType = target;
        }

        public static ReferenceType GetRefType(CheezType targetType)
        {
            if (targetType is ReferenceType r)
                return r;

            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
            {
                return sTypes[targetType];
            }

            var type = new ReferenceType(targetType);

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"ref {TargetType}";
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType p)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return TargetType.Match(concrete, polyTypes);
        }

        public override bool Equals(object obj)
        {
            if (obj is ReferenceType r)
            {
                return TargetType == r.TargetType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -1663075914;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(TargetType);
            return hashCode;
        }
    }

    public class ArrayType : CheezType
    {
        private static Dictionary<CheezType, ArrayType> sTypes = new Dictionary<CheezType, ArrayType>();

        public CheezType TargetType { get; set; }
        public int Length { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        private ArrayType(CheezType target, int length) : base()
        {
            TargetType = target;
            Length = length;
        }

        public static ArrayType GetArrayType(CheezType targetType, int length)
        {
            if (targetType == null)
                return null;

            var existing = sTypes.FirstOrDefault(t => t.Value.TargetType == targetType && t.Value.Length == length).Value;
            if (existing != null)
                return existing;

            var type = new ArrayType(targetType, length);

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"[{Length}]{TargetType}";
        }

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(TargetType);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ArrayType p && Length == p.Length)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is ArrayType r)
            {
                return TargetType == r.TargetType && Length == r.Length;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -687864485;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }
    }

    public class SliceType : CheezType
    {
        private static Dictionary<CheezType, SliceType> sTypes = new Dictionary<CheezType, SliceType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        private SliceType(CheezType target) : base(PointerType.PointerSize * 2, PointerType.PointerAlignment, true)
        {
            TargetType = target;
        }

        public static SliceType GetSliceType(CheezType targetType)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
                return sTypes[targetType];

            var type = new SliceType(targetType);

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"[]{TargetType}";
        }

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(TargetType);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is SliceType p)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is SliceType r)
            {
                return TargetType == r.TargetType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -1663075914;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(TargetType);
            return hashCode;
        }
    }

    public class RangeType : CheezType
    {
        private static Dictionary<CheezType, RangeType> sTypes = new Dictionary<CheezType, RangeType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        public static RangeType GetRangeType(CheezType targetType)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
                return sTypes[targetType];

            var type = new RangeType
            {
                TargetType = targetType
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}..{TargetType}";
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is RangeType p)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is RangeType r)
            {
                return TargetType == r.TargetType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -1576707978;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CheezType>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + IsErrorType.GetHashCode();
            hashCode = hashCode * -1521134295 + IsPolyType.GetHashCode();
            return hashCode;
        }
    }

    public class StringLiteralType : CheezType
    {
        public static StringLiteralType Instance { get; } = new StringLiteralType();
        public override bool IsPolyType => false;
        public override string ToString() => "string_literal";
        public override bool IsErrorType => false;

        private StringLiteralType() : base(0, 1, false) { }
    }

    public class CharType : CheezType
    {
        public static CharType Instance { get; } = new CharType();

        public override bool IsPolyType => false;
        public override string ToString() => "char";
        public override bool IsErrorType => false;

        private CharType() : base(1, 1, true) { }
    }
}
