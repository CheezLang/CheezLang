using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
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

        private void GenerateExpressionHelper(AstExpression expr, LLVMValueRef? target, bool deref)
        {
            var v = GenerateExpression(expr, target, true);
            if (v != null && target != null)
                builder.CreateStore(v.Value, target.Value);
        }

        private LLVMValueRef? GenerateExpression(AstExpression expr, LLVMValueRef? target, bool deref)
        {
            if (expr.Value != null)
            {
                return CheezValueToLLVMValue(expr.Type, expr.Value);
            }

            switch (expr)
            {
                case AstArgument a: return GenerateExpression(a.Expr, target, deref);
                case AstNumberExpr n: return GenerateNumberExpr(n);
                case AstBoolExpr n: return GenerateBoolExpr(n);
                case AstIdExpr i: return GenerateIdExpr(i, target, deref);
                case AstCharLiteral ch: return GenerateCharLiteralExpr(ch);
                case AstStringLiteral ch: return GenerateStringLiteralExpr(ch);
                case AstTupleExpr t: return GenerateTupleExpr(t, target, deref);
                case AstDotExpr t: return GenerateDotExpr(t, target, deref);
                case AstArrayAccessExpr t: return GenerateIndexExpr(t, target, deref);
                case AstStructValueExpr t: return GenerateStructValueExpr(t, target, deref);
                case AstCallExpr c: return GenerateCallExpr(c, target, deref);
                case AstUnaryExpr u: return GenerateUnaryExpr(u, target, deref);
                case AstTempVarExpr t: return GenerateTempVarExpr(t, target, deref);
                case AstSymbolExpr s: return GenerateSymbolExpr(s, target, deref);
                case AstBlockExpr block: return GenerateBlock(block, target, deref);
                case AstNullExpr nll: return LLVM.ConstPointerNull(CheezTypeToLLVMType(nll.Type));
                case AstAddressOfExpr ao: return GenerateAddressOf(ao, target, deref);
                case AstIfExpr iff: return GenerateIfExpr(iff, target, deref);
                case AstBinaryExpr bin: return GenerateBinaryExpr(bin, target, deref);
                case AstDereferenceExpr de: return GenerateDerefExpr(de, target, deref);
                case AstCastExpr cast: return GenerateCastExpr(cast, target, deref);
            }
            throw new NotImplementedException();
        }

        private LLVMValueRef? GenerateCastExpr(AstCastExpr cast, LLVMValueRef? target, bool deref)
        {
            var to = cast.Type;
            var from = cast.SubExpression.Type;

            var sub = GenerateExpression(cast.SubExpression, null, true);

            if (to == from) return sub;

            var toLLVM = CheezTypeToLLVMType(to);

            if (to is IntType && from is IntType) // int <- int
                return builder.CreateIntCast(sub.Value, toLLVM, "");
            if (to is PointerType && from is IntType) // * <- int
                return builder.CreateCast(LLVMOpcode.LLVMIntToPtr, sub.Value, toLLVM, "");
            if (to is IntType && from is PointerType) // int <- *
                return builder.CreateCast(LLVMOpcode.LLVMPtrToInt, sub.Value, toLLVM, "");
            if (to is PointerType && from is PointerType) // * <- *
                return builder.CreatePointerCast(sub.Value, toLLVM, "");
            if (to is FloatType && from is FloatType) // float <- float
                return builder.CreateFPCast(sub.Value, toLLVM, "");
            if (to is IntType i && from is FloatType) // int <- float
                return builder.CreateCast(i.Signed ? LLVMOpcode.LLVMFPToSI : LLVMOpcode.LLVMFPToUI, sub.Value, toLLVM, "");
            if (to is FloatType && from is IntType i2) // float <- int
                return builder.CreateCast(i2.Signed ? LLVMOpcode.LLVMSIToFP : LLVMOpcode.LLVMUIToFP, sub.Value, toLLVM, "");
            if (to is IntType && from is BoolType)
                return builder.CreateZExt(sub.Value, toLLVM, "");
            
            throw new NotImplementedException();
        }

        private LLVMValueRef? GenerateDerefExpr(AstDereferenceExpr de, LLVMValueRef? target, bool deref)
        {
            var sub = GenerateExpression(de.SubExpression, null, true);

            if (!deref) return sub;

            var val = builder.CreateLoad(sub.Value, "");

            if (target != null)
            {
                builder.CreateStore(val, target.Value);
                return null;
            }

            return val;
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
            };



        private Dictionary<string, Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>> builtInPointerOperators
            = new Dictionary<string, Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>>
            {
                { "==", GetICompare(LLVMIntPredicate.LLVMIntEQ) },
                { "!=", GetICompare(LLVMIntPredicate.LLVMIntNE) },
            };
        private LLVMValueRef? GenerateBinaryExpr(AstBinaryExpr bin, LLVMValueRef? target, bool deref)
        {
            if (bin.ActualOperator is BuiltInOperator)
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
                    var left = GenerateExpression(bin.Left, null, true);
                    var right = GenerateExpression(bin.Right, null, true);
                    var bo = builtInOperators[(bin.Operator, bin.Left.Type)];
                    var val = bo(GetRawBuilder(), left.Value, right.Value, "");
                    return val;
                }
            }
            else if (bin.ActualOperator is BuiltInPointerOperator)
            {
                var left = GenerateExpression(bin.Left, null, true);
                var right = GenerateExpression(bin.Right, null, true);
                var bo = builtInPointerOperators[bin.Operator];
                var val = bo(GetRawBuilder(), left.Value, right.Value, "");
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

            GenerateExpressionHelper(bin.Left, result, true);
            builder.CreateCondBr(builder.CreateLoad(result, ""), bbRight, bbEnd);

            builder.PositionBuilderAtEnd(bbRight);
            GenerateExpressionHelper(bin.Right, result, true);
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

            GenerateExpressionHelper(bin.Left, result, true);
            builder.CreateCondBr(builder.CreateLoad(result, ""), bbEnd, bbRight);

            builder.PositionBuilderAtEnd(bbRight);
            GenerateExpressionHelper(bin.Right, result, true);
            builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbEnd);

            result = builder.CreateLoad(result, "");
            return result;
        }

        private LLVMValueRef? GenerateIfExpr(AstIfExpr iff, LLVMValueRef? target, bool deref)
        {
            if (iff.PreAction != null)
            {
                GenerateVariableDecl(iff.PreAction);
            }

            LLVMValueRef? result = target;
            if (iff.Type != CheezType.Void && result == null) result = CreateLocalVariable(iff.Type);

            var cond = GenerateExpression(iff.Condition, null, true);

            var bbIf = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_true");
            var bbElse = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_false");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_end");

            builder.CreateCondBr(cond.Value, bbIf, bbElse);

            builder.PositionBuilderAtEnd(bbIf);
            GenerateExpressionHelper(iff.IfCase, result, true);

            if (!iff.IfCase.GetFlag(ExprFlags.Returns))
                builder.CreateBr(bbEnd);

            builder.PositionBuilderAtEnd(bbElse);
            if (iff.ElseCase != null)
            {
                GenerateExpressionHelper(iff.ElseCase, result, true);
                if (!iff.ElseCase.GetFlag(ExprFlags.Returns))
                    builder.CreateBr(bbEnd);
            }
            else
            {
                builder.CreateBr(bbEnd);
            }

            builder.PositionBuilderAtEnd(bbEnd);

            return null;
        }

        private LLVMValueRef? GenerateAddressOf(AstAddressOfExpr ao, LLVMValueRef? target, bool deref)
        {
            var ptr = GenerateExpression(ao.SubExpression, null, false);
            return ptr;
        }

        private LLVMValueRef? GenerateBlock(AstBlockExpr block, LLVMValueRef? target, bool deref)
        {
            LLVMValueRef? result = null;

            int end = block.Statements.Count;
            if (block.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                GenerateStatement(block.Statements[i]);
            }

            if (block.Statements.LastOrDefault() is AstExprStmt expr)
            {
                result = GenerateExpression(expr.Expr, target, deref);
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                GenerateStatement(block.DeferredStatements[i]);
            }

            return result;
        }

        private LLVMValueRef? GenerateSymbolExpr(AstSymbolExpr s, LLVMValueRef? target, bool deref)
        {
            var v = valueMap[s.Symbol];

            if (deref)
            {
                v = builder.CreateLoad(v, "");
            }

            if (target != null)
            {
                builder.CreateStore(v, target.Value);
                return null;
            }

            return v;
        }

        private LLVMValueRef? GenerateTempVarExpr(AstTempVarExpr t, LLVMValueRef? target, bool deref)
        {
            if (!valueMap.ContainsKey(t))
            {
                var x = CreateLocalVariable(t.Type);
                valueMap[t] = x;
                GenerateExpressionHelper(t.Expr, x, true);
            }

            var tmp = valueMap[t];

            if (deref)
            {
                tmp = builder.CreateLoad(tmp, "");
            }

            if (target != null)
            {
                builder.CreateStore(tmp, target.Value);
                return null;
            }

            return tmp;
        }

        private LLVMValueRef? GenerateUnaryExpr(AstUnaryExpr expr, LLVMValueRef? target, bool deref)
        {
            if (expr.Operator == "-")
            {
                var val = GenerateExpression(expr.SubExpr, null, true);
                if (val == null) throw new Exception("Bug! this should not be null");
                if (expr.Type is IntType)
                {
                    return builder.CreateNeg(val.Value, "");
                }
                else if (expr.Type is FloatType)
                {
                    return builder.CreateFNeg(val.Value, "");
                }
            }
            return null;
        }

        private LLVMValueRef? GenerateCallExpr(AstCallExpr c, LLVMValueRef? target, bool deref)
        {
            LLVMValueRef? func = null;
            if (c.Declaration != null)
            {
                func = valueMap[c.Declaration];
            }
            else
            {
                func = GenerateExpression(c.Function, null, false);
            }

            // arguments
            var args = c.Arguments.Select(a => GenerateExpression(a, null, true).Value).ToArray();

            var call = builder.CreateCall(func.Value, args, "");
            var callConv = LLVM.GetFunctionCallConv(func.Value);
            LLVM.SetInstructionCallConv(call, callConv);

            return call;
        }

        private LLVMValueRef? GenerateIndexExpr(AstArrayAccessExpr expr, LLVMValueRef? target, bool deref)
        {
            switch (expr.SubExpression.Type)
            {
                case TupleType t:
                    {
                        var index = ((NumberData)expr.Indexer.Value).ToLong();
                        var left = GenerateExpression(expr.SubExpression, null, false);

                        LLVMValueRef? result;
                        if (!expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(left.Value, (uint)index, "");
                        }
                        else
                        {
                            result = builder.CreateStructGEP(left.Value, (uint)index, "");
                            if (deref)
                                result = builder.CreateLoad(result.Value, "");
                        }

                        if (target != null)
                        {
                            builder.CreateStore(result.Value, target.Value);
                            return null;
                        }

                        return result;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public LLVMValueRef? GenerateStructValueExpr(AstStructValueExpr expr, LLVMValueRef? target, bool deref)
        {
            if (target != null)
            {
                foreach (var mem in expr.MemberInitializers)
                {
                    var ptr = builder.CreateStructGEP(target.Value, (uint)mem.Index, "");
                    GenerateExpressionHelper(mem.Value, ptr, false);
                }

                return null;
            }

            throw new NotImplementedException();
        }

        public LLVMValueRef? GenerateDotExpr(AstDotExpr expr, LLVMValueRef? target, bool deref)
        {
            var type = expr.Left.Type;
            var value = GenerateExpression(expr.Left, null, false).Value;

            if (!expr.IsDoubleColon)
            {
                while (type is PointerType p)
                {
                    type = p.TargetType;
                    value = builder.CreateLoad(value, "");
                }
            }

            switch (type)
            {
                case TupleType t:
                    {
                        var index = t.Members.IndexOf(m => m.name == expr.Right.Name);

                        LLVMValueRef? result;
                        if (!expr.Left.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(value, (uint)index, "");
                        }
                        else
                        {
                            result = builder.CreateStructGEP(value, (uint)index, "");
                            if (deref)
                                result = builder.CreateLoad(result.Value, "");
                        }

                        if (target != null)
                        {
                            builder.CreateStore(result.Value, target.Value);
                            return null;
                        }

                        return result;
                    }

                case SliceType slice:
                    {
                        if (expr.Right.Name == "data")
                        {
                            var dataPtrPtr = builder.CreateStructGEP(value, 1, "");
                            var dataPtr = builder.CreateLoad(dataPtrPtr, "");
                            return dataPtr;
                        }
                        else if (expr.Right.Name == "length")
                        {
                            var lengthPtr = builder.CreateStructGEP(value, 0, "");
                            var length = builder.CreateLoad(lengthPtr, "");
                            return length;
                        }
                        break;
                    }

                case StructType @struct:
                    {
                        var index = @struct.GetIndexOfMember(expr.Right.Name);
                        
                        var dataPtr = builder.CreateStructGEP(value, (uint)index, "");

                        var result = dataPtr;
                        if (deref) result = builder.CreateLoad(dataPtr, "");

                        return result;
                    }
            }
            throw new NotImplementedException();
        }

        public LLVMValueRef? GenerateTupleExpr(AstTupleExpr expr, LLVMValueRef? maybeTarget, bool deref)
        {
            if (maybeTarget != null)
            {
                var target = maybeTarget.Value;
                for (int i = 0; i < expr.Values.Count; i++)
                {
                    var memberPtr = builder.CreateStructGEP(target, (uint)i, "");
                    var v = GenerateExpression(expr.Values[i], memberPtr, true);
                    if (v != null)
                    {
                        builder.CreateStore(v.Value, memberPtr);
                    }
                }
                return null;
            }
            else
            {
                var tempVar = CreateLocalVariable(expr.Type);
                for (int i = 0; i < expr.Values.Count; i++)
                {
                    var memberPtr = builder.CreateStructGEP(tempVar, (uint)i, "");
                    var v = GenerateExpression(expr.Values[i], memberPtr, true);
                    if (v != null)
                    {
                        builder.CreateStore(v.Value, memberPtr);
                    }
                }

                if (deref)
                    tempVar = builder.CreateLoad(tempVar, "");
                return tempVar;
            }
        }

        public LLVMValueRef GenerateCharLiteralExpr(AstCharLiteral expr)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(LLVMTypeRef.Int8Type(), ch, true);
            return val;
        }

        public LLVMValueRef? GenerateStringLiteralExpr(AstStringLiteral expr)
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
                    LLVM.ConstInt(LLVM.Int32Type(), (ulong)ch.Length, true),
                    LLVM.ConstPointerCast(str, LLVM.PointerType(LLVM.Int8Type(), 0))
                });
            }
            else
            {
                throw new NotImplementedException();
            }
            
            return null;
        }

        public LLVMValueRef VisitStringLiteralExpr(AstStringLiteral expr)
        {
            throw new NotImplementedException();
        }

        public LLVMValueRef GenerateBoolExpr(AstBoolExpr expr)
        {
            var llvmType = CheezTypeToLLVMType(expr.Type);
            return LLVM.ConstInt(llvmType, expr.BoolValue ? 1u : 0u, false);
        }

        public LLVMValueRef GenerateNumberExpr(AstNumberExpr num)
        {
            var llvmType = CheezTypeToLLVMType(num.Type);
            if (num.Type is IntType)
            {
                var val = num.Data.ToUlong();
                return LLVM.ConstInt(llvmType, val, false);
            }
            else
            {
                var val = num.Data.ToDouble();
                var result = LLVM.ConstReal(llvmType, val);
                return result;
            }
        }

        public LLVMValueRef? GenerateIdExpr(AstIdExpr expr, LLVMValueRef? maybeTarget, bool deref)
        {
            LLVMValueRef v;
            if (expr.Symbol is AstDecl decl)
            {
                v = valueMap[decl];
            }
            else if (expr.Symbol is Using u)
            {
                v = GenerateExpression(u.Expr, null, false).Value;
            }
            else
            {
                v = valueMap[expr.Symbol];
            }

            if (deref)
                v = builder.CreateLoad(v, "");

            if (maybeTarget != null)
            {
                builder.CreateStore(v, maybeTarget.Value);
                return null;
            }
            return v;
        }

        //public override LLVMValueRef VisitStructValueExpression(AstStructValueExpr str)
        //{
        //    var value = GetTempValue(str.Type);

        //    var llvmType = CheezTypeToLLVMType(str.Type);

        //    foreach (var m in str.MemberInitializers)
        //    {
        //        var v = m.Value.Accept(this, data);
        //        var memberPtr = builder.CreateStructGEP(value, (uint)m.Index, "");
        //        var s = builder.CreateStore(v, memberPtr);
        //    }

        //    return value;
        //}
    }
}
