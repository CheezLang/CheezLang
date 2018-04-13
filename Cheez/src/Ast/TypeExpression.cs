using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class TypeExpression : Expression
    {
        public string Text { get; set; }

        public TypeExpression(LocationInfo beg, LocationInfo end, string text) : base(beg, end)
        {
            this.Text = text;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeExpression(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitTypeExpression(this, data);
        }
    }
}
