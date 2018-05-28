using System.Collections.Generic;

namespace Cheez.Compiler.Ast
{
    public class AstDirective
    {
        public ParseTree.PTDirective ParseTreeNode { get; }

        public string Name { get; }

        public List<AstExpression> Arguments { get; set; }

        public AstDirective(ParseTree.PTDirective node, string name, List<AstExpression> args)
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Arguments = args;
        }
    }
}
