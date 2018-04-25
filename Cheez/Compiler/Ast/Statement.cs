using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public abstract class AstStatement : IVisitorAcceptor
    {
        public int Id { get; }

        public Scope Scope { get; set; }

        public AstStatement()
        {
            this.Id = Util.NewId;
        }

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }

    public class AstWhileStmt : AstStatement
    {
        public ParseTree.PTWhileStmt ParseTreeNode { get; }

        public AstExpression Condition { get; set; }
        public AstStatement Body { get; set; }
        public AstStatement PreAction { get; set; }
        public AstStatement PostAction { get; set; }

        public AstWhileStmt(ParseTree.PTWhileStmt node, AstExpression cond, AstStatement body, AstStatement pre, AstStatement post) : base()
        {
            ParseTreeNode = node;
            this.Condition = cond;
            this.Body = body;
            this.PreAction = pre;
            this.PostAction = post;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitWhileStatement(this, data);
        }
    }
}
