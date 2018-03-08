using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class PrintStatement : Statement
    {
        public Expression Expr { get; set; }

        public PrintStatement(LocationInfo loc, Expression expr) : base(loc)
        {
            this.Expr = expr;
        }

        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitPrintStatement(this, data);
        }

        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitPrintStatement(this, data);
        }
    }
}
