using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Types.Primitive
{
    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();
        public override string ToString() => "void";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
    }

    public class AnyType : CheezType
    {
        public static AnyType Intance { get; } = new AnyType { Size = 8, Alignment = 8 };
        public override string ToString() => "any";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance = new BoolType { Size = 1, Alignment = 1 };
        private BoolType() { }
        public override string ToString() => "bool";
        public override bool IsPolyType => false;
        public override bool IsErrorType => false;
    }

    public class IntType : CheezType
    {
        private static Dictionary<(int, bool), IntType> sTypes = new Dictionary<(int, bool), IntType>();
        public static IntType LiteralType = new IntType { Signed = false, Size = 0 };
        public static IntType DefaultType => GetIntType(8, true);

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

            var type = new IntType
            {
                Size = sizeInBytes,
                Alignment = sizeInBytes,
                Signed = signed
            };

            sTypes[key] = type;
            return type;
        }

        public override string ToString()
        {
            return (Signed ? "i" : "u") + (Size * 8);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is IntType t)
            {
                if (t.Signed != this.Signed)
                    return -1;

                if (concrete.Size > this.Size)
                    return -1;
                if (concrete.Size < this.Size)
                    return 1;
                return 0;
            }
            return -1;
        }
    }

    public class FloatType : CheezType
    {
        private static Dictionary<int, FloatType> sTypes = new Dictionary<int, FloatType>();
        public static FloatType LiteralType = new FloatType { Size = 0 };
        public static FloatType DefaultType => GetFloatType(8);
        public override bool IsErrorType => false;
        public override bool IsPolyType => false;

        public static FloatType GetFloatType(int bytes)
        {
            if (sTypes.ContainsKey(bytes))
            {
                return sTypes[bytes];
            }

            var type = new FloatType
            {
                Size = bytes,
                Alignment = bytes
            };

            sTypes[bytes] = type;
            return type;
        }

        public override string ToString()
        {
            return "f" + (Size * 8);
        }

        public override int Match(CheezType concrete, Dictionary<string, CheezType> polyTypes)
        {
            if (concrete is ReferenceType r)
                concrete = r.TargetType;

            if (concrete is FloatType t)
            {
                if (concrete.Size > this.Size)
                    return -1;
                if (concrete.Size < this.Size)
                    return 1;
                return 0;
            }
            return -1;
        }
    }

    public class PointerType : CheezType
    {
        public static int PointerSize = 8;
        public static int PointerAlignment = 8;

        private static Dictionary<CheezType, PointerType> sTypes = new Dictionary<CheezType, PointerType>();
        public static PointerType NullLiteralType = new PointerType
        {
            TargetType = null,
            Size = PointerSize,
            Alignment = PointerAlignment
        };

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

            var type = new PointerType
            {
                TargetType = targetType,
                Size = PointerSize,
                Alignment = PointerAlignment
            };

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

            var type = new ReferenceType
            {
                TargetType = targetType,
                Size = PointerType.PointerSize,
                Alignment = PointerType.PointerAlignment
            };

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

        public override int Size => Length * TargetType.Size;
        public override int Alignment => TargetType.Alignment;

        public static ArrayType GetArrayType(CheezType targetType, int length)
        {
            if (targetType == null)
                return null;

            var existing = sTypes.FirstOrDefault(t => t.Value.TargetType == targetType && t.Value.Length == length).Value;
            if (existing != null)
                return existing;

            var type = new ArrayType
            {
                TargetType = targetType,
                Size = length * targetType.Size,
                Alignment = targetType.Alignment,
                Length = length
            };

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
            if (concrete is ArrayType p)
                return this.TargetType.Match(p.TargetType, polyTypes);
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is ArrayType r)
            {
                return TargetType == r.TargetType;
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

        public static SliceType GetSliceType(CheezType targetType)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
                return sTypes[targetType];

            var type = new SliceType
            {
                TargetType = targetType,
                Size = PointerType.PointerSize * 2,
                Alignment = PointerType.PointerAlignment
            };

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

    public class StringLiteralType : CheezType
    {
        public static StringLiteralType Instance = new StringLiteralType();
        public override bool IsPolyType => false;
        public override string ToString() => "string_literal";
        public override bool IsErrorType => false;
    }

    public class CharType : CheezType
    {
        public static CharType Instance = new CharType { Size = 1, Alignment = 1 };
        public override bool IsPolyType => false;
        public override string ToString() => "char";
        public override bool IsErrorType => false;
    }
}
