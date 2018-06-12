using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public class PTErrorTypeExpr : PTExpr
    {
        public PTErrorTypeExpr(TokenLocation beg, TokenLocation end = null) : base(beg, end ?? beg)
        {
        }

        public override AstExpression CreateAst()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "§";
        }
    }

    public class PTPointerTypeExpr : PTExpr
    {
        public PTExpr SubExpr { get; set; }

        public PTPointerTypeExpr(TokenLocation beg, TokenLocation end, PTExpr target) : base(beg, end)
        {
            this.SubExpr = target;
        }

        public override string ToString()
        {
            return $"{SubExpr}&";
        }

        public override AstExpression CreateAst()
        {
            return new AstPointerTypeExpr(this, SubExpr.CreateAst());
        }
    }

    public class PTArrayTypeExpr : PTExpr
    {
        public PTExpr SubExpr { get; set; }

        public PTArrayTypeExpr(TokenLocation beg, TokenLocation end, PTExpr target) : base(beg, end)
        {
            this.SubExpr = target;
        }

        public override string ToString()
        {
            return $"{SubExpr}[]";
        }

        public override AstExpression CreateAst()
        {
            return new AstArrayTypeExpr(this, SubExpr.CreateAst());
        }
    }

    public class PTFunctionTypeExpr : PTExpr
    {
        public PTExpr ReturnType { get; set; }
        public List<PTExpr> ParameterTypes { get; set; }

        public PTFunctionTypeExpr(TokenLocation beg, TokenLocation end, PTExpr target, List<PTExpr> pt) : base(beg, end)
        {
            this.ReturnType = target;
            this.ParameterTypes = pt;
        }

        public override string ToString()
        {
            var p = string.Join(", ", ParameterTypes);
            return $"fn {ReturnType}({p})";
        }

        public override AstExpression CreateAst()
        {
            throw new NotImplementedException();
        }
    }

}
