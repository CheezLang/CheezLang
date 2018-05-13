using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        [DebuggerStepThrough]
        public AstExpression()
        {
            this.Id = Util.NewId;
        }

        [DebuggerStepThrough]
        public void SetFlag(ExprFlags f)
        {
            mFlags |= 1 << (int)f;
        }

        [DebuggerStepThrough]
        public bool GetFlag(ExprFlags f)
        {
            return (mFlags & (1 << (int)f)) != 0;
        }

        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            return obj == this;
        }

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);

        [DebuggerStepThrough]
        public abstract AstExpression Clone();
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
        
        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstStringLiteral(ParseTreeNode, Value)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstDotExpr : AstExpression
    {
        public ParseTree.PTDotExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression Left { get; set; }
        public string Right { get; set; }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstDotExpr(ParseTreeNode, Left.Clone(), Right)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"{Left}.{Right}";
        }
    }

    public class AstCallExpr : AstExpression
    {
        public ParseTree.PTCallExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression Function { get; set; }
        public List<AstExpression> Arguments { get; set; }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstCallExpr(ParseTreeNode, Function.Clone(), Arguments.Select(a => a.Clone()).ToList())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstBinaryExpr : AstExpression
    {
        public ParseTree.PTBinaryExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public string Operator { get; set; }
        public AstExpression Left { get; set; }
        public AstExpression Right { get; set; }

        [DebuggerStepThrough]
        public AstBinaryExpr(ParseTree.PTBinaryExpr node, string op, AstExpression lhs, AstExpression rhs) : base()
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstBinaryExpr(ParseTreeNode, Operator, Left.Clone(), Right.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstBoolExpr : AstExpression
    {
        public ParseTree.PTBoolExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public bool Value => ParseTreeNode.Value;

        [DebuggerStepThrough]
        public AstBoolExpr(ParseTree.PTBoolExpr node)
        {
            ParseTreeNode = node;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBoolExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstBoolExpr(ParseTreeNode)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstAddressOfExpr : AstExpression
    {
        public ParseTree.PTAddressOfExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstAddressOfExpr(ParseTreeNode, SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstDereferenceExpr : AstExpression
    {
        public ParseTree.PTExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }

        [DebuggerStepThrough]
        public AstDereferenceExpr(ParseTree.PTExpr node, AstExpression sub)
        {
            ParseTreeNode = node;
            SubExpression = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDereferenceExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstDereferenceExpr(ParseTreeNode, SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstCastExpr : AstExpression
    {
        public ParseTree.PTCastExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstCastExpr(ParseTreeNode, SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }
    
    public class AstArrayAccessExpr : AstExpression
    {
        public ParseTree.PTArrayAccessExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public AstExpression SubExpression { get; set; }
        public AstExpression Indexer { get; set; }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstArrayAccessExpr(ParseTreeNode, SubExpression.Clone(), Indexer.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstNumberExpr : AstExpression
    {
        public ParseTree.PTNumberExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        private NumberData mData;
        public NumberData Data => mData;

        [DebuggerStepThrough]
        public AstNumberExpr(ParseTree.PTNumberExpr node, NumberData data) : base()
        {
            ParseTreeNode = node;
            mData = data;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitNumberExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstNumberExpr(ParseTreeNode, Data)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstIdentifierExpr : AstExpression
    {
        public ParseTree.PTIdentifierExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        public string Name { get; set; }

        [DebuggerStepThrough]
        public AstIdentifierExpr(ParseTree.PTIdentifierExpr node, string name) : base()
        {
            ParseTreeNode = node;
            this.Name = name;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIdentifierExpression(this, data);
        }

        public override string ToString()
        {
            return Name;
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstIdentifierExpr(ParseTreeNode, Name)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstTypeExpr : AstExpression
    {
        public ParseTree.PTTypeExpr ParseTreeNode { get; set; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        [DebuggerStepThrough]
        public AstTypeExpr(ParseTree.PTTypeExpr node) : base()
        {
            ParseTreeNode = node;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstTypeExpr(ParseTreeNode)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return ParseTreeNode?.ToString() ?? Type?.ToString() ?? base.ToString();
        }
    }
}
