using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public abstract class Expression
    {
        public LocationInfo Beginning { get; set; }

        public Expression(LocationInfo loc)
        {
            Beginning = loc;
        }

        public abstract T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D));
        public abstract void Visit<D>(IVoidVisitor<D> visitor, D data = default(D));
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
        }

        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitStringLiteral(this, data);
        }

        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitStringLiteral(this, data);
        }
    }
}
