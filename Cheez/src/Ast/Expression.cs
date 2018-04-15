using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public abstract class Expression : ILocation
    {
        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }
        public int Id { get; }

        public Expression(TokenLocation beg, TokenLocation end)
        {
            this.Beginning = beg;
            this.End = end;
            this.Id = Util.NewId;
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
        public Literal(TokenLocation beg, TokenLocation end) : base(beg, end)
        {
        }
    }

    public class StringLiteral : Literal
    {
        public string Value { get; set; }

        public StringLiteral(TokenLocation beg, TokenLocation end, string value) : base(beg, end)
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

        public DotExpression(TokenLocation beg, TokenLocation end, Expression left, string right) : base(beg, end)
        {
            this.Left = left;
            this.Right = right;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDotExpression(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitDotExpression(this, data);
        }
    }

    public class CallExpression : Expression
    {
        public Expression Function { get; }
        public List<Expression> Arguments { get; set; }

        public CallExpression(TokenLocation beg, TokenLocation end, Expression func, List<Expression> args) : base(beg, end)
        {
            Function = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitCallExpression(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitCallExpression(this, data);
        }
    }

    public enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    public static class BinareyOperatorExtensions
    {
        public static int GetPrecedence(this BinaryOperator self)
        {
            switch (self)
            {
                case BinaryOperator.Add:
                case BinaryOperator.Subtract:
                    return 5;

                case BinaryOperator.Multiply:
                case BinaryOperator.Divide:
                    return 10;

                default:
                    throw new System.Exception();
            }
        }
    }

    public class BinaryExpression : Expression
    {
        public BinaryOperator Operator { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public BinaryExpression(TokenLocation beg, TokenLocation end, BinaryOperator op, Expression lhs, Expression rhs) : base(beg, end)
        {
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBinaryExpression(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitBinaryExpression(this, data);
        }
    }
}
