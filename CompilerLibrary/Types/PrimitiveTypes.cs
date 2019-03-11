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
        public static IntType DefaultType => GetIntType(4, true);

        public bool Signed { get; private set; }
        public override bool IsErrorType => false;

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

        public override bool IsPolyType => false;
    }

    public class FloatType : CheezType
    {
        private static Dictionary<int, FloatType> sTypes = new Dictionary<int, FloatType>();
        public static FloatType LiteralType = new FloatType { Size = 0 };
        public static FloatType DefaultType => GetFloatType(8);
        public override bool IsErrorType => false;

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

        public override bool IsPolyType => false;
    }

    public class PointerType : CheezType
    {
        public static int PointerSize = 8;
        public static int PointerAlignment = 8;

        private static Dictionary<CheezType, PointerType> sTypes = new Dictionary<CheezType, PointerType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;

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
            return $"*{TargetType}";
        }

        public override bool IsPolyType => TargetType.IsPolyType;

    }

    public class ReferenceType : CheezType
    {
        private static Dictionary<CheezType, ReferenceType> sTypes = new Dictionary<CheezType, ReferenceType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;

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
                Size = PointerType.PointerSize
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"ref {TargetType}";
        }

        public override bool IsPolyType => TargetType.IsPolyType;
    }

    public class ArrayType : CheezType
    {
        private static Dictionary<CheezType, ArrayType> sTypes = new Dictionary<CheezType, ArrayType>();

        public CheezType TargetType { get; set; }
        public int Length { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;

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

        public override bool IsPolyType => TargetType.IsPolyType;
    }

    public class SliceType : CheezType
    {
        private static Dictionary<CheezType, SliceType> sTypes = new Dictionary<CheezType, SliceType>();

        public CheezType TargetType { get; set; }
        public override bool IsErrorType => TargetType.IsErrorType;

        public static SliceType GetSliceType(CheezType targetType)
        {
            if (targetType == null)
                return null;

            if (sTypes.ContainsKey(targetType))
                return sTypes[targetType];

            var type = new SliceType
            {
                TargetType = targetType,
                Size = PointerType.PointerSize + 4,
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

        public override bool IsPolyType => TargetType.IsPolyType;
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
