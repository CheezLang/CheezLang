using Cheez.Ast.Expressions;
using Cheez.Extras;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using LLVMSharp;
using System;
using System.Linq;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator
    {

        private LLVMValueRef? GenerateExpression(LLVMValueRef ptr, AstExpression stmt, LLVMValueRef? target, bool deref)
        {
            var v = GenerateExpression(stmt, target, true);
            if (v != null)
                builder.CreateStore(v.Value, ptr);

            return target ?? v;
        }

        private LLVMValueRef? GenerateExpression(AstExpression stmt, LLVMValueRef? target, bool deref)
        {
            switch (stmt)
            {
                case AstArgument a: return GenerateExpression(a.Expr, target, deref);
                case AstNumberExpr n: return GenerateNumberExpr(n);
                case AstBoolExpr n: return GenerateBoolExpr(n);
                case AstIdExpr i: return GenerateIdExpr(i, target, deref);
                case AstCharLiteral ch: return GenerateCharLiteralExpr(ch);
                case AstTupleExpr t: return GenerateTupleExpr(t, target, deref);
                case AstDotExpr t: return GenerateDotExpr(t, target, deref);
                case AstArrayAccessExpr t: return GenerateIndexExpr(t, target, deref);
                case AstStructValueExpr t: return GenerateStructValueExpr(t, target, deref);
                case AstCallExpr c: return GenerateCallExpr(c, target, deref);
            }
            throw new NotImplementedException();
        }

        private LLVMValueRef? GenerateCallExpr(AstCallExpr c, LLVMValueRef? target, bool deref)
        {
            var f = GenerateExpression(c.Function, target, false);

            // arguments
            var args = c.Arguments.Select(a => GenerateExpression(a, null, true).Value).ToArray();

            return builder.CreateCall(f.Value, args, "");
        }

        private LLVMValueRef? GenerateIndexExpr(AstArrayAccessExpr expr, LLVMValueRef? target, bool deref)
        {
            switch (expr.SubExpression.Type)
            {
                case TupleType t:
                    {
                        var index = ((NumberData)expr.Indexer.Value).ToLong();
                        var left = GenerateExpression(expr.SubExpression, null, false);

                        if (expr.SubExpression.GetFlag(ExprFlags.IsLValue))
                        {
                            return builder.CreateExtractValue(left.Value, (uint)index, "");
                        }

                        var ptr = builder.CreateStructGEP(left.Value, (uint)index, "");
                        if (deref)
                        {
                            ptr = builder.CreateLoad(ptr, "");
                        }

                        return ptr;
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
                    GenerateExpression(ptr, mem.Value, target, false);
                }

                return null;
            }

            throw new NotImplementedException();
        }

        public LLVMValueRef? GenerateDotExpr(AstDotExpr expr, LLVMValueRef? maybeTarget, bool deref)
        {
            switch (expr.Left.Type)
            {
                default:
                    throw new NotImplementedException();
            }
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
            if (expr.Symbol is CompTimeVariable ct)
            {
                v = valueMap[ct.Declaration];
            }
            else
            {
                v = valueMap[expr.Symbol];
            }

            if (maybeTarget != null)
            {
                v = builder.CreateLoad(v, "");
                builder.CreateStore(v, maybeTarget.Value);
                return null;
            }

            if (deref)
                return builder.CreateLoad(v, "");
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
