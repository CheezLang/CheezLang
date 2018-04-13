using Cheez.Ast;
using System.Collections.Generic;

namespace Cheez
{
    public class CTypeFactory
    {
        private Dictionary<string, CType> sTypes = new Dictionary<string, CType>();

        public CType GetCType(TypeExpression type)
        {
            if (sTypes.ContainsKey(type.Text))
            {
                return sTypes[type.Text];
            }

            return null;
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

        public int SizeInBytes { get; private set; }
        public bool Signed { get; private set; }

        public static IntType GetIntType(int size, bool signed)
        {
            var key = (size, signed);

            if (sTypes.ContainsKey(key))
            {
                return sTypes[key];
            }

            var type = new IntType
            {
                SizeInBytes = size,
                Signed = signed
            };

            sTypes[key] = type;
            return type;
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
    }

    public class StringType : CType
    {
        public static StringType Instance = new StringType();
    }

    public class StructType : CType
    {
        private static Dictionary<string, StructType> sTypes = new Dictionary<string, StructType>();

        public TypeDeclaration Declaration { get; set; }

        public static StructType GetStructType(string name)
        {
            if (sTypes.ContainsKey(name))
            {
                return sTypes[name];
            }

            var type = new StructType
            {
            };

            sTypes[name] = type;
            return type;
        }
    }
}
