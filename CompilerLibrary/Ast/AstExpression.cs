using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Visitors;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cheez.Ast.Expressions
{
    public enum ExprFlags
    {
        IsLValue = 0,
        Returns = 1
    }

    public abstract class AstExpression : IVisitorAcceptor, ILocation, IAstNode
    {
        protected int mFlags = 0;

        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public CheezType Type { get; set; }
        public object Value { get; set; }
        public Scope Scope { get; set; }

        // TODO: still necessary?
        public abstract bool IsPolymorphic { get; }

        protected bool IsCompTimeValue { get; set; } = false;

        public IAstNode Parent { get; set; }

        [DebuggerStepThrough]
        public AstExpression(ILocation Location = null)
        {
            this.Location = Location;
        }

        public void SetFlag(ExprFlags f, bool b)
        {
            if (b)
            {
                mFlags |= 1 << (int)f;
            }
            else
            {
                mFlags &= ~(1 << (int)f);
            }
        }

        [DebuggerStepThrough]
        public bool GetFlag(ExprFlags f) => (mFlags & (1 << (int)f)) != 0;

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);

        [DebuggerStepThrough]
        public abstract AstExpression Clone();

        protected T CopyValuesTo<T>(T to)
            where T : AstExpression
        {
            to.Location = this.Location;
            to.Scope = this.Scope;
            to.mFlags = this.mFlags;
            to.Value = this.Value;
            return to;
        }

        protected void CopyValuesFrom(AstExpression from)
        {
            this.Location = from.Location;
            this.Scope = from.Scope;
            this.mFlags = from.mFlags;
            this.Value = from.Value;
        }

        public override string ToString()
        {
            var sb = new StringWriter();
            new RawAstPrinter(sb).PrintExpression(this);
            return sb.GetStringBuilder().ToString();
        }
    }

    public class AstIfExpr : AstExpression
    {
        public Scope SubScope { get; set; }
        public AstExpression Condition { get; set; }
        public AstExpression IfCase { get; set; }
        public AstExpression ElseCase { get; set; }
        public AstVariableDecl PreAction { get; set; }

        public override bool IsPolymorphic => false;

        public AstIfExpr(AstExpression cond, AstExpression ifCase, AstExpression elseCase = null, AstVariableDecl pre = null, ILocation Location = null)
            : base(Location: Location)
        {
            this.Condition = cond;
            this.IfCase = ifCase;
            this.ElseCase = elseCase;
            this.PreAction = pre;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitIfExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstIfExpr(Condition.Clone(), IfCase.Clone(), ElseCase?.Clone(), PreAction?.Clone() as AstVariableDecl));
    }

    public class AstBlockExpr : AstExpression
    {
        public List<AstStatement> Statements { get; }
        public Scope SubScope { get; set; }

        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();

        public override bool IsPolymorphic => false;

        public AstBlockExpr(List<AstStatement> statements, ILocation Location = null) : base(Location: Location)
        {
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitBlockExpr(this, data);

        public override AstExpression Clone()
         => CopyValuesTo(new AstBlockExpr(Statements.Select(s => s.Clone()).ToList()));
    }

    public class AstEmptyExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstEmptyExpr(ILocation Location = null) : base(Location)
        { }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitEmptyExpression(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstEmptyExpr());
    }

    public abstract class AstLiteral : AstExpression
    {
        public AstLiteral(ILocation Location = null) : base(Location)
        { }
    }

    public class AstCharLiteral : AstLiteral
    {
        public override bool IsPolymorphic => false;
        public char CharValue { get; set; }
        public string RawValue { get; set; }

        [DebuggerStepThrough]
        public AstCharLiteral(string rawValue, ILocation Location = null) : base(Location)
        {
            this.RawValue = rawValue;
            this.IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitCharLiteralExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCharLiteral(RawValue) { CharValue = this.CharValue });
    }

    public class AstStringLiteral : AstLiteral
    {
        public override bool IsPolymorphic => false;
        public string StringValue => (string)Value;
        public string Suffix { get; set; }

        [DebuggerStepThrough]
        public AstStringLiteral(string value, string suffix = null, ILocation Location = null) : base(Location)
        {
            this.Value = value;
            this.Suffix = suffix;
            this.IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitStringLiteralExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstStringLiteral(Value as string, Suffix));
    }

    public class AstDotExpr : AstExpression
    {
        public AstExpression Left { get; set; }
        public AstIdExpr Right { get; set; }
        public bool IsDoubleColon { get; set; }
        public override bool IsPolymorphic => Left.IsPolymorphic;

        public int DerefCount { get; set; } = 0;

        [DebuggerStepThrough]
        public AstDotExpr(AstExpression left, AstIdExpr right, bool isDC, ILocation Location = null) : base(Location)
        {
            this.Left = left;
            this.Right = right;
            IsDoubleColon = isDC;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDotExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstDotExpr(Left.Clone(), Right.Clone() as AstIdExpr, IsDoubleColon));
    }

    public class AstArgument : AstExpression
    {
        public override bool IsPolymorphic => Expr.IsPolymorphic;
        public AstExpression Expr { get; set; }
        public AstIdExpr Name { get; set; }
        public int Index = -1;

        public AstArgument(AstExpression expr, AstIdExpr name = null, ILocation Location = null)
            : base(Location)
        {
            this.Expr = expr;
            this.Name = name;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArgumentExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstArgument(Expr.Clone(), Name?.Clone() as AstIdExpr));
    }

    public class AstCallExpr : AstExpression
    {
        public AstExpression Function { get; set; }
        public List<AstArgument> Arguments { get; set; }
        public override bool IsPolymorphic => Function.IsPolymorphic || Arguments.Any(a => a.IsPolymorphic);

        public AstFunctionDecl Declaration { get; internal set; }

        [DebuggerStepThrough]
        public AstCallExpr(AstExpression func, List<AstArgument> args, ILocation Location = null) : base(Location)
        {
            Function = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCallExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCallExpr(Function.Clone(),  Arguments.Select(a => a.Clone() as AstArgument).ToList()));
    }

    public class AstCompCallExpr : AstExpression
    {
        public AstIdExpr Name { get; set; }
        public List<AstExpression> Arguments { get; set; }

        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstCompCallExpr(AstIdExpr func, List<AstExpression> args, ILocation Location = null) : base(Location)
        {
            Name = func;
            Arguments = args;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCompCallExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCompCallExpr(Name.Clone() as AstIdExpr, Arguments.Select(a => a.Clone()).ToList()));
    }

    public class AstBinaryExpr : AstExpression
    {
        public string Operator { get; set; }
        public AstExpression Left { get; set; }
        public AstExpression Right { get; set; }

        public override bool IsPolymorphic => Left.IsPolymorphic || Right.IsPolymorphic;

        public IOperator ActualOperator { get; set; }

        [DebuggerStepThrough]
        public AstBinaryExpr(string op, AstExpression lhs, AstExpression rhs, ILocation Location = null) : base(Location)
        {
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitBinaryExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstBinaryExpr(Operator, Left.Clone(), Right.Clone()));
    }

    public class AstUnaryExpr : AstExpression
    {
        public string Operator { get; set; }
        public AstExpression SubExpr { get; set; }
        public override bool IsPolymorphic => SubExpr.IsPolymorphic;

        [DebuggerStepThrough]
        public AstUnaryExpr(string op, AstExpression sub, ILocation Location = null) : base(Location)
        {
            Operator = op;
            SubExpr = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitUnaryExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstUnaryExpr(Operator, SubExpr.Clone()));
    }

    public class AstBoolExpr : AstLiteral
    {
        public override bool IsPolymorphic => false;

        public bool BoolValue => (bool)Value;

        [DebuggerStepThrough]
        public AstBoolExpr(bool value, ILocation Location = null) : base(Location)
        {
            this.Value = value;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitBoolExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstBoolExpr((bool)Value));
    }

    public class AstAddressOfExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic;
        public bool IsReference = false;

        [DebuggerStepThrough]
        public AstAddressOfExpr(AstExpression sub, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitAddressOfExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstAddressOfExpr(SubExpression.Clone()) { IsReference = this.IsReference });
    }

    public class AstDereferenceExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic;

        [DebuggerStepThrough]
        public AstDereferenceExpr(AstExpression sub, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDerefExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstDereferenceExpr(SubExpression.Clone()));
    }

    public class AstCastExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public AstTypeExpr TypeExpr { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Type.IsPolyType;

        public AstIdExpr Name => null;

        [DebuggerStepThrough]
        public AstCastExpr(AstTypeExpr typeExpr, AstExpression sub, ILocation Location = null) : base(Location)
        {
            this.TypeExpr = typeExpr;
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCastExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstCastExpr(TypeExpr?.Clone() as AstTypeExpr, SubExpression.Clone()));
    }

    public class AstArrayAccessExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public AstExpression Indexer { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Indexer.IsPolymorphic;

        [DebuggerStepThrough]
        public AstArrayAccessExpr(AstExpression sub, AstExpression index, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
            Indexer = index;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArrayAccessExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstArrayAccessExpr(SubExpression.Clone(), Indexer.Clone()));
    }

    public class AstNullExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstNullExpr(ILocation Location = null) : base(Location)
        {
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitNullExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstNullExpr());

        public override string ToString()
        {
            return "<null>";
        }
    }

    public class AstTupleExpr : AstExpression
    {
        public override bool IsPolymorphic => false;
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTupleExpr(this, data);

        public List<AstExpression> Values { get; set; }

        public AstTupleExpr(List<AstExpression> values, ILocation Location)
            : base(Location)
        {
            this.Values = values;
        }

        public override AstExpression Clone()
            => CopyValuesTo(new AstTupleExpr(Values.Select(v => v.Clone()).ToList(), Location));
    }

    public class AstNumberExpr : AstLiteral
    {
        private NumberData mData;
        public NumberData Data => mData;
        public override bool IsPolymorphic => false;
        public string Suffix { get; set; }

        [DebuggerStepThrough]
        public AstNumberExpr(NumberData data, string suffix = null, ILocation Location = null) : base(Location)
        {
            mData = data;
            IsCompTimeValue = true;
            this.Suffix = suffix;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitNumberExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstNumberExpr(Data, Suffix));
    }

    public class AstIdExpr : AstExpression
    {
        public string Name { get; set; }
        public ISymbol Symbol { get; set; }

        public override bool IsPolymorphic => _isPolymorphic;
        private bool _isPolymorphic;

        [DebuggerStepThrough]
        public AstIdExpr(string name, bool isPolyTypeExpr, ILocation Location = null) : base(Location)
        {
            this.Name = name;
            this._isPolymorphic = isPolyTypeExpr;
        }

        public void SetIsPolymorphic(bool b)
        {
            _isPolymorphic = b;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitIdExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstIdExpr(Name, IsPolymorphic));
    }

    public class AstStructMemberInitialization: ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstIdExpr Name { get; set; }
        public AstExpression Value { get; set; }

        public int Index { get; set; }

        public AstStructMemberInitialization(AstIdExpr name, AstExpression expr, ILocation Location = null)
        {
            this.Location = Location;
            this.Name = name;
            this.Value = expr;
        }
    }

    public class AstStructValueExpr : AstExpression
    {
        public AstTypeExpr TypeExpr { get; set; }
        public List<AstStructMemberInitialization> MemberInitializers { get; }
        public override bool IsPolymorphic => false;

        public AstIdExpr Name => null;

        [DebuggerStepThrough]
        public AstStructValueExpr(AstTypeExpr type, List<AstStructMemberInitialization> inits, ILocation Location = null) : base(Location)
        {
            this.TypeExpr = type;
            this.MemberInitializers = inits;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitStructValueExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstStructValueExpr(TypeExpr, MemberInitializers));
    }

    public class AstArrayExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public List<AstExpression> Values { get; set; }

        public AstIdExpr Name => null;

        public AstArrayExpr(List<AstExpression> values, ILocation Location = null) : base(Location)
        {
            this.Values = values;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArrayExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstArrayExpr(Values.Select(v => v.Clone()).ToList()));
    }
}
