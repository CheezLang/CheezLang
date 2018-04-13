using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class TypeExpression : Expression
    {
        public TypeExpression(TokenLocation beg, TokenLocation end) : base(beg, end)
        {
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

    public class NamedTypeExression : TypeExpression
    {
        public string Name { get; set; }

        public NamedTypeExression(TokenLocation beg, TokenLocation end, string name) : base(beg, end)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class PointerTypeExpression : TypeExpression
    {
        public TypeExpression TargetType { get; set; }

        public PointerTypeExpression(TokenLocation beg, TokenLocation end, TypeExpression target) : base(beg, end)
        {
            this.TargetType = target;
        }

        public override string ToString()
        {
            return $"{TargetType}*";
        }
    }

    public class ArrayTypeExpression : TypeExpression
    {
        public TypeExpression ElementType { get; set; }

        public ArrayTypeExpression(TokenLocation beg, TokenLocation end, TypeExpression target) : base(beg, end)
        {
            this.ElementType = target;
        }

        public override string ToString()
        {
            return $"{ElementType}[]";
        }
    }

}
