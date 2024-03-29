using Cheez.Extras;
using Cheez.Types.Complex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Cheez.Types.Primitive
{
    public class VoidType : CheezType
    {
        public static VoidType Instance { get; } = new VoidType();
        public override string ToString() => "void";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        private VoidType() : base(0, 1, true) { }
    }

    public class AnyType : CheezType
    {
        public static AnyType Intance { get; } = new AnyType();

        private AnyType() : base(0, 0, false) { }

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

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
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

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
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

        private static Dictionary<(CheezType, bool), PointerType> sTypes = new Dictionary<(CheezType, bool), PointerType>();
        public static PointerType NullLiteralType { get; } = new PointerType(null, true);

        private PointerType(CheezType target, bool mutable) : base(
            target switch {
                AnyType t   => PointerSize * 3,
                TraitType t => PointerSize * 2,
                _           => PointerSize * 1,
            }, PointerAlignment, true)
        {
            TargetType = target;
            Mutable = mutable;
            IsFatPointer = target is TraitType || target is AnyType;
        }

        public bool IsFatPointer { get; set; }

        public CheezType TargetType { get; set; }
        public bool Mutable { get; private set; }
        public override bool IsErrorType => TargetType?.IsErrorType ?? false;
        public override bool IsPolyType => TargetType?.IsPolyType ?? false;

        public static PointerType GetPointerType(CheezType targetType, bool mutable)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey((targetType, mutable)))
            {
                return sTypes[(targetType, mutable)];
            }

            var type = new PointerType(targetType, mutable);

            sTypes[(targetType, mutable)] = type;
            return type;
        }

        public override string ToString()
        {
            if (Mutable) return $"^mut {TargetType}";
            else return $"^{TargetType}";
        }

        public override bool Equals(object obj)
        {
            if (obj is PointerType p)
                return TargetType == p.TargetType && Mutable == p.Mutable;
            return false;
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is PointerType p)
            {
                var targetMatch = this.TargetType.Match(p.TargetType, polyTypes);
                if (targetMatch == -1)
                    return -1;
                if (!this.Mutable && p.Mutable)
                    return Math.Max(1, targetMatch);
                return this.Mutable == p.Mutable ? targetMatch : -1;
            }
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
        private static Dictionary<(CheezType, bool), ReferenceType> sTypes = new Dictionary<(CheezType, bool), ReferenceType>();

        public CheezType TargetType { get; set; }
        public bool Mutable { get; private set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        private ReferenceType(CheezType target, bool mutable) : base((target is TraitType || target is AnyType) ? PointerType.PointerSize * 2 : PointerType.PointerSize, PointerType.PointerAlignment, false)
        {
            TargetType = target;
            Mutable = mutable;
            IsFatReference = target is TraitType || target is AnyType;
        }

        public bool IsFatReference { get; set; }

        public static ReferenceType GetRefType(CheezType targetType, bool mutable)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey((targetType, mutable)))
            {
                return sTypes[(targetType, mutable)];
            }

            var type = new ReferenceType(targetType, mutable);

            sTypes[(targetType, mutable)] = type;
            return type;
        }

        public override string ToString()
        {
            if (Mutable) return $"&mut {TargetType}";
            else return $"&{TargetType}";
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ReferenceType r)
            {
                var targetMatch = this.TargetType.Match(r.TargetType, polyTypes);
                if (targetMatch == -1)
                    return -1;
                if (!this.Mutable && r.Mutable)
                    return Math.Max(1, targetMatch);
                return this.Mutable == r.Mutable ? targetMatch : -1;
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReferenceType r)
            {
                return TargetType == r.TargetType && Mutable == r.Mutable;
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
        public object Length { get; private set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType || Length is PolyValue;

        private ArrayType(CheezType target, object length) : base()
        {
            TargetType = target;
            Length = length;
        }

        public static ArrayType GetArrayType(CheezType targetType, int length)
        {
            return GetArrayType(targetType, NumberData.FromBigInt(length));
        }

        public static ArrayType GetArrayType(CheezType targetType, object length)
        {
            if (targetType == null)
                return null;

            var existing = sTypes.FirstOrDefault(t => t.Value.TargetType == targetType && t.Value.Length.Equals(length)).Value;
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
            return PointerType.GetPointerType(TargetType, true);
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ArrayType p && Length.Equals(p.Length))
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is ArrayType r)
            {
                return TargetType == r.TargetType && Length.Equals(r.Length);
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
        private static Dictionary<(CheezType, bool), SliceType> sTypes = new Dictionary<(CheezType, bool), SliceType>();

        public CheezType TargetType { get; set; }
        public bool Mutable { get; private set; }
        public override bool IsErrorType => TargetType.IsErrorType;
        public override bool IsPolyType => TargetType.IsPolyType;

        private SliceType(CheezType target, bool mutable) : base(PointerType.PointerSize * 2, PointerType.PointerAlignment, true)
        {
            TargetType = target;
            Mutable = mutable;
        }

        public static SliceType GetSliceType(CheezType targetType, bool mutable)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey((targetType, mutable)))
                return sTypes[(targetType, mutable)];

            var type = new SliceType(targetType, mutable);

            sTypes[(targetType, mutable)] = type;
            return type;
        }

        public override string ToString()
        {
            if (Mutable) return $"[]mut {TargetType}";
            else return $"[]{TargetType}";
        }

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(TargetType, Mutable);
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is SliceType p)
            {
                var targetMatch = this.TargetType.Match(p.TargetType, polyTypes);
                if (targetMatch == -1)
                    return -1;
                if (!this.Mutable && p.Mutable)
                    return Math.Max(2, targetMatch);
                return this.Mutable == p.Mutable ? targetMatch : -1;
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is SliceType r)
            {
                return TargetType == r.TargetType && Mutable == r.Mutable;
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

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
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

    public class CharType : CheezType
    {
        private static Dictionary<int, CharType> sTypes = new Dictionary<int, CharType>();
        public static CharType LiteralType { get; } = new CharType(0);
        public static CharType DefaultType => GetCharType(4);

        private CharType(int size) : base(size, size, true)
        {
        }

        public bool Signed { get; private set; }
        public override bool IsErrorType => false;
        public override bool IsPolyType => false;

        public static CharType GetCharType(int sizeInBytes)
        {
            var key = sizeInBytes;

            if (sTypes.ContainsKey(key))
            {
                return sTypes[key];
            }

            var type = new CharType(sizeInBytes);

            sTypes[key] = type;
            return type;
        }

        public override string ToString()
        {
            return "char" + (GetSize() * 8);
        }

        public override int Match(CheezType concrete, Dictionary<string, (CheezType type, object value)> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is CharType t)
            {
                if (concrete.GetSize() != this.GetSize())
                    return -1;
                return 0;
            }
            return -1;
        }

        public BigInteger MinValue => GetSize() switch
        {
            1 => byte.MinValue,
            2 => ushort.MinValue,
            4 => uint.MinValue,
            _ => throw new NotImplementedException()
        };

        public BigInteger MaxValue => GetSize() switch
        {
            1 => byte.MaxValue,
            2 => ushort.MaxValue,
            4 => uint.MaxValue,
            _ => throw new NotImplementedException()
        };
    }

    public class StringType : CheezType
    {
        public static StringType Instance { get; } = new StringType();

        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        private StringType() : base(16, 8, true) { }
        public override string ToString() => "string";
    }

    public class StringLiteralType : CheezType
    {
        public static StringLiteralType Instance { get; } = new StringLiteralType();
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;

        private StringLiteralType() : base(0, 1, false) { }
        public override string ToString() => "string_literal";
    }
}
