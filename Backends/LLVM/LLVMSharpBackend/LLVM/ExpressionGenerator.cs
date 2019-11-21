using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator : IDisposable
    {
        //[DebuggerStepThrough()]
        private LLVMValueRef GenerateExpression(AstExpression expr, bool deref)
        {
            if (expr.Value != null)
            {
                return CheezValueToLLVMValue(expr.Type, expr.Value);
            }

            switch (expr)
            {
                case AstNullExpr nll: return GenerateNullExpr(nll);
                case AstBoolExpr n: return GenerateBoolExpr(n);
                case AstNumberExpr n: return GenerateNumberExpr(n);
                case AstStringLiteral ch: return GenerateStringLiteralExpr(ch);
                case AstCharLiteral ch: return GenerateCharLiteralExpr(ch);
                case AstIdExpr i: return GenerateIdExpr(i, deref);
                case AstAddressOfExpr ao: return GenerateAddressOf(ao);
                case AstDereferenceExpr de: return GenerateDerefExpr(de, deref);
                case AstTupleExpr t: return GenerateTupleExpr(t);
                case AstStructValueExpr t: return GenerateStructValueExpr(t);
                case AstArgument a: return GenerateArgumentExpr(a);
                case AstDotExpr t: return GenerateDotExpr(t, deref);
                case AstArrayAccessExpr t: return GenerateIndexExpr(t, deref);
                case AstCallExpr c: return GenerateCallExpr(c);
                case AstUnaryExpr u: return GenerateUnaryExpr(u);
                case AstTempVarExpr t: return GenerateTempVarExpr(t, deref);
                case AstSymbolExpr s: return GenerateSymbolExpr(s, deref);
                case AstBlockExpr block: return GenerateBlock(block, deref);
                case AstIfExpr iff: return GenerateIfExpr(iff);
                case AstBinaryExpr bin: return GenerateBinaryExpr(bin);
                case AstCastExpr cast: return GenerateCastExpr(cast);
                case AstUfcFuncExpr ufc: return GenerateUfcFuncExpr(ufc);
                case AstArrayExpr arr: return GenerateArrayExpr(arr, deref);
                case AstDefaultExpr def: return GenerateDefaultExpr(def);
                case AstMatchExpr m: return GenerateMatchExpr(m);
                case AstEnumValueExpr eve: return GenerateEnumValueExpr(eve);
                case AstCompCallExpr cc: return GenerateCompCallExpr(cc);
                case AstLambdaExpr l: return GenerateLambdaExpr(l);
                case AstBreakExpr b: return GenerateBreak(b);
                case AstContinueExpr c: return GenerateContinue(c);
                case AstRangeExpr r: return GenerateRangeExpr(r);
                case AstVariableRef v: return GenerateVariableRefExpr(v, deref);
                case AstConstantRef v: return GenerateConstantRefExpr(v);
            }
            throw new NotImplementedException();
        }

        private LLVMValueRef GenerateVariableRefExpr(AstVariableRef v, bool deref)
        {
            var val = valueMap[v.Declaration];

            if (deref)
                val = builder.CreateLoad(val, "");
            return val;
        }

        private LLVMValueRef GenerateConstantRefExpr(AstConstantRef v)
        {
            return CheezValueToLLVMValue(v.Declaration.Type, v.Declaration.Value);
        }

        private LLVMValueRef GenerateRangeExpr(AstRangeExpr r)
        {
            var from = GenerateExpression(r.From, true);
            var to = GenerateExpression(r.To, true);

            var result = LLVM.GetUndef(CheezTypeToLLVMType(r.Type));
            result = builder.CreateInsertValue(result, from, 0, "");
            result = builder.CreateInsertValue(result, to, 1, "");
            return result;
        }

        private LLVMValueRef GenerateContinue(AstContinueExpr cont)
        {
            // generate destructor calls and deferred statements
            if (cont.Destructions != null)
                foreach (var dest in cont.Destructions)
                    GenerateStatement(dest);

            var postAction = loopBodyMap[cont.Loop];
            builder.CreateBr(postAction);

            var bbNext = LLVM.AppendBasicBlock(currentLLVMFunction, "_cont_next");
            builder.PositionBuilderAtEnd(bbNext);

            return LLVM.GetUndef(LLVM.VoidType());
        }

        private LLVMValueRef GenerateBreak(AstBreakExpr br)
        {
            // generate destructor calls and deferred statements
            if (br.Destructions != null)
                foreach (var dest in br.Destructions)
                    GenerateStatement(dest);

            var end = breakTargetMap[br.Breakable];
            builder.CreateBr(end);

            var bbNext = LLVM.AppendBasicBlock(currentLLVMFunction, "_br_next");
            builder.PositionBuilderAtEnd(bbNext);

            return LLVM.GetUndef(LLVM.VoidType());
        }

        private LLVMValueRef GenerateLambdaExpr(AstLambdaExpr lambda)
        {
            var name = $"lambda.{lambda.Type}.che";

            var funcType = FuncTypeToLLVMType(lambda.FunctionType);
            var func = module.AddFunction(name, funcType);
            var locals = func.AppendBasicBlock("locals");
            var entry = func.AppendBasicBlock("entry");

            var prevBuilder = builder;
            var prevLLVMFunc = currentLLVMFunction;
            currentLLVMFunction = func;
            builder = new IRBuilder();
            builder.PositionBuilderAtEnd(locals);
            builder.CreateBr(entry);
            builder.PositionBuilderAtEnd(entry);

            for (int i = 0; i < lambda.Parameters.Count; i++)
            {
                var p = CreateLocalVariable(lambda.Parameters[i]);
                builder.CreateStore(func.GetParam((uint)i), p);
                valueMap[lambda.Parameters[i]] = p;
            }

            var val = GenerateExpression(lambda.Body, true);
            if (!lambda.Body.GetFlag(ExprFlags.Returns))
            {
                if (lambda.FunctionType.ReturnType == CheezType.Void)
                {
                    builder.CreateRetVoid();
                }
                else
                {
                    builder.CreateRet(val);
                }
            }

            builder = prevBuilder;
            currentLLVMFunction = prevLLVMFunc;
            return func;
        }

        private LLVMValueRef GenerateCompCallExpr(AstCompCallExpr cc)
        {
            if (cc.GetFlag(ExprFlags.IgnoreInCodeGen))
                return LLVM.GetUndef(CheezTypeToLLVMType(cc.Type));

            if (cc.Name.Name == "type_info")
            {
                return typeInfoTable[cc.Arguments[0].Expr.Value as CheezType];
            }

            if (cc.Name.Name == "alloca")
            {
                var size = GenerateExpression(cc.Arguments[1], true);
                var mem = builder.CreateArrayAlloca(LLVM.Int8Type(), size, "");
                mem.SetAlignment(8);
                var anyPtr = builder.CreatePointerCast(mem, CheezTypeToLLVMType(cc.Type), "");
                return anyPtr;
            }

            if (cc.Name.Name == "bin_or")
            {
                var result = GenerateExpression(cc.Arguments[0], true);
                for (int i = 1; i < cc.Arguments.Count; i++)
                {
                    var v = GenerateExpression(cc.Arguments[i], true);
                    result = builder.CreateOr(result, v, "");
                }

                return result;
            }

            if (cc.Name.Name == "bin_xor")
            {
                var result = GenerateExpression(cc.Arguments[0], true);
                for (int i = 1; i < cc.Arguments.Count; i++)
                {
                    var v = GenerateExpression(cc.Arguments[i], true);
                    result = builder.CreateXor(result, v, "");
                }

                return result;
            }

            if (cc.Name.Name == "bin_and")
            {
                var result = GenerateExpression(cc.Arguments[0], true);
                for (int i = 1; i < cc.Arguments.Count; i++)
                {
                    var v = GenerateExpression(cc.Arguments[i], true);
                    result = builder.CreateAnd(result, v, "");
                }

                return result;
            }

            if (cc.Name.Name == "bin_lsl")
            {
                var val = GenerateExpression(cc.Arguments[0], true);
                var shift_count = GenerateExpression(cc.Arguments[1], true);
                var result = builder.CreateShl(val, shift_count, "");
                return result;
            }

            if (cc.Name.Name == "bin_lsr")
            {
                var val = GenerateExpression(cc.Arguments[0], true);
                var shift_count = GenerateExpression(cc.Arguments[1], true);
                var result = builder.CreateLShr(val, shift_count, "");
                return result;
            }

            if (cc.Name.Name == "panic")
            {
                var message = GenerateExpression(cc.Arguments[0], true);
                var len = builder.CreateExtractValue(message, 0, "");
                var str = builder.CreateExtractValue(message, 1, "");

                UpdateStackTracePosition(cc);
                CreateExit($"[PANIC] {cc.Location.Beginning}: %.*s", 1, len, str);
                return default;
            }

            if (cc.Name.Name == "assert")
            {
                var msg = "Assertion failed";
                if (cc.Arguments.Count >= 2)
                    msg = cc.Arguments[1].Value as string;
                var cond = GenerateExpression(cc.Arguments[0], true);

                var bbTrue = currentLLVMFunction.AppendBasicBlock("assert_true");
                var bbFalse = currentLLVMFunction.AppendBasicBlock("assert_false");
                builder.CreateCondBr(cond, bbTrue, bbFalse);

                builder.PositionBuilderAtEnd(bbFalse);

                UpdateStackTracePosition(cc);
                CreateExit($"[ASSERT] {cc.Location.Beginning}: {msg}\n{cc.Arguments[0].ToString().Indent("> ")}", 1);

                builder.PositionBuilderAtEnd(bbTrue);
                return default;
            }

            if (cc.Name.Name == "static_assert")
            {
                // do nothing
                return default;
            }

            if (cc.Name.Name == "destruct")
            {
                // @todo
                var arg = cc.Arguments[0].Expr;
                var dtor = GetDestructor(arg.Type);

                var argVal = GenerateExpression(arg, false);
                builder.CreateCall(dtor, new LLVMValueRef[] { argVal }, "");
                return default;
            }

            if (cc.Name.Name == "dup")
            {
                var type = cc.Type as ArrayType;
                var arg = cc.Arguments[0].Expr;

                var value = GenerateExpression(arg, true);
                var vals = new LLVMValueRef[type.Length];

                var arr = LLVM.GetUndef(CheezTypeToLLVMType(type));

                for (int i = 0; i < vals.Length; i++)
                    arr = builder.CreateInsertValue(arr, value, (uint)i, "");
                return arr;
            }

            throw new NotImplementedException($"{nameof(GenerateCompCallExpr)}: {cc.Name.Name} is not implemented yet");
        }

        private LLVMValueRef GenerateEnumValueExpr(AstEnumValueExpr eve)
        {
            var v = CreateLocalVariable(eve.Type);

            var ptr = builder.CreateStructGEP(v, 0, "");
            var val = LLVM.ConstInt(CheezTypeToLLVMType(eve.EnumDecl.TagType), eve.Member.Value.ToUlong(), false);
            builder.CreateStore(val, ptr);

            if (eve.Argument != null)
            {
                ptr = builder.CreateStructGEP(v, 1, "");
                ptr = builder.CreatePointerCast(ptr, CheezTypeToLLVMType(PointerType.GetPointerType(eve.Argument.Type)), "");

                val = GenerateExpression(eve.Argument, true);
                builder.CreateStore(val, ptr);
            }

            v = builder.CreateLoad(v, "");
            return v;
        }

        private LLVMValueRef GenerateMatchExpr(AstMatchExpr m)
        {
            // TODO: check if m can be a simple switch
            if (m.IsSimpleIntMatch)
            {
                LLVMValueRef result = default;
                if (m.Type != CheezType.Void) result = CreateLocalVariable(m.Type);
                var bbElse = currentLLVMFunction.AppendBasicBlock("_switch_else");
                var cond = GenerateExpression(m.SubExpression, false);
                var sw = builder.CreateSwitch(cond, bbElse, (uint)m.Cases.Count);

                foreach (var c in m.Cases)
                {
                    var patt = GenerateExpression(c.Pattern, true);
                    var bb = currentLLVMFunction.AppendBasicBlock($"_switch_case_{c.Pattern}");

                    builder.PositionBuilderAtEnd(bb);
                    var b = GenerateExpression(c.Body, true);
                    if (m.Type != CheezType.Void)
                        builder.CreateStore(b, result);

                    builder.CreateBr(bbElse);

                    sw.AddCase(patt, bb);
                }

                builder.PositionBuilderAtEnd(bbElse);

                if (m.Type != CheezType.Void)
                    result = builder.CreateLoad(result, "");
                return result;
            }
            else
            {
                LLVMValueRef result = default;
                if (m.Type != CheezType.Void) result = CreateLocalVariable(m.Type);
                LLVMBasicBlockRef bbElse = currentLLVMFunction.AppendBasicBlock($"_switch_else");
                LLVMBasicBlockRef bbNext = default;
                var cond = GenerateExpression(m.SubExpression, false);

                foreach (var c in m.Cases)
                {
                    var patt = GeneratePatternCondition(c.Pattern, cond);

                    if (c.Condition != null)
                    {
                        var v = GenerateExpression(c.Condition, true);
                        patt = builder.CreateAnd(patt, v, "");
                    }

                    var bb = currentLLVMFunction.AppendBasicBlock($"_switch_case_{c.Pattern}");
                    bbNext = currentLLVMFunction.AppendBasicBlock($"_switch_next");

                    builder.CreateCondBr(patt, bb, bbNext);


                    builder.PositionBuilderAtEnd(bb);
                    var b = GenerateExpression(c.Body, true);
                    if (m.Type != CheezType.Void && c.Body.Type != CheezType.Void)
                        builder.CreateStore(b, result);

                    if (!c.Body.GetFlag(ExprFlags.Returns))
                        builder.CreateBr(bbElse);

                    builder.PositionBuilderAtEnd(bbNext);
                }

                builder.CreateBr(bbElse);
                builder.PositionBuilderAtEnd(bbElse);

                if (m.Type != CheezType.Void)
                    result = builder.CreateLoad(result, "");
                return result;
            }
        }

        private LLVMValueRef GeneratePatternCondition(AstExpression pattern, LLVMValueRef value)
        {
            //if (pattern.GetFlag(ExprFlags.PatternRefersToReference))
            //{
            //    value = builder.CreateLoad(value, "");
            //}

            switch (pattern)
            {
                case AstTupleExpr t:
                    {
                        var result = LLVM.ConstInt(LLVM.Int1Type(), 1, false);

                        for (int i = 0; i < t.Values.Count; i++)
                        {
                            var c = builder.CreateStructGEP(value, (uint)i, "");
                            var v = GeneratePatternCondition(t.Values[i], c);
                            result = builder.CreateAnd(result, v, "");
                        }

                        return result;
                    }

                case AstNumberExpr _:
                    {
                        value = builder.CreateLoad(value, "");
                        var v = GenerateExpression(pattern, true);
                        if (pattern.Type is IntType)
                            return builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, value, v, "");
                        if (pattern.Type is FloatType)
                            return builder.CreateFCmp(LLVMRealPredicate.LLVMRealOEQ, value, v, "");
                        break;
                    }

                case AstCharLiteral _:
                    {
                        value = builder.CreateLoad(value, "");
                        var v = GenerateExpression(pattern, true);
                        return builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, value, v, "");
                    }

                case AstIdExpr n:
                    {
                        if (n.IsPolymorphic)
                        {
                            GenerateExpression(n, false);
                            return LLVM.ConstInt(LLVM.Int1Type(), 1, false);
                        }
                        else
                        {
                            value = builder.CreateLoad(value, "");
                            var v = GenerateExpression(pattern, true);
                            return builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, value, v, "");
                        }
                    }

                case AstEnumValueExpr e:
                    {
                        var tag = LLVM.ConstInt(LLVM.Int64Type(), e.Member.Value.ToUlong(), true);

                        var valueTagPtr = builder.CreateStructGEP(value, 0, "");
                        var valueTag = builder.CreateLoad(valueTagPtr, "");

                        var comp1 = builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, valueTag, tag, "");

                        if (e.Argument == null)
                            return comp1;

                        var valPtr = builder.CreateStructGEP(value, 1, "");
                        
                        valPtr = builder.CreatePointerCast(valPtr, CheezTypeToLLVMType(PointerType.GetPointerType(e.Argument.Type)), "");
                        var comp2 = GeneratePatternCondition(e.Argument, valPtr);

                        return builder.CreateAnd(comp1, comp2, "");
                    }

                case AstReferenceTypeExpr r:
                    {
                        if (r.Target is AstIdExpr id && id.IsPolymorphic)
                        {
                            GenerateExpression(id, false);
                            return LLVM.ConstInt(LLVM.Int1Type(), 1, false);
                        }
                        break;
                    }

            }
            throw new NotImplementedException();
        }

        private LLVMValueRef GenerateDefaultExpr(AstDefaultExpr def)
        {
            return GetDefaultLLVMValue(def.Type);
        }

        private LLVMValueRef GenerateArrayExpr(AstArrayExpr arr, bool deref)
        {
            var ptr = CreateLocalVariable(arr.Type);

            uint index = 0;
            foreach (var value in arr.Values)
            {
                var p = builder.CreateGEP(ptr, new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0)),
                    LLVM.ConstInt(LLVM.Int32Type(), index, new LLVMBool(0))
                }, "");
                var v = GenerateExpression(value, true);
                builder.CreateStore(v, p);

                index++;
            }

            if (deref)
                return builder.CreateLoad(ptr, "");
            return ptr;
        }

        private LLVMValueRef GenerateUfcFuncExpr(AstUfcFuncExpr ufc)
        {
            if (ufc.FunctionDecl.TraitFunction != null)
            {
                // call to a trait function
                // get function pointer from trait object

            }

            // normal function call
            return valueMap[ufc.FunctionDecl];
        }

        private LLVMValueRef GenerateArgumentExpr(AstArgument a)
        {
            return GenerateExpression(a.Expr, true);
        }

        private LLVMValueRef GenerateNullExpr(AstNullExpr expr)
        {
            if (expr.Type is PointerType)
            {
                return LLVM.ConstPointerNull(CheezTypeToLLVMType(expr.Type));
            }
            if (expr.Type is TraitType t)
            {
                return LLVM.ConstNamedStruct(CheezTypeToLLVMType(expr.Type), new LLVMValueRef[]
                {
                    LLVM.ConstPointerNull(pointerType),
                    LLVM.ConstPointerNull(pointerType)
                });
            }
            if (expr.Type is SliceType s)
            {
                return LLVM.ConstNamedStruct(CheezTypeToLLVMType(expr.Type), new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int64Type(), 0, false),
                    LLVM.ConstPointerNull(CheezTypeToLLVMType(s.TargetType))
                });
            }
            if (expr.Type is FunctionType f)
            {
                var llvmType = CheezTypeToLLVMType(f);
                return LLVM.ConstNull(llvmType);
            }
            else throw new NotImplementedException();
        }

        private LLVMValueRef CreateIntCast(IntType from, IntType to, LLVMValueRef value)
        {
            var toLLVM = CheezTypeToLLVMType(to);

            if (to.Signed && from.Signed)
            {
                return builder.CreateIntCast(value, toLLVM, "");
            }
            else if (to.Signed) // s <- u
            {
                if (to.GetSize() > from.GetSize())
                    return builder.CreateZExtOrBitCast(value, toLLVM, "");
                else
                    return builder.CreateTruncOrBitCast(value, toLLVM, "");
            }
            else if (from.Signed) // u <- s
            {
                if (to.GetSize() > from.GetSize())
                    return builder.CreateZExtOrBitCast(value, toLLVM, "");
                else
                    return builder.CreateTruncOrBitCast(value, toLLVM, "");
            }
            else // u <- u
            {
                if (to.GetSize() > from.GetSize())
                    return builder.CreateZExtOrBitCast(value, toLLVM, "");
                else
                    return builder.CreateTruncOrBitCast(value, toLLVM, "");
            }
        }

        private LLVMValueRef GenerateCastExpr(AstCastExpr cast)
        {
            var to = cast.Type;
            var from = cast.SubExpression.Type;
            var toLLVM = CheezTypeToLLVMType(to);

            if (to is TraitType trait)
            {
                var ptr = GenerateExpression(cast.SubExpression, false);
                ptr = builder.CreatePointerCast(ptr, pointerType, "");

                var impl = trait.Declaration.Implementations[from];
                var vtablePtr = vtableMap[impl];

                var traitObject = LLVM.GetUndef(toLLVM);
                traitObject = builder.CreateInsertValue(traitObject, vtablePtr, 0, "");
                traitObject = builder.CreateInsertValue(traitObject, ptr, 1, "");
                return traitObject;
            }

            if (to == from) return GenerateExpression(cast.SubExpression, true);
            
            if (to is IntType t1 && from is IntType f1) // int <- int
            {
                var v = GenerateExpression(cast.SubExpression, true);
                return CreateIntCast(f1, t1, v);
            }
            if (to is PointerType && from is IntType) // * <- int
                return builder.CreateCast(LLVMOpcode.LLVMIntToPtr, GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is IntType && from is PointerType) // int <- *
                return builder.CreateCast(LLVMOpcode.LLVMPtrToInt, GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is PointerType && from is PointerType) // * <- *
                return builder.CreatePointerCast(GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is FloatType && from is FloatType) // float <- float
            return builder.CreateFPCast(GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is IntType i && from is FloatType) // int <- float
                return builder.CreateCast(i.Signed ? LLVMOpcode.LLVMFPToSI : LLVMOpcode.LLVMFPToUI, GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is FloatType && from is IntType i2) // float <- int
                return builder.CreateCast(i2.Signed ? LLVMOpcode.LLVMSIToFP : LLVMOpcode.LLVMUIToFP, GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is IntType && from is BoolType) // int <- bool
                return builder.CreateZExt(GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is IntType && from is CharType) // int <- char
                return builder.CreateSExt(GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is CharType && from is IntType) // char <- int
                return builder.CreateTrunc(GenerateExpression(cast.SubExpression, true), toLLVM, "");
            if (to is SliceType s && from is PointerType p) // [] <- *
            {
                var withLen = builder.CreateInsertValue(LLVM.GetUndef(CheezTypeToLLVMType(s)), LLVM.ConstInt(LLVM.Int64Type(), 0, false), 0, "");
                var result = builder.CreateInsertValue(withLen, GenerateExpression(cast.SubExpression, true), 1, "");

                return result;
            }

            if (to is IntType i3 && from is EnumType e)
            {
                var v = GenerateExpression(cast.SubExpression, true);
                var tag = builder.CreateExtractValue(v, 0, "");
                return CreateIntCast(i3, e.Declaration.TagType, tag);
            }

            if (to is BoolType && from is FunctionType)
            {
                var func = GenerateExpression(cast.SubExpression, true);
                var ptr = builder.CreatePtrToInt(func, LLVM.Int64Type(), "");
                var res = builder.CreateICmp(LLVMIntPredicate.LLVMIntNE, ptr, LLVM.ConstInt(LLVM.Int64Type(), 0, false), "");
                return res;
            }

            if (to is FunctionType fto && fto.IsFatFunction && cast.SubExpression is AstUfcFuncExpr ufc)
            {
                var func = GenerateExpression(cast.SubExpression, true);
                var data = GenerateExpression(ufc.SelfArg, false);
                data = builder.CreatePointerCast(data, LLVM.Int8Type().GetPointerTo(), "");

                var result = LLVM.GetUndef(CheezTypeToLLVMType(to));
                result = builder.CreateInsertValue(result, func, 0, "");
                result = builder.CreateInsertValue(result, data, 1, "");
                return result;
            }

            if (to is FunctionType tf && from is FunctionType ff)
            {
                switch (tf.IsFatFunction, ff.IsFatFunction)
                {
                    case (false, false):
                        {
                            var func = GenerateExpression(cast.SubExpression, true);
                            var res = builder.CreatePointerCast(func, toLLVM, "");
                            return res;
                        }

                    case (true, true):
                        {
                            throw new NotImplementedException();
                        }

                    case (true, false):
                        {
                            var func = GenerateExpression(cast.SubExpression, true);
                            func = builder.CreatePointerCast(func, LLVM.Int8Type().GetPointerTo(), "");

                            var helpFunc = CreateFatFuncHelper(ff);

                            var result = LLVM.GetUndef(CheezTypeToLLVMType(to));
                            result = builder.CreateInsertValue(result, helpFunc, 0, "");
                            result = builder.CreateInsertValue(result, func, 1, "");
                            return result;
                        }

                    case (false, true):
                        {
                            // not possible
                            throw new NotImplementedException();
                        }
                }
            }

            if (to is FunctionType && from is PointerType)
            {
                var func = GenerateExpression(cast.SubExpression, true);
                var res = builder.CreatePointerCast(func, toLLVM, "");
                return res;
            }

            if (to is PointerType && from is ArrayType) // * <- [x]
            {
                var sub = GenerateExpression(cast.SubExpression, false);
                return builder.CreatePointerCast(sub, toLLVM, "");
            }

            if (to is SliceType s2 && from is ArrayType a)
            {
                var slice = LLVM.GetUndef(CheezTypeToLLVMType(s2));
                slice = builder.CreateInsertValue(slice, LLVM.ConstInt(LLVM.Int64Type(), (ulong)a.Length, false), 0, "");

                var sub = GenerateExpression(cast.SubExpression, false);
                var ptr = builder.CreatePointerCast(sub, CheezTypeToLLVMType(s2.ToPointerType()), "");
                slice = builder.CreateInsertValue(slice, ptr, 1, "");

                return slice;
            }

            if (to == CheezType.Any)
            {
                LLVMValueRef val = default;
                if (cast.SubExpression.GetFlag(ExprFlags.IsLValue))
                {
                    val = GenerateExpression(cast.SubExpression, false);
                }
                else
                {
                    val = CreateLocalVariable(from, "temp.any");
                    var v = GenerateExpression(cast.SubExpression, true);
                    builder.CreateStore(v, val);
                }

                val = builder.CreatePointerCast(val, LLVM.PointerType(LLVM.Int8Type(), 0), "");

                var llvmAny = CheezTypeToLLVMType(CheezType.Any);
                var typeInfo = typeInfoTable[from];

                var any = builder.CreateInsertValue(LLVM.GetUndef(llvmAny), typeInfo, 0, "");
                any = builder.CreateInsertValue(any, val, 1, "");

                return any;
            }

            throw new NotImplementedException();
        }

        private LLVMValueRef GenerateDerefExpr(AstDereferenceExpr de, bool deref)
        {
            var ptr = GenerateExpression(de.SubExpression, true);

            if (!deref) return ptr;

            var sub = builder.CreateLoad(ptr, "");

            return sub;
        }

        private static Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef> GetICompare(LLVMIntPredicate pred)
        {
            return (a, b, c, d) => LLVM.BuildICmp(a, pred, b, c, d);
        }

        private static Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef> GetFCompare(LLVMRealPredicate pred)
        {
            return (a, b, c, d) => LLVM.BuildFCmp(a, pred, b, c, d);
        }

        private Dictionary<(string, CheezType), Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>> builtInOperators
            = new Dictionary<(string, CheezType), Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>>
            {
                // 
                { ("+", IntType.GetIntType(1, true)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(2, true)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(4, true)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(8, true)), LLVM.BuildAdd },

                { ("+", IntType.GetIntType(1, false)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(2, false)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(4, false)), LLVM.BuildAdd },
                { ("+", IntType.GetIntType(8, false)), LLVM.BuildAdd },

                { ("-", IntType.GetIntType(1, true)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(2, true)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(4, true)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(8, true)), LLVM.BuildSub },

                { ("-", IntType.GetIntType(1, false)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(2, false)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(4, false)), LLVM.BuildSub },
                { ("-", IntType.GetIntType(8, false)), LLVM.BuildSub },

                { ("*", IntType.GetIntType(1, true)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(2, true)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(4, true)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(8, true)), LLVM.BuildMul },

                { ("*", IntType.GetIntType(1, false)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(2, false)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(4, false)), LLVM.BuildMul },
                { ("*", IntType.GetIntType(8, false)), LLVM.BuildMul },

                { ("/", IntType.GetIntType(1, true)), LLVM.BuildSDiv },
                { ("/", IntType.GetIntType(2, true)), LLVM.BuildSDiv },
                { ("/", IntType.GetIntType(4, true)), LLVM.BuildSDiv },
                { ("/", IntType.GetIntType(8, true)), LLVM.BuildSDiv },

                { ("/", IntType.GetIntType(1, false)), LLVM.BuildUDiv },
                { ("/", IntType.GetIntType(2, false)), LLVM.BuildUDiv },
                { ("/", IntType.GetIntType(4, false)), LLVM.BuildUDiv },
                { ("/", IntType.GetIntType(8, false)), LLVM.BuildUDiv },

                { ("%", IntType.GetIntType(1, true)), LLVM.BuildSRem },
                { ("%", IntType.GetIntType(2, true)), LLVM.BuildSRem },
                { ("%", IntType.GetIntType(4, true)), LLVM.BuildSRem },
                { ("%", IntType.GetIntType(8, true)), LLVM.BuildSRem },

                { ("%", IntType.GetIntType(1, false)), LLVM.BuildURem },
                { ("%", IntType.GetIntType(2, false)), LLVM.BuildURem },
                { ("%", IntType.GetIntType(4, false)), LLVM.BuildURem },
                { ("%", IntType.GetIntType(8, false)), LLVM.BuildURem },

                { ("+", FloatType.GetFloatType(4)), LLVM.BuildFAdd },
                { ("+", FloatType.GetFloatType(8)), LLVM.BuildFAdd },

                { ("-", FloatType.GetFloatType(4)), LLVM.BuildFSub },
                { ("-", FloatType.GetFloatType(8)), LLVM.BuildFSub },

                { ("*", FloatType.GetFloatType(4)), LLVM.BuildFMul },
                { ("*", FloatType.GetFloatType(8)), LLVM.BuildFMul },

                { ("/", FloatType.GetFloatType(4)), LLVM.BuildFDiv },
                { ("/", FloatType.GetFloatType(8)), LLVM.BuildFDiv },

                { ("%", FloatType.GetFloatType(4)), LLVM.BuildFRem },
                { ("%", FloatType.GetFloatType(8)), LLVM.BuildFRem },

                //
                { ("==", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("==", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                
                { ("!=", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { ("!=", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntNE) },

                { ("<", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntULT) },
                { ("<", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntULT) },
                { ("<", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntULT) },
                { ("<", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntULT) },
                { ("<", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntSLT) },
                { ("<", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntSLT) },
                { ("<", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntSLT) },
                { ("<", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntSLT) },

                { ("<=", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntULE) },
                { ("<=", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntULE) },
                { ("<=", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntULE) },
                { ("<=", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntULE) },
                { ("<=", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntSLE) },
                { ("<=", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntSLE) },
                { ("<=", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntSLE) },
                { ("<=", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntSLE) },

                { (">", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntUGT) },
                { (">", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntUGT) },
                { (">", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntUGT) },
                { (">", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntUGT) },
                { (">", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntSGT) },
                { (">", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntSGT) },
                { (">", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntSGT) },
                { (">", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntSGT) },

                { (">=", IntType.GetIntType(1, false)), GetICompare(LLVMIntPredicate.LLVMIntUGE) },
                { (">=", IntType.GetIntType(2, false)), GetICompare(LLVMIntPredicate.LLVMIntUGE) },
                { (">=", IntType.GetIntType(4, false)), GetICompare(LLVMIntPredicate.LLVMIntUGE) },
                { (">=", IntType.GetIntType(8, false)), GetICompare(LLVMIntPredicate.LLVMIntUGE) },
                { (">=", IntType.GetIntType(1, true)), GetICompare(LLVMIntPredicate.LLVMIntSGE) },
                { (">=", IntType.GetIntType(2, true)), GetICompare(LLVMIntPredicate.LLVMIntSGE) },
                { (">=", IntType.GetIntType(4, true)), GetICompare(LLVMIntPredicate.LLVMIntSGE) },
                { (">=", IntType.GetIntType(8, true)), GetICompare(LLVMIntPredicate.LLVMIntSGE) },
                
                { ("==", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealOEQ) },
                { ("==", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealOEQ) },
                
                { ("!=", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealONE) },
                { ("!=", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealONE) },
                
                { ("<", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealOLT) },
                { ("<", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealOLT) },
                
                { ("<=", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealOLE) },
                { ("<=", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealOLE) },
                
                { (">", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealOGT) },
                { (">", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealOGT) },
                
                { (">=", FloatType.GetFloatType(4)), GetFCompare(LLVMRealPredicate.LLVMRealOGE) },
                { (">=", FloatType.GetFloatType(8)), GetFCompare(LLVMRealPredicate.LLVMRealOGE) },

                //
                { ("==", CheezType.Bool), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("!=", CheezType.Bool), GetICompare(LLVMIntPredicate.LLVMIntNE) },

                //
                { ("+", CheezType.Char), LLVM.BuildAdd },
                { ("-", CheezType.Char), LLVM.BuildSub },
                { ("==", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { ("!=", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntNE) },
                { (">", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntSGT) },
                { (">=", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntSGE) },
                { ("<", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntSLT) },
                { ("<=", CheezType.Char), GetICompare(LLVMIntPredicate.LLVMIntSLE) },
            };

        private Dictionary<string, Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>> builtInPointerOperators
            = new Dictionary<string, Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>>
            {
                { "==", GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { "!=", GetICompare(LLVMIntPredicate.LLVMIntNE) },
            };

        private LLVMValueRef GenerateBinaryExpr(AstBinaryExpr bin)
        {
            if (bin.ActualOperator is BuiltInBinaryOperator)
            {
                if (bin.Operator == "and")
                {
                    return GenerateAndExpr(bin);
                }
                else if (bin.Operator == "or")
                {
                    return GenerateOrExpr(bin);
                }
                else
                {
                    var left = GenerateExpression(bin.Left, true);
                    var right = GenerateExpression(bin.Right, true);
                    var bo = builtInOperators[(bin.Operator, bin.Left.Type)];
                    var val = bo(GetRawBuilder(), left, right, "");
                    return val;
                }
            }
            else if (bin.ActualOperator is BuiltInPointerOperator)
            {
                var left = GenerateExpression(bin.Left, true);
                var right = GenerateExpression(bin.Right, true);

                left = builder.CreatePointerCast(left, pointerType, "");
                right = builder.CreatePointerCast(right, pointerType, "");

                var bo = builtInPointerOperators[bin.Operator];
                var val = bo(GetRawBuilder(), left, right, "");
                return val;
            }
            else if (bin.ActualOperator is BuiltInTraitNullOperator tno)
            {
                var left = GenerateExpression(bin.Left, true);

                var vtablePtr = builder.CreateExtractValue(left, 0, "");
                var toPointer = builder.CreateExtractValue(left, 1, "");

                vtablePtr = builder.CreatePtrToInt(vtablePtr, LLVM.Int64Type(), "");
                toPointer = builder.CreatePtrToInt(toPointer, LLVM.Int64Type(), "");

                var together = builder.CreateAnd(vtablePtr, toPointer, "");

                var op = LLVMIntPredicate.LLVMIntEQ;
                if (tno.Name == "==")
                    op = LLVMIntPredicate.LLVMIntEQ;
                else if (tno.Name == "!=")
                    op = LLVMIntPredicate.LLVMIntNE;
                else
                    throw new NotImplementedException();

                var result = builder.CreateICmp(op, together, LLVM.ConstInt(LLVM.Int64Type(), 0, false), "");
                return result;
            }
            else if (bin.ActualOperator is BuiltInFunctionOperator fun)
            {
                var left = GenerateExpression(bin.Left, true);
                var right = GenerateExpression(bin.Right, true);
                if (fun.Name == "==")
                    return builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "");
                if (fun.Name == "!=")
                    return builder.CreateICmp(LLVMIntPredicate.LLVMIntNE, left, right, "");

                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private Dictionary<(string, CheezType), Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef>> builtInUnaryOperators
            = new Dictionary<(string, CheezType), Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef>>
            {
                { ("-", FloatType.GetFloatType(4)), LLVM.BuildFNeg },
                { ("-", FloatType.GetFloatType(8)), LLVM.BuildFNeg },

                { ("-", IntType.GetIntType(1, true)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(2, true)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(4, true)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(8, true)), LLVM.BuildNeg },

                { ("-", IntType.GetIntType(1, false)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(2, false)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(4, false)), LLVM.BuildNeg },
                { ("-", IntType.GetIntType(8, false)), LLVM.BuildNeg },

                { ("!", CheezType.Bool), LLVM.BuildNot },
            };
        private LLVMValueRef GenerateUnaryExpr(AstUnaryExpr expr)
        {
            if (expr.ActualOperator is BuiltInUnaryOperator)
            {
                var left = GenerateExpression(expr.SubExpr, true);
                var bo = builtInUnaryOperators[(expr.Operator, expr.SubExpr.Type)];
                var val = bo(GetRawBuilder(), left, "");
                return val;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private LLVMValueRef GenerateAndExpr(AstBinaryExpr bin)
        {
            var result = CreateLocalVariable(CheezType.Bool);

            var bbRight = LLVM.AppendBasicBlock(currentLLVMFunction, "_and_right");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_and_end");

            var left = GenerateExpression(bin.Left, true);
            builder.CreateStore(left, result);
            builder.CreateCondBr(builder.CreateLoad(result, ""), bbRight, bbEnd);

            builder.PositionBuilderAtEnd(bbRight);
            var right = GenerateExpression(bin.Right, true);
            builder.CreateStore(right, result);
            builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbEnd);

            result = builder.CreateLoad(result, "");
            return result;
        }

        private LLVMValueRef GenerateOrExpr(AstBinaryExpr bin)
        {
            var result = CreateLocalVariable(CheezType.Bool);

            var bbRight = LLVM.AppendBasicBlock(currentLLVMFunction, "_or_right");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_or_end");

            var left = GenerateExpression(bin.Left, true);
            builder.CreateStore(left, result);
            builder.CreateCondBr(builder.CreateLoad(result, ""), bbEnd, bbRight);

            builder.PositionBuilderAtEnd(bbRight);
            var right = GenerateExpression(bin.Right, true);
            builder.CreateStore(right, result);
            builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbEnd);

            result = builder.CreateLoad(result, "");
            return result;
        }

        private LLVMValueRef GenerateIfExpr(AstIfExpr iff)
        {
            LLVMValueRef result = default;
            if (iff.Type != CheezType.Void) result = CreateLocalVariable(iff.Type);

            var cond = GenerateExpression(iff.Condition, true);

            var bbIf = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_true");
            var bbElse = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_false");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_end");

            builder.CreateCondBr(cond, bbIf, bbElse);

            builder.PositionBuilderAtEnd(bbIf);
            if (iff.Type != CheezType.Void && iff.IfCase.Type != CheezType.Void)
            {
                var r = GenerateExpression(iff.IfCase, true);
                builder.CreateStore(r, result);
            }
            else
            {
                GenerateExpression(iff.IfCase, false);
            }

            if (!iff.IfCase.GetFlag(ExprFlags.Returns))
                builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbElse);
            if (iff.ElseCase != null)
            {
                if (iff.Type != CheezType.Void && iff.ElseCase.Type != CheezType.Void)
                {
                    var r = GenerateExpression(iff.ElseCase, true);
                    builder.CreateStore(r, result);
                }
                else
                {
                    GenerateExpression(iff.ElseCase, false);
                }
                if (!iff.ElseCase.GetFlag(ExprFlags.Returns))
                    builder.CreateBr(bbEnd);
            }
            else
            {
                builder.CreateBr(bbEnd);
            }

            builder.PositionBuilderAtEnd(bbEnd);

            if (iff.Type != CheezType.Void)
            {
                result = builder.CreateLoad(result, "");
            }

            return result;
        }

        private LLVMValueRef GenerateAddressOf(AstAddressOfExpr ao)
        {
            var ptr = GenerateExpression(ao.SubExpression, false);
            return ptr;
        }

        private LLVMValueRef GenerateBlock(AstBlockExpr block, bool deref)
        {
            LLVMBasicBlockRef? bbBody = null;
            LLVMBasicBlockRef? bbEnd = null;
            if (block.Label != null)
            {
                bbBody = LLVM.AppendBasicBlock(currentLLVMFunction, "_block_body");
                bbEnd  = LLVM.AppendBasicBlock(currentLLVMFunction, "_block_end");

                if (block.Label != null)
                    breakTargetMap[block] = bbEnd.Value;

                builder.CreateBr(bbBody.Value);
                builder.PositionBuilderAtEnd(bbBody.Value);
            }

            LLVMValueRef result = default;

            int end = block.Statements.Count;
            if (block.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                GenerateStatement(block.Statements[i]);

                // check if the current statement returs and there are more statements after that
                if (block.Statements[i].GetFlag(StmtFlags.Returns) && i < block.Statements.Count - 1)
                {
                    // create new basic block
                    var bbNext = LLVM.AppendBasicBlock(currentLLVMFunction, "_unreachable");
                    builder.PositionBuilderAtEnd(bbNext);
                }
            }

            if (block.Statements.LastOrDefault() is AstExprStmt expr)
            {
                result = GenerateExpression(expr.Expr, deref);
                if (expr.Destructions != null)
                {
                    foreach (var dest in expr.Destructions)
                    {
                        GenerateStatement(dest);
                    }
                }
            }

            if (block.Destructions != null)
            {
                foreach (var dest in block.Destructions)
                {
                    GenerateStatement(dest);
                }
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                GenerateStatement(block.DeferredStatements[i]);
            }

            if (block.Label != null)
            {
                if (builder.GetInsertBlock().GetBasicBlockTerminator().Pointer.ToInt64() == 0)
                    builder.CreateBr(bbEnd.Value);
                builder.PositionBuilderAtEnd(bbEnd.Value);
            }

            return result;
        }

        private LLVMValueRef GenerateSymbolExpr(AstSymbolExpr s, bool deref)
        {
            var v = valueMap[s.Symbol];

            if (deref)
            {
                v = builder.CreateLoad(v, "");
            }

            return v;
        }

        private LLVMValueRef GenerateTempVarExpr(AstTempVarExpr t, bool deref)
        {
            if (!valueMap.ContainsKey(t))
            {
                var type = t.Type;
                if (t.StorePointer) type = PointerType.GetPointerType(type);

                var x = CreateLocalVariable(type);
                valueMap[t] = x;
                var v = GenerateExpression(t.Expr, !t.StorePointer);
                builder.CreateStore(v, x);
            }

            var tmp = valueMap[t];

            if (t.StorePointer)
            {
                tmp = builder.CreateLoad(tmp, "");
            }

            if (deref)
            {
                tmp = builder.CreateLoad(tmp, "");
            }

            return tmp;
        }

        private LLVMValueRef GenerateCallExpr(AstCallExpr c)
        {
            if (c.Declaration?.Trait != null)
            {
                // call to a trait function
                // get function pointer from trait object
                var functionIndex = vtableIndices[c.Declaration];
                var funcType = FuncTypeToLLVMType(c.Declaration.FunctionType);

                var selfArg = GenerateExpression(c.Arguments[0], true);
                selfArg = builder.CreateLoad(selfArg, "");

                var vtablePtr = builder.CreateExtractValue(selfArg, 0, "");
                var toPointer = builder.CreateExtractValue(selfArg, 1, "");
                toPointer = builder.CreatePointerCast(toPointer, funcType.GetParamTypes()[0], "");

                // check if pointer is null
                if (checkForNullTraitObjects)
                {
                    CheckPointerNull(vtablePtr, c.Arguments[0].Expr, "vtable pointer of trait object is null");
                    CheckPointerNull(toPointer, c.Arguments[0].Expr, "object pointer of trait object is null");
                }

                // load function pointer
                var vtableType = vtableTypes[c.Declaration.Trait.TraitType];
                vtablePtr = builder.CreatePointerCast(vtablePtr, vtableType.GetPointerTo(), "");

                var funcPointer = builder.CreateStructGEP(vtablePtr, (uint)functionIndex, "");
                funcPointer = builder.CreateLoad(funcPointer, "");

                var arguments = new List<LLVMValueRef>();
                // self arg
                arguments.Add(toPointer);

                // rest of arguments
                foreach (var a in c.Arguments.Skip(1))
                    arguments.Add(GenerateExpression(a, true));

                UpdateStackTracePosition(c);
                var traitCall = builder.CreateCall(funcPointer, arguments.ToArray(), "");
                LLVM.SetInstructionCallConv(traitCall, LLVM.GetFunctionCallConv(funcPointer));

                return traitCall;
            }

            LLVMValueRef func;
            if (c.Declaration != null)
            {
                func = valueMap[c.Declaration];

                // arguments
                var args = c.Arguments.Select(a => {
                    return GenerateExpression(a, true);
                }).ToArray();

                UpdateStackTracePosition(c);
                var call = builder.CreateCall(func, args, "");
                var callConv = LLVM.GetFunctionCallConv(func);
                LLVM.SetInstructionCallConv(call, callConv);
                return call;
            }
            else
            {
                func = GenerateExpression(c.FunctionExpr, true);
                var ftype = c.FunctionExpr.Type as FunctionType;

                // arguments
                IEnumerable<LLVMValueRef> GetFnArg()
                {
                    if (ftype.IsFatFunction)
                    {
                        var agg = func;
                        func = builder.CreateExtractValue(agg, 0, "func");
                        yield return builder.CreateExtractValue(agg, 1, "data");
                    }
                    yield break;
                }

                var args = GetFnArg().Concat(c.Arguments.Select(a => GenerateExpression(a, true))).ToArray();

                if (ftype.CC == FunctionType.CallingConvention.Stdcall)
                {
                    func.SetFunctionCallConv((uint)LLVMCallConv.LLVMX86StdcallCallConv);
                }

                CheckPointerNull(func, c.FunctionExpr, "Attempting to call null function pointer");
                UpdateStackTracePosition(c);
                var call = builder.CreateCall(func, args, "");
                var callConv = LLVM.GetFunctionCallConv(func);
                LLVM.SetInstructionCallConv(call, callConv);
                return call;
            }
        }

        private LLVMValueRef GenerateIndexExpr(AstArrayAccessExpr expr, bool deref)
        {
            switch (expr.SubExpression.Type)
            {
                case TupleType t:
                    {
                        var index = ((NumberData)expr.Arguments[0].Value).ToLong();
                        var left = GenerateExpression(expr.SubExpression, false);

                        LLVMValueRef result;
                        if (!expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                        {
                            if (expr.SubExpression is AstTempVarExpr)
                            {
                                result = builder.CreateStructGEP(left, (uint)index, "");
                                if (deref)
                                    result = builder.CreateLoad(result, "");
                            }
                            else
                            {
                                result = builder.CreateExtractValue(left, (uint)index, "");
                            }
                        }
                        else
                        {
                            result = builder.CreateStructGEP(left, (uint)index, "");
                            if (deref)
                                result = builder.CreateLoad(result, "");
                        }

                        return result;
                    }

                case SliceType s:
                    {
                        switch (expr.Arguments[0].Type)
                        {
                            case IntType _:
                                {
                                    var index = GenerateExpression(expr.Arguments[0], true);
                                    var slice = GenerateExpression(expr.SubExpression, false);

                                    var dataPtrPtr = builder.CreateStructGEP(slice, 1, "");
                                    var dataPtr = builder.CreateLoad(dataPtrPtr, "");

                                    var ptr = builder.CreateInBoundsGEP(dataPtr, new LLVMValueRef[] { index }, "");

                                    var val = ptr;
                                    if (deref)
                                        val = builder.CreateLoad(ptr, "");
                                    return val;
                                }

                            case RangeType _:
                                {
                                    var range = GenerateExpression(expr.Arguments[0], true);
                                    var slice = GenerateExpression(expr.SubExpression, false);

                                    var range_begin = builder.CreateExtractValue(range, 0, "range_begin");
                                    range_begin = builder.CreateIntCast(range_begin, LLVM.Int64Type(), "range_begin_int");

                                    var range_end = builder.CreateExtractValue(range, 1, "range_end");
                                    range_end = builder.CreateIntCast(range_end, LLVM.Int64Type(), "range_end_int");

                                    var dataPtrPtr = builder.CreateStructGEP(slice, 1, "slice_data_ptr");
                                    var dataPtr = builder.CreateLoad(dataPtrPtr, "slice_data");

                                    var length_ptr = builder.CreateStructGEP(slice, 0, "slice_length_ptr");
                                    length_ptr = builder.CreateLoad(length_ptr, "slice_length");

                                    var dataOffset = builder.CreateMul(range_begin, LLVM.ConstInt(LLVM.Int64Type(), (ulong)s.TargetType.GetSize(), false), "");
                                    dataPtr = builder.CreatePtrToInt(dataPtr, LLVM.Int64Type(), "");
                                    dataPtr = builder.CreateAdd(dataPtr, dataOffset, "data_new");
                                    dataPtr = builder.CreateIntToPtr(dataPtr, CheezTypeToLLVMType(PointerType.GetPointerType(s.TargetType)), "data_new_ptr");
                                    length_ptr = builder.CreateSub(range_end, range_begin, "length_new");

                                    var result = builder.CreateInsertValue(LLVM.GetUndef(CheezTypeToLLVMType(s)), length_ptr, 0, "result");
                                    result = builder.CreateInsertValue(result, dataPtr, 1, "result");

                                    return result;
                                }

                            default:
                                throw new NotImplementedException();
                        }
                    }

                case ArrayType s:
                    {
                        var index = GenerateExpression(expr.Arguments[0], true);
                        var arr = GenerateExpression(expr.SubExpression, false);

                        var dataPtr = builder.CreatePointerCast(arr, CheezTypeToLLVMType(s.ToPointerType()), "");

                        var ptr = builder.CreateInBoundsGEP(dataPtr, new LLVMValueRef[] { index }, "");

                        var val = ptr;
                        if (deref)
                            val = builder.CreateLoad(ptr, "");
                        return val;
                    }

                case PointerType p:
                    {
                        var index = GenerateExpression(expr.Arguments[0], true);
                        var pointer = GenerateExpression(expr.SubExpression, true);

                        var ptr = builder.CreateInBoundsGEP(pointer, new LLVMValueRef[] { index }, "");

                        var val = ptr;
                        if (deref)
                            val = builder.CreateLoad(ptr, "");
                        return val;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private LLVMValueRef GenerateStructValueExpr(AstStructValueExpr expr)
        {
            var str = LLVM.GetUndef(CheezTypeToLLVMType(expr.Type));

            foreach (var mem in expr.MemberInitializers)
            {
                var v = GenerateExpression(mem.Value, true);
                str = builder.CreateInsertValue(str, v, (uint)mem.Index, "");
            }

            return str;
        }

        private LLVMValueRef GenerateDotExpr(AstDotExpr expr, bool deref)
        {
            var type = expr.Left.Type;
            var value = GenerateExpression(expr.Left, false);

            while (type is PointerType p)
            {
                type = p.TargetType;
                value = builder.CreateLoad(value, "");
            }

            switch (type)
            {
                case AnyType _:
                    {
                        uint index = 0;

                        switch (expr.Right.Name)
                        {
                            case "typ": index = 0; break;
                            case "val": index = 1; break;
                            default: throw new NotImplementedException();
                        }

                        LLVMValueRef result;
                        if (!expr.Left.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(value, index, "");
                        }
                        else
                        {
                            result = builder.CreateStructGEP(value, index, "");
                            if (deref)
                                result = builder.CreateLoad(result, "");
                        }

                        return result;
                    }

                case RangeType range:
                    {
                        uint index = 0;

                        switch (expr.Right.Name)
                        {
                            case "start": index = 0; break;
                            case "end": index = 1; break;
                            default: throw new NotImplementedException();
                        }

                        LLVMValueRef result;
                        if (!expr.Left.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(value, index, "");
                        }
                        else
                        {
                            result = builder.CreateStructGEP(value, index, "");
                            if (deref)
                                result = builder.CreateLoad(result, "");
                        }

                        return result;
                    }

                case EnumType @enum:
                    {
                        var memName = expr.Right.Name;
                        var mem = @enum.Declaration.Members.FirstOrDefault(m => m.Name == memName);

                        var assType = CheezTypeToLLVMType(PointerType.GetPointerType(mem.AssociatedTypeExpr.Value as CheezType));

                        var subPtr = builder.CreateStructGEP(value, 1, "");
                        subPtr = builder.CreatePointerCast(subPtr, assType, "");

                        if (deref)
                        {
                            var v = builder.CreateLoad(subPtr, "");
                            return v;
                        }

                        return subPtr;
                    }

                case TupleType t:
                    {
                        var index = t.Members.IndexOf(m => m.name == expr.Right.Name);

                        LLVMValueRef result;
                        if (!expr.Left.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(value, (uint)index, "");
                        }
                        else
                        {
                            result = builder.CreateStructGEP(value, (uint)index, "");
                            if (deref)
                                result = builder.CreateLoad(result, "");
                        }

                        return result;
                    }

                case SliceType slice:
                    {
                        if (expr.Right.Name == "data")
                        {
                            var dataPtrPtr = builder.CreateStructGEP(value, 1, "");
                            if (!deref) return dataPtrPtr;
                            var dataPtr = builder.CreateLoad(dataPtrPtr, "");
                            return dataPtr;
                        }
                        else if (expr.Right.Name == "length")
                        {
                            var lengthPtr = builder.CreateStructGEP(value, 0, "");
                            if (!deref) return lengthPtr;
                            var length = builder.CreateLoad(lengthPtr, "");
                            return length;
                        }
                        break;
                    }

                case ArrayType arr:
                    {
                        if (expr.Right.Name == "data")
                        {
                            var dataPtr = builder.CreateStructGEP(value, 1, "");
                            return dataPtr;
                        }
                        else if (expr.Right.Name == "length")
                        {
                            return LLVM.ConstInt(LLVM.Int64Type(), (ulong)arr.Length, false);
                        }
                        break;
                    }

                case StructType @struct:
                    {
                        var index = @struct.GetIndexOfMember(expr.Right.Name);
                        
                        if (!expr.Left.GetFlag(ExprFlags.IsLValue))
                        {
                            var data = builder.CreateExtractValue(value, (uint)index, "");
                            return data;
                        }

                        var dataPtr = builder.CreateStructGEP(value, (uint)index, "");

                        var result = dataPtr;
                        if (deref) result = builder.CreateLoad(dataPtr, "");

                        return result;
                    }

                case TraitType trait:
                    {
                        var decl = trait.Declaration;
                        var member = trait.Declaration.Variables.First(v => v.Name.Name == expr.Right.Name);

                        var vtablePtr = builder.CreateStructGEP(value, 0, "");
                        vtablePtr = builder.CreateLoad(vtablePtr, "");
                        var toPointer = builder.CreateStructGEP(value, 1, "");
                        toPointer = builder.CreateLoad(toPointer, "");

                        // check if pointer is null
                        if (checkForNullTraitObjects)
                        {
                            CheckPointerNull(vtablePtr, expr.Left, "vtable pointer of trait object is null");
                            CheckPointerNull(toPointer, expr.Right, "object pointer of trait object is null");
                        }

                        // load function pointer
                        var vtableType = vtableTypes[trait];
                        vtablePtr = builder.CreatePointerCast(vtablePtr, vtableType.GetPointerTo(), "");

                        var memberOffsetIndex = vtableIndices[member];
                        var memberOffsetPointer = builder.CreateStructGEP(vtablePtr, (uint)memberOffsetIndex, "");
                        var memberOffset = builder.CreateLoad(memberOffsetPointer, "");

                        toPointer = builder.CreateCast(LLVMOpcode.LLVMPtrToInt, toPointer, LLVM.Int64Type(), "");
                        var result = builder.CreateAdd(toPointer, memberOffset, "");
                        result = builder.CreateCast(LLVMOpcode.LLVMIntToPtr, result, CheezTypeToLLVMType(PointerType.GetPointerType(member.Type)), "");
                        if (deref) result = builder.CreateLoad(result, "");
                        return result;
                    }
            }
            throw new NotImplementedException();
        }

        private LLVMValueRef GenerateTupleExpr(AstTupleExpr expr)
        {
            var tuple = LLVM.GetUndef(CheezTypeToLLVMType(expr.Type));

            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = GenerateExpression(expr.Values[i], true);
                tuple = builder.CreateInsertValue(tuple, v, (uint)i, "");
            }

            return tuple;
        }

        private LLVMValueRef GenerateCharLiteralExpr(AstCharLiteral expr)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(CheezTypeToLLVMType(expr.Type), ch, true);
            return val;
        }

        private LLVMValueRef GenerateStringLiteralExpr(AstStringLiteral expr)
        {
            var ch = expr.StringValue;

            if (expr.Type == CheezType.CString)
            {
                return builder.CreateGlobalStringPtr(ch, "");
            }
            else if (expr.Type == CheezType.String)
            {
                var str = builder.CreateGlobalString(ch, "");
                return LLVM.ConstNamedStruct(CheezTypeToLLVMType(expr.Type), new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int64Type(), (ulong)ch.Length, true),
                    LLVM.ConstPointerCast(str, LLVM.PointerType(LLVM.Int8Type(), 0))
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private LLVMValueRef GenerateBoolExpr(AstBoolExpr expr)
        {
            var llvmType = CheezTypeToLLVMType(expr.Type);
            return LLVM.ConstInt(llvmType, expr.BoolValue ? 1u : 0u, false);
        }

        private LLVMValueRef GenerateNumberExpr(AstNumberExpr expr)
        {
            var llvmType = CheezTypeToLLVMType(expr.Type);
            if (expr.Type is IntType i)
            {
                var val = expr.Data.ToUlong();
                return LLVM.ConstInt(llvmType, val, i.Signed);
            }
            else
            {
                var val = expr.Data.ToDouble();
                var result = LLVM.ConstReal(llvmType, val);
                return result;
            }
        }

        private LLVMValueRef GenerateIdExpr(AstIdExpr expr, bool deref)
        {
            LLVMValueRef v;
            if (expr.Symbol is AstFuncExpr func)
            {
                //v =
                v = valueMap[func];
                return v;
            }
            else if (expr.Symbol is AstDecl decl)
            {
                v = valueMap[decl];
            }
            else if (expr.Symbol is Using u)
            {
                v = GenerateExpression(u.Expr, false);
            }
            else
            {
                v = valueMap[expr.Symbol];
            }

            if (deref)
                v = builder.CreateLoad(v, "");

            return v;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                builder.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
