using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class ReturnStatement : Statement
    {
        public Expression ReturnValue { get; set; }

        public ReturnStatement(TokenLocation beg, TokenLocation end, Expression value) : base(beg, end)
        {
            ReturnValue = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitReturnStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitReturnStatement(this, data);
        }
    }
}
