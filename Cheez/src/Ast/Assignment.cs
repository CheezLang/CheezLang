using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class Assignment : Statement
    {
        public Expression Target { get; set; }
        public Expression Value { get; set; }

        public Assignment(TokenLocation beg, TokenLocation end, Expression target, Expression value) : base(beg, end)
        {
            this.Target = target;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitAssignment(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitAssignment(this, data);
        }
    }
}
