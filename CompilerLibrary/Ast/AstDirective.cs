using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cheez.Ast.Expressions;
using Cheez.Visitors;

namespace Cheez.Ast
{
    public class AstDirective : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstIdExpr Name { get; }

        public List<AstExpression> Arguments { get; set; }

        public AstDirective(AstIdExpr name, List<AstExpression> args, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Arguments = args;
        }

        [DebuggerStepThrough]
        public T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDirective(this, data);

        public AstDirective Clone()
        {
            return new AstDirective(Name.Clone() as AstIdExpr, Arguments.Select(a => a.Clone()).ToList(), Location);
        }
    }
}
