using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class PrintStatement : Statement
    {
        public List<Expression> Expressions { get; }
        public Expression Seperator { get; }

        public PrintStatement(LocationInfo loc, List<Expression> expr, Expression seperator = null) : base(loc)
        {
            this.Expressions = expr;
            this.Seperator = seperator;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitPrintStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitPrintStatement(this, data);
        }
    }
}
