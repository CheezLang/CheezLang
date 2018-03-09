using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class PrintStatement : Statement
    {
        public Expression Expr { get; set; }

        public PrintStatement(LocationInfo loc, Expression expr) : base(loc)
        {
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitPrintStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitPrintStatement(this, data);
        }
    }
}
