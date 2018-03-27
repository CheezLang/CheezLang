using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public abstract class Expression
    {
        public LocationInfo Beginning { get; set; }
        public CType Type { get; set; }

        public Expression(LocationInfo loc)
        {
            Beginning = loc;
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D));

        [DebuggerStepThrough]
        public abstract void Accept<D>(IVoidVisitor<D> visitor, D data = default(D));
    }

    public abstract class Literal : Expression
    {
        public Literal(LocationInfo loc) : base(loc)
        {
        }
    }

    public class StringLiteral : Literal
    {
        public string Value { get; set; }

        public StringLiteral(LocationInfo loc, string value) : base(loc)
        {
            this.Value = value;
            Type = StringType.Instance;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitStringLiteral(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitStringLiteral(this, data);
        }
    }

    public class DotExpression : Expression
    {
        public Expression Left { get; set; }
        public string Right { get; set; }

        public DotExpression(LocationInfo loc, Expression left, string right) : base(loc)
        {
            this.Left = left;
            this.Right = right;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDotExpression(this, data);
        }

        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitDotExpression(this, data);
        }
    }
}
