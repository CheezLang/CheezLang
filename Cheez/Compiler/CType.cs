using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using System.Collections.Generic;
using System.Linq;

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

            CreateAlias("byte", IntType.GetIntType(1, true));
            CreateAlias("short", IntType.GetIntType(2, true));
            CreateAlias("int", IntType.GetIntType(4, true));
            CreateAlias("long", IntType.GetIntType(8, true));

            CreateAlias("u8", IntType.GetIntType(1, false));
            CreateAlias("u16", IntType.GetIntType(2, false));
            CreateAlias("u32", IntType.GetIntType(4, false));
            CreateAlias("u64", IntType.GetIntType(8, false));

            CreateAlias("ubyte", IntType.GetIntType(1, false));
            CreateAlias("ushort", IntType.GetIntType(2, false));
            CreateAlias("uint", IntType.GetIntType(4, false));
            CreateAlias("ulong", IntType.GetIntType(8, false));

            CreateAlias("f32", FloatType.GetFloatType(4));
            CreateAlias("float", FloatType.GetFloatType(4));
            CreateAlias("f64", FloatType.GetFloatType(8));
            CreateAlias("double", FloatType.GetFloatType(8));

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

                case PTFunctionTypeExpr f:
                    return FunctionType.GetFunctionType(GetCheezType(f.ReturnType), f.ParameterTypes.Select(pt => GetCheezType(pt)).ToArray());

                case null:
                    return CheezType.Void;

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
        public static CheezType Error => ErrorType.Instance;
    }

    public class ErrorType : CheezType
    {
        public static ErrorType Instance { get; } = new ErrorType();
    }

    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();

        public override string ToString()
        {
            return "void";
        }
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance = new BoolType();

        public override string ToString()
        {
            return "bool";
        }
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
        public static FloatType LiteralType = new FloatType { SizeInBytes = 0 };
        public static FloatType DefaultType => GetFloatType(8);

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

        public CheezType TargetType { get; private set; }

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

    public class ReferenceType : CheezType
    {
        private static Dictionary<CheezType, ReferenceType> sTypes = new Dictionary<CheezType, ReferenceType>();

        public CheezType TargetType { get; private set; }

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
                TargetType = targetType
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}&";
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

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(TargetType);
        }
    }

    public class StringType : CheezType
    {
        public static StringType Instance = new StringType();

        public override string ToString()
        {
            return $"string";
        }

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(IntType.GetIntType(1, true));
        }
    }

    public class StructType : CheezType
    {
        public AstTypeDecl Declaration { get; }

        public bool Analyzed { get; set; } = false;

        public StructType(AstTypeDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString()
        {
            return $"{Declaration.Name}";
        }
    }

    public class EnumType : CheezType
    {
        public string Name { get; }
        public Dictionary<string, int> Members { get; }

        public EnumType(AstEnumDecl en)
        {
            Name = en.Name;
            Members = new Dictionary<string, int>();
            int value = 0;
            foreach (var m in en.Members)
            {
                Members.Add(m.Name, value++);
            }
        }

        public override string ToString()
        {
            var vals = string.Join("\n", Members.Select(kv => $"{kv.Key} = {kv.Value}"));
            return $"enum {{\n{vals.Indent(4)}\n}}";
        }
    }

    public class FunctionType : CheezType
    {
        private static List<FunctionType> sTypes = new List<FunctionType>();
                
        public CheezType[] ParameterTypes { get; private set; }
        public CheezType ReturnType { get; private set; }

        private FunctionType(CheezType returnType, CheezType[] parameterTypes)
        {
            this.ReturnType = returnType;
            this.ParameterTypes = parameterTypes;
        }

        public static FunctionType GetFunctionType(AstFunctionDecl decl)
        {
            var pt = decl.Parameters.Select(p => p.Type).ToArray();
            return GetFunctionType(decl.ReturnType, pt);
        }

        public static FunctionType GetFunctionType(CheezType returnType, CheezType[] parameterTypes)
        {
            var f = sTypes.FirstOrDefault(ft =>
            {
                if (ft.ReturnType != returnType)
                    return false;
                if (ft.ParameterTypes.Length != parameterTypes.Length)
                    return false;

                return ft.ParameterTypes.Zip(parameterTypes, (a, b) => a == b).All(b => b);
            });

            if (f != null)
                return f;

            var type = new FunctionType(returnType, parameterTypes);
            sTypes.Add(type);
            return type;
        }

        public override string ToString()
        {
            var args = string.Join(", ", ParameterTypes.ToList());
            return $"fn({args}): {ReturnType}";
        }
    }
}
