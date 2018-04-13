using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class ConstantDeclaration : Statement
    {
        public string Name { get; set; }
        public Expression Value { get; set; }

        public ConstantDeclaration(LocationInfo beg, LocationInfo end, string name, Expression value) : base(beg, end)
        {
            this.Name = name;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitConstantDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitConstantDeclaration(this, data);
        }
    }
}
