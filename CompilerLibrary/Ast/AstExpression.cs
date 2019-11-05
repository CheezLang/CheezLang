using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Parsing;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cheez.Ast.Expressions
{
    public enum ExprFlags
    {
        IsLValue = 0,
        Returns = 1,
        AssignmentTarget = 2,
        SetAccess = 3,
        Anonymous = 4,
        Link = 5,
        FromMacroExpansion = 6,
        IgnoreInCodeGen = 7,
        DontApplySymbolStatuses = 8,
        RequireInitializedSymbol = 9,
        Breaks = 10,
        ValueRequired = 11
    }

    public abstract class AstExpression : IVisitorAcceptor, ILocation, IAstNode
    {
        protected int mFlags = 0;

        public ILocation Location { get; set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public CheezType Type { get; set; }

        private object _value;
        public object Value {
            get => _value;
            set {
                _value = value;
                IsCompTimeValue = _value != null;
            }
        }
        public Scope Scope { get; set; }

        // TODO: still necessary?
        public abstract bool IsPolymorphic { get; }

        public bool IsCompTimeValue { get; set; } = false;

        public IAstNode Parent { get; set; }
        //public IAstNode LinkParent { get; set; }

        public bool TypeInferred { get; set; } = false;

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
            to.IsCompTimeValue = this.IsCompTimeValue;
            return to;
        }

        protected void CopyValuesFrom(AstExpression from)
        {
            this.Location = from.Location;
            this.Scope = from.Scope;
            this.mFlags = from.mFlags;
            this.Value = from.Value;
            this.IsCompTimeValue = from.IsCompTimeValue;
        }

        public override string ToString()
        {
            if (TypeInferred)
                return Accept(new AnalysedAstPrinter());
            else
                return Accept(new RawAstPrinter(new StringWriter()));
        }

        public void Replace(AstExpression expr)
        {
            this.Scope = expr.Scope;
            this.Parent = expr.Parent;
            this.mFlags = expr.mFlags;
        }

        public void AttachTo(AstExpression expr, Scope scope = null)
        {
            this.Scope = scope ?? expr.Scope;
            this.Parent = expr;
        }

        public void AttachTo(AstStatement stmt, Scope scope = null)
        {
            this.Scope = scope ?? stmt.Scope;
            this.Parent = stmt;
        }
    }

    public abstract class AstNestedExpression : AstExpression
    {
        public Scope SubScope { get; set; }

        public AstNestedExpression(ILocation Location = null)
            : base(Location: Location)
        {
        }
    }

    public class AstLambdaExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public List<AstParameter> Parameters { get; set; }
        public AstExpression ReturnTypeExpr { get; set; }
        public AstExpression Body { get; set; }

        public FunctionType FunctionType => Type as FunctionType;

        public AstLambdaExpr(List<AstParameter> parameters, AstExpression body, AstExpression retType, ILocation location = null)
            : base(location)
        {
            this.Parameters = parameters;
            this.ReturnTypeExpr = retType;
            this.Body = body;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
            => visitor.VisitLambdaExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstLambdaExpr(Parameters.Select(p => p.Clone()).ToList(), Body.Clone(), ReturnTypeExpr?.Clone()));
    }

    public class AstMatchCase : ILocation
    {
        public ILocation Location { get; private set; }
        public TokenLocation Beginning => Location?.Beginning;
        public TokenLocation End => Location?.End;

        public AstExpression Pattern { get; set; }
        public AstExpression Condition { get; set; }
        public AstExpression Body { get; set; }

        public Scope SubScope { get; set; }

        public AstMatchCase(AstExpression pattern, AstExpression condition, AstExpression body, ILocation Location = null)
        {
            this.Location = Location;
            this.Pattern = pattern;
            this.Body = body;
            this.Condition = condition;
        }

        public AstMatchCase Clone() => new AstMatchCase(Pattern.Clone(), Condition?.Clone(), Body.Clone(), Location);

        public override string ToString()
        {
            return new RawAstPrinter(null).VisitMatchCase(this);
        }
    }

    public class AstMatchExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public List<AstMatchCase> Cases { get; set; }

        public override bool IsPolymorphic => false;

        public bool IsSimpleIntMatch { get; internal set; } = false;

        public AstMatchExpr(AstExpression value, List<AstMatchCase> cases, ILocation Location = null)
            : base(Location)
        {
            this.SubExpression = value;
            this.Cases = cases;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitMatchExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstMatchExpr(SubExpression.Clone(), Cases.Select(c => c.Clone()).ToList()));
    }

    public class AstIfExpr : AstNestedExpression
    {
        public AstExpression Condition { get; set; }
        public AstExpression IfCase { get; set; }
        public AstExpression ElseCase { get; set; }
        public AstVariableDecl PreAction { get; set; }
        public bool IsConstIf { get; set; }

        public override bool IsPolymorphic => false;

        public AstIfExpr(AstExpression cond, AstExpression ifCase, AstExpression elseCase = null, AstVariableDecl pre = null, bool isConstIf = false, ILocation Location = null)
            : base(Location: Location)
        {
            this.Condition = cond;
            this.IfCase = ifCase;
            this.ElseCase = elseCase;
            this.PreAction = pre;
            this.IsConstIf = isConstIf;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitIfExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstIfExpr(
                Condition.Clone(),
                IfCase.Clone(),
                ElseCase?.Clone(),
                PreAction?.Clone() as AstVariableDecl,
                IsConstIf));
    }

    public class AstBlockExpr : AstNestedExpression
    {
        public List<AstStatement> Statements { get; set; }

        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();
        public List<AstExpression> Destructions { get; private set; } = null;

        public override bool IsPolymorphic => false;

        public AstBlockExpr(List<AstStatement> statements, ILocation Location = null) : base(Location: Location)
        {
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitBlockExpr(this, data);

        public override AstExpression Clone()
         => CopyValuesTo(new AstBlockExpr(Statements.Select(s => s.Clone()).ToList()));

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }
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

    public class AstDefaultExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstDefaultExpr(ILocation Location) : base(Location)
        { }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitDefaultExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstDefaultExpr(Location));
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
        public override bool IsPolymorphic => Left.IsPolymorphic;

        public int DerefCount { get; set; } = 0;

        [DebuggerStepThrough]
        public AstDotExpr(AstExpression left, AstIdExpr right, ILocation Location = null) : base(Location)
        {
            this.Left = left;
            this.Right = right;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDotExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstDotExpr(Left.Clone(), Right.Clone() as AstIdExpr));
    }

    public class AstArgument : AstExpression
    {
        public override bool IsPolymorphic => Expr.IsPolymorphic;
        public AstExpression Expr { get; set; }
        public AstIdExpr Name { get; set; }
        public int Index = -1;

        public bool IsDefaultArg = false;
        public bool IsConstArg = false;

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
        public AstExpression FunctionExpr { get; set; }
        public List<AstArgument> Arguments { get; set; }
        public override bool IsPolymorphic => FunctionExpr.IsPolymorphic || Arguments.Any(a => a.IsPolymorphic);

        public AstFunctionDecl Declaration { get; internal set; }
        public FunctionType FunctionType = null;

        public bool UnifiedFunctionCall { get; set; }

        [DebuggerStepThrough]
        public AstCallExpr(AstExpression func, List<AstArgument> args, ILocation Location = null) : base(Location)
        {
            FunctionExpr = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCallExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCallExpr(FunctionExpr.Clone(),  Arguments.Select(a => a.Clone() as AstArgument).ToList()));
    }

    public class AstCompCallExpr : AstExpression
    {
        public AstIdExpr Name { get; set; }
        public List<AstArgument> Arguments { get; set; }

        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstCompCallExpr(AstIdExpr func, List<AstArgument> args, ILocation Location = null) : base(Location)
        {
            Name = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCompCallExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCompCallExpr(Name.Clone() as AstIdExpr, Arguments.Select(a => a.Clone() as AstArgument).ToList()));
    }

    public class AstNaryOpExpr : AstExpression
    {
        public string Operator { get; set; }
        public List<AstExpression> Arguments { get; set; }

        public override bool IsPolymorphic => Arguments.Any(a => a.IsPolymorphic);

        public INaryOperator ActualOperator { get; set; }

        [DebuggerStepThrough]
        public AstNaryOpExpr(string op, List<AstExpression> args, ILocation Location = null) : base(Location)
        {
            Operator = op;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitNaryOpExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstNaryOpExpr(Operator, Arguments.Select(a => a.Clone()).ToList()));
    }

    public class AstBinaryExpr : AstExpression
    {
        public string Operator { get; set; }
        public AstExpression Left { get; set; }
        public AstExpression Right { get; set; }

        public override bool IsPolymorphic => Left.IsPolymorphic || Right.IsPolymorphic;

        public IBinaryOperator ActualOperator { get; set; }

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

        public IUnaryOperator ActualOperator { get; internal set; } = null;

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
        public bool Reference = false;

        [DebuggerStepThrough]
        public AstAddressOfExpr(AstExpression sub, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitAddressOfExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstAddressOfExpr(SubExpression.Clone())
            {
                Reference = Reference
            });
    }

    public class AstDereferenceExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic;

        public bool Reference { get; set; } = false;

        [DebuggerStepThrough]
        public AstDereferenceExpr(AstExpression sub, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitDerefExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstDereferenceExpr(SubExpression.Clone())
            {
                Reference = Reference
            });
    }

    public class AstCastExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public AstExpression TypeExpr { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Type.IsPolyType;

        public AstIdExpr Name => null;

        [DebuggerStepThrough]
        public AstCastExpr(AstExpression typeExpr, AstExpression sub, ILocation Location = null) : base(Location)
        {
            this.TypeExpr = typeExpr;
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitCastExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstCastExpr(TypeExpr?.Clone(), SubExpression.Clone()));
    }

    public class AstArrayAccessExpr : AstExpression
    {
        public AstExpression SubExpression { get; set; }
        public List<AstExpression> Arguments { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Arguments.Any(a => a.IsPolymorphic);

        [DebuggerStepThrough]
        public AstArrayAccessExpr(AstExpression sub, List<AstExpression> args, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public AstArrayAccessExpr(AstExpression sub, AstExpression arg, ILocation Location = null) : base(Location)
        {
            SubExpression = sub;
            Arguments = new List<AstExpression> { arg };
        }

        [DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitArrayAccessExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => 
            CopyValuesTo(new AstArrayAccessExpr(
                SubExpression.Clone(),
                Arguments.Select(a => a.Clone()).ToList()));
    }

    public class AstNullExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstNullExpr(ILocation Location = null) : base(Location)
        {
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
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitTupleExpr(this, data);

        public bool IsTypeExpr { get; set; } = false;
        public bool IsFullyNamed => Types.All(t => t.Name != null);
        public List<AstExpression> Values { get; set; }
        public List<AstParameter> Types { get; set; }

        public AstTupleExpr(List<AstParameter> values, ILocation Location)
            : base(Location)
        {
            this.Types = values;
            this.Values = Types.Select(t => t.TypeExpr).ToList();
        }

        public override AstExpression Clone()
            => CopyValuesTo(new AstTupleExpr(Types.Select(v => v.Clone()).ToList(), Location));
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
            Value = data;
            this.Suffix = suffix;
        }

        [DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IVisitor<TReturn, TData> visitor, TData data = default) => visitor.VisitNumberExpr(this, data);

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

        internal AstStructMemberInitialization Clone()
        {
            return new AstStructMemberInitialization(Name?.Clone() as AstIdExpr, Value.Clone(), Location);
        }
    }

    public class AstStructValueExpr : AstExpression
    {
        public AstExpression TypeExpr { get; set; }
        public List<AstStructMemberInitialization> MemberInitializers { get; }
        public override bool IsPolymorphic => false;

        public bool FromCall { get; }

        public AstIdExpr Name => null;

        [DebuggerStepThrough]
        public AstStructValueExpr(AstExpression type, List<AstStructMemberInitialization> inits, bool fromCall = false, ILocation Location = null) : base(Location)
        {
            this.TypeExpr = type;
            this.MemberInitializers = inits;
            FromCall = fromCall;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitStructValueExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstStructValueExpr(TypeExpr?.Clone(), MemberInitializers.Select(m => m.Clone()).ToList(), FromCall));
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

    public class AstBreakExpr : AstExpression
    {
        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();
        public List<AstExpression> Destructions { get; private set; } = null;
        public AstWhileStmt Loop { get; set; }

        public AstIdExpr Label { get; set; }

        public override bool IsPolymorphic => false;

        public AstBreakExpr(AstIdExpr label = null, ILocation Location = null) : base(Location: Location)
        {
            this.Label = label;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitBreakExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstBreakExpr(Label?.Clone() as AstIdExpr));

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }
    }

    public class AstContinueExpr : AstExpression
    {
        public List<AstStatement> DeferredStatements { get; } = new List<AstStatement>();
        public List<AstExpression> Destructions { get; private set; } = null;
        public AstWhileStmt Loop { get; set; }

        public AstIdExpr Label { get; set; }

        public override bool IsPolymorphic => throw new System.NotImplementedException();

        public AstContinueExpr(AstIdExpr label = null, ILocation Location = null) : base(Location: Location)
        {
            this.Label = label;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitContinueExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstContinueExpr(Label?.Clone() as AstIdExpr));

        public void AddDestruction(AstExpression dest)
        {
            if (dest == null)
                return;
            if (Destructions == null)
                Destructions = new List<AstExpression>();
            Destructions.Add(dest);
        }
    }

    public class AstRangeExpr : AstExpression
    {
        public AstExpression From { get; set; }
        public AstExpression To { get; set; }

        public override bool IsPolymorphic => false;

        public AstRangeExpr(AstExpression from, AstExpression to, ILocation Location = null) : base(Location: Location)
        {
            this.From = from;
            this.To = to;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitRangeExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstRangeExpr(
                From.Clone(),
                To.Clone()));
    }
}
