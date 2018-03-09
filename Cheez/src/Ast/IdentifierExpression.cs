using System;
using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class IdentifierExpression : Expression
    {
        public String Name { get; set; }
        public CType Type { get; set; }

        public IdentifierExpression(LocationInfo loc, string name) : base(loc)
        {
            this.Name = name;
        }

        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIdentifierExpression(this, data);
        }

        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitIdentifierExpression(this, data);
        }
    }
}
