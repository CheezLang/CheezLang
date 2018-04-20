using System.Diagnostics;
using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        public Statement IfCase { get; set; }
        public Statement ElseCase { get; set; }

        public IfStatement(TokenLocation beg, TokenLocation end, Expression cond, Statement ifCase, Statement elseCase = null) : base(beg, end)
        {
            this.Condition = cond;
            this.IfCase = ifCase;
            this.ElseCase = elseCase;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIfStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitIfStatement(this, data);
        }
    }
}
