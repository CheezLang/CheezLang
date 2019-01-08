using LLVMCS;
using System;
using System.Linq;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGeneratorNew
    {
        private void GenerateIntrinsicDeclarations()
        {
            memcpy32 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i32", TypeRef.GetVoidType(),
                TypeRef.GetIntType(8).GetPointerTo(),
                TypeRef.GetIntType(8).GetPointerTo(),
                TypeRef.GetIntType(32),
                TypeRef.GetIntType(1));

            memcpy64 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i64", TypeRef.GetVoidType(),
                TypeRef.GetIntType(8).GetPointerTo(),
                TypeRef.GetIntType(8).GetPointerTo(),
                TypeRef.GetIntType(64),
                TypeRef.GetIntType(1));
        }

        private ValueRef GenerateIntrinsicDeclaration(string name, TypeRef retType, params TypeRef[] paramTypes)
        {
            var ltype = TypeRef.GetFunctionType(retType, paramTypes);
            var lfunc = module.AddFunction(name, ltype);
            return lfunc;
        }

        private bool CanPassByValue(CheezType ct)
        {
            switch (ct)
            {
                case IntType _:
                case FloatType _:
                case PointerType _:
                case BoolType _:
                case AnyType _:
                    return true;

                case VoidType _:
                    throw new Exception("Bug!");

                default:
                    return false;
            }
        }

        private TypeRef ParamTypeToLLVMType(CheezType ct)
        {
            var t = CheezTypeToLLVMType(ct);
            if (!CanPassByValue(ct))
                t = t.GetPointerTo();
            return t;
        }

        private ValueRef CreateLocalVariable(ITypedSymbol sym)
        {
            if (valueMap.ContainsKey(sym))
                return valueMap[sym];

            var t = CreateLocalVariable(sym.Type);
            valueMap[sym] = t;
            return t;
        }

        private ValueRef CreateLocalVariable(CheezType exprType)
        {
            var builder = new IRBuilder();

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            builder.PositionBefore(brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = builder.Alloca(type);
            var alignment = targetData.GetAlignmentOf(type);
            result.SetAlignment(alignment);

            builder.Dispose();
            return result;
        }

        private void CastIfAny(CheezType targetType, CheezType sourceType, ref ValueRef value)
        {
            if (targetType == CheezType.Any && sourceType != CheezType.Any)
            {
                var type = CheezTypeToLLVMType(targetType);
                if (sourceType is IntType)
                    value = builder.IntCast(value, type);
                else if (sourceType is BoolType)
                    value = builder.ZExtOrBitCast(value, type);
                else if (sourceType is PointerType || sourceType is ArrayType)
                    value = builder.PtrToInt(value, type);
                else
                    throw new NotImplementedException("any cast");
            }
        }

        private TypeRef CheezTypeToLLVMType(CheezType ct)
        {
            if (typeMap.TryGetValue(ct, out var tt)) return tt;
            var t = CheezTypeToLLVMTypeHelper(ct);
            typeMap[ct] = t;
            return t;
        }

        private TypeRef CheezTypeToLLVMTypeHelper(CheezType ct)
        {
            switch (ct)
            {
                case TraitType t:
                    {
                        var str = TypeRef.GetNamedStruct(t.ToString());
                        TypeRef.SetStructBody(str,
                            TypeRef.GetIntType(8).GetPointerTo(),
                            TypeRef.GetIntType(8).GetPointerTo());
                        return str;
                    }

                case AnyType a:
                    return TypeRef.GetIntType(64);

                case BoolType b:
                    return TypeRef.GetIntType(1);

                case IntType i:
                    return TypeRef.GetIntType(i.Size * 8);

                case FloatType f:
                    return TypeRef.GetFloatType(f.Size * 8);

                case CharType c:
                    return TypeRef.GetIntType(8);

                case PointerType p:
                    if (p.TargetType == VoidType.Intance)
                        return TypeRef.GetIntType(8).GetPointerTo();
                    return CheezTypeToLLVMType(p.TargetType).GetPointerTo();

                case ArrayType a:
                    return TypeRef.GetArrayType(CheezTypeToLLVMType(a.TargetType), a.Length);

                case SliceType s:
                    {
                        var str = TypeRef.GetNamedStruct(s.ToString());
                        TypeRef.SetStructBody(str,
                            CheezTypeToLLVMType(s.TargetType).GetPointerTo(),
                            TypeRef.GetIntType(32));
                        return str;
                    }

                case ReferenceType r:
                    return CheezTypeToLLVMType(r.TargetType).GetPointerTo();

                case VoidType _:
                    return TypeRef.GetVoidType();

                case FunctionType f:
                    {
                        var paramTypes = f.ParameterTypes.Select(rt => CheezTypeToLLVMType(rt)).ToList();
                        var returnType = CheezTypeToLLVMType(f.ReturnType);

                        if (f.VarArgs)
                            return TypeRef.GetFunctionTypeVarargs(returnType, paramTypes.ToArray());
                        else
                            return TypeRef.GetFunctionType(returnType, paramTypes.ToArray());
                    }

                case EnumType e:
                    {
                        return CheezTypeToLLVMType(e.MemberType);
                    }

                case StructType s:
                    {
                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        var str = TypeRef.GetNamedStruct(s.Declaration.Name.Name);
                        TypeRef.SetStructBody(str, memTypes);
                        return str;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private ValueRef GetDefaultLLVMValue(CheezType type)
        {
            switch (type)
            {
                case PointerType p:
                    return ValueRef.ConstIntToPtr(ValueRef.ConstUInt(TypeRef.GetIntType(pointerSize * 8), 0), CheezTypeToLLVMType(type));

                case IntType i:
                    if (i.Signed)
                        return ValueRef.ConstInt(CheezTypeToLLVMType(type), 0);
                    else
                        return ValueRef.ConstUInt(CheezTypeToLLVMType(type), 0);

                case BoolType b:
                    return ValueRef.ConstUInt(CheezTypeToLLVMType(type), 0);

                case FloatType f:
                    return ValueRef.ConstFloat(CheezTypeToLLVMType(type), 0.0);

                case CharType c:
                    return ValueRef.ConstUInt(CheezTypeToLLVMType(type), 0);

                case StructType p:
                    return ValueRef.ConstStruct(p.Declaration.Members.Select(m => GetDefaultLLVMValue(m.Type)).ToArray());

                case TraitType t:
                    return ValueRef.ConstStruct(
                        ValueRef.ConstNullPointer(TypeRef.GetIntType(8).GetPointerTo()),
                        ValueRef.ConstNullPointer(TypeRef.GetIntType(8).GetPointerTo())
                    );

                case AnyType a:
                    return ValueRef.ConstUInt(TypeRef.GetIntType(64), 0);

                case ArrayType a:
                    {
                        //ValueRef[] vals = new ValueRef[a.Length];
                        //ValueRef def = GetDefaultLLVMValue(a.TargetType);
                        //for (int i = 0; i < vals.Length; ++i)
                        //    vals[i] = def;

                        return ValueRef.ConstZeroArray(CheezTypeToLLVMType(a.TargetType));
                    }

                case SliceType s:
                    return ValueRef.ConstStruct(
                        GetDefaultLLVMValue(s.ToPointerType()),
                        ValueRef.ConstInt(TypeRef.GetIntType(32), 0)
                    );

                default:
                    throw new NotImplementedException();
            }
        }

        private ValueRef GetTempValue(CheezType exprType)
        {
            var builder = new IRBuilder();

            var brInst = currentTempBasicBlock.GetLastInstruction();
            builder.PositionBefore(brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = builder.Alloca(type);

            builder.Dispose();

            return result;
        }
    }
}
