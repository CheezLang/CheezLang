using Cheez.Ast.Expressions;
using Cheez.Visitors;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cheez.Ast.Statements
{
    public enum StmtFlags
    {
        GlobalScope,
        Returns,
        IsLastStatementInBlock,
        NoDefaultInitializer,
        MembersComputed,
        ExcludeFromVtable,
        IsMacroFunction,
        IsForExtension,
        IsCopy,
        Breaks,
        IsLocal
    }

    public interface IAstNode {
        IAstNode Parent { get; }
    }

    public abstract class AstStatement : IVisitorAcceptor, ILocation, IAstNode
    {
        protected int mFlags { get; private set; } = 0;

        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public PTFile SourceFile { get; set; }

        public Scope Scope { get; set; }
        public List<AstDirective> Directives { get; protected set; }

        public IAstNode Parent { get; set; }

        public int Position { get; set; } = 0;

        public AstStatement(List<AstDirective> dirs = null, ILocation Location = null)
        {
            this.Directives = dirs ?? new List<AstDirective>();
            this.Location = Location;
        }

        public void SetFlag(StmtFlags f, bool b = true)
        {
            if (b)
                mFlags |= 1 << (int)f;
            else
                mFlags &= ~(1 << (int)f);
        }
        
        public bool GetFlag(StmtFlags f) => (mFlags & (1 << (int)f)) != 0;
        public bool HasDirective(string name) => Directives.Find(d => d.Name.Name == name) != null;

        public AstDirective GetDirective(string name)
        {
            return Directives.FirstOrDefault(d => d.Name.Name == name);
        }

        public bool TryGetDirective(string name, out AstDirective dir)
        {
            dir = Directives.FirstOrDefault(d => d.Name.Name == name);
            return dir != null;
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);

        public abstract AstStatement Clone();

        protected T CopyValuesTo<T>(T to)
            where T : AstStatement
        {
            to.Location = this.Location;
            to.Parent = this.Parent;
            to.Scope = this.Scope;
            to.Directives = this.Directives;
            
            // @TODO: i feel like flags should not be copieds
            //to.mFlags = this.mFlags;
            return to;
        }

        public override string ToString()
        {
            var sb = new StringWriter();
            new RawAstPrinter(sb).PrintStatement(this);
            return sb.GetStringBuilder().ToString();
        }
    }

    public class AstDirectiveStatement : AstStatement
    {
        public AstDirective Directive { get; }

        public AstDirectiveStatement(AstDirective Directive, ILocation Location = null) : base(Location: Location)
        {
            this.Directive = Directive;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDirectiveStmt(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstDirectiveStatement(Directive));
    }

    public class AstEmptyStatement : AstStatement
    {
        public AstEmptyStatement(ILocation Location = null) : base(Location: Location) {}
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitEmptyStmt(this, data);
        public override AstStatement Clone() =>  CopyValuesTo(new AstEmptyStatement());
    }

    public class AstDeferStmt : AstStatement, ISymbol
    {
        public AstStatement Deferred { get; set; }

        public string Name => null;

        public AstDeferStmt(AstStatement deferred, List<AstDirective> Directives = null, ILocation Location = null)
            : base(Directives, Location)
        {
            this.Deferred = deferred;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDeferStmt(this, data);

        public override AstStatement Clone() => CopyValuesTo(new AstDeferStmt(Deferred.Clone()));
    }

    public class AstWhileStmt : AstStatement, ISymbol
    {
        public AstExpression Condition { get; set; }
        public AstBlockExpr Body { get; set; }

        public List<AstVariableDecl> PreActions { get; set; }
        public AstStatement PostAction { get; set; }

        public Scope PreScope { get; set; }
        public Scope SubScope { get; set; }

        public AstIdExpr Label { get; set; }

        public string Name => Label.Name;

        public AstWhileStmt(AstExpression cond, AstBlockExpr body, AstVariableDecl pre, AstStatement post, AstIdExpr label, ILocation Location = null)
            : base(Location: Location)
        {
            this.Condition = cond;
            this.Body = body;
            this.PreActions = pre != null ? new List<AstVariableDecl> { pre } : null;
            this.PostAction = post;
            this.Label = label;
        }

        public AstWhileStmt(AstExpression cond, AstBlockExpr body, List<AstVariableDecl> pre, AstStatement post, AstIdExpr label, ILocation Location = null)
            : base(Location: Location)
        {
            this.Condition = cond;
            this.Body = body;
            this.PreActions = pre;
            this.PostAction = post;
            this.Label = label;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitWhileStmt(this, data);
        public override AstStatement Clone() 
            => CopyValuesTo(new AstWhileStmt(
                Condition.Clone(),
                Body.Clone() as AstBlockExpr,
                PreActions?.Select(v => v.Clone() as AstVariableDecl)?.ToList(),
                PostAction?.Clone(),
                Label?.Clone() as AstIdExpr
                ));
    }

    public class AstForStmt : AstStatement
    {
        public AstIdExpr VarName { get; set; }
        public AstIdExpr IndexName { get; set; }
        public AstExpression Collection { get; set; }
        public AstExpression Body { get; set; }
        public List<AstArgument> Arguments { get; set; }
        public AstIdExpr Label { get; set; }

        public Scope SubScope { get; set; }

        public AstForStmt(
            AstIdExpr varName,
            AstIdExpr indexName,
            AstExpression collection,
            AstExpression body,
            List<AstArgument> arguments,
            AstIdExpr label,
            ILocation Location = null)
            : base(Location: Location)
        {
            this.VarName = varName;
            this.IndexName = indexName;
            this.Collection = collection;
            this.Body = body;
            this.Arguments = arguments;
            this.Label = label;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitForStmt(this, data);
        public override AstStatement Clone()
            => CopyValuesTo(new AstForStmt(
                    VarName?.Clone() as AstIdExpr,
                    IndexName?.Clone() as AstIdExpr,
                    Collection.Clone(),
                    Body.Clone(),
                    Arguments?.Select(a => a.Clone() as AstArgument)?.ToList(),
                    Label?.Clone() as AstIdExpr));
    }

    public class AstReturnStmt : AstStatement
    {
        public AstExpression ReturnValue { get; set; }
        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();
        public List<AstExpression> Destructions { get; private set; } = null;

        public AstReturnStmt(AstExpression values, ILocation Location = null)
            : base(Location: Location)
        {
            ReturnValue = values;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitReturnStmt(this, data);
        public override AstStatement Clone() => CopyValuesTo(new AstReturnStmt(ReturnValue?.Clone()));

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }
    }

    public class AstAssignment : AstStatement
    {
        public AstExpression Pattern { get; set; }
        public AstExpression Value { get; set; }
        public string Operator { get; set; }

        public List<AstAssignment> SubAssignments { get; set; }
        public bool OnlyGenerateValue { get; internal set; } = false;

        public List<AstExpression> Destructions { get; private set; } = null;

        public AstAssignment(AstExpression target, AstExpression value, string op = null, ILocation Location = null)
            : base(Location: Location)
        {
            this.Pattern = target;
            this.Value = value;
            this.Operator = op;
        }

        public void AddSubAssignment(AstAssignment ass)
        {
            if (SubAssignments == null) SubAssignments = new List<AstAssignment>();
            SubAssignments.Add(ass);
        }

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitAssignmentStmt(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstAssignment(Pattern.Clone(), Value.Clone(), Operator));
    }

    public class AstExprStmt : AstStatement
    {
        public AstExpression Expr { get; set; }
        public List<AstExpression> Destructions { get; private set; } = null;

        [DebuggerStepThrough]
        public AstExprStmt(AstExpression expr, ILocation Location = null) : base(Location: Location)
        {
            this.Expr = expr;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitExpressionStmt(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstExprStmt(Expr.Clone()));

        public override string ToString() => $"#expr {base.ToString()}";

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }
    }

    public class AstUsingStmt : AstStatement
    {
        public AstExpression Value { get; set; }

        [DebuggerStepThrough]
        public AstUsingStmt(AstExpression expr, List<AstDirective> Directives = null, ILocation Location = null)
            : base(Directives, Location)
        {
            Value = expr;
        }
        
        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitUsingStmt(this, data);

        public override AstStatement Clone()
            => CopyValuesTo(new AstUsingStmt(Value.Clone()));
    }
}
