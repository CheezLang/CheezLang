using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public AstTypeExpr(CheezType type, ILocation Location = null) : base(Location)
        {
            this.Type = CheezType.Type;
            this.Value = type;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => visitor.VisitTypeExpr(this, data);

        public override AstExpression Clone() => CopyValuesTo(new AstTypeExpr(Type));
    }

    public class AstStructExpression : AstExpression
    {
        public AstStructDecl Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstStructExpression(AstStructDecl @struct, AstExpression original, ILocation Location = null)
            : base(Location)
        {
            Declaration = @struct;
            Type = @struct.Type;
            this.Original = original;
            Value = @struct;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => Original.Accept(visitor, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstStructExpression(Declaration, Original));
    }

    public class AstVariableExpression : AstExpression
    {
        public AstVariableDecl Declaration { get; }
        public AstExpression Original { get; set; }
        public override bool IsPolymorphic => false;

        public AstVariableExpression(AstVariableDecl let, AstExpression original, ILocation Location = null)
            : base(Location)
        {
            Declaration = let;
            Type = let.Type;
            this.Original = original;
            Value = let;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
            => Original != null ? Original.Accept(visitor, data) : visitor.VisitVariableExpression(this, data);

        public override AstExpression Clone()
            => CopyValuesTo(new AstVariableExpression(Declaration, Original));
    }

    public class AstFunctionExpression : AstExpression
    {
        public AstFunctionDecl Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstFunctionExpression(AstFunctionDecl func, AstExpression original, ILocation Location = null) : base(Location)
        {
            Declaration = func;
            Type = func.Type;
            this.Original = original;
            Value = func;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default) => Original != null ? Original.Accept(visitor, data) : default;

        public override AstExpression Clone()
            => CopyValuesTo(new AstFunctionExpression(Declaration, Original));
    }

}
