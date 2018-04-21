using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstReturnStmt : AstStatement
    {
        public ParseTree.PTReturnStmt ParseTreeNode { get; }

        public AstExpression ReturnValue { get; set; }

        public AstReturnStmt(ParseTree.PTReturnStmt node, AstExpression value) : base()
        {
            ParseTreeNode = node;
            ReturnValue = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitReturnStatement(this, data);
        }
    }
}
