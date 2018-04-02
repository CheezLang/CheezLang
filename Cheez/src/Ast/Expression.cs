using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public abstract class Expression
    {
        public LocationInfo Beginning { get; set; }
        public int Id { get; }

        public Expression(LocationInfo loc)
        {
            Beginning = loc;
            Id = Util.NewId;
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D));

        [DebuggerStepThrough]
        public abstract void Accept<D>(IVoidVisitor<D> visitor, D data = default(D));

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
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

    public class CallExpression : Expression
    {
        public Expression Function { get; }
        public List<Expression> Arguments { get; set; }

        public CallExpression(LocationInfo loc, Expression func, List<Expression> args) : base(loc)
        {
            Function = func;
            Arguments = args;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitCallExpression(this, data);
        }

        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitCallExpression(this, data);
        }
    }
}
