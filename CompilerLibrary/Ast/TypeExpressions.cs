using Cheez.Ast.Statements;
using Cheez.Visitors;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Ast.Expressions.Types
{
    //public abstract class AstTypeExpr : AstExpression
    //{
    //    [DebuggerStepThrough]
    //    public AstTypeExpr(ILocation Location = null) : base(Location)
    //    {
    //        IsCompTimeValue = true;
    //    }
    //}

    public class AstErrorTypeExpr : AstExpression
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

    public class AstSliceTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstExpression Target { get; set; }

        public AstSliceTypeExpr(AstExpression target, ILocation Location = null) : base(Location)
        {
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitSliceTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstSliceTypeExpr(Target.Clone()));
    }

    public class AstArrayTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstExpression Target { get; set; }
        public AstExpression SizeExpr { get; set; }

        public AstArrayTypeExpr(AstExpression target, AstExpression size, ILocation Location = null) : base(Location)
        {
            this.Target = target;
            this.SizeExpr = size;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArrayTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstArrayTypeExpr(Target.Clone(), SizeExpr.Clone()));
    }

    public class AstFunctionTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => ParameterTypes.Any(p => p.IsPolymorphic) || ParameterTypes.Any(p => p.IsPolymorphic);

        public AstExpression ReturnType { get; set; }
        public List<AstExpression> ParameterTypes { get; set; }

        public AstFunctionTypeExpr(List<AstExpression> parTypes, AstExpression returnType, ILocation Location = null) : base(Location)
        {
            this.ParameterTypes = parTypes;
            this.ReturnType = returnType;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitFunctionTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone()
            => CopyValuesTo(new AstFunctionTypeExpr(
                ParameterTypes.Select(p => p.Clone()).ToList(),
                ReturnType.Clone()));
    }

    public class AstReferenceTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;
        public AstExpression Target { get; set; }

        [DebuggerStepThrough]
        public AstReferenceTypeExpr(AstExpression target, ILocation Location = null) : base(Location)
        {
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitReferenceTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstReferenceTypeExpr(Target.Clone()));
    }
}

