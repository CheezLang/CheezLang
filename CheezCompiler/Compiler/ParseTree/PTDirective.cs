using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public class PTDirective : ILocation
    {
        public TokenLocation Beginning { get; }
        public TokenLocation End { get; }

        public PTIdentifierExpr Name { get; }

        public List<PTExpr> Arguments { get; set; }

        public PTDirective(TokenLocation end, PTIdentifierExpr name, List<PTExpr> args)
        {
            this.Beginning = name.Beginning;
            this.End = end;
            this.Name = name;
            this.Arguments = args;
        }

        public AstDirective CreateAst()
        {
            var args = Arguments.Select(a => a.CreateAst()).ToList();
            return new AstDirective(this, Name.Name, args);
        }
    }
}
