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
        //public int Id { get; }

        public abstract ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public CheezType Type { get; set; }
        public object Value { get; set; }
        public Scope Scope { get; set; }
        private int mFlags = 0;

        public abstract bool IsPolymorphic { get; }

        [DebuggerStepThrough]
        public AstExpression()
        {
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
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);

        [DebuggerStepThrough]
        public abstract AstExpression Clone();
    }

    public class AstFunctionExpression : AstExpression
    {
        public AstFunctionDecl Declaration { get; }
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstFunctionExpression(ParseTree.PTExpr genericParseTreeNode, AstFunctionDecl func, AstExpression original)
        {
            GenericParseTreeNode = genericParseTreeNode;
            Declaration = func;
            Type = func.Type;
            this.Original = original;
            Value = func;
        }
        
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return Original.Accept(visitor, data);
        }

        public override AstExpression Clone()
        {
            return new AstFunctionExpression(GenericParseTreeNode, Declaration, Original)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }

    public class AstEmptyExpr : AstExpression
    {
        //public ParseTree.PTStringLiteral ParseTreeNode => GenericParseTreeNode as ParseTree.PTStringLiteral;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstEmptyExpr(ParseTree.PTExpr node) : base()
        {
            GenericParseTreeNode = node;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitEmptyExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstEmptyExpr(GenericParseTreeNode)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return "()";
        }
    }

    public abstract class AstLiteral : AstExpression
    {
        public AstLiteral() : base()
        {
        }
    }

    public class AstStringLiteral : AstLiteral
    {
        //public ParseTree.PTStringLiteral ParseTreeNode => GenericParseTreeNode as ParseTree.PTStringLiteral;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public string Value { get; set; }
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstStringLiteral(ParseTree.PTExpr node, string value) : base()
        {
            GenericParseTreeNode = node;
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
            return new AstStringLiteral(GenericParseTreeNode, Value)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return "string-lit";
        }
    }

    public class AstDotExpr : AstExpression
    {
        //public ParseTree.PTDotExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTDotExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression Left { get; set; }
        public string Right { get; set; }
        public bool IsDoubleColon { get; set; }
        public override bool IsPolymorphic => Left.IsPolymorphic;

        [DebuggerStepThrough]
        public AstDotExpr(ParseTree.PTExpr node, AstExpression left, string right, bool isDC) : base()
        {
            GenericParseTreeNode = node;
            this.Left = left;
            this.Right = right;
            IsDoubleColon = isDC;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitDotExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstDotExpr(GenericParseTreeNode, Left.Clone(), Right, IsDoubleColon)
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
        //public ParseTree.PTCallExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTCallExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression Function { get; set; }
        public List<AstExpression> Arguments { get; set; }
        public override bool IsPolymorphic => Function.IsPolymorphic || Arguments.Any(a => a.IsPolymorphic);

        [DebuggerStepThrough]
        public AstCallExpr(ParseTree.PTExpr node, AstExpression func, List<AstExpression> args) : base()
        {
            GenericParseTreeNode = node;
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
            return new AstCallExpr(GenericParseTreeNode, Function.Clone(), Arguments.Select(a => a.Clone()).ToList())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"{Function}(...)";
        }
    }

    public class AstCompCallExpr : AstExpression
    {
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstIdentifierExpr Name { get; set; }
        public List<AstExpression> Arguments { get; set; }

        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstCompCallExpr(ParseTree.PTExpr node, AstIdentifierExpr func, List<AstExpression> args) : base()
        {
            GenericParseTreeNode = node;
            Name = func;
            Arguments = args;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitCompCallExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstCompCallExpr(GenericParseTreeNode, Name.Clone() as AstIdentifierExpr, Arguments.Select(a => a.Clone()).ToList())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments);
            return $"@{Name}({args})";
        }
    }

    public class AstBinaryExpr : AstExpression, ITempVariable
    {
        //public ParseTree.PTBinaryExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTBinaryExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public string Operator { get; set; }
        public AstExpression Left { get; set; }
        public AstExpression Right { get; set; }

        public AstIdentifierExpr Name => null;
        public override bool IsPolymorphic => Left.IsPolymorphic || Right.IsPolymorphic;

        [DebuggerStepThrough]
        public AstBinaryExpr(ParseTree.PTExpr node, string op, AstExpression lhs, AstExpression rhs) : base()
        {
            GenericParseTreeNode = node;
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
            return new AstBinaryExpr(GenericParseTreeNode, Operator, Left.Clone(), Right.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }
    }

    public class AstUnaryExpr : AstExpression
    {
        //public ParseTree.PTBinaryExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTBinaryExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public string Operator { get; set; }
        public AstExpression SubExpr { get; set; }
        public override bool IsPolymorphic => SubExpr.IsPolymorphic;

        [DebuggerStepThrough]
        public AstUnaryExpr(ParseTree.PTExpr node, string op, AstExpression sub) : base()
        {
            GenericParseTreeNode = node;
            Operator = op;
            SubExpr = sub;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitUnaryExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstUnaryExpr(GenericParseTreeNode, Operator, SubExpr.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"{Operator}({SubExpr})";
        }
    }

    public class AstBoolExpr : AstExpression
    {
        //public ParseTree.PTBoolExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTBoolExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public bool Value { get; }
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstBoolExpr(ParseTree.PTExpr node, bool value)
        {
            GenericParseTreeNode = node;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBoolExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstBoolExpr(GenericParseTreeNode, Value)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return "bool-lit";
        }
    }

    public class AstAddressOfExpr : AstExpression
    {
        //public ParseTree.PTAddressOfExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTAddressOfExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression SubExpression { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic;

        [DebuggerStepThrough]
        public AstAddressOfExpr(ParseTree.PTExpr node, AstExpression sub)
        {
            GenericParseTreeNode = node;
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
            return new AstAddressOfExpr(GenericParseTreeNode, SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"&({SubExpression})";
        }
    }

    public class AstDereferenceExpr : AstExpression
    {
        //public ParseTree.PTExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression SubExpression { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic;

        [DebuggerStepThrough]
        public AstDereferenceExpr(ParseTree.PTExpr node, AstExpression sub)
        {
            GenericParseTreeNode = node;
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
            return new AstDereferenceExpr(GenericParseTreeNode, SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"*({SubExpression})";
        }
    }

    public class AstCastExpr : AstExpression
    {
        public ParseTree.PTCastExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTCastExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression SubExpression { get; set; }
        public AstExpression TypeExpr { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Type.IsPolyType;

        [DebuggerStepThrough]
        public AstCastExpr(ParseTree.PTExpr node, AstExpression typeExpr, AstExpression sub)
        {
            GenericParseTreeNode = node;
            this.TypeExpr = typeExpr;
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
            return new AstCastExpr(GenericParseTreeNode, TypeExpr.Clone(), SubExpression.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"({Type})({SubExpression})";
        }
    }
    
    public class AstArrayAccessExpr : AstExpression
    {
        //public ParseTree.PTArrayAccessExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTArrayAccessExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public AstExpression SubExpression { get; set; }
        public AstExpression Indexer { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Indexer.IsPolymorphic;

        [DebuggerStepThrough]
        public AstArrayAccessExpr(ParseTree.PTExpr node, AstExpression sub, AstExpression index)
        {
            GenericParseTreeNode = node;
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
            return new AstArrayAccessExpr(GenericParseTreeNode, SubExpression.Clone(), Indexer.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return $"{SubExpression}[{Indexer}]";
        }
    }

    public class AstNumberExpr : AstExpression
    {
        //public ParseTree.PTNumberExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTNumberExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        private NumberData mData;
        public NumberData Data => mData;
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstNumberExpr(ParseTree.PTExpr node, NumberData data) : base()
        {
            GenericParseTreeNode = node;
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
            return new AstNumberExpr(GenericParseTreeNode, Data)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return mData.StringValue;
        }
    }

    public class AstIdentifierExpr : AstExpression
    {
        //public ParseTree.PTIdentifierExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTIdentifierExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public string Name { get; set; }
        public ISymbol Symbol { get; set; }

        public override bool IsPolymorphic => IsPolymorphicExpression;
        public bool IsPolymorphicExpression { get; set; }

        [DebuggerStepThrough]
        public AstIdentifierExpr(ParseTree.PTExpr node, string name, bool isPolyTypeExpr) : base()
        {
            GenericParseTreeNode = node;
            this.Name = name;
            this.IsPolymorphicExpression = isPolyTypeExpr;
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
            return new AstIdentifierExpr(GenericParseTreeNode, Name, IsPolymorphic)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }
    
    public class AstPointerTypeExpr : AstExpression
    {
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }
        public AstExpression Target { get; set; }
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public bool IsReference { get; set; }

        public AstPointerTypeExpr(ParseTree.PTExpr node, AstExpression target) : base()
        {
            this.GenericParseTreeNode = node;
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstPointerTypeExpr(GenericParseTreeNode, Target.Clone())
            {
                Type = this.Type,
                Scope = this.Scope,
                IsReference = this.IsReference
            };
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitPointerTypeExpr(this, data);
        }

        public override string ToString()
        {
            return $"{Target}^";
        }
    }

    public class AstArrayTypeExpr : AstExpression
    {
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }
        public AstExpression Target { get; set; }
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstArrayTypeExpr(ParseTree.PTExpr node, AstExpression target) : base()
        {
            this.GenericParseTreeNode = node;
            this.Target = target;
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstArrayTypeExpr(GenericParseTreeNode, Target.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitArrayTypeExpr(this, data);
        }

        public override string ToString()
        {
            return $"{Target}[]";
        }
    }

    public class AstStructMemberInitialization
    {
        public ParseTree.PTStructMemberInitialization GenericParseTreeNode { get; set; }

        public string Name { get; set; }
        public AstExpression Value { get; set; }

        public AstStructMemberInitialization(ParseTree.PTStructMemberInitialization node, string name, AstExpression expr)
        {
            this.GenericParseTreeNode = node;
            this.Name = name;
            this.Value = expr;
        }
    }

    public class AstStructValueExpr : AstExpression
    {
        public ParseTree.PTStructValueExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTStructValueExpr;
        public override ParseTree.PTExpr GenericParseTreeNode { get; set; }
        public AstExpression TypeExpr { get; }
        public AstStructMemberInitialization[] MemberInitializers { get; }
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstStructValueExpr(ParseTree.PTExpr node, AstExpression name, AstStructMemberInitialization[] inits) : base()
        {
            GenericParseTreeNode = node;
            this.TypeExpr = name;
            this.MemberInitializers = inits;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitStructValueExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstStructValueExpr(GenericParseTreeNode, TypeExpr, MemberInitializers)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            var i = string.Join(", ", MemberInitializers.Select(m => m.Name != null ? $"{m.Name} = {m.Value}" : m.Value.ToString()));
            return $"{TypeExpr} {{ {i} }}";
        }
    }
}
