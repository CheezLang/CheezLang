using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public enum StmtFlags
    {
        Returns
    }

    public abstract class AstStatement : IVisitorAcceptor
    {
        private int mFlags = 0;

        public abstract ParseTree.PTStatement GenericParseTreeNode { get; }

        public Scope Scope { get; set; }
        public Dictionary<string, AstDirective> Directives { get; }

        public AstStatement(Dictionary<string, AstDirective> dirs = null)
        {
            this.Directives = dirs ?? new Dictionary<string, AstDirective>();
        }

        public void SetFlag(StmtFlags f)
        {
            mFlags |= 1 << (int)f;
        }

        public bool GetFlag(StmtFlags f)
        {
            return (mFlags & (1 << (int)f)) != 0;
        }

        public bool HasDirective(string name)
        {
            return Directives.ContainsKey(name);
        }

        public AstDirective GetDirective(string name)
        {
            if (!Directives.ContainsKey(name))
                return null;
            return Directives[name];
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }

    public class AstWhileStmt : AstStatement
    {
        public ParseTree.PTWhileStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

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

    public class AstReturnStmt : AstStatement
    {
        public ParseTree.PTReturnStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

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
