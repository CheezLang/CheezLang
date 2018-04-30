using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstExprStmt : AstStatement
    {
        public ParseTree.PTExprStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public AstExpression Expr { get; set; }

        public AstExprStmt(ParseTree.PTExprStmt node, AstExpression expr) : base()
        {
            ParseTreeNode = node;
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitExpressionStatement(this, data);
        }
    }
}
