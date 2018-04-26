using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;

namespace Cheez.Compiler.ParseTree
{
    public class PTDirective : ILocation
    {
        public TokenLocation Beginning { get; }
        public TokenLocation End { get; }

        public PTIdentifierExpr Name { get; }

        public PTDirective(TokenLocation beg, TokenLocation end, PTIdentifierExpr name)
        {
            this.Beginning = beg;
            this.End = end;
            this.Name = name;
        }

        public AstDirective CreateAst()
        {
            return new AstDirective(this, Name.Name);
        }
    }
}
