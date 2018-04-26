using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public enum ExprFlags
    {
        IsLValue = 0
    }

    public abstract class AstExpression : IVisitorAcceptor
    {
        public int Id { get; }

        public abstract ParseTree.PTExpr GenericParseTreeNode { get; }

        public CheezType Type { get; set; }
        public Scope Scope { get; set; }
        private int mFlags = 0;

        public AstExpression()
        {
            this.Id = Util.NewId;
        }

        public void SetFlag(ExprFlags f)
        {
            mFlags |= 1 << (int)f;
        }

        public bool GetFlag(ExprFlags f)
        {
            return (mFlags & (1 << (int)f)) != 0;
        }

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }

    public abstract class AstLiteral : AstExpression
    {
        public AstLiteral() : base()
        {
        }
    }

    public class AstStringLiteral : AstLiteral
    {
        public ParseTree.PTStringLiteral ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public string Value { get; set; }


        public AstStringLiteral(ParseTree.PTStringLiteral node, string value) : base()
        {
            ParseTreeNode = node;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitStringLiteral(this, data);
        }
    }

    public class AstDotExpr : AstExpression
    {
        public ParseTree.PTDotExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression Left { get; set; }
        public string Right { get; set; }

        public AstDotExpr(ParseTree.PTDotExpr node, AstExpression left, string right) : base()
        {
            ParseTreeNode = node;
            this.Left = left;
            this.Right = right;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDotExpression(this, data);
        }
    }

    public class AstCallExpr : AstExpression
    {
        public ParseTree.PTCallExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression Function { get; }
        public List<AstExpression> Arguments { get; set; }

        public AstCallExpr(ParseTree.PTCallExpr node, AstExpression func, List<AstExpression> args) : base()
        {
            ParseTreeNode = node;
            Function = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitCallExpression(this, data);
        }
    }

    public class AstBinaryExpr : AstExpression
    {
        public ParseTree.PTBinaryExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public Operator Operator { get; set; }
        public AstExpression Left { get; set; }
        public AstExpression Right { get; set; }

        public AstBinaryExpr(ParseTree.PTBinaryExpr node, Operator op, AstExpression lhs, AstExpression rhs) : base()
        {
            ParseTreeNode = node;
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBinaryExpression(this, data);
        }
    }

    public class AstBoolExpr : AstExpression
    {
        public ParseTree.PTBoolExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public bool Value => ParseTreeNode.Value;

        public AstBoolExpr(ParseTree.PTBoolExpr node)
        {
            ParseTreeNode = node;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBoolExpression(this, data);
        }
    }

    public class AstAddressOfExpr : AstExpression
    {
        public ParseTree.PTAddressOfExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }

        public AstAddressOfExpr(ParseTree.PTAddressOfExpr node, AstExpression sub)
        {
            ParseTreeNode = node;
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitAddressOfExpression(this, data);
        }
    }

    public class AstCastExpr : AstExpression
    {
        public ParseTree.PTCastExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }

        public AstCastExpr(ParseTree.PTCastExpr node, AstExpression sub)
        {
            ParseTreeNode = node;
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitCastExpression(this, data);
        }
    }
    
    public class AstArrayAccessExpr : AstExpression
    {
        public ParseTree.PTArrayAccessExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }
        public AstExpression Indexer { get; set; }

        public AstArrayAccessExpr(ParseTree.PTArrayAccessExpr node, AstExpression sub, AstExpression index)
        {
            ParseTreeNode = node;
            SubExpression = sub;
            Indexer = index;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitArrayAccessExpression(this, data);
        }
    }
}
