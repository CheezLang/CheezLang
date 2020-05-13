using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Visitors;
using CompilerLibrary.Extras;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast.Expressions
{
    public class AstEnumValueExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public bool IsComplete => !(Member.AssociatedTypeExpr != null && Argument == null);
        
        public AstEnumTypeExpr EnumDecl { get; set; }
        public AstEnumMemberNew Member { get; set; }
        public AstExpression Argument { get; set; }

        public AstEnumValueExpr(AstEnumTypeExpr ed, AstEnumMemberNew member, AstExpression arg = null, ILocation loc = null)
            : base(loc)
        {
            Member = member;
            Type = ed.EnumType;
            Argument = arg;
            EnumDecl = ed;

            if (ed.IsFlags)
                Value = new EnumValue(ed.EnumType, member);
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitEnumValueExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstEnumValueExpr(EnumDecl, Member, Argument));
    }

    public class AstUfcFuncExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public AstExpression SelfArg { get; }
        public AstFuncExpr FunctionDecl { get; }

        public AstUfcFuncExpr(AstExpression self, AstFuncExpr func, ILocation Location) : base(Location)
        {
            this.SelfArg = self;
            this.FunctionDecl = func;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitUfcFuncExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstUfcFuncExpr(SelfArg, FunctionDecl, Location));
    }

    public class AstSymbolExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public ITypedSymbol Symbol { get; set; }

        public AstSymbolExpr(ITypedSymbol sym) : base(sym.Location)
        {
            this.Symbol = sym;
            this.Type = sym.Type;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitSymbolExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstSymbolExpr(Symbol));
    }

    public class AstTempVarExpr : AstExpression
    {
        private static int _id_gen = 0;

        public override bool IsPolymorphic => false;

        public AstExpression Expr { get; set; }
        private readonly int _id = ++_id_gen;
        public int Id => _id;

        public bool StorePointer { get; set; } = false;

        public AstTempVarExpr(AstExpression expr, bool storePointer = false) : base(null)
        {
            this.Expr = expr;
            this.StorePointer = storePointer;
            CopyValuesFrom(expr);
            Value = null;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTempVarExpr(this, data);

        public override AstExpression Clone() => this;// CopyValuesTo(new AstTempVarExpr(Expr));
    }

    public class AstTypeRef : AstExpression
    {
        public override bool IsPolymorphic => false;

        public AstTypeRef(CheezType type, ILocation Location = null) : base(Location)
        {
            this.Type = CheezType.Type;
            this.Value = type;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTypeExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstTypeRef(Type));
    }

    public class AstStructRef : AstExpression
    {
        public AstStructTypeExpr Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstStructRef(AstStructTypeExpr @struct, AstExpression original, ILocation Location = null)
            : base(Location)
        {
            Declaration = @struct;
            Type = @struct.Type;
            this.Original = original;
            Value = @struct;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => Original.Accept(visitor, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstStructRef(Declaration, Original));
    }

    public class AstVariableRef : AstExpression
    {
        public AstVariableDecl Declaration { get; }
        public override bool IsPolymorphic => false;

        public AstVariableRef(AstVariableDecl let, ILocation Location = null)
            : base(Location)
        {
            Declaration = let;
            Type = let.Type;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitVariableRef(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstVariableRef(Declaration));
    }

    public class AstConstantRef : AstExpression
    {
        public AstConstantDeclaration Declaration { get; }
        public override bool IsPolymorphic => false;

        public AstConstantRef(AstConstantDeclaration let, ILocation Location = null)
            : base(Location)
        {
            Declaration = let;
            Type = let.Type;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitConstantRef(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstConstantRef(Declaration));
    }

    public class AstFunctionRef : AstExpression
    {
        public AstFuncExpr Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstFunctionRef(AstFuncExpr func, AstExpression original, ILocation Location = null) : base(Location)
        {
            Declaration = func;
            Type = func.Type;
            this.Original = original;
            Value = func;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => Original != null ? Original.Accept(visitor, data) : default;

        public override AstExpression Clone()
            => CopyValuesTo(new AstFunctionRef(Declaration, Original));
    }

}
