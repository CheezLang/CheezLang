using System.Collections.Generic;
using Cheez.Compiler.Parsing;

namespace Cheez.Compiler.Ast
{
    public class AstDirective : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstIdentifierExpr Name { get; }

        public List<AstExpression> Arguments { get; set; }

        public AstDirective(AstIdentifierExpr name, List<AstExpression> args, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Arguments = args;
        }
    }
}
