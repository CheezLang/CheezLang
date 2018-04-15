using System.Diagnostics;
using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class VariableDeclaration : Statement
    {
        public string Name { get; set; }
        public TypeExpression Type { get; set; }
        public Expression Initializer { get; set; }
        public ILocation NameLocation { get; internal set; }

        public VariableDeclaration(TokenLocation beg, TokenLocation end, TokenLocation nameLocation, string name, TypeExpression type = null, Expression init = null) : base(beg, end)
        {
            this.Name = name;
            this.Type = type;
            this.Initializer = init;
            this.NameLocation = new Location(nameLocation);
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitVariableDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitVariableDeclaration(this, data);
        }
    }
}
