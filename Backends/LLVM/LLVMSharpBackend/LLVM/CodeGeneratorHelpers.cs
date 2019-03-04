using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using LLVMSharp;
using System;
using System.Linq;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator
    {
        private void GenerateIntrinsicDeclarations()
        {
            memcpy32 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i32", LLVM.VoidType(),
                LLVM.Int8Type().GetPointerTo(),
                LLVM.Int8Type().GetPointerTo(),
                LLVM.Int32Type(),
                LLVM.Int1Type());

            memcpy64 = GenerateIntrinsicDeclaration("llvm.memcpy.p0i8.p0i8.i64", LLVM.VoidType(),
                LLVM.Int8Type().GetPointerTo(),
                LLVM.Int8Type().GetPointerTo(),
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
                t = t.GetPointerTo();
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
            var builder = new IRBuilder();

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            if (brInst.Pointer.ToInt64() == 0)
                builder.PositionBuilderAtEnd(bb);
            else
                builder.PositionBuilderBefore(brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = builder.CreateAlloca(type, "");
            var alignment = targetData.AlignmentOfType(type);
            result.SetAlignment(alignment);

            builder.Dispose();
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
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int8Type().GetPointerTo(),
                            LLVM.Int8Type().GetPointerTo()
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
                        return LLVMTypeRef.FloatType();
                    else if (f.Size == 8)
                        return LLVMTypeRef.FloatType();
                    else throw new NotImplementedException();

                case CharType c:
                    return LLVM.Int8Type();

                case PointerType p:
                    if (p.TargetType == VoidType.Intance)
                        return LLVM.Int8Type().GetPointerTo();
                    return CheezTypeToLLVMType(p.TargetType).GetPointerTo();

                case ArrayType a:
                    return LLVMTypeRef.ArrayType(CheezTypeToLLVMType(a.TargetType), (uint)a.Length);

                case SliceType s:
                    {
                        var str = LLVM.StructCreateNamed(context, s.ToString());
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int32Type(),
                            CheezTypeToLLVMType(s.TargetType).GetPointerTo()
                        }, false);
                        return str;
                    }

                case ReferenceType r:
                    return CheezTypeToLLVMType(r.TargetType).GetPointerTo();

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f:
                    {
                        var paramTypes = f.Parameters.Select(rt => CheezTypeToLLVMType(rt.type)).ToList();
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        return LLVMTypeRef.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
                    }

                case EnumType e:
                    {
                        return CheezTypeToLLVMType(e.MemberType);
                    }

                case StructType s:
                    {
                        //var memTypes2 = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        //return LLVM.StructType(memTypes2, false);

                        var name = $"struct.{s.Declaration.Name.Name}";

                        if (s.Declaration.IsPolyInstance)
                        {
                            var types = string.Join(".", s.Declaration.Parameters.Select(p => p.Value.ToString()));
                            name += "." + types;
                        }

                        var llvmType = LLVM.StructCreateNamed(context, name);
                        typeMap[s] = llvmType;

                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        LLVM.StructSetBody(llvmType, memTypes, false);
                        return llvmType;
                    }

                case TupleType t:
                    {
                        var memTypes = t.Members.Select(m => CheezTypeToLLVMType(m.type)).ToArray();
                        return LLVM.StructType(memTypes, false);
                    }


                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef CheezValueToLLVMValue(CheezType type, object v)
        {
            switch (type)
            {
                case BoolType _: return LLVM.ConstInt(CheezTypeToLLVMType(type), (bool)v ? 1ul : 0ul, false);
                case CharType _: return LLVM.ConstInt(CheezTypeToLLVMType(type), (char)v, false);
                case IntType i: return LLVM.ConstInt(CheezTypeToLLVMType(type), ((NumberData)v).ToUlong(), i.Signed);
                default:
                    if (type == CheezType.String)
                    {
                        var s = v as string;
                        return LLVM.ConstStruct(new LLVMValueRef[] {
                            LLVM.ConstInt(LLVM.Int32Type(), (ulong)s.Length, true),
                            LLVM.ConstPointerCast(LLVM.ConstString(s, (uint)s.Length, true), LLVM.PointerType(LLVM.Int8Type(), 0))
                        }, false);
                    }
                    throw new NotImplementedException();
            }

        }

        private LLVMValueRef GetDefaultLLVMValue(CheezType type)
        {
            switch (type)
            {
                case PointerType p:
                    return LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType((uint)pointerSize * 8), 0, false), CheezTypeToLLVMType(type));

                case IntType i:
                    if (i.Signed)
                        return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, true);
                    else
                        return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case BoolType b:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case FloatType f:
                    return LLVM.ConstReal(CheezTypeToLLVMType(type), 0.0);

                case CharType c:
                    return LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false);

                case StructType p:
                    {
                        //return LLVM.ConstPointerNull(CheezTypeToLLVMType(p));
                        var members = p.Declaration.Members.Select(m => GetDefaultLLVMValue(m.Type));
                        return LLVM.ConstStruct(members.ToArray(), false);
                    }

                case TraitType t:
                    return LLVMValueRef.ConstStruct(new LLVMValueRef[] {
                        LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo()),
                        LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo())
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
                        LLVM.ConstInt(LLVM.Int32Type(), 0, true),
                        GetDefaultLLVMValue(s.ToPointerType())
                    }, false);

                case TupleType t:
                    {
                        var members = t.Members.Select(m => GetDefaultLLVMValue(m.type));
                        return LLVM.ConstStruct(members.ToArray(), false);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef GetTempValue(CheezType exprType)
        {
            var builder = new IRBuilder();

            var brInst = currentTempBasicBlock.GetLastInstruction();
            builder.PositionBuilderBefore(brInst);

            var type = CheezTypeToLLVMType(exprType);
            var result = builder.CreateAlloca(type, "");

            builder.Dispose();

            return result;
        }
    }
}
