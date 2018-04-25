using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using System.Collections.Generic;

namespace Cheez.Compiler
{
    public class CTypeFactory
    {
        private Dictionary<string, CheezType> sTypes = new Dictionary<string, CheezType>();

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

            CreateAlias("bool", BoolType.Instance);
            CreateAlias("string", StringType.Instance);
            CreateAlias("void", VoidType.Intance);
        }

        public CheezType GetCheezType(string name)
        {
            if (!sTypes.ContainsKey(name))
            {
                return null;
            }
            return sTypes[name];
        }

        public CheezType GetCheezType(PTTypeExpr expr)
        {
            switch (expr)
            {
                case PTNamedTypeExpr n:
                    if (sTypes.ContainsKey(n.Name))
                    {
                        return sTypes[n.Name];
                    }

                    return null;

                case PTPointerTypeExpr p:
                    return PointerType.GetPointerType(GetCheezType(p.TargetType));

                case PTArrayTypeExpr p:
                    return ArrayType.GetArrayType(GetCheezType(p.ElementType));

                default:
                    return null;
            }
        }

        public void CreateAlias(string name, CheezType type)
        {
            sTypes[name] = type;
        }
    }

    public class CheezType
    {
        public static CheezType Void => VoidType.Intance;
        public static CheezType String => StringType.Instance;
        public static CheezType Bool => BoolType.Instance;
    }

    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance = new BoolType();
    }

    public class IntType : CheezType
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

    public class FloatType : CheezType
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

    public class PointerType : CheezType
    {
        private static Dictionary<CheezType, PointerType> sTypes = new Dictionary<CheezType, PointerType>();

        public CheezType TargetType { get; set; }

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

    public class ArrayType : CheezType
    {
        private static Dictionary<CheezType, ArrayType> sTypes = new Dictionary<CheezType, ArrayType>();

        public CheezType TargetType { get; set; }

        public static ArrayType GetArrayType(CheezType targetType)
        {
            if (targetType == null)
                return null;

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

    public class StringType : CheezType
    {
        public static StringType Instance = new StringType();

        public override string ToString()
        {
            return $"string";
        }
    }

    public class StructType : CheezType
    {
        public AstTypeDecl Declaration { get; set; }

        public StructType(AstTypeDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString()
        {
            return $"struct {Declaration.Name}";
        }
    }
}
