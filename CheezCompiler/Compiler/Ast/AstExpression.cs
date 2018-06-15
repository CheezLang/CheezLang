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

        public ParseTree.PTExpr GenericParseTreeNode { get; set; }

        public CheezType Type { get; set; }
        public object Value { get; set; }
        public Scope Scope { get; set; }
        private int mFlags = 0;

        public abstract bool IsPolymorphic { get; }

        public bool IsCompTimeValue { get; set; } = false;

        [DebuggerStepThrough]
        public AstExpression(ParseTree.PTExpr node = null)
        {
            this.GenericParseTreeNode = node;
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

    public class AstStructExpression : AstExpression
    {
        public AstStructDecl Declaration { get; }

        public AstExpression Original { get; set; }

        public override bool IsPolymorphic => false;

        public AstStructExpression(ParseTree.PTExpr genericParseTreeNode, AstStructDecl @struct, AstExpression original)
        {
            GenericParseTreeNode = genericParseTreeNode;
            Declaration = @struct;
            Type = @struct.Type;
            this.Original = original;
            Value = @struct;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return Original.Accept(visitor, data);
        }

        public override AstExpression Clone()
        {
            return new AstStructExpression(GenericParseTreeNode, Declaration, Original)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return Original.ToString();
        }
    }

    public class AstFunctionExpression : AstExpression
    {
        public AstFunctionDecl Declaration { get; }

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

        [DebuggerStepThrough]
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

        public override string ToString()
        {
            return Original.ToString();
        }
    }

    public class AstEmptyExpr : AstExpression
    {
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

        public override bool IsPolymorphic => false;
        public string StringValue => (string)Value;
        public char CharValue { get; set; }

        public bool IsChar { get; set; }

        [DebuggerStepThrough]
        public AstStringLiteral(ParseTree.PTExpr node, string value, bool isChar) : base()
        {
            GenericParseTreeNode = node;
            this.Value = value;
            this.IsCompTimeValue = true;
            this.IsChar = isChar;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitStringLiteral(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstStringLiteral(GenericParseTreeNode, Value as string, IsChar)
            {
                Type = this.Type,
                Scope = this.Scope,
                CharValue = this.CharValue
            };
        }

        public override string ToString()
        {
            if (IsChar)
                return $"'{Value}'";
            return '"' + StringValue + '"';
        }
    }

    public class AstDotExpr : AstExpression
    {

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
            var args = string.Join(", ", Arguments);
            return $"{Function}({args})";
        }
    }

    public class AstCompCallExpr : AstExpression
    {

        public AstIdentifierExpr Name { get; set; }
        public List<AstExpression> Arguments { get; set; }

        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstCompCallExpr(ParseTree.PTExpr node, AstIdentifierExpr func, List<AstExpression> args) : base()
        {
            GenericParseTreeNode = node;
            Name = func;
            Arguments = args;
            IsCompTimeValue = true;
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

        public override bool IsPolymorphic => false;

        public bool BoolValue => (bool)Value;

        [DebuggerStepThrough]
        public AstBoolExpr(ParseTree.PTExpr node, bool value)
        {
            GenericParseTreeNode = node;
            this.Value = value;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBoolExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstBoolExpr(GenericParseTreeNode, (bool)Value)
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

    public class AstCastExpr : AstExpression, ITempVariable
    {
        public AstExpression SubExpression { get; set; }
        public AstExpression TypeExpr { get; set; }
        public override bool IsPolymorphic => SubExpression.IsPolymorphic || Type.IsPolyType;

        public AstIdentifierExpr Name => null;

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

    public class AstNullExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstNullExpr(ParseTree.PTExpr node) : base()
        {
            GenericParseTreeNode = node;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitNullExpression(this, data);
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstNullExpr(GenericParseTreeNode)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return "nullptr";
        }
    }

    public class AstNumberExpr : AstExpression
    {
        private NumberData mData;
        public NumberData Data => mData;
        public override bool IsPolymorphic => false;

        [DebuggerStepThrough]
        public AstNumberExpr(ParseTree.PTExpr node, NumberData data) : base()
        {
            GenericParseTreeNode = node;
            mData = data;
            IsCompTimeValue = true;
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
        public string Name { get; set; }
        public ISymbol Symbol { get; set; }

        public override bool IsPolymorphic => _isPolymorphic;
        private bool _isPolymorphic;

        [DebuggerStepThrough]
        public AstIdentifierExpr(ParseTree.PTExpr node, string name, bool isPolyTypeExpr) : base()
        {
            GenericParseTreeNode = node;
            this.Name = name;
            this._isPolymorphic = isPolyTypeExpr;
        }

        public void SetIsPolymorphic(bool b)
        {
            _isPolymorphic = b;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitIdentifierExpression(this, data);
        }

        public override string ToString()
        {
            if (IsPolymorphic)
                return $"${Name}";
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

    public class AstTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => false;

        public AstTypeExpr(ParseTree.PTExpr node, CheezType type) : base(node)
        {
            this.Type = type;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeExpr(this, data);
        }

        public override AstExpression Clone()
        {
            return new AstTypeExpr(GenericParseTreeNode, Type)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public class AstPointerTypeExpr : AstExpression
    {
        public AstExpression Target { get; set; }
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public bool IsReference { get; set; }

        public AstPointerTypeExpr(ParseTree.PTExpr node, AstExpression target) : base()
        {
            this.GenericParseTreeNode = node;
            this.Target = target;
            IsCompTimeValue = true;
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
            return $"{Target}&";
        }
    }

    public class AstArrayTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => Target.IsPolymorphic;

        public AstExpression Target { get; set; }

        public AstArrayTypeExpr(ParseTree.PTExpr node, AstExpression target) : base()
        {
            this.GenericParseTreeNode = node;
            this.Target = target;
            IsCompTimeValue = true;
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

    public class AstFunctionTypeExpr : AstExpression
    {
        public override bool IsPolymorphic => ParameterTypes.Any(p => p.IsPolymorphic) || (ReturnType?.IsPolymorphic ?? false);

        public AstExpression ReturnType { get; set; }
        public List<AstExpression> ParameterTypes { get; set; }

        public AstFunctionTypeExpr(ParseTree.PTExpr node, List<AstExpression> parTypes, AstExpression returnType) : base()
        {
            this.GenericParseTreeNode = node;
            this.ParameterTypes = parTypes;
            this.ReturnType = returnType;
            IsCompTimeValue = true;
        }

        [DebuggerStepThrough]
        public override AstExpression Clone()
        {
            return new AstFunctionTypeExpr(GenericParseTreeNode, ParameterTypes.Select(p => p.Clone()).ToList(), ReturnType?.Clone())
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitFunctionTypeExpr(this, data);
        }

        public override string ToString()
        {
            var p = string.Join(", ", ParameterTypes);
            return $"fn ({p}) -> {ReturnType?.ToString() ?? "void"}";
        }
    }

    public class AstStructMemberInitialization
    {
        public ParseTree.PTStructMemberInitialization GenericParseTreeNode { get; set; }

        public string Name { get; set; }
        public AstExpression Value { get; set; }

        public int Index { get; set; }

        public AstStructMemberInitialization(ParseTree.PTStructMemberInitialization node, string name, AstExpression expr)
        {
            this.GenericParseTreeNode = node;
            this.Name = name;
            this.Value = expr;
        }
    }

    public class AstStructValueExpr : AstExpression, ITempVariable
    {
        public ParseTree.PTStructValueExpr ParseTreeNode => GenericParseTreeNode as ParseTree.PTStructValueExpr;
        public AstExpression TypeExpr { get; set; }
        public AstStructMemberInitialization[] MemberInitializers { get; }
        public override bool IsPolymorphic => false;

        public AstIdentifierExpr Name => null;

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

    public class AstArrayExpression : AstExpression, ITempVariable
    {
        public override bool IsPolymorphic => false;

        public List<AstExpression> Values { get; set; }

        public AstIdentifierExpr Name => null;

        public AstArrayExpression(ParseTree.PTExpr node, List<AstExpression> values) : base(node)
        {
            this.Values = values;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitArrayExpression(this, data);
        }

        public override AstExpression Clone()
        {
            return new AstArrayExpression(GenericParseTreeNode, Values)
            {
                Type = this.Type,
                Scope = this.Scope
            };
        }
    }
}
