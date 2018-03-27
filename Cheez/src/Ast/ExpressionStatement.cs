﻿using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class ExpressionStatement : Statement
    {
        public Expression Expr { get; set; }

        public ExpressionStatement(LocationInfo loc, Expression expr) : base(loc)
        {
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitExpressionStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitExpressionStatement(this, data);
        }
    }
}
