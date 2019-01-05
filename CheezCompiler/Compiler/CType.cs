﻿using Cheez.Compiler.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler
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
    }

    public abstract class AbstractType : CheezType { }

    public class VarDeclType : AbstractType
    {
        public override bool IsPolyType => false;
        public AstVariableDecl Declaration { get; }

        public VarDeclType(AstVariableDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString() => $"<var decl> {Declaration.Name.Name}";
    }

    public class CombiType : AbstractType
    {
        public override bool IsPolyType => false;
        public List<AbstractType> SubTypes { get; }

        public CombiType(List<AbstractType> decls)
        {
            SubTypes = decls;
        }

        public override string ToString() => $"<decls> ({string.Join(", ", SubTypes)})";
    }

    public class AliasType : AbstractType
    {
        public override bool IsPolyType => false;
        public AstTypeAliasDecl Declaration { get; }

        public AliasType(AstTypeAliasDecl decl)
        {
            Declaration = decl;
        }

        public override string ToString() => $"<type alias> {Declaration.Name.Name}";
    }

    public class CheezTypeType : CheezType
    {
        public static CheezTypeType Instance { get; } = new CheezTypeType();
        public override bool IsPolyType => false;
        public override string ToString() => "type";
    }

    public class GenericFunctionType : CheezType
    {
        public AstFunctionDecl Declaration { get; }

        public override bool IsPolyType => false;

        public GenericFunctionType(AstFunctionDecl decl)
        {
            Declaration = decl;
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

    public class GenericTraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }

        public override bool IsPolyType => false;

        public GenericTraitType(AstTraitDeclaration decl)
        {
            Declaration = decl;
        }
    }

    public class ErrorType : CheezType
    {
        public static ErrorType Instance { get; } = new ErrorType { Size = 0 };
        public override bool IsPolyType => false;
        public override string ToString() => "<Error Type>";
    }

    public class VoidType : CheezType
    {
        public static VoidType Intance { get; } = new VoidType();
        public override string ToString() => "void";
        public override bool IsPolyType => false;
    }

    public class AnyType : CheezType
    {
        public static AnyType Intance { get; } = new AnyType { Size = 8, Alignment = 8 };
        public override string ToString() => "any";
        public override bool IsPolyType => false;
    }

    public class BoolType : CheezType
    {
        public static BoolType Instance = new BoolType { Size = 1, Alignment = 1 };
        private BoolType() {}
        public override string ToString() => "bool";
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
        public static FloatType DefaultType => GetFloatType(4);

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
        public static int PointerSize = 4;
        public static int PointerAlignment = 4;

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
    }

    public class CharType : CheezType
    {
        public static CharType Instance = new CharType { Size = 1, Alignment = 1 };
        public override bool IsPolyType => false;
        public override string ToString() => "char";

    }

    public class TraitType : CheezType
    {
        public AstTraitDeclaration Declaration { get; }
        public override bool IsPolyType => false;

        public TraitType(AstTraitDeclaration decl)
        {
            Size = 2 * PointerType.PointerSize;
            Alignment = PointerType.PointerAlignment;
            Declaration = decl;
        }

        public override string ToString() => Declaration.Name.Name;
    }

    public class StructType : CheezType
    {
        public AstStructDecl Declaration { get; }
        public CheezType[] Arguments { get; }
        public int[] MemberOffsets { get; private set; }

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
            MemberOffsets = new int[Declaration.Members.Count];
            for (int i = 0; i < Declaration.Members.Count; i++)
            {
                var m = Declaration.Members[i];
                MemberOffsets[i] = Size;
                Size += m.Type.Size;
            }

            Alignment = Size;
            if (Alignment == 0)
                Alignment = 4;
        }

        public override string ToString()
        {
            if (Declaration.IsPolyInstance)
            {
                var args = string.Join(", ", Declaration.Parameters.Select(p => $"{p.Value}"));
                return $"{Declaration.Name.Name}({args})";
            }
            return $"{Declaration.Name.Name}";
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

            Alignment = memberType.Alignment;

            Name = en.Name.Name;
            Members = new Dictionary<string, int>();
            MemberType = memberType;

            int value = 0;
            foreach (var m in en.Members)
            {
                Members.Add(m.Name.Name, value++);
            }
        }

        public override string ToString() => $"enum {Name}";
        public override bool IsPolyType => false;
    }

    public class FunctionType : CheezType
    {
        private static List<FunctionType> sTypes = new List<FunctionType>();
                
        public CheezType[] ParameterTypes { get; private set; }
        public CheezType[] ReturnTypes { get; private set; }
        public bool VarArgs { get; set; } = false;

        private FunctionType(CheezType[] parameterTypes, CheezType[] returnTypes)
        {
            this.ParameterTypes = parameterTypes;
            this.ReturnTypes = returnTypes;
        }

        public static FunctionType GetFunctionType(AstFunctionDecl decl)
        {
            var pt = decl.Parameters.Select(p => p.Type).ToArray();
            var rt = decl.ReturnValues.Select(p => p.Type).ToArray();
            return GetFunctionType(pt, rt);
        }

        public static FunctionType GetFunctionType(CheezType[] parameterTypes, CheezType[] returnTypes)
        {
            var f = sTypes.FirstOrDefault(ft =>
            {
                if (ft.ReturnTypes.Length != returnTypes.Length)
                    return false;
                if (ft.ParameterTypes.Length != parameterTypes.Length)
                    return false;

                if (!ft.ParameterTypes.Zip(parameterTypes, (a, b) => a == b).All(b => b)) return false;
                return ft.ReturnTypes.Zip(returnTypes, (a, b) => a == b).All(b => b);
            });

            if (f != null)
                return f;

            var type = new FunctionType(parameterTypes, returnTypes);
            sTypes.Add(type);
            return type;
        }

        public override string ToString()
        {
            var args = string.Join(", ", ParameterTypes.ToList());
            var rts = string.Join(", ", ReturnTypes.ToList());
            if (ReturnTypes.Length != 0)
                return $"fn({args}) -> {rts}";
            else
                return $"fn({args})";
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

        public override string ToString() => "$" + Name;
    }
}
