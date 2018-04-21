using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstTypeExpr : AstExpression
    {
        public ParseTree.PTTypeExpr ParseTreeNode { get; set; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstTypeExpr(ParseTree.PTTypeExpr node) : base()
        {
            ParseTreeNode = node;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeExpression(this, data);
        }
    }
}
