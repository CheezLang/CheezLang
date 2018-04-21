using System.Diagnostics;
using Cheez.Compiler.Visitor;

namespace Cheez.Compiler.Ast
{
    public class AstIfStmt : AstStatement
    {
        public ParseTree.PTIfStmt ParseTreeNode { get; }

        public AstExpression Condition { get; set; }
        public AstStatement IfCase { get; set; }
        public AstStatement ElseCase { get; set; }

        public AstIfStmt(ParseTree.PTIfStmt node, AstExpression cond, AstStatement ifCase, AstStatement elseCase = null) : base()
        {
            ParseTreeNode = node;
            this.Condition = cond;
            this.IfCase = ifCase;
            this.ElseCase = elseCase;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIfStatement(this, data);
        }
    }
}
