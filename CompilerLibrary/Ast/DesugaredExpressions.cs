using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Visitors;
using System;
using System.Diagnostics;

namespace Cheez.Ast.Expressions
{
    public class AstSymbolExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public ITypedSymbol Symbol { get; set; }

        public AstSymbolExpr(ITypedSymbol sym) : base(null)
        {
            this.Symbol = sym;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitSymbolExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstSymbolExpr(Symbol));
    }

    public class AstTempVarExpr : AstExpression
    {
        private static int _id_gen = 0;

        public override bool IsPolymorphic => false;

        public AstExpression Expr { get; set; }
        public readonly int Id = ++_id_gen;

        public bool StorePointer { get; set; } = false;

        public AstTempVarExpr(AstExpression expr, bool storePointer = false) : base(null)
        {
            this.Expr = expr;
            this.StorePointer = storePointer;
            CopyValuesFrom(expr);
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTempVarExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstTempVarExpr(Expr));
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
        public AstStructDecl Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstStructRef(AstStructDecl @struct, AstExpression original, ILocation Location = null)
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
        public AstExpression Original { get; set; }
        public override bool IsPolymorphic => false;

        public AstVariableRef(AstVariableDecl let, AstExpression original, ILocation Location = null)
            : base(Location)
        {
            Declaration = let;
            Type = let.Type;
            this.Original = original;
            Value = let;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
            => Original != null ? Original.Accept(visitor, data) : visitor.VisitVariableRef(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstVariableRef(Declaration, Original));
    }

    public class AstFunctionRef : AstExpression
    {
        public AstFunctionDecl Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstFunctionRef(AstFunctionDecl func, AstExpression original, ILocation Location = null) : base(Location)
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
