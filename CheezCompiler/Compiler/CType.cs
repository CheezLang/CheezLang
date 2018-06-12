using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler
{
    public abstract class CheezType
    {
        public static CheezType Void => VoidType.Intance;
        public static CheezType String => StringType.Instance;
        public static CheezType Char => CharType.Instance;
        public static CheezType Bool => BoolType.Instance;
        public static CheezType Error => ErrorType.Instance;
        public static CheezType Type => CheezTypeType.Instance;
        public static CheezType Any => AnyType.Intance;

        public abstract bool IsPolyType { get; }
        public int Size { get; set; } = 0;
    }

    public class CheezTypeType : CheezType
    {
        public static CheezTypeType Instance { get; } = new CheezTypeType();

        public override bool IsPolyType => false;

        public override string ToString()
        {
            return "type";
        }
    }

    public class GenericFunctionType : CheezType
    {
        //public string[] GenericParameters { get; }
        public AstFunctionDecl Declaration { get; }

        public override bool IsPolyType => false;

        public GenericFunctionType(AstFunctionDecl decl)
        {
            Declaration = decl;
            //GenericParameters = decl.Generics.Select(g => g.Name).ToArray();
        }
    }

    public class GenericStructType : CheezType
    {
        public AstStructDecl Declaration { get; }

        public override bool IsPolyType => false;

        public GenericStructType(AstStructDecl decl)
        {
            Declaration = decl;
        }
    }

    public class ErrorType : CheezType
    {
        public static ErrorType Instance { get; } = new ErrorType { Size = 0 };

        public override bool IsPolyType => false;

        public override string ToString()
        {
            return "<Error Type>";
        }
    }

    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();

        public override string ToString()
        {
            return "void";
        }

        public override bool IsPolyType => false;
    }

    public class AnyType : CheezType
    {
        public static AnyType Intance { get; } = new AnyType { Size = 8 };

        public override string ToString()
        {
            return "any";
        }

        public override bool IsPolyType => false;
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance = new BoolType { Size = 1 };

        private BoolType()
        {}

        public override string ToString()
        {
            return "bool";
        }

        public override bool IsPolyType => false;
    }

    public class IntType : CheezType
    {
        private static Dictionary<(int, bool), IntType> sTypes = new Dictionary<(int, bool), IntType>();
        public static IntType LiteralType = new IntType { Signed = false, Size = 0 };
        public static IntType DefaultType => GetIntType(4, true);
        
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
                Size = sizeInBytes,
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
        public static FloatType DefaultType => GetFloatType(4);

        public static FloatType GetFloatType(int size)
        {
            if (sTypes.ContainsKey(size))
            {
                return sTypes[size];
            }

            var type = new FloatType
            {
                Size = size
            };

            sTypes[size] = type;
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
                TargetType = targetType,
                Size = PointerSize
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}&";
        }

        public override bool IsPolyType => TargetType.IsPolyType;
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
                TargetType = targetType,
                Size = PointerType.PointerSize
            };

            sTypes[targetType] = type;
            return type;
        }

        public override string ToString()
        {
            return $"{TargetType}&";
        }

        public override bool IsPolyType => TargetType.IsPolyType;
    }

    public class ArrayType : CheezType
    {
        private static Dictionary<CheezType, ArrayType> sTypes = new Dictionary<CheezType, ArrayType>();

        public CheezType TargetType { get; set; }
        public int Length { get; set; }

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
            return $"{TargetType}[{Length}]";
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

        public override bool IsPolyType => TargetType.IsPolyType;
    }

    public class StringType : CheezType
    {
        public static StringType Instance = new StringType { Size = PointerType.PointerSize };

        public override string ToString()
        {
            return $"string";
        }

        public PointerType ToPointerType()
        {
            return PointerType.GetPointerType(IntType.GetIntType(1, true));
        }

        public override bool IsPolyType => false;
    }

    public class CharType : CheezType
    {
        public static CharType Instance = new CharType { Size = 1 };
        public override bool IsPolyType => false;

        public override string ToString()
        {
            return $"char";
        }

    }

    public class StructType : CheezType
    {
        public AstStructDecl Declaration { get; }

        private bool _analyzed = false;
        public bool Analyzed
        {
            get => _analyzed;
            set
            {
                _analyzed = value;
                CalculateSize();
            }
        }

        public CheezType[] Arguments { get; }

        public StructType(AstStructDecl decl)
        {
            Declaration = decl;
            Arguments = decl.Parameters.Select(p => p.Value as CheezType).ToArray();
        }
        
        public StructType(AstStructDecl decl, CheezType[] args)
        {
            Declaration = decl;
            Arguments = args;
        }

        private void CalculateSize()
        {
            Size = 0;
            foreach (var m in Declaration.Members)
            {
                Size += m.Type.Size;
            }
        }

        public override string ToString()
        {
            return Declaration.ToString();
        }

        public int GetIndexOfMember(string right)
        {
            return Declaration.Members.FindIndex(m => m.Name.Name == right);
        }

        public override bool IsPolyType => false;
    }

    public class EnumType : CheezType
    {
        public string Name { get; }
        public Dictionary<string, int> Members { get; }

        public CheezType MemberType { get; set; }

        public EnumType(AstEnumDecl en, CheezType memberType = null)
        {
            if (memberType == null)
                memberType = IntType.DefaultType;

            Name = en.Name.Name;
            Members = new Dictionary<string, int>();
            MemberType = memberType;

            int value = 0;
            foreach (var m in en.Members)
            {
                Members.Add(m.Name, value++);
            }
        }

        public override string ToString()
        {
            return $"enum {Name}";
        }

        public override bool IsPolyType => false;
    }

    public class FunctionType : CheezType
    {
        private static List<FunctionType> sTypes = new List<FunctionType>();
                
        public CheezType[] ParameterTypes { get; private set; }
        public CheezType ReturnType { get; private set; }
        public bool VarArgs { get; set; } = false;

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

        public override bool IsPolyType => false;
    }

    public class PolyType : CheezType
    {
        public string Name { get; }
        public override bool IsPolyType => true;

        public PolyType(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return $"${Name}";
        }
    }
}
