using System.Diagnostics;
using Cheez.Compiler.Visitor;

namespace Cheez.Compiler.Ast
{
    public class AstIdentifierExpr : AstExpression
    {
        public ParseTree.PTIdentifierExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public string Name { get; set; }


        public AstIdentifierExpr(ParseTree.PTIdentifierExpr node, string name) : base()
        {
            ParseTreeNode = node;
            this.Name = name;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIdentifierExpression(this, data);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
