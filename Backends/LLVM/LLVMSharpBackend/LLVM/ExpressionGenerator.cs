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
                case AstCastExpr cast: return GenerateCastExpr(cast, deref);
                case AstUfcFuncExpr ufc: return GenerateUfcFuncExpr(ufc, deref);
                case AstArrayExpr arr: return GenerateArrayExpr(arr, deref);
            }
            throw new NotImplementedException();
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

        private LLVMValueRef GenerateUfcFuncExpr(AstUfcFuncExpr ufc, bool deref)
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
            else if (expr.Type is SliceType s)
            {
                return LLVM.ConstNamedStruct(CheezTypeToLLVMType(expr.Type), new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int64Type(), 0, false),
                    LLVM.ConstPointerNull(CheezTypeToLLVMType(s.TargetType))
                });
            }
            else throw new NotImplementedException();
        }

        private LLVMValueRef GenerateCastExpr(AstCastExpr cast, bool deref)
        {
            var to = cast.Type;
            var from = cast.SubExpression.Type;
            var toLLVM = CheezTypeToLLVMType(to);

            if (to is TraitType trait)
            {
                var ptr = GenerateExpression(cast.SubExpression, false);
                ptr = builder.CreatePointerCast(ptr, pointerType, "");

                var vtablePtr = vtableMap[from];

                var traitObject = LLVM.GetUndef(toLLVM);
                traitObject = builder.CreateInsertValue(traitObject, vtablePtr, 0, "");
                traitObject = builder.CreateInsertValue(traitObject, ptr, 1, "");
                return traitObject;
            }

            if (to == from) return GenerateExpression(cast.SubExpression, true);
            
            if (to is IntType && from is IntType) // int <- int
                return builder.CreateIntCast(GenerateExpression(cast.SubExpression, true), toLLVM, "");
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
            if (iff.PreAction != null)
            {
                GenerateVariableDecl(iff.PreAction);
            }

            LLVMValueRef result = LLVM.GetUndef(CheezTypeToLLVMType(iff.Type));
            if (iff.Type != CheezType.Void) result = CreateLocalVariable(iff.Type);

            var cond = GenerateExpression(iff.Condition, true);

            var bbIf = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_true");
            var bbElse = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_false");
            var bbEnd = LLVM.AppendBasicBlock(currentLLVMFunction, "_if_end");

            builder.CreateCondBr(cond, bbIf, bbElse);

            builder.PositionBuilderAtEnd(bbIf);
            if (iff.Type != CheezType.Void)
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
                if (iff.Type != CheezType.Void)
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
            LLVMValueRef result = LLVM.GetUndef(CheezTypeToLLVMType(block.Type));

            int end = block.Statements.Count;
            if (block.Statements.LastOrDefault() is AstExprStmt) --end;

            for (int i = 0; i < end; i++)
            {
                GenerateStatement(block.Statements[i]);
            }

            if (block.Statements.LastOrDefault() is AstExprStmt expr)
            {
                result = GenerateExpression(expr.Expr, deref);
            }

            for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
            {
                GenerateStatement(block.DeferredStatements[i]);
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
            if (c.Declaration?.IsTraitFunction ?? false)
            {
                // call to a trait function
                // get function pointer from trait object
                var functionIndex = vtableIndices[c.Declaration];
                var funcType = CheezTypeToLLVMType(c.Declaration.Type);

                var selfArg = GenerateExpression(c.Arguments[0], true);
                selfArg = builder.CreateLoad(selfArg, "");

                var vtablePtr = builder.CreateExtractValue(selfArg, 0, "");
                var toPointer = builder.CreateExtractValue(selfArg, 1, "");
                toPointer = builder.CreatePointerCast(toPointer, funcType.GetParamTypes()[0], "");

                // load function pointer
                vtablePtr = builder.CreatePointerCast(vtablePtr, vtableType.GetPointerTo(), "");

                var funcPointer = builder.CreateStructGEP(vtablePtr, (uint)functionIndex, "");
                funcPointer = builder.CreateLoad(funcPointer, "");

                var arguments = new List<LLVMValueRef>();
                // self arg
                arguments.Add(toPointer);

                // rest of arguments
                foreach (var a in c.Arguments.Skip(1))
                    arguments.Add(GenerateExpression(a, true));

                var traitCall = builder.CreateCall(funcPointer, arguments.ToArray(), "");
                LLVM.SetInstructionCallConv(traitCall, LLVM.GetFunctionCallConv(funcPointer));

                return traitCall;
            }


            LLVMValueRef func;
            if (c.Declaration != null)
            {
                func = valueMap[c.Declaration];
            }
            else
            {
                func = GenerateExpression(c.Function, false);
            }

            // arguments
            var args = c.Arguments.Select(a => GenerateExpression(a, true)).ToArray();

            var call = builder.CreateCall(func, args, "");
            var callConv = LLVM.GetFunctionCallConv(func);
            LLVM.SetInstructionCallConv(call, callConv);

            return call;
        }

        private LLVMValueRef GenerateIndexExpr(AstArrayAccessExpr expr, bool deref)
        {
            switch (expr.SubExpression.Type)
            {
                case TupleType t:
                    {
                        var index = ((NumberData)expr.Indexer.Value).ToLong();
                        var left = GenerateExpression(expr.SubExpression, false);

                        LLVMValueRef result;
                        if (!expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                        {
                            result = builder.CreateExtractValue(left, (uint)index, "");
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
                        var index = GenerateExpression(expr.Indexer, true);
                        var slice = GenerateExpression(expr.SubExpression, false);

                        var dataPtrPtr = builder.CreateStructGEP(slice, 1, "");
                        var dataPtr = builder.CreateLoad(dataPtrPtr, "");

                        var ptr = builder.CreateInBoundsGEP(dataPtr, new LLVMValueRef[] { index }, "");

                        var val = ptr;
                        if (deref)
                            val = builder.CreateLoad(ptr, "");
                        return val;
                    }

                case PointerType p:
                    {
                        var index = GenerateExpression(expr.Indexer, true);
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

        public LLVMValueRef GenerateStructValueExpr(AstStructValueExpr expr)
        {
            var str = LLVM.GetUndef(CheezTypeToLLVMType(expr.Type));

            foreach (var mem in expr.MemberInitializers)
            {
                var v = GenerateExpression(mem.Value, true);
                str = builder.CreateInsertValue(str, v, (uint)mem.Index, "");
            }

            return str;
        }

        public LLVMValueRef GenerateDotExpr(AstDotExpr expr, bool deref)
        {
            var type = expr.Left.Type;
            var value = GenerateExpression(expr.Left, false);

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

        public LLVMValueRef GenerateTupleExpr(AstTupleExpr expr)
        {
            var tuple = LLVM.GetUndef(CheezTypeToLLVMType(expr.Type));

            for (int i = 0; i < expr.Values.Count; i++)
            {
                var v = GenerateExpression(expr.Values[i], true);
                tuple = builder.CreateInsertValue(tuple, v, (uint)i, "");
            }

            return tuple;
        }

        public LLVMValueRef GenerateCharLiteralExpr(AstCharLiteral expr)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(CheezTypeToLLVMType(expr.Type), ch, true);
            return val;
        }

        public LLVMValueRef GenerateStringLiteralExpr(AstStringLiteral expr)
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

        public LLVMValueRef VisitStringLiteralExpr(AstStringLiteral expr)
        {
            throw new NotImplementedException();
        }

        public LLVMValueRef GenerateBoolExpr(AstBoolExpr expr)
        {
            var llvmType = CheezTypeToLLVMType(expr.Type);
            return LLVM.ConstInt(llvmType, expr.BoolValue ? 1u : 0u, false);
        }

        public LLVMValueRef GenerateNumberExpr(AstNumberExpr expr)
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

        public LLVMValueRef GenerateIdExpr(AstIdExpr expr, bool deref)
        {
            LLVMValueRef v;
            if (expr.Symbol is AstDecl decl)
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
    }
}
