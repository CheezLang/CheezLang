using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.Ast
{
    public class AstArrayTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstExpression Target { get; set; }

        public AstArrayTypeExpr(AstExpression target, ILocation Location = null) : base(Location)
        {
            this.Target = target;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitArrayTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstArrayTypeExpr(Target.Clone()));
    }

    public class AstFunctionTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => ParameterTypes.Any(p => p.IsPolymorphic) || (ReturnType?.IsPolymorphic ?? false);

        public AstExpression ReturnType { get; set; }
        public List<AstExpression> ParameterTypes { get; set; }

        public AstFunctionTypeExpr(List<AstExpression> parTypes, AstExpression returnType, ILocation Location = null) : base(Location)
        {
            this.ParameterTypes = parTypes;
            this.ReturnType = returnType;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitFunctionTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstFunctionTypeExpr(ParameterTypes.Select(p => p.Clone()).ToList(), ReturnType?.Clone()));
    }

}
