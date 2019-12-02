using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var currentTop = CreateLocalVariable(LLVM.PointerType(stackTraceType, 0), "stack_trace.print.current");
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
            builder.CreateCall(exitThread, new LLVMValueRef[] {
                LLVM.ConstInt(LLVM.Int32Type(), (uint)exitCode, true)
            }, "");
        }

        private LLVMValueRef GenerateIntrinsicDeclaration(string name, LLVMTypeRef retType, params LLVMTypeRef[] paramTypes)
        {
            var ltype = LLVM.FunctionType(retType, paramTypes, false);
            var lfunc = module.AddFunction(name, ltype);
            return lfunc;
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
            return CreateLocalVariable(CheezTypeToLLVMType(exprType), name);
        }

        private LLVMValueRef CreateLocalVariable(LLVMTypeRef type, string name = "temp")
        {
            var builder = new IRBuilder();

            var bb = currentLLVMFunction.GetFirstBasicBlock();
            var brInst = bb.GetLastInstruction();
            if (brInst.Pointer.ToInt64() == 0)
                builder.PositionBuilderAtEnd(bb);
            else
                builder.PositionBuilderBefore(brInst);

            var result = builder.CreateAlloca(type, name);
            var alignment = targetData.AlignmentOfType(type);
            result.SetAlignment(alignment);

            builder.Dispose();
            return result;
        }

        private LLVMTypeRef FuncTypeToLLVMType(FunctionType f)
        {
            var paramTypes = f.Parameters.Select(rt => CheezTypeToLLVMType(rt.type)).ToList();
            var returnType = CheezTypeToLLVMType(f.ReturnType);
            return LLVMTypeRef.FunctionType(returnType, paramTypes.ToArray(), f.VarArgs);
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
                        var str = LLVM.StructCreateNamed(context, t.ToString());
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int8Type().GetPointerTo(),
                            LLVM.Int8Type().GetPointerTo()
                        }, false);
                        return str;
                    }

                case AnyType a:
                    {
                        var str = LLVM.StructCreateNamed(context, "any");
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                                CheezTypeToLLVMType(PointerType.GetPointerType(workspace.GlobalScope.GetStruct("TypeInfo").StructType)),
                                LLVM.Int8Type().GetPointerTo()
                            }, false);
                        return str;
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
                    if (p.TargetType == VoidType.Intance)
                        return LLVM.Int8Type().GetPointerTo();
                    return CheezTypeToLLVMType(p.TargetType).GetPointerTo();

                case ArrayType a:
                    return LLVMTypeRef.ArrayType(CheezTypeToLLVMType(a.TargetType), (uint)a.Length);

                case SliceType s:
                    {
                        var str = LLVM.StructCreateNamed(context, s.ToString());
                        LLVM.StructSetBody(str, new LLVMTypeRef[] {
                            LLVM.Int64Type(),
                            CheezTypeToLLVMType(PointerType.GetPointerType(s.TargetType))
                        }, false);
                        return str;
                    }

                case ReferenceType r:
                    return CheezTypeToLLVMType(PointerType.GetPointerType(r.TargetType));

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

                        var memTypes = s.Declaration.Members.Select(m => CheezTypeToLLVMType(m.Type)).ToArray();
                        LLVM.StructSetBody(llvmType, memTypes, false);
                        return llvmType;
                    }

                case TupleType t:
                    {
                        var memTypes = t.Members.Select(m => CheezTypeToLLVMType(m.type)).ToArray();
                        return LLVM.StructType(memTypes, false);
                    }

                case SelfType self:
                    return CheezTypeToLLVMType(self.traitType);

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
                    throw new NotImplementedException();
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

                case FunctionType f when f.Declaration != null:
                    return valueMap[f.Declaration];

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
                            glob
                        });
                    }
                    if (type == CheezType.CString && v is string)
                    {
                        var s = v as string;
                        return builder.CreateGlobalStringPtr(s, "");
                    }

                    if (type is PointerType p)
                    {
                        var val = LLVM.ConstInt(LLVM.Int64Type(), ((NumberData)v).ToUlong(), false);
                        var t = CheezTypeToLLVMType(p);
                        return LLVM.ConstIntToPtr(val, t);
                    }

                    throw new NotImplementedException();
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
                new LLVMValueRef[a.Length].Populate(GetDefaultLLVMValue(a.TargetType))),

            _ => throw new NotImplementedException()
        };

        private void GenerateVTables()
        {
            // create vtable type
            foreach (var trait in workspace.Traits)
            {
                var funcTypes = new List<LLVMTypeRef>();
                foreach (var v in trait.Variables)
                {
                    vtableIndices[v] = funcTypes.Count;
                    funcTypes.Add(LLVM.Int64Type());
                }
                foreach (var func in trait.Functions)
                {
                    if (func.ExcludeFromVTable)
                        continue;

                    if (func.IsGeneric)
                    {
                        throw new NotImplementedException();
                    }
                    else if (func.SelfType == SelfParamType.Reference)
                    {
                        vtableIndices[func] = funcTypes.Count;

                        var funcType = CheezTypeToLLVMType(func.Type);
                        funcTypes.Add(funcType);
                    }
                }

                var vtableType = LLVM.StructCreateNamed(context, $"__vtable_type_{trait.TraitType}");
                LLVM.StructSetBody(vtableType, funcTypes.ToArray(), false);
                vtableTypes[trait.TraitType] = vtableType;
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
                var traits = kv.Value;

                foreach (var impl in traits)
                {
                    var trait = impl.Trait;
                    var vtableType = vtableTypes[trait];
                    var vfuncTypes = LLVM.GetStructElementTypes(vtableType);
                    var vfuncCount = vfuncTypes.Length;


                    var functions = new LLVMValueRef[vfuncCount];
                    for (int i = 0; i < functions.Length; i++)
                    {
                        var funcType = vfuncTypes[i];
                        functions[i] = LLVM.ConstNull(funcType);
                    }

                    if (impl.TargetType is StructType str && impl.Trait.Declaration.Variables.Count > 0)
                    {
                        var strType = CheezTypeToLLVMType(str);
                        foreach (var v in impl.Trait.Declaration.Variables)
                        {
                            var mem = str.Declaration.Members.First(m => m.Name == v.Name.Name);
                            var offset = LLVM.OffsetOfElement(targetData, strType, (uint)mem.Index);
                            var index = vtableIndices[v];
                            functions[index] = LLVM.ConstInt(LLVM.Int64Type(), offset, false);
                        }
                    }

                    foreach (var func in impl.Functions)
                    {
                        var traitFunc = func.TraitFunction;
                        if (traitFunc == null || func.SelfType != SelfParamType.Reference)
                            continue;
                        if (traitFunc.ExcludeFromVTable)
                            continue;

                        var index = vtableIndices[traitFunc];
                        functions[index] = valueMap[func];
                    }

                    var defValue = LLVM.ConstNamedStruct(vtableType, functions);

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
                LLVM.VoidType(), new LLVMTypeRef[] { CheezTypeToLLVMType(PointerType.GetPointerType(type)) }, false);
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
            }

            builder.CreateRetVoid();
            builder.Dispose();
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

        private void GenerateTypeInfos()
        {
            sTypeInfo = workspace.GlobalScope.GetStruct("TypeInfo").StructType;
            sTypeInfoKind = workspace.GlobalScope.GetEnum("TypeInfoKind").EnumType;
            sTypeInfoInt = workspace.GlobalScope.GetStruct("TypeInfoInt").StructType;
            sTypeInfoStruct = workspace.GlobalScope.GetStruct("TypeInfoStruct").StructType;
            sTypeInfoStructMember = workspace.GlobalScope.GetStruct("TypeInfoStructMember").StructType;
            sTypeInfoEnum = workspace.GlobalScope.GetStruct("TypeInfoEnum").StructType;
            sTypeInfoEnumMember = workspace.GlobalScope.GetStruct("TypeInfoEnumMember").StructType;
            sTypeInfoTrait = workspace.GlobalScope.GetStruct("TypeInfoTrait").StructType;
            sTypeInfoTraitFunction = workspace.GlobalScope.GetStruct("TypeInfoTraitFunction").StructType;
            sTypeInfoTraitImpl = workspace.GlobalScope.GetStruct("TypeInfoTraitImpl").StructType;
            sTypeInfoAttribute = workspace.GlobalScope.GetStruct("TypeInfoAttribute").StructType;

            rttiTypeInfo = CheezTypeToLLVMType(sTypeInfo);
            rttiTypeInfoKind = CheezTypeToLLVMType(sTypeInfoKind);
            rttiTypeInfoInt = CheezTypeToLLVMType(sTypeInfoInt);
            rttiTypeInfoStruct = CheezTypeToLLVMType(sTypeInfoStruct);
            rttiTypeInfoStructMember = CheezTypeToLLVMType(sTypeInfoStructMember);
            rttiTypeInfoEnum = CheezTypeToLLVMType(sTypeInfoEnum);
            rttiTypeInfoEnumMember = CheezTypeToLLVMType(sTypeInfoEnumMember);
            rttiTypeInfoTrait = CheezTypeToLLVMType(sTypeInfoTrait);
            rttiTypeInfoTraitFunction = CheezTypeToLLVMType(sTypeInfoTraitFunction);
            rttiTypeInfoTraitImpl = CheezTypeToLLVMType(sTypeInfoTraitImpl);
            rttiTypeInfoAttribute = CheezTypeToLLVMType(sTypeInfoAttribute);

            // create globals
            foreach (var type in workspace.TypesRequiredAtRuntime)
            {
                var llvmType = CheezTypeToLLVMType(type);

                var global = module.AddGlobal(rttiTypeInfo, $"ti.{type}");
                global.SetInitializer(LLVM.GetUndef(rttiTypeInfo));

                typeInfoTable[type] = global;
            }
        }

        private void SetTypeInfos()
        {
            // set values
            foreach (var type in workspace.TypesRequiredAtRuntime)
            {
                var llvmType = CheezTypeToLLVMType(type);
                var global = typeInfoTable[type];

                var kind = LLVM.GetUndef(rttiTypeInfoKind);

                builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)type.GetSize(), true), builder.CreateStructGEP(global, 0, "ti.size.ptr"));
                builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)type.GetAlignment(), true), builder.CreateStructGEP(global, 1, "ti.align.ptr"));

                var tag = type switch
                {
                    IntType _ => 0,
                    FloatType _ => 1,
                    BoolType _ => 2,
                    CharType _ => 3,
                    StructType _ => 4,
                    PointerType _ => 5,
                    ReferenceType _ => 6,
                    SliceType _ => 7,
                    EnumType _ => 8,
                    TraitType _ => 9,
                    VoidType _ => 10,
                    _ => throw new NotImplementedException()
                };
                var kindPtr = builder.CreateStructGEP(global, 2, "ti.kind.ptr");
                var tagPtr = builder.CreateStructGEP(kindPtr, 0, "ti.kind.tag.ptr");
                var assPtr = builder.CreateStructGEP(kindPtr, 1, "ti.kind.ass.ptr");
                builder.CreateStore(LLVM.ConstInt(LLVM.Int64Type(), (ulong)tag, true), tagPtr);

                switch (type)
                {
                    case VoidType _:
                    case FloatType _:
                    case BoolType _:
                    case CharType _:
                        break;

                    case IntType i:
                        {
                            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoInt.GetPointerTo(), "ti.kind.type_info_int.ptr");
                            builder.CreateStore(LLVM.ConstNamedStruct(rttiTypeInfoInt, new LLVMValueRef[]
                            {
                                LLVM.ConstInt(LLVM.Int1Type(), (ulong)(i.Signed ? 1 : 0), false)
                            }), ptr);
                            break;
                        }

                    case PointerType p:
                        {
                            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfo.GetPointerTo().GetPointerTo(), "ti.kind.type_info_pointer.ptr");
                            builder.CreateStore(typeInfoTable[p.TargetType], ptr);
                            break;
                        }

                    case ReferenceType p:
                        {
                            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfo.GetPointerTo().GetPointerTo(), "ti.kind.type_info_ref.ptr");
                            builder.CreateStore(typeInfoTable[p.TargetType], ptr);
                            break;
                        }

                    case SliceType p:
                        {
                            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfo.GetPointerTo().GetPointerTo(), "ti.kind.type_info_slice.ptr");
                            builder.CreateStore(typeInfoTable[p.TargetType], ptr);
                            break;
                        }

                    case StructType s:
                        GenerateRTTIForStruct(s, assPtr);
                        break;

                    case EnumType e:
                        GenerateRTTIForEnum(e, assPtr);
                        break;

                    case TraitType t:
                        GenerateRTTIForTrait(t, assPtr);
                        break;


                    default: throw new NotImplementedException();
                }
            }
        }

        private void GenerateRTTIForEnum(EnumType enumType, LLVMValueRef assPtr)
        {
            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoEnum.GetPointerTo(), "ti.kind.type_info_enum.ptr");

            // create trait function array
            var memberSlice = CreateSlice(
                sTypeInfoEnumMember,
                $"ti.{enumType.Declaration.Name}.members",
                enumType.Declaration.Members.Select(m => GenerateRTTIForEnumMember(enumType, m))
            );

            // create type info
            var val = LLVM.ConstNamedStruct(rttiTypeInfoEnum, new LLVMValueRef[]
            {
                CheezValueToLLVMValue(CheezType.String, enumType.Declaration.Name),
                memberSlice,
                typeInfoTable[enumType.Declaration.TagType]
            });
            builder.CreateStore(val, ptr);
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
                mem.AssociatedType != null ? typeInfoTable[mem.AssociatedType] : LLVM.ConstPointerNull(rttiTypeInfo.GetPointerTo()),
                LLVM.ConstInt(LLVM.Int64Type(), mem.Value.ToUlong(), false),
                attributes
            });
        }

        private void GenerateRTTIForTrait(TraitType traitType, LLVMValueRef assPtr)
        {
            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoTrait.GetPointerTo(), "ti.kind.type_info_trait.ptr");

            // create trait function array
            var memberSlice = CreateSlice(
                sTypeInfoStructMember,
                $"ti.{traitType.Declaration.Name}.functions",
                traitType.Declaration.Functions.Select(m => LLVM.ConstNamedStruct(rttiTypeInfoTraitFunction, new LLVMValueRef[]
                {
                    CheezValueToLLVMValue(CheezType.String, m.Name),
                    LLVM.ConstInt(LLVM.Int64Type(), (ulong)vtableIndices[m], false)
                }))
            );

            // create type info
            var val = LLVM.ConstNamedStruct(rttiTypeInfoTrait, new LLVMValueRef[]
            {
                CheezValueToLLVMValue(CheezType.String, traitType.Declaration.Name),
                memberSlice
            });
            builder.CreateStore(val, ptr);
        }

        private void GenerateRTTIForStruct(StructType s, LLVMValueRef assPtr)
        {
            var ptr = builder.CreatePointerCast(assPtr, rttiTypeInfoStruct.GetPointerTo(), "ti.kind.type_info_struct.ptr");

            var memberSlice = CreateSlice(
                sTypeInfoStructMember,
                $"ti.{s.Name}.members",
                s.Declaration.Members.Select(m => GenerateRTTIForStructMember(s, m))
            );

            // create trait impl array
            var traitImplSlice = CreateSlice(
                sTypeInfoTraitImpl,
                $"ti.{s.Name}.trait_impls",
                s.Declaration.Traits.Select(t => GenerateRTTIForTraitImpl(s, t))
            );

            // create type info
            builder.CreateStore(LLVM.ConstNamedStruct(rttiTypeInfoStruct, new LLVMValueRef[]
            {
                CheezValueToLLVMValue(CheezType.String, s.Name),
                memberSlice,
                traitImplSlice
            }), ptr);
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
            var default_value = m.Decl.Initializer != null ?
                GenerateRTTIForAny(m.Decl.Initializer) :
                GenerateRTTIForNullAny();

            //
            var off = LLVM.OffsetOfElement(targetData, CheezTypeToLLVMType(s), (uint)m.Index);
            return LLVM.ConstNamedStruct(rttiTypeInfoStructMember, new LLVMValueRef[]
            {
                LLVM.ConstInt(LLVM.Int64Type(), off, true),
                CheezValueToLLVMValue(CheezType.String, m.Name),
                typeInfoTable[m.Type],
                default_value,
                attributes
            });
        }

        private LLVMValueRef GenerateRTTIForTraitImpl(StructType s, TraitType t)
        {
            var impl = t.Declaration.Implementations[s];
            var vtablePtr = vtableMap[impl];

            return LLVM.ConstNamedStruct(rttiTypeInfoTraitImpl, new LLVMValueRef[]
            {
                typeInfoTable[t],
                vtablePtr
            });
        }

        private LLVMValueRef GenerateRTTIForAttribute(AstDirective dir)
        {
            var arguments = CreateSlice(CheezType.Any,
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
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(CheezType.Any), new LLVMValueRef[]
            {
                LLVM.ConstPointerNull(rttiTypeInfo.GetPointerTo()),
                LLVM.ConstPointerNull(LLVM.Int8Type().GetPointerTo())
            });
        }

        private LLVMValueRef GenerateRTTIForAny(AstExpression expr)
        {
            var valueGlobal = module.AddGlobal(CheezTypeToLLVMType(expr.Type), "any");
            var init = CheezValueToLLVMValue(expr.Type, expr.Value);
            valueGlobal.SetInitializer(init);
            return LLVM.ConstNamedStruct(CheezTypeToLLVMType(CheezType.Any), new LLVMValueRef[]
            {
                typeInfoTable[expr.Type],
                LLVM.ConstPointerCast(valueGlobal, LLVM.Int8Type().GetPointerTo())
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
            var targetTypeLLVM = CheezTypeToLLVMType(SliceType.GetSliceType(targetType));
            return LLVM.ConstNamedStruct(targetTypeLLVM, new LLVMValueRef[]
            {
                LLVM.ConstInt(LLVM.Int64Type(), (ulong)length, true),
                data
            });
        }
    }
}
