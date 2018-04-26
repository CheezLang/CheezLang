namespace Cheez.Compiler.Ast
{
    public class AstDirective
    {
        public ParseTree.PTDirective ParseTreeNode { get; }

        public string Name { get; }

        public AstDirective(ParseTree.PTDirective node, string name)
        {
            ParseTreeNode = node;
            this.Name = name;
        }
    }
}
