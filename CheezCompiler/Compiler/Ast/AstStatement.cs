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

        public ParseTree.PTStatement GenericParseTreeNode { get; set; }

        public Scope Scope { get; set; }
        public Dictionary<string, AstDirective> Directives { get; protected set; }

        public AstStatement(Dictionary<string, AstDirective> dirs = null)
        {
            this.Directives = dirs ?? new Dictionary<string, AstDirective>();
        }

        public AstStatement(ParseTree.PTStatement node, Dictionary<string, AstDirective> dirs = null)
        {
            this.GenericParseTreeNode = node;
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

    public class AstEmptyStatement : AstStatement
    {
        public AstEmptyStatement(ParseTree.PTStatement node) : base(node)
        {
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitEmptyStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstEmptyStatement(GenericParseTreeNode)
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    public class AstDeferStmt : AstStatement
    {
        public AstStatement Deferred { get; set; }

        public AstDeferStmt(ParseTree.PTStatement node, AstStatement deferred, Dictionary<string, AstDirective> dirs = null) : base(node, dirs)
        {
            this.Deferred = deferred;
        }


        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDeferStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstDeferStmt(GenericParseTreeNode, Deferred.Clone(), Directives)
            {
                Scope = this.Scope,
                mFlags = this.mFlags
            };
        }
    }

    public class AstWhileStmt : AstStatement
    {
        public AstExpression Condition { get; set; }
        public AstStatement Body { get; set; }


        public AstWhileStmt(ParseTree.PTStatement node, AstExpression cond, AstStatement body) : base(node)
        {
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
            return new AstWhileStmt(GenericParseTreeNode, Condition.Clone(), Body.Clone())
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
        public AstExpression ReturnValue { get; set; }

        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();

        public AstReturnStmt(ParseTree.PTStatement node, AstExpression value) : base(node)
        {
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
            return new AstReturnStmt(GenericParseTreeNode, ReturnValue?.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    public class AstIfStmt : AstStatement
    {
        public AstExpression Condition { get; set; }
        public AstStatement IfCase { get; set; }
        public AstStatement ElseCase { get; set; }

        public AstIfStmt(ParseTree.PTStatement node, AstExpression cond, AstStatement ifCase, AstStatement elseCase = null) : base(node)
        {
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
            return new AstIfStmt(GenericParseTreeNode, Condition.Clone(), IfCase.Clone(), ElseCase?.Clone())
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
        public List<AstStatement> Statements { get; }
        public Scope SubScope { get; set; }

        public AstBlockStmt Parent { get; set; }

        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();

        public AstBlockStmt(ParseTree.PTStatement node, List<AstStatement> statements) : base(node)
        {
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBlockStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstBlockStmt(GenericParseTreeNode, Statements.Select(s => s.Clone()).ToList())
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
        public AstExpression Target { get; set; }
        public AstExpression Value { get; set; }

        public AstAssignment(ParseTree.PTStatement node, AstExpression target, AstExpression value) : base(node)
        {
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
            return new AstAssignment(GenericParseTreeNode, Target.Clone(), Value.Clone())
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
        public AstExpression Expr { get; set; }

        [DebuggerStepThrough]
        public AstExprStmt(ParseTree.PTStatement node, AstExpression expr) : base(node)
        {
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitExpressionStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstExprStmt(GenericParseTreeNode, Expr.Clone())
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
        public AstExpression Value { get; set; }

        [DebuggerStepThrough]
        public AstUsingStmt(ParseTree.PTStatement node, AstExpression expr, Dictionary<string, AstDirective> dirs = null) : base(node, dirs)
        {
            Value = expr;
        }
        
        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitUsingStatement(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstUsingStmt(GenericParseTreeNode, Value.Clone())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }
}
