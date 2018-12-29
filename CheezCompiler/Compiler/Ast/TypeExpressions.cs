﻿using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.Ast
{
    public abstract class AstTypeExpr : AstExpression
    {
        [DebuggerStepThrough]
        public AstTypeExpr(ILocation Location = null) : base(Location)
        {
            IsCompTimeValue = true;
        }
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
        public override bool IsPolymorphic => ParameterTypes.Any(p => p.IsPolymorphic) || (ReturnType?.IsPolymorphic ?? false);

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
        public override AstExpression Clone() => CopyValuesTo(new AstFunctionTypeExpr(ParameterTypes.Select(p => p.Clone() as AstTypeExpr).ToList(), ReturnType?.Clone() as AstTypeExpr));
    }

    public class AstIdTypeExpr : AstTypeExpr
    {
        public override bool IsPolymorphic => false;
        public string Name { get; set; }

        [DebuggerStepThrough]
        public AstIdTypeExpr(string name, ILocation Location = null) : base(Location)
        {
            this.Name = name;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitIdTypeExpr(this, data);

        [DebuggerStepThrough]
        public override AstExpression Clone() => CopyValuesTo(new AstIdTypeExpr(Name));
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
}

