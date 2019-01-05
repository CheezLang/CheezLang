using LLVMSharp;
using System;
using System.Linq;

namespace Cheez.Compiler.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGeneratorNew
    {
        private void GenerateIntrinsicDeclarations()
        {
            memcpy32 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i32", LLVM.VoidType(),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int32Type(),
                LLVM.Int1Type());

            memcpy64 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i64", LLVM.VoidType(),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int64Type(),
                LLVM.Int1Type());
        }

        private LLVMValueRef GenerateIntrinsicDeclaration(string name, LLVMTypeRef retType, params LLVMTypeRef[] paramTypes)
        {
            var ltype = LLVM.FunctionType(retType, paramTypes, false);
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

        private LLVMTypeRef ParamTypeToLLVMType(CheezType ct)
        {
            var t = CheezTypeToLLVMType(ct);
            if (!CanPassByValue(ct))
                t = LLVM.PointerType(t, 0);
            return t;
        }

        private LLVMValueRef CreateLocalVariable(ITypedSymbol sym)
        {
            if (valueMap.ContainsKey(sym))
                return valueMap[sym];

            var t = CreateLocalVariable(sym.Type);
            valueMap[sym] = t;
            return t;
        }

        private LLVMValueRef CreateLocalVariable(CheezType exprType)
        {
            var builder = LLVM.CreateBuilder();

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            LLVM.PositionBuilderBefore(builder, brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = LLVM.BuildAlloca(builder, type, "");
            var alignment = targetData.AlignmentOfType(type);
            LLVM.SetAlignment(result, alignment);

            LLVM.DisposeBuilder(builder);

            return result;
        }

        private void CastIfAny(CheezType targetType, CheezType sourceType, ref LLVMValueRef value)
        {
            if (targetType == CheezType.Any && sourceType != CheezType.Any)
            {
                var type = CheezTypeToLLVMType(targetType);
                if (sourceType is IntType)
                    value = builder.CreateIntCast(value, type, "");
                else if (sourceType is BoolType)
                    value = builder.CreateZExtOrBitCast(value, type, "");
                else if (sourceType is PointerType || sourceType is ArrayType)
                    value = builder.CreatePtrToInt(value, type, "");
                else
                    throw new NotImplementedException("any cast");
            }
        }

        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct)
        {
            if (typeMap.TryGetValue(ct, out var tt)) return tt;
            var t = CheezTypeToLLVMTypeHelper(ct);
            typeMap[ct] = t;
            return t;
        }

        private LLVMTypeRef CheezTypeToLLVMTypeHelper(CheezType ct)
        {
            switch (ct)
            {
                case TraitType t:
                    {
                        var str = LLVM.StructCreateNamed(context, t.ToString());
                        LLVM.StructSetBody(str, new LLVMTypeRef[]
                        {
                            LLVM.PointerType(LLVM.Int8Type(), 0),
                            LLVM.PointerType(LLVM.Int8Type(), 0)
                        }, false);
                        return str;
                    }

                case AnyType a:
                    return LLVM.Int64Type();

                case BoolType b:
                    return LLVM.Int1Type();

                case IntType i:
                    return LLVM.IntType((uint)i.Size * 8);

                case FloatType f:
                    if (f.Size == 4)
                        return LLVM.FloatType();
                    else if (f.Size == 8)
                        return LLVM.DoubleType();
                    else
                        throw new NotImplementedException();

                case CharType c:
                    return LLVM.Int8Type();

                case PointerType p:
                    if (p.TargetType == VoidType.Intance)
                        return LLVM.PointerType(LLVM.Int8Type(), 0);
                    return LLVM.PointerType(CheezTypeToLLVMType(p.TargetType), 0);

                case ArrayType a:
                    return LLVM.ArrayType(CheezTypeToLLVMType(a.TargetType), (uint)a.Length);

                case SliceType s:
                    {
                        var str = LLVM.StructCreateNamed(context, s.ToString());
                        LLVM.StructSetBody(str, new LLVMTypeRef[]
                        {
                            LLVM.PointerType(CheezTypeToLLVMType(s.TargetType), 0),
                            LLVM.Int32Type()
                        }, false);
                        return str;
                    }

                case ReferenceType r:
                    return LLVM.PointerType(CheezTypeToLLVMType(r.TargetType), 0);

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f:
                    {
                        var paramTypes = f.ParameterTypes.Select(rt => CheezTypeToLLVMType(rt)).ToList();
                        var returnType = CheezTypeToLLVMType(f.ReturnType);

                        var func = LLVM.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
                        return func;
                    }

                case EnumType e:
                    {
                        return CheezTypeToLLVMType(e.MemberType);
                    }

                case StructType s:
                    {
                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        var str = LLVM.StructCreateNamed(context, s.Declaration.Name.Name);
                        LLVM.StructSetBody(str, memTypes, false);
                        return str;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef GetDefaultLLVMValue(CheezType type)
        {
            switch (type)
            {
                case PointerType p:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType(pointerSize * 8), 0, false), CheezTypeToLLVMType(type));

                case IntType i:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case BoolType b:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case FloatType f:
                    return LLVM.ConstReal(CheezTypeToLLVMType(type), 0.0);

                case CharType c:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case StructType p:
                    return LLVM.ConstStruct(p.Declaration.Members.Select(m => GetDefaultLLVMValue(m.Type)).ToArray(), false);

                case TraitType t:
                    return LLVM.ConstStruct(new LLVMValueRef[] {
                        LLVM.ConstPointerNull(LLVM.PointerType(LLVM.Int8Type(), 0)),
                        LLVM.ConstPointerNull(LLVM.PointerType(LLVM.Int8Type(), 0))
                    }, false);

                case AnyType a:
                    return LLVM.ConstInt(LLVM.Int64Type(), 0, false);

                case ArrayType a:
                    {
                        LLVMValueRef[] vals = new LLVMValueRef[a.Length];
                        LLVMValueRef def = GetDefaultLLVMValue(a.TargetType);
                        for (int i = 0; i < vals.Length; ++i)
                            vals[i] = def;

                        return LLVM.ConstArray(CheezTypeToLLVMType(a.TargetType), vals);
                    }

                case SliceType s:
                    return LLVM.ConstStruct(new LLVMValueRef[] {
                        GetDefaultLLVMValue(s.ToPointerType()),
                        LLVM.ConstInt(LLVM.Int32Type(), 0, true)
                    }, false);

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef GetTempValue(CheezType exprType)
        {
            var builder = LLVM.CreateBuilder();

            var brInst = currentTempBasicBlock.GetLastInstruction();
            LLVM.PositionBuilderBefore(builder, brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = LLVM.BuildAlloca(builder, type, "");

            LLVM.DisposeBuilder(builder);

            return result;
        }
    }
}
