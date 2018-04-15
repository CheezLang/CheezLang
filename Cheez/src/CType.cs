using Cheez.Ast;
using System.Collections.Generic;

namespace Cheez
{
    public class CTypeFactory
    {
        private Dictionary<string, CType> sTypes = new Dictionary<string, CType>();

        public CTypeFactory()
        {
            // create default types
            CreateAlias("i8", IntType.GetIntType(1, true));
            CreateAlias("i16", IntType.GetIntType(2, true));
            CreateAlias("i32", IntType.GetIntType(4, true));
            CreateAlias("i64", IntType.GetIntType(8, true));

            CreateAlias("u8", IntType.GetIntType(1, false));
            CreateAlias("u16", IntType.GetIntType(2, false));
            CreateAlias("u32", IntType.GetIntType(4, false));
            CreateAlias("u64", IntType.GetIntType(8, false));

            //CreateAlias("bool", BoolType.Instance);
        }

        public CType GetCType(TypeExpression expr)
        {
            switch (expr)
            {
                case NamedTypeExression n:
                    if (sTypes.ContainsKey(n.Name))
                    {
                        return sTypes[n.Name];
                    }

                    return null;

                case PointerTypeExpression p:
                    return PointerType.GetPointerType(GetCType(p.TargetType));

                case ArrayTypeExpression p:
                    return ArrayType.GetArrayType(GetCType(p.ElementType));

                default:
                    return null;
            }
        }

        public void CreateAlias(string name, CType type)
        {
            sTypes[name] = type;
        }
    }

    public class CType
    {
        public static VoidType Void => VoidType.Intance;
    }

    public class VoidType : CType
    {
        public static VoidType Intance { get; } = new VoidType();
    }

    public class IntType : CType
    {
        private static Dictionary<(int, bool), IntType> sTypes = new Dictionary<(int, bool), IntType>();
        public static IntType LiteralType = new IntType { Signed = false, SizeInBytes = 0 };
        public static IntType DefaultType => GetIntType(8, true);

        public int SizeInBytes { get; private set; }
        public bool Signed { get; private set; }

        public static IntType GetIntType(int sizeInBytes, bool signed)
        {
            var key = (sizeInBytes, signed);

            if (sTypes.ContainsKey(key))
            {
                return sTypes[key];
            }

            var type = new IntType
            {
                SizeInBytes = sizeInBytes,
                Signed = signed
            };

            sTypes[key] = type;
            return type;
        }

        public override string ToString()
        {
            return (Signed ? "i" : "u") + (SizeInBytes * 8);
        }
    }

    public class FloatType : CType
    {
        private static Dictionary<int, FloatType> sTypes = new Dictionary<int, FloatType>();

        public int SizeInBytes { get; set; }

        public static FloatType GetFloatType(int size)
        {
            if (sTypes.ContainsKey(size))
            {
                return sTypes[size];
            }

            var type = new FloatType
            {
                SizeInBytes = size
            };

            sTypes[size] = type;
            return type;
        }

        public override string ToString()
        {
            return "f" + (SizeInBytes * 8);
        }
    }

    public class PointerType : CType
    {
        private static Dictionary<CType, PointerType> sTypes = new Dictionary<CType, PointerType>();

        public CType TargetType { get; set; }

        public static PointerType GetPointerType(CType targetType)
        {
            if (sTypes.ContainsKey(targetType))
            {
                return sTypes[targetType];
            }

            var type = new PointerType
            {
                TargetType = targetType
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}*";
        }
    }

    public class ArrayType : CType
    {
        private static Dictionary<CType, ArrayType> sTypes = new Dictionary<CType, ArrayType>();

        public CType TargetType { get; set; }

        public static ArrayType GetArrayType(CType targetType)
        {
            if (sTypes.ContainsKey(targetType))
            {
                return sTypes[targetType];
            }

            var type = new ArrayType
            {
                TargetType = targetType
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}[]";
        }
    }

    public class StringType : CType
    {
        public static StringType Instance = new StringType();

        public override string ToString()
        {
            return $"string";
        }
    }

    public class StructType : CType
    {
        public TypeDeclaration Declaration { get; set; }

        public override string ToString()
        {
            return $"struct {Declaration.Name}";
        }
    }
}
