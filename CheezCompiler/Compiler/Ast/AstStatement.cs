using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.Ast
{
    public enum StmtFlags
    {
        Returns,
        GlobalScope,
        IsLastStatementInBlock
    }

    public abstract class AstStatement : IVisitorAcceptor
    {
        protected int mFlags = 0;

        public abstract ParseTree.PTStatement GenericParseTreeNode { get; }

        public Scope Scope { get; set; }
        public Dictionary<string, AstDirective> Directives { get; protected set; }

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

        public abstract AstStatement Clone();
    }

    public class AstWhileStmt : AstStatement
    {
        public ParseTree.PTWhileStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public AstExpression Condition { get; set; }
        public AstStatement Body { get; set; }


        public AstWhileStmt(ParseTree.PTWhileStmt node, AstExpression cond, AstStatement body) : base()
        {
            ParseTreeNode = node;
            this.Condition = cond;
            this.Body = body;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitWhileStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstWhileStmt(ParseTreeNode, Condition.Clone(), Body.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }

        public override string ToString()
        {
            return $"while {Condition} {{ ... }}";
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

        public override string ToString()
        {
            return $"return {ReturnValue}";
        }

        public override AstStatement Clone()
        {
            return new AstReturnStmt(ParseTreeNode, ReturnValue.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    public class AstIfStmt : AstStatement
    {
        public ParseTree.PTIfStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

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

        public override AstStatement Clone()
        {
            return new AstIfStmt(ParseTreeNode, Condition.Clone(), IfCase.Clone(), ElseCase?.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }

        public override string ToString()
        {
            return $"if {Condition} {{ ... }}";
        }
    }

    public class AstBlockStmt : AstStatement
    {
        public ParseTree.PTBlockStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public List<AstStatement> Statements { get; }
        public Scope SubScope { get; set; }

        public AstBlockStmt(ParseTree.PTBlockStmt node, List<AstStatement> statements) : base()
        {
            ParseTreeNode = node;
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBlockStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstBlockStmt(ParseTreeNode, Statements.Select(s => s.Clone()).ToList())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }

        public override string ToString()
        {
            return "{ ... }";
        }
    }

    public class AstAssignment : AstStatement
    {
        public ParseTree.PTAssignment ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public AstExpression Target { get; set; }
        public AstExpression Value { get; set; }

        public AstAssignment(ParseTree.PTAssignment node, AstExpression target, AstExpression value) : base()
        {
            ParseTreeNode = node;
            this.Target = target;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitAssignment(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstAssignment(ParseTreeNode, Target.Clone(), Value.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }

        public override string ToString()
        {
            return $"{Target} = {Value}";
        }
    }

    public class AstExprStmt : AstStatement
    {
        public ParseTree.PTExprStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public AstExpression Expr { get; set; }

        [DebuggerStepThrough]
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

        public override AstStatement Clone()
        {
            return new AstExprStmt(ParseTreeNode, Expr.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }

        public override string ToString()
        {
            return Expr.ToString();
        }
    }

    public class AstUsingStmt : AstStatement
    {
        public ParseTree.PTUsingStatement ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public AstExpression Value { get; set; }

        [DebuggerStepThrough]
        public AstUsingStmt(ParseTree.PTUsingStatement node, AstExpression expr, Dictionary<string, AstDirective> dirs = null) : base(dirs)
        {
            ParseTreeNode = node;
            Value = expr;
        }
        
        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitUsingStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstUsingStmt(ParseTreeNode, Value.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }
}
