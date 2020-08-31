using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using CompilerLibrary.Extras;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator
    {
        private void SetupStackTraceStuff()
        {
            stackTraceType = LLVM.StructCreateNamed(module.GetModuleContext(), "stacktrace.type");
            stackTraceType.StructSetBody(new LLVMTypeRef[]
            {
                LLVM.PointerType(stackTraceType, 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.PointerType(LLVM.Int8Type(), 0),
                LLVM.Int64Type(),
                LLVM.Int64Type(),
            }, false);

            stackTraceTop = module.AddGlobal(LLVM.PointerType(stackTraceType, 0), "stacktrace.top");
            stackTraceTop.SetThreadLocal(true);
            stackTraceTop.SetInitializer(LLVM.ConstPointerNull(LLVM.PointerType(stackTraceType, 0)));
            stackTraceTop.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
        }

        private void PushStackTrace(AstFuncExpr function)
        {
            if (!keepTrackOfStackTrace)
                return;
            var stackEntry = builder.CreateAlloca(stackTraceType, "stack_entry");
            stackEntry.SetAlignment(8);

            var previousPointer = builder.CreateStructGEP(stackEntry, 0, "stack_entry.previous.ptr");
            var functionNamePointer = builder.CreateStructGEP(stackEntry, 1, "stack_entry.function.ptr");
            var locationPointer = builder.CreateStructGEP(stackEntry, 2, "stack_entry.location.ptr");
            var linePointer = builder.CreateStructGEP(stackEntry, 3, "stack_entry.line.ptr");
            var columnPointer = builder.CreateStructGEP(stackEntry, 4, "stack_entry.col.ptr");

            var previous = builder.CreateLoad(stackTraceTop, "stack_trace.top");
            builder.CreateStore(previous, previousPointer);

            builder.CreateStore(builder.CreateGlobalStringPtr(function.Name, ""), functionNamePointer);
            builder.CreateStore(builder.CreateGlobalStringPtr(function.Beginning.file, ""), locationPointer);
            builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)function.Beginning.line, true), linePointer);
            builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)(function.Beginning.index - function.Beginning.lineStartIndex + 1), true), columnPointer);

            // save current global
            builder.CreateStore(stackEntry, stackTraceTop);
        }

        private void PopStackTrace()
        {
            if (!keepTrackOfStackTrace)
                return;
            var current = builder.CreateLoad(stackTraceTop, "stack_trace.top");
            var previousPtr = builder.CreateStructGEP(current, 0, "stack_trace.previous.ptr");
            var previous = builder.CreateLoad(previousPtr, "stack_trace.previous");
            builder.CreateStore(previous, stackTraceTop);
        }

        private void UpdateStackTracePosition(ILocation location)
        {
            if (!keepTrackOfStackTrace)
                return;

            // right now we're checking if the current stack entry is not null, 
            // but this should not be necessary. I think there's a bug somewhere else related to that.
            // Maybe we generate this code in places where the current stack top pointer can point to null.
            var current = builder.CreateLoad(stackTraceTop, "");

            var bbDo = currentLLVMFunction.AppendBasicBlock("stack_trace.update.do");
            var bbEnd = currentLLVMFunction.AppendBasicBlock("stack_trace.update.end");

            var isNull = builder.CreateIsNull(current, "");
            builder.CreateCondBr(isNull, bbEnd, bbDo);

            builder.PositionBuilderAtEnd(bbDo);
            var linePtr = builder.CreateStructGEP(current, 3, "");
            var colPtr = builder.CreateStructGEP(current, 4, "");
            builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)location.Beginning.line, true), linePtr);
            builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)(location.Beginning.index - location.Beginning.lineStartIndex + 1), true), colPtr);
            builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbEnd);
        }

        void Sprintf(LLVMValueRef buffer, params LLVMValueRef[] args)
        {
            var b = builder.CreateLoad(buffer, "");

            args = args.Prepend(b).ToArray();

            var count = builder.CreateCall(sprintf, args, "");
            count = builder.CreateIntCast(count, LLVM.Int64Type(), "");
            b = builder.CreatePtrToInt(b, LLVM.Int64Type(), "");
            b = builder.CreateAdd(b, count, "");
            b = builder.CreateIntToPtr(b, LLVM.Int8Type().GetPointerTo(), "");

            builder.CreateStore(b, buffer);
        }

        private void PrintStackTrace(LLVMValueRef buffer)
        {
            if (!keepTrackOfStackTrace)
                return;

            Sprintf(buffer, builder.CreateGlobalStringPtr("at\n", ""));

            var bbCond = currentLLVMFunction.AppendBasicBlock("stack_trace.print.cond");
            var bbBody = currentLLVMFunction.AppendBasicBlock("stack_trace.print.body");
            var bbEnd = currentLLVMFunction.AppendBasicBlock("stack_trace.print.end");

            var currentTop = CreateLocalVariable(LLVM.PointerType(stackTraceType, 0), 8, "stack_trace.print.current");
            {
                var v = builder.CreateLoad(stackTraceTop, "");
                builder.CreateStore(v, currentTop);
            }

            builder.CreateBr(bbCond);
            builder.PositionBuilderAtEnd(bbCond);

            // get current stack top
            var current = builder.CreateLoad(currentTop, "");
            var isNull = builder.CreateIsNull(current, "");
            builder.CreateCondBr(isNull, bbEnd, bbBody);

            builder.PositionBuilderAtEnd(bbBody);
            // print current entry
            {
                var namePtr = builder.CreateStructGEP(current, 1, "");
                var name = builder.CreateLoad(namePtr, "");
                var locationPtr = builder.CreateStructGEP(current, 2, "");
                var location = builder.CreateLoad(locationPtr, "");
                var line = builder.CreateLoad(builder.CreateStructGEP(current, 3, ""), "");
                var col = builder.CreateLoad(builder.CreateStructGEP(current, 4, ""), "");
                Sprintf(buffer, builder.CreateGlobalStringPtr("  %s (%s:%lld:%lld)\n", ""), name, location, line, col);
            }
            // load previous entry
            {
                var previousPtr = builder.CreateStructGEP(current, 0, "");
                var previous = builder.CreateLoad(previousPtr, "");
                builder.CreateStore(previous, currentTop);
            }
            builder.CreateBr(bbCond);

            builder.PositionBuilderAtEnd(bbEnd);
        }

        private void CheckPointerNull(LLVMValueRef pointer, ILocation location, string message)
        {
            if (!checkForNullTraitObjects)
                return;

            var bbNull = currentLLVMFunction.AppendBasicBlock("cpn.null");
            var bbEnd = currentLLVMFunction.AppendBasicBlock("cpn.end");

            var isNull = builder.CreateIsNull(pointer, "");
            builder.CreateCondBr(isNull, bbNull, bbEnd);

            builder.PositionBuilderAtEnd(bbNull);

            UpdateStackTracePosition(location);
            CreateExit($"[{location.Beginning}] {message}", 2);
            builder.CreateUnreachable();

            builder.PositionBuilderAtEnd(bbEnd);
        }

        private void CreateCLibFunctions()
        {
            void CreateFunc(ref LLVMValueRef func, string name, LLVMTypeRef returnType, bool isVarargs, params LLVMTypeRef[] argTypes)
            {
                func = module.GetNamedFunction(name);
                if (func.Pointer.ToInt64() == 0)
                {
                    var ltype = LLVM.FunctionType(returnType, argTypes, isVarargs);
                    func = module.AddFunction(name, ltype);
                }
            }
            //LLVMValueRef ___chkstk_ms = default;
            //CreateFunc(ref ___chkstk_ms, "___chkstk_ms", LLVM.VoidType(), false);

            exit = module.GetNamedFunction("exit");
            if (exit.Pointer.ToInt64() == 0)
                exit = GenerateIntrinsicDeclaration("exit", LLVM.VoidType(), LLVM.Int32Type());

            CreateFunc(ref puts, "puts", LLVM.VoidType(), false, LLVM.Int8Type().GetPointerTo());

            printf = module.GetNamedFunction("printf");
            if (printf.Pointer.ToInt64() == 0)
            {
                var ltype = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] {
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, true);
                printf = module.AddFunction("printf", ltype);
            }

            sprintf = module.GetNamedFunction("sprintf");
            if (sprintf.Pointer.ToInt64() == 0)
            {
                var ltype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] {
                    LLVM.PointerType(LLVM.Int8Type(), 0),
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, true);
                sprintf = module.AddFunction("sprintf", ltype);
            }


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                exitThread = module.GetNamedFunction("ExitThread");
                if (exitThread.Pointer.ToInt64() == 0)
                {
                    var ltype = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[]
                    {
                        LLVM.Int32Type()
                    }, false);
                    exitThread = module.AddFunction("ExitThread", ltype);
                    exitThread.SetFunctionCallConv((uint)LLVMCallConv.LLVMX86StdcallCallConv);
                }
            }
        }

        private void CreateExit(string msg, int exitCode, params LLVMValueRef[] p)
        {
            var args = new List<LLVMValueRef>
            {
                builder.CreateGlobalStringPtr(msg + "\n", "")
            };
            args.AddRange(p);

            // create temporary buffer for message and stack trace
            var buffer = builder.CreateAlloca(LLVM.Int8Type().GetPointerTo(), "stack_trace.print.buffer");
            builder.CreateStore(builder.CreateArrayMalloc(LLVM.Int8Type(), LLVM.ConstInt(LLVM.Int32Type(), 1024 * 4, false), ""), buffer);
            var originalBuffer = builder.CreateLoad(buffer, "");

            // print message and stack trace to temporary buffer
            Sprintf(buffer, args.ToArray());
            PrintStackTrace(buffer);

            // print
            builder.CreateCall(puts, new LLVMValueRef[] { originalBuffer }, "");
            builder.CreateFree(originalBuffer);

            // exit thread
            if (exitThread.Pointer.ToInt64() != 0) {
                builder.CreateCall(exitThread, new LLVMValueRef[] {
                    LLVM.ConstInt(LLVM.Int32Type(), (uint)exitCode, true)
                }, "");
            }
        }

        private LLVMValueRef GenerateIntrinsicDeclaration(string name, LLVMTypeRef retType, params LLVMTypeRef[] paramTypes)
        {
            var ltype = LLVM.FunctionType(retType, paramTypes, false);
            var lfunc = module.AddFunction(name, ltype);
            return lfunc;
        }

        private AstImplBlock GetTraitImpl(TraitType trait, CheezType type)
        {
            if (trait.Declaration.Implementations.TryGetValue(type, out var impl))
                return impl;

            if (type is StructType str && str.Declaration.Extends != null)
                return GetTraitImpl(trait, str.Declaration.Extends);

            Debug.Assert(false);
            return null;
        }

        private static bool CanPassByValue(CheezType ct)
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

        private LLVMValueRef CreateLocalVariable(CheezType exprType, string name = "temp")
        {
            return CreateLocalVariable(CheezTypeToLLVMType(exprType), exprType.GetAlignment(), name);
        }

        private LLVMValueRef CreateLocalVariable(LLVMTypeRef type, int alignment, string name = "temp")
        {
            var builder = new IRBuilder();

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            if (brInst.Pointer.ToInt64() == 0)
                builder.PositionBuilderAtEnd(bb);
            else
                builder.PositionBuilderBefore(brInst);

            var result = builder.CreateAlloca(type, name);
            var llvmAlignment = targetData.AlignmentOfType(type);
            Debug.Assert(alignment >= llvmAlignment && alignment % llvmAlignment == 0);
            result.SetAlignment((uint)alignment);

            builder.Dispose();
            return result;
        }

        private LLVMTypeRef FuncTypeToLLVMType(FunctionType f)
        {
            var paramTypes = f.Parameters.Select(rt => CheezTypeToLLVMType(rt.type)).ToList();
            var returnType = CheezTypeToLLVMType(f.ReturnType);
            return LLVMTypeRef.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
        }

        private CheezType NormalizeType(CheezType type)
        {
            return type switch
            {
                ReferenceType r => ReferenceType.GetRefType(NormalizeType(r.TargetType), false),
                PointerType r => PointerType.GetPointerType(NormalizeType(r.TargetType), false),
                SliceType r => SliceType.GetSliceType(NormalizeType(r.TargetType), false),
                ArrayType r => ArrayType.GetArrayType(NormalizeType(r.TargetType), r.Length),
                TupleType r => TupleType.GetTuple(r.Members.Select(m => (m.name, NormalizeType(m.type))).ToArray()),
                FunctionType r => new FunctionType(r.Parameters.Select(m => (m.name, NormalizeType(m.type), m.defaultValue)).ToArray(), NormalizeType(r.ReturnType), r.IsFatFunction, r.CC),
                _ => type,
            };
        }

        private LLVMTypeRef CheezTypeToLLVMType(CheezType ct)
        {
            ct = NormalizeType(ct);
            //switch (ct)
            //{
            //    case ReferenceType r when r.Mutable:
            //        ct = ReferenceType.GetRefType(r.TargetType, false);
            //        break;
            //    case PointerType r when r.Mutable:
            //        ct = PointerType.GetPointerType(r.TargetType, false);
            //        break;
            //    case SliceType r when r.Mutable:
            //        ct = SliceType.GetSliceType(r.TargetType, false);
            //        break;
            //}

            if (typeMap.TryGetValue(ct, out var tt)) return tt;
            var t = CheezTypeToLLVMTypeHelper(ct);
            typeMap[ct] = t;
            return t;
        }

        private LLVMTypeRef CheezTypeToLLVMTypeHelper(CheezType ct)
        {
            switch (ct)
            {
                case StringType s:
                    {
                        var str = context.StructCreateNamed("string");
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int64Type(),
                            LLVM.Int8Type().GetPointerTo()
                        }, false);
                        return str;
                    }

                case TraitType t:
                    {
                        Console.WriteLine($"[ERROR] trait type {t}");
                        return LLVM.VoidType();
                    }

                case AnyType a:
                    {
                        Console.WriteLine($"[ERROR] any type");
                        return LLVM.VoidType();
                    }

                case BoolType b:
                    return LLVM.Int1Type();

                case IntType i:
                    return LLVM.IntType((uint)i.GetSize() * 8);

                case FloatType f:
                    if (f.GetSize() == 4)
                        return LLVMTypeRef.FloatType();
                    else if (f.GetSize() == 8)
                        return LLVMTypeRef.DoubleType();
                    else throw new NotImplementedException();

                case CharType c:
                    return LLVM.IntType((uint)c.GetSize() * 8);

                case PointerType p:
                    {
                        if (p.TargetType == CheezType.Any)
                        {
                            var str = LLVM.StructCreateNamed(context, $"^{p.TargetType.ToString()}");
                            LLVM.StructSetBody(str, new LLVMTypeRef[] {
                                LLVM.Int8Type().GetPointerTo(),
                                rttiTypeInfoPtr
                            }, false);
                            return str;
                        }
                        else if (p.TargetType is TraitType)
                        {
                            var str = LLVM.StructCreateNamed(context, $"^{p.TargetType.ToString()}");
                                LLVM.StructSetBody(str, new LLVMTypeRef[] {
                                LLVM.Int8Type().GetPointerTo(),
                                LLVM.Int8Type().GetPointerTo()
                            }, false);
                            return str;
                        }
                        else
                        {
                            if (p.TargetType == VoidType.Instance)
                                return LLVM.Int8Type().GetPointerTo();
                            return CheezTypeToLLVMType(p.TargetType).GetPointerTo();
                        }
                    }

                case ReferenceType r:
                    {
                        if (r.TargetType == CheezType.Any)
                        {
                            var str = LLVM.StructCreateNamed(context, $"&{r.TargetType.ToString()}");
                            LLVM.StructSetBody(str, new LLVMTypeRef[] {
                                LLVM.Int8Type().GetPointerTo(),
                                rttiTypeInfoPtr
                            }, false);
                            return str;
                        }
                        else if (r.TargetType is TraitType)
                        {
                            var str = LLVM.StructCreateNamed(context, $"&{r.TargetType.ToString()}");
                            LLVM.StructSetBody(str, new LLVMTypeRef[] {
                                LLVM.Int8Type().GetPointerTo(),
                                LLVM.Int8Type().GetPointerTo()
                            }, false);
                            return str;
                        }
                        else
                        {
                            return CheezTypeToLLVMType(r.TargetType).GetPointerTo();
                        }
                    }

                case SelfType self:
                    return LLVM.Int8Type();
                //return CheezTypeToLLVMType(self.traitType);

                case ArrayType a:
                    return LLVMTypeRef.ArrayType(CheezTypeToLLVMType(a.TargetType), (uint)((NumberData)a.Length).ToUlong());

                case SliceType s:
                    {
                        var str = LLVM.StructCreateNamed(context, $"[]{s.TargetType.ToString()}");
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int64Type(),
                            CheezTypeToLLVMType(PointerType.GetPointerType(s.TargetType, true))
                        }, false);
                        return str;
                    }

                case VoidType _:
                    return LLVM.VoidType();

                case FunctionType f when f.IsFatFunction:
                    {
                        var paramTypes = f.Parameters.Select(rt => CheezTypeToLLVMType(rt.type)).ToList();
                        paramTypes.Insert(0, LLVM.PointerType(LLVM.Int8Type(), 0));
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        var funcType = LLVMTypeRef.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);

                        var llvmType = LLVM.StructCreateNamed(context, f.ToString());
                        llvmType.StructSetBody(new LLVMTypeRef[] {
                            funcType.GetPointerTo(),
                            LLVM.PointerType(LLVM.Int8Type(), 0)
                        }, false);
                        return llvmType;
                    }

                case FunctionType f when !f.IsFatFunction:
                    {
                        var paramTypes = f.Parameters.Select(rt => CheezTypeToLLVMType(rt.type)).ToList();
                        var returnType = CheezTypeToLLVMType(f.ReturnType);
                        var funcType = LLVMTypeRef.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
                        return funcType.GetPointerTo();
                    }

                case EnumType e:
                    {
                        if (e.Declaration.IsReprC)
                            return CheezTypeToLLVMType(e.Declaration.TagType);

                        var llvmType = LLVM.StructCreateNamed(context, $"enum.{e}");

                        //if (e.Declaration.HasAssociatedTypes)
                        //{
                        var restSize = e.GetSize() - e.Declaration.TagType.GetSize();
                        llvmType.StructSetBody(new LLVMTypeRef[]
                        {
                            CheezTypeToLLVMType(e.Declaration.TagType),
                            LLVM.ArrayType(LLVM.Int8Type(), (uint)restSize)
                        }, false);
                        //}
                        //else
                        //{
                        //    llvmType.StructSetBody(new LLVMTypeRef[]
                        //    {
                        //        CheezTypeToLLVMType(e.TagType)
                        //    }, false);
                        //}

                        return llvmType;
                    }

                case StructType s:
                    {
                        //var memTypes2 = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        //return LLVM.StructType(memTypes2, false);

                        var name = $"struct.{s.Name}";

                        if (s.Declaration.IsPolyInstance)
                        {
                            var types = string.Join(".", s.Declaration.Parameters.Select(p => p.Value.ToString()));
                            name += "." + types;
                        }

                        var llvmType = LLVM.StructCreateNamed(context, name);
                        typeMap[s] = llvmType;

                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type));

                        LLVM.StructSetBody(llvmType, memTypes.ToArray(), false);

                        foreach (var m in s.Declaration.Members) {
                            int myOffset = m.Offset;
                            int llvmOffset = (int)LLVM.OffsetOfElement(targetData, llvmType, (uint)m.Index);

                            if (myOffset != llvmOffset) {
                                System.Console.WriteLine($"[ERROR] {s.Declaration.Name}: offset mismatch at {m.Index}: cheez {myOffset}, llvm {llvmOffset}");
                            }
                        }

                        if (targetData.SizeOfTypeInBits(llvmType) / 8ul != (ulong)s.GetSize())
                        {
                            System.Console.WriteLine($"[ERROR] {s.Declaration.Name}: struct size mismatch: cheez {s.GetSize()}, llvm {targetData.SizeOfTypeInBits(llvmType) / 8}");
                        }

                        if (targetData.AlignmentOfType(llvmType) != (uint)s.GetAlignment())
                        {
                            System.Console.WriteLine($"[ERROR] {s.Declaration.Name}: struct alignment mismatch: cheez {s.GetAlignment()}, llvm {targetData.AlignmentOfType(llvmType)}");
                        }

                        return llvmType;
                    }

                case TupleType t:
                    {
                        var memTypes = t.Members.Select(m => CheezTypeToLLVMType(m.type)).ToArray();
                        return LLVM.StructType(memTypes, false);
                    }

                case RangeType r:
                    {
                        var name = $"range.{r.TargetType}";

                        var llvmType = LLVM.StructCreateNamed(context, name);
                        typeMap[r] = llvmType;

                        var memTypes = new LLVMTypeRef[]
                        {
                            CheezTypeToLLVMType(r.TargetType),
                            CheezTypeToLLVMType(r.TargetType)
                        };
                        LLVM.StructSetBody(llvmType, memTypes, false);
                        return llvmType;
                    }

                default:
                    throw new NotImplementedException(ct.ToString());
            }
        }

        private LLVMValueRef CheezValueToLLVMValue(CheezType type, object v)
        {
            if (type == IntType.LiteralType || type == FloatType.LiteralType)
                throw new Exception();

            switch (type)
            {
                case BoolType _: return LLVM.ConstInt(CheezTypeToLLVMType(type), (bool)v ? 1ul : 0ul, false);
                case CharType _: return LLVM.ConstInt(CheezTypeToLLVMType(type), (char)v, false);
                case IntType i: return LLVM.ConstInt(CheezTypeToLLVMType(type), ((NumberData)v).ToUlong(), i.Signed);
                case FloatType f: return LLVM.ConstReal(CheezTypeToLLVMType(type), ((NumberData)v).ToDouble());
                case ArrayType arr when arr.TargetType is CharType ct && v is string s:
                    return LLVM.ConstArray(CheezTypeToLLVMType(ct), s.ToCharArray().Select(c => CheezValueToLLVMValue(ct, c)).ToArray());

                case TraitType _ when v == null: return LLVM.ConstNamedStruct(CheezTypeToLLVMType(type), new LLVMValueRef[]{
                    LLVM.ConstPointerNull(voidPointerType),
                    LLVM.ConstPointerNull(voidPointerType)
                });

                case EnumType e: return new LLVMValueRef();

                case FunctionType f when v is AstFuncExpr:
                    return valueMap[v];

                default:
                    if (type == CheezType.String && v is string)
                    {
                        var s = v as string;
                        var bytes = Encoding.UTF8.GetBytes(s);
                        var byteValues = new LLVMValueRef[bytes.Length];
                        for (int i = 0; i < bytes.Length; i++)
                            byteValues[i] = LLVM.ConstInt(LLVM.Int8Type(), (ulong)bytes[i], false);

                        var glob = module.AddGlobal(LLVM.ArrayType(LLVM.Int8Type(), (uint)bytes.Length), "string");
                        glob.SetInitializer(LLVM.ConstArray(LLVM.Int8Type(), byteValues));

                        return LLVM.ConstNamedStruct(CheezTypeToLLVMType(type), new LLVMValueRef[] {
                            LLVM.ConstInt(LLVM.Int64Type(), (ulong)byteValues.Length, true),
                            LLVM.ConstPointerCast(glob, LLVM.Int8Type().GetPointerTo())
                        });
                    }
                    if (type == CheezType.CString && v is string)
                    {
                        var s = v as string;
                        return builder.CreateGlobalStringPtr(s, "");
                    }

                    if (type is PointerType p)
                    {
                        var t = CheezTypeToLLVMType(p);
                        switch (p.TargetType)
                        {
                            case AnyType _:
                                {
                                    if (v != null) throw new Exception();
                                    return LLVM.ConstNamedStruct(t, new LLVMValueRef[]
                                    {
                                        GetZeroInitializer(voidPointerType),
                                        GetZeroInitializer(rttiTypeInfoPtr),
                                    });
                                }
                            case TraitType _:
                                {
                                    if (v != null) throw new Exception();
                                    return LLVM.ConstNamedStruct(t, new LLVMValueRef[]
                                    {
                                        GetZeroInitializer(voidPointerType),
                                        GetZeroInitializer(voidPointerType),
                                    });
                                }

                            default:
                                {
                                    if (v == null)
                                    {
                                        return LLVM.ConstPointerNull(t);
                                    }
                                    else
                                    {
                                        var val = LLVM.ConstInt(LLVM.Int64Type(), ((NumberData)v).ToUlong(), false);
                                        return LLVM.ConstIntToPtr(val, t);
                                    }
                                }
                        }
                    }

                    return new LLVMValueRef();
            }

        }

        private LLVMValueRef GetDefaultLLVMValue(CheezType type) => type switch
        {
            PointerType p => LLVM.ConstIntToPtr(LLVM.ConstInt(LLVM.IntType((uint)pointerSize * 8), 0, false), CheezTypeToLLVMType(type)),
            IntType i => LLVM.ConstInt(CheezTypeToLLVMType(type), 0, i.Signed),
            BoolType b => LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false),
            FloatType f => LLVM.ConstReal(CheezTypeToLLVMType(type), 0.0),
            CharType c => LLVM.ConstInt(CheezTypeToLLVMType(type), 0, false),
            AnyType a => LLVM.ConstInt(LLVM.Int64Type(), 0, false),
            FunctionType f when f.IsFatFunction => LLVM.ConstNamedStruct(CheezTypeToLLVMType(f), new LLVMValueRef[] {
                LLVM.ConstNull(CheezTypeToLLVMType(f)),
                LLVM.ConstPointerNull(LLVM.Int8Type()),
            }),
            FunctionType f when !f.IsFatFunction => LLVM.ConstNull(CheezTypeToLLVMType(f)),
            TupleType t => LLVM.ConstStruct(t.Members.Select(m => GetDefaultLLVMValue(m.type)).ToArray(), false),

            StructType p => p.Declaration.Members.Aggregate(
                LLVM.GetUndef(CheezTypeToLLVMType(p)),
                (str, m) => builder.CreateInsertValue(str, GenerateExpression(m.Decl.Initializer, true), (uint)m.Index, "")),

            TraitType t => LLVM.ConstNamedStruct(CheezTypeToLLVMType(t), new LLVMValueRef[] {
                        LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo()),
                        LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo())
                    }),

            StringType _ => LLVM.ConstNamedStruct(CheezTypeToLLVMType(type), new LLVMValueRef[] {
                        LLVM.ConstInt(LLVM.Int64Type(), 0, true),
                        LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo())
                    }),

            SliceType s => LLVM.ConstNamedStruct(CheezTypeToLLVMType(s), new LLVMValueRef[] {
                        LLVM.ConstInt(LLVM.Int64Type(), 0, true),
                        GetDefaultLLVMValue(s.ToPointerType())
                    }),

            ArrayType a => LLVM.ConstArray(
                CheezTypeToLLVMType(a.TargetType),
                new LLVMValueRef[((NumberData)a.Length).ToUlong()].Populate(GetDefaultLLVMValue(a.TargetType))),

            _ => throw new NotImplementedException()
        };

        private void GenerateVTables()
        {
            // create vtable type
            foreach (var trait in workspace.Traits)
            {
                var entryTypes = new List<LLVMTypeRef>();

                // entry for type info
                entryTypes.Add(rttiTypeInfoPtr);

                // entries for functions
                foreach (var func in trait.Functions)
                {
                    // if (func.ExcludeFromVTable)
                    //     continue;

                    if (func.IsGeneric)
                    {
                        throw new NotImplementedException();
                    }

                    vtableIndices[func] = entryTypes.Count;

                    var funcType = CheezTypeToLLVMType(func.Type);
                    entryTypes.Add(funcType);
                }

                var vtableType = LLVM.StructCreateNamed(context, $"__vtable_type_{trait.TraitType}");
                LLVM.StructSetBody(vtableType, entryTypes.ToArray(), false);
                vtableTypes[trait.TraitType] = vtableType;

                // entries for functions
                foreach (var func in trait.Functions)
                {
                    int index = vtableIndices[func];
                    ulong offset = LLVM.OffsetOfElement(targetData, vtableType, (uint)index);
                    vtableOffsets[func] = offset;
                }
            }


            foreach (var kv in workspace.TypeTraitMap)
            {
                var type = kv.Key;

                foreach (var impl in kv.Value)
                {
                    var trait = impl.Trait;
                    var vtableType = vtableTypes[trait];
                    var vtable = module.AddGlobal(vtableType, $"__vtable_{trait}_for_{type}");
                    LLVM.SetLinkage(vtable, LLVMLinkage.LLVMInternalLinkage);
                    vtableMap[impl] = vtable;
                }
            }
        }

        private void SetVTables()
        {
            foreach (var kv in workspace.TypeTraitMap)
            {
                var type = kv.Key;
                var traitImpls = kv.Value;

                foreach (var impl in traitImpls)
                {
                    var trait = impl.Trait;
                    var vtableType = vtableTypes[trait];
                    var entryTypes = LLVM.GetStructElementTypes(vtableType);

                    var entries = new LLVMValueRef[entryTypes.Length];
                    for (int i = 0; i < entries.Length; i++)
                    {
                        entries[i] = LLVM.ConstNull(entryTypes[i]);
                    }

                    // set type info if available
                    if (typeInfoTable.TryGetValue(impl.TargetType, out var ptr_vtable))
                    {
                        entries[0] = RTTITypeInfoAsPtr(impl.TargetType);
                    }

                    // set function pointers
                    foreach (var func in impl.Functions)
                    {
                        var traitFunc = func.TraitFunction;
                        if (traitFunc == null)
                            continue;
                        // if (traitFunc.ExcludeFromVTable)
                        //     continue;

                        var index = vtableIndices[traitFunc];
                        entries[index] = LLVM.ConstPointerCast(valueMap[func], entryTypes[index]);
                    }

                    var defValue = LLVM.ConstNamedStruct(vtableType, entries);

                    var vtable = vtableMap[impl];
                    LLVM.SetInitializer(vtable, defValue);
                }
            }
        }

        // destructors
        private LLVMValueRef GetDestructor(CheezType type)
        {
            if (mDestructorMap.TryGetValue(type, out var dtor))
                return dtor;

            var func = CreateDestructorSignature(type);
            mDestructorMap[type] = func;
            return func;
        }

        private LLVMValueRef CreateDestructorSignature(CheezType type)
        {
            var llvmType = LLVM.FunctionType(
                LLVM.VoidType(), new LLVMTypeRef[] { CheezTypeToLLVMType(PointerType.GetPointerType(type, true)) }, false);
            var func = module.AddFunction($"{type}.dtor.che", llvmType);

            // set attributes
            func.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
            func.AddFunctionAttribute(context, LLVMAttributeKind.NoUnwind);

            return func;
        }

        private void GenerateDestructors()
        {
            foreach (var kv in mDestructorMap)
            {
                GenerateDestructors(kv.Key, kv.Value);
            }
        }

        private void GenerateDestructors(CheezType type, LLVMValueRef func)
        {
            currentLLVMFunction = func;

            var self = func.GetParam(0);
            var builder = new IRBuilder();
            var entry = func.AppendBasicBlock("entry");

            builder.PositionBuilderAtEnd(entry);


            // call drop func
            var dropFunc = workspace.GetDropFuncForType(type);
            if (dropFunc != null)
            {
                var llvmDropFunc = valueMap[dropFunc];
                builder.CreateCall(llvmDropFunc, new LLVMValueRef[] { self }, "");
            }

            // call destructors for members if struct or enum or tuple
            switch (type)
            {
                case StructType @struct:
                    GenerateDestructorStruct(@struct, builder, self);
                    break;

                case EnumType @enum:
                    GenerateDestructorEnum(@enum, builder, self);
                    break;
            }

            builder.CreateRetVoid();
            builder.Dispose();
        }

        private void GenerateDestructorEnum(EnumType @enum, IRBuilder builder, LLVMValueRef self)
        {
            var bbEnd = currentLLVMFunction.AppendBasicBlock("end");
            var bbElse = currentLLVMFunction.AppendBasicBlock("else");

            var tag = builder.CreateStructGEP(self, 0, "tag.ptr");
            tag = builder.CreateLoad(tag, "tag");
            var match = builder.CreateSwitch(tag, bbElse, (uint)@enum.Declaration.Members.Count);
            
            foreach (var m in @enum.Declaration.Members)
            {
                var memberTag = CheezValueToLLVMValue(@enum.Declaration.TagType, m.Value);
                var bbCase = currentLLVMFunction.AppendBasicBlock($"case {m.Name}");
                match.AddCase(memberTag, bbCase);

                builder.PositionBuilderAtEnd(bbCase);

                if (m.AssociatedType != null && workspace.TypeHasDestructor(m.AssociatedType))
                {
                    var memDtor = GetDestructor(m.AssociatedType);
                    var memPtr = builder.CreateStructGEP(self, 1, "");
                    memPtr = builder.CreatePointerCast(memPtr, CheezTypeToLLVMType(m.AssociatedType).GetPointerTo(), "");
                    UpdateStackTracePosition(m.Location);
                    builder.CreateCall(memDtor, new LLVMValueRef[] { memPtr }, "");
                }

                builder.CreateBr(bbEnd);
            }

            builder.PositionBuilderAtEnd(bbElse);
            //CreateExit($"[{@enum.Declaration.Location.Beginning}] Enum tag invalid", 69);
            builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbEnd);
        }

        private void GenerateDestructorStruct(StructType type, IRBuilder builder, LLVMValueRef self)
        {
            foreach (var mem in type.Declaration.Members)
            {
                var memType = mem.Type;

                if (workspace.TypeHasDestructor(memType))
                {
                    var memDtor = GetDestructor(memType);
                    var memPtr = builder.CreateStructGEP(self, (uint)mem.Index, "");
                    UpdateStackTracePosition(mem.Location);
                    builder.CreateCall(memDtor, new LLVMValueRef[] { memPtr }, "");
                }
            }
        }

        // fat function stuff
        private LLVMValueRef CreateFatFuncHelper(FunctionType type)
        {
            // TODO: cache functions

            var paramTypes = new List<LLVMTypeRef>() { CheezTypeToLLVMType(type) };
            paramTypes.AddRange(type.Parameters.Select(p => CheezTypeToLLVMType(p.type)));

            var llvmType = LLVM.FunctionType(CheezTypeToLLVMType(type.ReturnType), paramTypes.ToArray(), false);
            var func = module.AddFunction($"{type}.ffh.che", llvmType);

            var builder = new IRBuilder();
            builder.PositionBuilderAtEnd(func.AppendBasicBlock("entry"));

            // call func
            var funcArg = func.GetParam(0);
            var args = func.GetParams().Skip(1).ToArray();
            var res = builder.CreateCall(funcArg, args, "");

            // return
            if (type.ReturnType == CheezType.Void)
                builder.CreateRetVoid();
            else
                builder.CreateRet(res);

            builder.Dispose();

            return func;
        }

        private void InitTypeInfoLLVMTypes()
        {
            sTypeInfoAttribute          = workspace.GlobalScope.GetStruct("TypeInfoAttribute").StructType;
            sTypeInfo                   = workspace.GlobalScope.GetTrait("TypeInfo").TraitType;
            sTypeInfoInt                = workspace.GlobalScope.GetStruct("TypeInfoInt").StructType;
            sTypeInfoVoid               = workspace.GlobalScope.GetStruct("TypeInfoVoid").StructType;
            sTypeInfoFloat              = workspace.GlobalScope.GetStruct("TypeInfoFloat").StructType;
            sTypeInfoBool               = workspace.GlobalScope.GetStruct("TypeInfoBool").StructType;
            sTypeInfoChar               = workspace.GlobalScope.GetStruct("TypeInfoChar").StructType;
            sTypeInfoString             = workspace.GlobalScope.GetStruct("TypeInfoString").StructType;
            sTypeInfoPointer            = workspace.GlobalScope.GetStruct("TypeInfoPointer").StructType;
            sTypeInfoReference          = workspace.GlobalScope.GetStruct("TypeInfoReference").StructType;
            sTypeInfoSlice              = workspace.GlobalScope.GetStruct("TypeInfoSlice").StructType;
            sTypeInfoArray              = workspace.GlobalScope.GetStruct("TypeInfoArray").StructType;
            sTypeInfoTuple              = workspace.GlobalScope.GetStruct("TypeInfoTuple").StructType;
            sTypeInfoFunction           = workspace.GlobalScope.GetStruct("TypeInfoFunction").StructType;
            sTypeInfoAny                = workspace.GlobalScope.GetStruct("TypeInfoAny").StructType;
            sTypeInfoStruct             = workspace.GlobalScope.GetStruct("TypeInfoStruct").StructType;
            sTypeInfoStructMember       = workspace.GlobalScope.GetStruct("TypeInfoStructMember").StructType;
            sTypeInfoTupleMember        = workspace.GlobalScope.GetStruct("TypeInfoTupleMember").StructType;
            sTypeInfoEnum               = workspace.GlobalScope.GetStruct("TypeInfoEnum").StructType;
            sTypeInfoEnumMember         = workspace.GlobalScope.GetStruct("TypeInfoEnumMember").StructType;
            sTypeInfoTrait              = workspace.GlobalScope.GetStruct("TypeInfoTrait").StructType;
            sTypeInfoImplFunction       = workspace.GlobalScope.GetStruct("TypeInfoImplFunction").StructType;
            sTypeInfoTraitFunction      = workspace.GlobalScope.GetStruct("TypeInfoTraitFunction").StructType;
            sTypeInfoTraitImpl          = workspace.GlobalScope.GetStruct("TypeInfoTraitImpl").StructType;
            sTypeInfoType               = workspace.GlobalScope.GetStruct("TypeInfoType").StructType;
            sTypeInfoCode               = workspace.GlobalScope.GetStruct("TypeInfoCode").StructType;

            rttiTypeInfoPtr             = CheezTypeToLLVMType(PointerType.GetPointerType(sTypeInfo, true));
            rttiTypeInfoRef             = CheezTypeToLLVMType(ReferenceType.GetRefType(sTypeInfo, true));
            rttiTypeInfoInt             = CheezTypeToLLVMType(sTypeInfoInt);
            rttiTypeInfoVoid            = CheezTypeToLLVMType(sTypeInfoVoid);
            rttiTypeInfoFloat           = CheezTypeToLLVMType(sTypeInfoFloat);
            rttiTypeInfoBool            = CheezTypeToLLVMType(sTypeInfoBool);
            rttiTypeInfoChar            = CheezTypeToLLVMType(sTypeInfoChar);
            rttiTypeInfoString          = CheezTypeToLLVMType(sTypeInfoString);
            rttiTypeInfoPointer         = CheezTypeToLLVMType(sTypeInfoPointer);
            rttiTypeInfoReference       = CheezTypeToLLVMType(sTypeInfoReference);
            rttiTypeInfoSlice           = CheezTypeToLLVMType(sTypeInfoSlice);
            rttiTypeInfoArray           = CheezTypeToLLVMType(sTypeInfoArray);
            rttiTypeInfoTuple           = CheezTypeToLLVMType(sTypeInfoTuple);
            rttiTypeInfoFunction        = CheezTypeToLLVMType(sTypeInfoFunction);
            rttiTypeInfoAny             = CheezTypeToLLVMType(sTypeInfoAny);
            rttiTypeInfoStruct          = CheezTypeToLLVMType(sTypeInfoStruct);
            rttiTypeInfoStructMember    = CheezTypeToLLVMType(sTypeInfoStructMember);
            rttiTypeInfoTupleMember     = CheezTypeToLLVMType(sTypeInfoTupleMember);
            rttiTypeInfoEnum            = CheezTypeToLLVMType(sTypeInfoEnum);
            rttiTypeInfoEnumMember      = CheezTypeToLLVMType(sTypeInfoEnumMember);
            rttiTypeInfoTrait           = CheezTypeToLLVMType(sTypeInfoTrait);
            rttiTypeInfoImplFunction    = CheezTypeToLLVMType(sTypeInfoImplFunction);
            rttiTypeInfoTraitFunction   = CheezTypeToLLVMType(sTypeInfoTraitFunction);
            rttiTypeInfoTraitImpl       = CheezTypeToLLVMType(sTypeInfoTraitImpl);
            rttiTypeInfoAttribute       = CheezTypeToLLVMType(sTypeInfoAttribute);
            rttiTypeInfoType            = CheezTypeToLLVMType(sTypeInfoType);
            rttiTypeInfoCode            = CheezTypeToLLVMType(sTypeInfoCode);

            rttiTypeInfoStructMemberInitializer = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { voidPointerType }, false);
        }

        private void CreateStructMemberInitializerFunctions(StructType type)
        {
            foreach (var mem in type.Declaration.Members)
            {
                if (mem.Decl.Initializer == null)
                    continue;

                var funcType = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { CheezTypeToLLVMType(mem.Type).GetPointerTo() }, false);
                var func = module.AddFunction($"init.{type.Declaration.Name}.{mem.Name}.che", funcType);

                Debug.Assert(!valueMap.ContainsKey(mem));
                valueMap[mem] = func;
            }
        }

        private void FinishStructMemberInitializers()
        {
            var builderPrev = builder;
            builder = new IRBuilder();
            foreach (var type in workspace.TypesRequiredAtRuntime)
            {
                if (type is StructType str)
                    FinishStructMemberInitializerFunctions(str);
            }
            builder.Dispose();
            builder = builderPrev;
        }

        private void FinishStructMemberInitializerFunctions(StructType type)
        {
            foreach (var mem in type.Declaration.Members)
            {
                if (mem.Decl.Initializer == null)
                    continue;

                var func = valueMap[mem];
                currentLLVMFunction = func;

                var bb = func.AppendBasicBlock("entry");
                builder.PositionBuilderAtEnd(bb);

                var value = GenerateExpression(mem.Decl.Initializer, true);
                var memberPtr = func.GetParam(0);
                builder.CreateStore(value, memberPtr);
                builder.CreateRetVoid();
            }
        }

        private void GenerateTypeInfos()
        {
            // create globals
            foreach (var type in workspace.TypesRequiredAtRuntime)
            {
                var (sType, rttiType) = type switch
                {
                    VoidType t      => (sTypeInfoVoid,      rttiTypeInfoVoid),
                    StructType t    => (sTypeInfoStruct,    rttiTypeInfoStruct),
                    EnumType t      => (sTypeInfoEnum,      rttiTypeInfoEnum),
                    TraitType t     => (sTypeInfoTrait,     rttiTypeInfoTrait),
                    IntType t       => (sTypeInfoInt,       rttiTypeInfoInt),
                    FloatType t     => (sTypeInfoFloat,     rttiTypeInfoFloat),
                    BoolType t      => (sTypeInfoBool,      rttiTypeInfoBool),
                    CharType t      => (sTypeInfoChar,      rttiTypeInfoChar),
                    StringType t    => (sTypeInfoString,    rttiTypeInfoString),
                    PointerType t   => (sTypeInfoPointer,   rttiTypeInfoPointer),
                    ReferenceType t => (sTypeInfoReference, rttiTypeInfoReference),
                    SliceType t     => (sTypeInfoSlice,     rttiTypeInfoSlice),
                    ArrayType t     => (sTypeInfoArray,     rttiTypeInfoArray),
                    TupleType t     => (sTypeInfoTuple,     rttiTypeInfoTuple),
                    FunctionType t  => (sTypeInfoFunction,  rttiTypeInfoFunction),
                    AnyType t       => (sTypeInfoAny,       rttiTypeInfoAny),
                    CheezTypeType t => (sTypeInfoType,      rttiTypeInfoType),
                    CodeType t      => (sTypeInfoCode,      rttiTypeInfoCode),
                    _ => throw new NotImplementedException(type.ToString()),
                };
                var global = module.AddGlobal(rttiType, $"ti.{type}");
                global.SetInitializer(GetZeroInitializer(rttiType));

                typeInfoTable[type] = (global, vtableMap[GetTraitImpl(sTypeInfo, sType)]);

                // if its a struct, create functions for default values of members
                if (type is StructType str)
                    CreateStructMemberInitializerFunctions(str);
            }
        }

        private LLVMValueRef GetZeroInitializer(LLVMTypeRef rttiType)
        {
            return rttiType.TypeKind switch {
                LLVMTypeKind.LLVMStructTypeKind => 
                    LLVM.ConstNamedStruct(rttiType, rttiType.GetStructElementTypes().Select(t => GetZeroInitializer(t)).ToArray()),

                LLVMTypeKind.LLVMPointerTypeKind => LLVM.ConstPointerNull(rttiType),
                LLVMTypeKind.LLVMIntegerTypeKind => LLVM.ConstInt(rttiType, 0, false),
                LLVMTypeKind.LLVMFloatTypeKind => LLVM.ConstReal(rttiType, 0),

                _ => throw new NotImplementedException(),
            };
        }

        private void SetTypeInfos()
        {
            // size of 
            uint offset = (uint)sTypeInfo.Declaration.Members.Count;

            // set values
            foreach (var type in workspace.TypesRequiredAtRuntime)
            {
                var (global, _) = typeInfoTable[type];

                // create trait impl array
                var traitImplSlice = CreateSlice(
                    sTypeInfoTraitImpl,
                    $"ti.{type}.trait_impls",
                    workspace.TypeTraitMap.GetValueOrDefault(type)?.Select(impl => GenerateRTTIForTraitImpl(type, impl))
                );
                // create impl function array
                var implFunctionSlice = CreateSlice(
                    sTypeInfoImplFunction,
                    $"ti.{type}.impls_functions",
                    workspace
                            .GetImplsForType(type)
                            .Select(impl => impl.Functions)
                            .SelectMany(list => list)
                            .Where(func => valueMap.ContainsKey(func))
                            .Select(func => LLVM.ConstNamedStruct(rttiTypeInfoImplFunction, new LLVMValueRef[] {
                        RTTITypeInfoAsPtr(func.FunctionType),
                        CheezValueToLLVMValue(CheezType.String, func.Name),
                        LLVM.ConstPointerCast(valueMap[func], LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[0], false).GetPointerTo())
                    }))
                );

                builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)type.GetSize(), true), builder.CreateStructGEP(global, 0, "ti.size.ptr"));
                builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)type.GetAlignment(), true), builder.CreateStructGEP(global, 1, "ti.align.ptr"));
                builder.CreateStore(traitImplSlice, builder.CreateStructGEP(global, 2, "ti.traits.ptr"));
                builder.CreateStore(implFunctionSlice, builder.CreateStructGEP(global, 3, "ti.functions.ptr"));
                
                switch (type)
                {
                    case VoidType _:
                    case FloatType _:
                    case BoolType _:
                    case StringType _:
                    case CharType _:
                    case AnyType _:
                    case CheezTypeType _:
                    case CodeType _:
                        break;
                    
                    // @todo
                    case FunctionType f:
                        var paramTypes = CreateSlice(
                            PointerType.GetPointerType(sTypeInfo, true),
                            $"ti.function.{f}.members",
                            f.Parameters.Select(m => RTTITypeInfoAsPtr(m.type))
                        );
                        builder.CreateStore(
                            RTTITypeInfoAsPtr(f.ReturnType),
                            builder.CreateStructGEP(global, offset + 0, ""));

                        builder.CreateStore(
                            paramTypes,
                            builder.CreateStructGEP(global, offset + 1, ""));
                        break;

                    case TupleType t: {
                        var llvmType = CheezTypeToLLVMType(t);

                        var members = CreateSlice(
                            sTypeInfoTupleMember,
                            $"ti.tuple.{t}.members",
                            t.Members.Select((m, i) =>
                                GenerateRTTIForTupleMember(
                                    m,
                                    (ulong)i,
                                    LLVM.OffsetOfElement(targetData, llvmType, (uint)i)))
                        );
                        builder.CreateStore(
                            members,
                            builder.CreateStructGEP(global, offset + 0, ""));
                        break;
                    }

                    case IntType i:
                        {
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int1Type(), i.Signed ? 1ul : 0ul, false),
                                builder.CreateStructGEP(global, offset + 0, "ti.kind.type_info_int.ptr"));
                            break;
                        }

                    case PointerType p:
                        {
                            // target
                            builder.CreateStore(
                                RTTITypeInfoAsPtr(p.TargetType),
                                builder.CreateStructGEP(global, offset + 0, ""));
                            // is_mut
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int1Type(), 1ul, false),
                                builder.CreateStructGEP(global, offset + 1, ""));
                            // is_fat
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int1Type(), p.IsFatPointer ? 1ul : 0ul, false),
                                builder.CreateStructGEP(global, offset + 2, ""));
                            break;
                        }

                    case ReferenceType p:
                        {
                            // target
                            builder.CreateStore(
                                RTTITypeInfoAsPtr(p.TargetType),
                                builder.CreateStructGEP(global, offset + 0, ""));
                            // is_mut
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int1Type(), 1ul, false),
                                builder.CreateStructGEP(global, offset + 1, ""));
                            // is_fat
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int1Type(), p.IsFatReference ? 1ul : 0ul, false),
                                builder.CreateStructGEP(global, offset + 2, ""));
                            break;
                        }

                    case SliceType p:
                        {
                            builder.CreateStore(
                                RTTITypeInfoAsPtr(p.TargetType),
                                builder.CreateStructGEP(global, offset + 0, ""));
                            break;
                        }

                    case ArrayType p:
                        {
                            builder.CreateStore(
                                LLVM.ConstInt(LLVM.Int64Type(), ((NumberData)p.Length).ToUlong(), true),
                                builder.CreateStructGEP(global, offset + 0, ""));
                            builder.CreateStore(
                                RTTITypeInfoAsPtr(p.TargetType),
                                builder.CreateStructGEP(global, offset + 1, ""));
                            break;
                        }

                    case StructType s:
                        GenerateRTTIForStruct(offset, s, global);
                        break;

                    case EnumType e:
                        GenerateRTTIForEnum(offset, e, global);
                        break;

                    case TraitType t:
                        GenerateRTTIForTrait(offset, t, global);
                        break;


                    default: throw new NotImplementedException();
                }
            }
        }

        private void GenerateRTTIForEnum(uint offset, EnumType enumType, LLVMValueRef assPtr)
        {
            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoEnum.GetPointerTo(), "ti.kind.type_info_enum.ptr");

            // create trait function array
            var memberSlice = CreateSlice(
                sTypeInfoEnumMember,
                $"ti.{enumType.Declaration.Name}.members",
                enumType.Declaration.Members.Select(m => GenerateRTTIForEnumMember(enumType, m))
            );

            // create type info
            // name
            builder.CreateStore(
               CheezValueToLLVMValue(CheezType.String, enumType.Declaration.Name),
               builder.CreateStructGEP(ptr, offset + 0, ""));
            // [] members
            builder.CreateStore(
               memberSlice,
               builder.CreateStructGEP(ptr, offset + 1, ""));
            // tag_type
            builder.CreateStore(
               RTTITypeInfoAsPtr(enumType.Declaration.TagType),
               builder.CreateStructGEP(ptr, offset + 2, ""));
        }

        private LLVMValueRef GenerateRTTIForEnumMember(EnumType enumType, AstEnumMemberNew mem)
        {
            // create directives
            var attributes = CreateSlice(
                sTypeInfoAttribute,
                $"ti.{enumType.Declaration.Name}.member.{mem.Name}.attributes",
                mem.Decl.Directives.Select(dir => GenerateRTTIForAttribute(dir))
            );
            return LLVM.ConstNamedStruct(rttiTypeInfoEnumMember, new LLVMValueRef[]
            {
                CheezValueToLLVMValue(CheezType.String, mem.Name),
                mem.AssociatedType != null ? RTTITypeInfoAsPtr(mem.AssociatedType) : CreateNullFatPtr(PointerType.GetPointerType(sTypeInfo, true)),
                LLVM.ConstInt(LLVM.Int64Type(), mem.Value.ToUlong(), false),
                attributes
            });
        }

        private void GenerateRTTIForTrait(uint offset, TraitType traitType, LLVMValueRef assPtr)
        {
            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoTrait.GetPointerTo(), "ti.kind.type_info_trait.ptr");

            // create trait function array
            var functions = CreateSlice(
                sTypeInfoTraitFunction,
                $"ti.{traitType.Declaration.Name}.functions",
                traitType.Declaration.Functions.Select(m => LLVM.ConstNamedStruct(rttiTypeInfoTraitFunction, new LLVMValueRef[]
                {
                    CheezValueToLLVMValue(CheezType.String, m.Name),
                    LLVM.ConstInt(LLVM.Int64Type(), vtableOffsets[m], false)
                }))
            );

            // create type info
            // name
            builder.CreateStore(
               CheezValueToLLVMValue(CheezType.String, traitType.Declaration.Name),
               builder.CreateStructGEP(ptr, offset + 0, ""));
               
            // functions
            builder.CreateStore(
               functions,
               builder.CreateStructGEP(ptr, offset + 1, ""));
        }

        private void GenerateRTTIForStruct(uint offset, StructType s, LLVMValueRef ptr)
        {
            var memberSlice = CreateSlice(
                sTypeInfoStructMember,
                $"ti.{s.Name}.members",
                s.Declaration.Members.Select(m => GenerateRTTIForStructMember(s, m))
            );

            // create type info
            // name
            builder.CreateStore(
               CheezValueToLLVMValue(CheezType.String, s.Name),
               builder.CreateStructGEP(ptr, offset + 0, ""));
            // [] members
            builder.CreateStore(
               memberSlice,
               builder.CreateStructGEP(ptr, offset + 1, ""));
        }

        private LLVMValueRef GenerateRTTIForStructMember(StructType s, AstStructMemberNew m)
        {
            // create directives
            var attributes = CreateSlice(
                sTypeInfoAttribute,
                $"ti.{s.Name}.member.{m.Name}.attributes",
                m.Decl.Directives.Select(dir => GenerateRTTIForAttribute(dir))
            );

            //
            var default_value = m.Decl.Initializer != null && m.Decl.Initializer.IsCompTimeValue ?
                GenerateRTTIForAny(m.Decl.Initializer) :
                GenerateRTTIForNullAny();

            //
            var off = LLVM.OffsetOfElement(targetData, CheezTypeToLLVMType(s), (uint)m.Index);
            off = (ulong)m.Offset;
            var initializer = valueMap.GetValueOrDefault(m, LLVM.ConstNull(rttiTypeInfoStructMemberInitializer.GetPointerTo()));

            //return LLVM.GetUndef(rttiTypeInfoStructMember);
            return LLVM.ConstNamedStruct(rttiTypeInfoStructMember, new LLVMValueRef[]
            {
                LLVM.ConstInt(LLVM.Int64Type(), (ulong)m.Index, true),  // index
                LLVM.ConstInt(LLVM.Int64Type(), off, true),             // offset
                CheezValueToLLVMValue(CheezType.String, m.Name),        // name
                RTTITypeInfoAsPtr(m.Type),                              // typ
                default_value,                                          // default value
                LLVM.ConstPointerCast(initializer, rttiTypeInfoStructMemberInitializer.GetPointerTo()), // initializer
                attributes                                              // attributes
            });
        }

        private LLVMValueRef GenerateRTTIForTupleMember((string name, CheezType type) mem, ulong index, ulong offset)
        {
            // create directives

            //return LLVM.GetUndef(rttiTypeInfoStructMember);
            return LLVM.ConstNamedStruct(rttiTypeInfoTupleMember, new LLVMValueRef[]
            {
                LLVM.ConstInt(LLVM.Int64Type(), index, true),   // index
                LLVM.ConstInt(LLVM.Int64Type(), offset, true),  // offset
                RTTITypeInfoAsPtr(mem.type),                    // typ
            });
        }

        private LLVMValueRef GenerateRTTIForTraitImpl(CheezType type, AstImplBlock impl)
        {
            var vtablePtr = vtableMap[impl];

            return LLVM.ConstNamedStruct(rttiTypeInfoTraitImpl, new LLVMValueRef[]
            {
                RTTITypeInfoAsPtr(impl.Trait),
                LLVM.ConstPointerCast(vtablePtr, LLVM.Int8Type().GetPointerTo()),
            });
        }

        private LLVMValueRef GenerateRTTIForAttribute(AstDirective dir)
        {
            var arguments = CreateSlice(PointerType.GetPointerType(CheezType.Any, true),
                $"ti.{dir.Name}.args",
                dir.Arguments.Select(arg => GenerateRTTIForAny(arg)));

            return LLVM.ConstNamedStruct(rttiTypeInfoAttribute, new LLVMValueRef[]
            {
                CheezValueToLLVMValue(CheezType.String, dir.Name.Name),
                arguments
            });
        }

        private LLVMValueRef GenerateRTTIForNullAny()
        {
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(PointerType.GetPointerType(CheezType.Any, true)), new LLVMValueRef[]
            {
                GetZeroInitializer(LLVM.Int8Type().GetPointerTo()),
                GetZeroInitializer(rttiTypeInfoPtr)
            });
        }

        private LLVMValueRef GenerateRTTIForAny(AstExpression expr)
        {
            var valueGlobal = module.AddGlobal(CheezTypeToLLVMType(expr.Type), "any");

            LLVMValueRef init = default;
            if (expr is AstEnumValueExpr ev)
            {
                if (ev.Member.AssociatedTypeExpr != null)
                    throw new Exception("ev.Member.AssociatedTypeExpr != null");
                init = LLVM.ConstNamedStruct(CheezTypeToLLVMType(expr.Type), new LLVMValueRef[]
                {
                    CheezValueToLLVMValue(ev.EnumDecl.TagType, ev.Member.Value),
                    LLVM.GetUndef(LLVM.ArrayType(LLVM.Int8Type(), (uint)(ev.EnumDecl.EnumType.GetSize() - ev.EnumDecl.TagType.GetSize())))
                });
            }
            else
            {
                init = CheezValueToLLVMValue(expr.Type, expr.Value);
            }
            valueGlobal.SetInitializer(init);
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(PointerType.GetPointerType(CheezType.Any, true)), new LLVMValueRef[]
            {
                LLVM.ConstPointerCast(valueGlobal, voidPointerType),
                RTTITypeInfoAsPtr(expr.Type),
            });
        }


        LLVMValueRef CreateSlice(CheezType targetType, string name, IEnumerable<LLVMValueRef> data)
        {
            int length = data != null ? data.Count() : 0;
            var dataArrayLLVM = data != null ? data.ToArray() : Array.Empty<LLVMValueRef>();

            var targetTypeLLVM = CheezTypeToLLVMType(targetType);
            var dataGlobalArray = module.AddGlobal(LLVM.ArrayType(targetTypeLLVM, (uint)length), name);
            dataGlobalArray.SetInitializer(LLVM.ConstArray(targetTypeLLVM, dataArrayLLVM));

            var resultSlice = CreateSliceHelper(targetType, length, dataGlobalArray);
            return resultSlice;
        }

        private LLVMValueRef CreateSliceHelper(CheezType targetType, int length, LLVMValueRef data)
        {
            var targetTypeLLVM = CheezTypeToLLVMType(SliceType.GetSliceType(targetType, true));
            var elemTypes = targetTypeLLVM.GetStructElementTypes();
            return LLVM.ConstNamedStruct(targetTypeLLVM, new LLVMValueRef[]
            {
                LLVM.ConstInt(LLVM.Int64Type(), (ulong)length, true),
                LLVM.ConstPointerCast(data, elemTypes[1]),
            });
        }
        
        private LLVMValueRef RTTITypeInfoAsPtr(CheezType type)
        {
            var (typeInfo, vtable) = typeInfoTable[type];
            return CreateConstFatPtr(PointerType.GetPointerType(sTypeInfo, true), typeInfo, vtable);
        }

        private LLVMValueRef CreateConstFatPtr(PointerType ptrType, LLVMValueRef value, LLVMValueRef vtable)
        {
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(ptrType), new LLVMValueRef[] {
                LLVM.ConstPointerCast(value, voidPointerType),
                LLVM.ConstPointerCast(vtable, voidPointerType)
            });
        }

        private LLVMValueRef CreateFatRef(ReferenceType refType, LLVMValueRef value, LLVMValueRef vtable)
        {
            var result = LLVM.GetUndef(CheezTypeToLLVMType(refType));
            result = builder.CreateInsertValue(result, builder.CreatePointerCast(value, voidPointerType, ""), 0, "");
            result = builder.CreateInsertValue(result, builder.CreatePointerCast(vtable, voidPointerType, ""), 1, "");
            return result;
        }

        private LLVMValueRef CreateNullFatPtr(PointerType ptrType)
        {
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(ptrType), new LLVMValueRef[] {
                LLVM.ConstPointerNull(LLVM.Int8Type()),
                LLVM.ConstPointerNull(LLVM.Int8Type())
            });
        }

        private LLVMValueRef CreateFatPtr(PointerType ptrType, LLVMValueRef value, LLVMValueRef vtable)
        {
            var result = LLVM.GetUndef(CheezTypeToLLVMType(ptrType));
            result = builder.CreateInsertValue(result, builder.CreatePointerCast(value, voidPointerType, ""), 0, "");
            result = builder.CreateInsertValue(result, builder.CreatePointerCast(vtable, voidPointerType, ""), 1, "");
            return result;
        }

        private LLVMValueRef CreateLLVMSlice(CheezType sliceType, LLVMValueRef value, LLVMValueRef length)   
        {
            var result = LLVM.GetUndef(CheezTypeToLLVMType(sliceType));
            result = builder.CreateInsertValue(result, length, 0, "");
            result = builder.CreateInsertValue(result, value, 1, "");
            return result;
        }

        private LLVMValueRef GetTraitPtr(LLVMValueRef value)
        {
            return builder.CreateExtractValue(value, 0, "ptr");
        }

        private LLVMValueRef GetTraitVtable(LLVMValueRef value)
        {
            return builder.CreateExtractValue(value, 1, "vtable");
        }

        private LLVMValueRef GetAnyPtr(LLVMValueRef value)
        {
            return builder.CreateExtractValue(value, 0, "ptr");
        }

        private LLVMValueRef GetAnyTypeInfo(LLVMValueRef value)
        {
            return builder.CreateExtractValue(value, 1, "type_info");
        }

        private LLVMValueRef Deref(LLVMValueRef value, CheezType type)
        {
            return type switch
            {
                PointerType p when p.IsFatPointer => throw new Exception(),
                ReferenceType p when p.IsFatReference => throw new Exception(),

                _ => builder.CreateLoad(value, "")
            };
        }
    }
}
