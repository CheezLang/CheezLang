using Cheez.Visitors;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Ast.Expressions.Types
{
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

    public class AstImplTraitTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic || Trait.IsPolymorphic;

        public AstExpression Target { get; set; }
        public AstExpression Trait { get; set; }

        public AstImplTraitTypeExpr(AstExpression target, AstExpression trait, ILocation Location = null) : base(Location)
        {
            this.Target = target;
            this.Trait = trait;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitImplTraitTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstArrayTypeExpr(Target.Clone(), Trait.Clone()));
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

        public List<AstDirective> Directives { get; set; }

        public bool IsFatFunction { get; set; }

        public AstFunctionTypeExpr(List<AstExpression> parTypes, AstExpression returnType, bool isFatFunction, List<AstDirective> dirs, ILocation Location = null) : base(Location)
        {
            this.ParameterTypes = parTypes;
            this.ReturnType = returnType;
            this.Directives = dirs;
            this.IsFatFunction = isFatFunction;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitFunctionTypeExpr(this, data);
        
        public override AstExpression Clone()
        {
            return CopyValuesTo(new AstFunctionTypeExpr(
                ParameterTypes.Select(p => p.Clone()).ToList(),
                ReturnType?.Clone(),
                IsFatFunction,
                Directives));
        }

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

