using Cheez.Ast.Statements;
using Cheez.Visitors;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Ast.Expressions.Types
{
    public abstract class AstTypeExpr : AstExpression
    {
        [DebuggerStepThrough]
        public AstTypeExpr(ILocation Location = null) : base(Location)
        {
            IsCompTimeValue = true;
        }
    }

    public class AstErrorTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstErrorTypeExpr(ILocation Location = null) : base(Location)
        { }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitErrorTypeExpression(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstErrorTypeExpr());
    }

    public class AstExprTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => false;
        public AstExpression Expression { get; set; }

        [DebuggerStepThrough]
        public AstExprTypeExpr(AstExpression expr, ILocation Location = null) : base(Location)
        {
            Expression = expr;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D)) => visitor.VisitExprTypeExpression(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstExprTypeExpr(Expression));
    }

    public class AstSliceTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstTypeExpr Target { get; set; }

        public AstSliceTypeExpr(AstTypeExpr target, ILocation Location = null) : base(Location)
        {
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitSliceTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstSliceTypeExpr(Target.Clone() as AstTypeExpr));
    }

    public class AstArrayTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstTypeExpr Target { get; set; }
        public AstExpression SizeExpr { get; set; }

        public AstArrayTypeExpr(AstTypeExpr target, AstExpression size, ILocation Location = null) : base(Location)
        {
            this.Target = target;
            this.SizeExpr = size;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArrayTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstArrayTypeExpr(Target.Clone() as AstTypeExpr, SizeExpr.Clone()));
    }

    public class AstFunctionTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => ParameterTypes.Any(p => p.IsPolymorphic) || ParameterTypes.Any(p => p.IsPolymorphic);

        public AstTypeExpr ReturnType { get; set; }
        public List<AstTypeExpr> ParameterTypes { get; set; }

        public AstFunctionTypeExpr(List<AstTypeExpr> parTypes, AstTypeExpr returnType, ILocation Location = null) : base(Location)
        {
            this.ParameterTypes = parTypes;
            this.ReturnType = returnType;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitFunctionTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstFunctionTypeExpr(
                ParameterTypes.Select(p => p.Clone() as AstTypeExpr).ToList(),
                ReturnType.Clone() as AstTypeExpr));
    }

    public class AstIdTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic { get; }
        public string Name { get; set; }

        [DebuggerStepThrough]
        public AstIdTypeExpr(string name, bool poly, ILocation Location = null) : base(Location)
        {
            this.Name = name;
            this.IsPolymorphic = poly;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitIdTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstIdTypeExpr(Name, IsPolymorphic));
    }

    public class AstPointerTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;
        public AstTypeExpr Target { get; set; }

        [DebuggerStepThrough]
        public AstPointerTypeExpr(AstTypeExpr target, ILocation Location = null) : base(Location)
        {
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitPointerTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstPointerTypeExpr(Target));
    }

    public class AstTupleTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => Members.Any(m => m.TypeExpr.IsPolymorphic);
        public List<AstParameter> Members { get; set; }
        public bool IsFullyNamed { get; }

        public AstTupleTypeExpr(List<AstParameter> members, ILocation Location = null)
            : base(Location)
        {
            this.Members = members;
            IsFullyNamed = members.All(m => m.Name != null);
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTupleTypeExpr(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstTupleTypeExpr(Members.Select(m => m.Clone()).ToList(), Location));
    }

    public class AstPolyStructTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => Struct.IsPolymorphic || Arguments.Any(a => a.IsPolymorphic);
        public AstTypeExpr Struct { get; set; }
        public List<AstTypeExpr> Arguments { get; set; }

        [DebuggerStepThrough]
        public AstPolyStructTypeExpr(AstTypeExpr str, List<AstTypeExpr> args, ILocation Location = null) : base(Location)
        {
            this.Struct = str;
            this.Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitPolyStructTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstPolyStructTypeExpr(Struct.Clone() as AstTypeExpr, Arguments.Select(a => a.Clone() as AstTypeExpr).ToList()));
    }
}

