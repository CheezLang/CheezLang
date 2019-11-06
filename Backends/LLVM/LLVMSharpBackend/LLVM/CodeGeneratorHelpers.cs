using Cheez.Ast;
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

        private void PushStackTrace(AstFunctionDecl function)
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

            builder.CreateStore(builder.CreateGlobalStringPtr(function.Name.Name, ""), functionNamePointer);
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

        private void PrintStackTrace()
        {
            if (!keepTrackOfStackTrace)
                return;

            builder.CreateCall(printf, new LLVMValueRef[] { builder.CreateGlobalStringPtr("at\n", "") }, "");

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
                LLVMValueRef[] args = { builder.CreateGlobalStringPtr("  %s (%s:%lld:%lld)\n", ""), name, location, line, col };
                builder.CreateCall(printf, args, "");
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
            exit = module.GetNamedFunction("exit");
            if (exit.Pointer.ToInt64() == 0)
                exit = GenerateIntrinsicDeclaration("exit", LLVM.VoidType(), LLVM.Int32Type());

            printf = module.GetNamedFunction("printf");
            if (printf.Pointer.ToInt64() == 0)
            {
                var ltype = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] {
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, true);
                printf = module.AddFunction("printf", ltype);
            }
        }

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

        private void CreateExit(string msg, int exitCode, params LLVMValueRef[] p)
        {
            var args = new List<LLVMValueRef>
            {
                builder.CreateGlobalStringPtr(msg + "\n", "")
            };
            args.AddRange(p);
            var pf = builder.CreateCall(printf, args.ToArray(), "");

            PrintStackTrace();

            builder.CreateCall(exit, new LLVMValueRef[] {
                LLVM.ConstInt(LLVM.Int32Type(), (uint)exitCode, true)
            }, "");
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

        private LLVMValueRef CreateLocalVariable(CheezType exprType, string name = "")
        {
            return CreateLocalVariable(CheezTypeToLLVMType(exprType));
        }

        private LLVMValueRef CreateLocalVariable(LLVMTypeRef type, string name = "")
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
                        return LLVMTypeRef.DoubleType();
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
                            LLVM.Int64Type(),
                            CheezTypeToLLVMType(s.TargetType).GetPointerTo()
                        }, false);
                        return str;
                    }

                case ReferenceType r:
                    return CheezTypeToLLVMType(r.TargetType).GetPointerTo();

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
                        var llvmType = LLVM.StructCreateNamed(context, $"enum.{e}");

                        if (e.Declaration.HasAssociatedTypes)
                        {
                            var restSize = e.Size - e.TagType.Size;
                            llvmType.StructSetBody(new LLVMTypeRef[]
                            {
                                CheezTypeToLLVMType(e.TagType),
                                LLVM.ArrayType(LLVM.Int8Type(), (uint)restSize)
                            }, false);
                        }
                        else
                        {
                            llvmType.StructSetBody(new LLVMTypeRef[]
                            {
                                CheezTypeToLLVMType(e.TagType)
                            }, false);
                        }

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
                case ArrayType arr when arr.TargetType == CheezType.Char && v is string s:
                    return LLVM.ConstArray(CheezTypeToLLVMType(CheezType.Char), s.ToCharArray().Select(c => CheezValueToLLVMValue(CheezType.Char, c)).ToArray());

                default:
                    if (type == CheezType.String)
                    {
                        var s = v as string;
                        return LLVM.ConstNamedStruct(CheezTypeToLLVMType(type), new LLVMValueRef[] {
                            LLVM.ConstInt(LLVM.Int64Type(), (ulong)s.Length, true),
                            LLVM.ConstPointerCast(builder.CreateGlobalStringPtr(s, ""), LLVM.PointerType(LLVM.Int8Type(), 0))
                        });
                    }
                    if (type == CheezType.CString)
                    {
                        var s = v as string;
                        return builder.CreateGlobalStringPtr(s, "");
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
                    if (func.GetFlag(StmtFlags.ExcludeFromVtable))
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

                var vtableType = LLVM.StructCreateNamed(context, $"__vtable_type_{trait.Type}");
                LLVM.StructSetBody(vtableType, funcTypes.ToArray(), false);
                vtableTypes[trait.Type] = vtableType;
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
                    vtableMap[(type, trait)] = vtable;
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
                        if (traitFunc.GetFlag(StmtFlags.ExcludeFromVtable))
                            continue;

                        var index = vtableIndices[traitFunc];
                        functions[index] = valueMap[func];
                    }

                    var defValue = LLVM.ConstNamedStruct(vtableType, functions);

                    var vtable = vtableMap[(type, trait)];
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
                LLVM.VoidType(), new LLVMTypeRef[] { CheezTypeToLLVMType(type).GetPointerTo() }, false);
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
    }
}
