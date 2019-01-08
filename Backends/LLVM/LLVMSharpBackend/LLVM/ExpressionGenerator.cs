using Cheez.Ast.Expressions;
using Cheez.Types.Primitive;
using LLVMSharp;
using System;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public partial class LLVMCodeGenerator
    {

        private LLVMValueRef GenerateExpression(AstExpression stmt, bool deref)
        {
            switch (stmt)
            {
                case AstNumberExpr n: return GenerateNumberExpr(n);
                case AstIdExpr i: return GenerateIdExpr(i, deref);
            }
            return default;
        }

        public LLVMValueRef VisitCharLiteralExpr(AstCharLiteral expr, LLVMCodeGeneratorNewContext data = default)
        {
            var ch = expr.CharValue;
            var val = LLVM.ConstInt(LLVMTypeRef.Int8Type(), ch, true);
            return val;
        }

        public LLVMValueRef VisitStringLiteralExpr(AstStringLiteral expr, LLVMCodeGeneratorNewContext data = default)
        {
            throw new NotImplementedException();
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

        public LLVMValueRef GenerateIdExpr(AstIdExpr expr, bool deref)
        {
            var v = valueMap[expr.Symbol];
            if (deref)
                return builder.CreateLoad(v, "");
            return v;
        }

        //public override LLVMLLVMValueRef VisitStructValueExpression(AstStructValueExpr str, object data = null)
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
