using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public abstract class PTExpr : ILocation
    {
        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public PTExpr(TokenLocation beg, TokenLocation end)
        {
            this.Beginning = beg;
            this.End = end;
        }

        public abstract AstExpression CreateAst();

        //public T CreateAst<T>()
        //    where T : AstExpression
        //{
        //    return (T)CreateGenericAst();
        //}

        public override string ToString()
        {
            return $"{Beginning.file}: ({Beginning.line}:{Beginning.index - Beginning.lineStartIndex}) - ({End.line}:{End.index - End.lineStartIndex})";
        }
    }

    public class PTErrorExpr : PTExpr
    {
        public string Reason { get; set; }

        public PTErrorExpr(TokenLocation beg, TokenLocation end = null, string reason = null) : base(beg, end ?? beg)
        {
            this.Reason = reason;
        }

        public override AstExpression CreateAst()
        {
            return new AstEmptyExpr(this);
        }
    }

    public abstract class PTLiteral : PTExpr
    {
        public PTLiteral(TokenLocation beg, TokenLocation end) : base(beg, end)
        {
        }
    }

    public class PTStringLiteral : PTLiteral
    {
        public string Value { get; set; }
        public bool IsChar { get; set; }

        public PTStringLiteral(TokenLocation beg, string value, bool isChar) : base(beg, beg)
        {
            this.Value = value;
            this.IsChar = isChar;
        }

        public override AstExpression CreateAst()
        {
            return new AstStringLiteral(this, Value, IsChar);
        }
    }

    public class PTNullExpr : PTLiteral
    {
        public PTNullExpr(TokenLocation loc) : base(loc, loc)
        {
        }

        public override AstExpression CreateAst()
        {
            return new AstNullExpr(this);
        }

        public override string ToString()
        {
            return "null";
        }
    }

    public class PTNumberExpr : PTLiteral
    {
        private NumberData mData;
        public NumberData Data => mData;

        public PTNumberExpr(TokenLocation loc, NumberData data) : base(loc, loc)
        {
            mData = data;
        }

        public override AstExpression CreateAst()
        {
            return new AstNumberExpr(this, Data);
        }

        public override string ToString()
        {
            return mData.StringValue;
        }
    }

    public class PTBoolExpr : PTLiteral
    {
        public bool Value { get; }

        public PTBoolExpr(TokenLocation loc, bool value) : base(loc, loc)
        {
            this.Value = value;
        }

        public override AstExpression CreateAst()
        {
            return new AstBoolExpr(this, Value);
        }
    }

    public class PTDotExpr : PTExpr
    {
        public PTExpr Left { get; }
        public PTIdentifierExpr Right { get; }
        public bool IsDoubleColon { get; }

        public PTDotExpr(TokenLocation beg, TokenLocation end, PTExpr left, PTIdentifierExpr right, bool isDC) : base(beg, end)
        {
            this.Left = left;
            this.Right = right;
            this.IsDoubleColon = isDC;
        }

        public override AstExpression CreateAst()
        {
            return new AstDotExpr(this, Left.CreateAst(), Right.Name, IsDoubleColon);
        }
    }

    public class PTCallExpr : PTExpr
    {
        public PTExpr Function { get; }
        public List<PTExpr> Arguments { get; set; }

        public PTCallExpr(TokenLocation beg, TokenLocation end, PTExpr func, List<PTExpr> args) : base(beg, end)
        {
            Function = func;
            Arguments = args;
        }

        public override AstExpression CreateAst()
        {
            var args = Arguments.Select(a => a.CreateAst()).ToList();
            return new AstCallExpr(this, Function.CreateAst(), args);
        }
    }

    public class PTCompCallExpr : PTExpr
    {
        public PTIdentifierExpr Name { get; }
        public List<PTExpr> Arguments { get; set; }

        public PTCompCallExpr(TokenLocation end, PTIdentifierExpr func, List<PTExpr> args) : base(func.Beginning, end)
        {
            Name = func;
            Arguments = args;
        }

        public override AstExpression CreateAst()
        {
            var args = Arguments.Select(a => a.CreateAst()).ToList();
            return new AstCompCallExpr(this, Name.CreateAst() as AstIdentifierExpr, args);
        }
    }

    public class PTBinaryExpr : PTExpr
    {
        public string Operator { get; set; }
        public PTExpr Left { get; set; }
        public PTExpr Right { get; set; }

        public PTBinaryExpr(TokenLocation beg, TokenLocation end, string op, PTExpr lhs, PTExpr rhs) : base(beg, end)
        {
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        public override AstExpression CreateAst()
        {
            return new AstBinaryExpr(this, Operator, Left.CreateAst(), Right.CreateAst());
        }
    }

    public class PTUnaryExpr : PTExpr
    {
        public string Operator { get; set; }
        public PTExpr SubExpr { get; set; }

        public PTUnaryExpr(TokenLocation beg, TokenLocation end, string op, PTExpr lhs) : base(beg, end)
        {
            Operator = op;
            SubExpr = lhs;
        }

        public override AstExpression CreateAst()
        {
            return new AstUnaryExpr(this, Operator, SubExpr.CreateAst());
        }
    }

    public class PTIdentifierExpr : PTExpr
    {
        public string Name { get; set; }
        public bool IsPolyTypeExpr { get; }

        public PTIdentifierExpr(TokenLocation beg, string name, bool isPolyTypeExpr) : base(beg, beg)
        {
            this.Name = name;
            this.IsPolyTypeExpr = isPolyTypeExpr;
        }

        public override string ToString()
        {
            return Name;
        }

        public override AstExpression CreateAst()
        {
            return new AstIdentifierExpr(this, Name, IsPolyTypeExpr);
        }
    }

    public class PTAddressOfExpr : PTExpr
    {
        public PTExpr SubExpression { get; set; }

        public PTAddressOfExpr(TokenLocation beg, TokenLocation end, PTExpr v) : base(beg, end)
        {
            this.SubExpression = v;
        }

        public override string ToString()
        {
            return $"&{SubExpression}";
        }

        public override AstExpression CreateAst()
        {
            return new AstAddressOfExpr(this, SubExpression.CreateAst());
        }
    }

    public class PTDereferenceExpr : PTExpr
    {
        public PTExpr SubExpression { get; set; }

        public PTDereferenceExpr(TokenLocation beg, TokenLocation end, PTExpr v) : base(beg, end)
        {
            this.SubExpression = v;
        }

        public override string ToString()
        {
            return $"&{SubExpression}";
        }

        public override AstExpression CreateAst()
        {
            return new AstDereferenceExpr(this, SubExpression.CreateAst());
        }
    }

    public class PTCastExpr : PTExpr
    {
        public PTExpr SubExpression { get; set; }
        public PTExpr TargetType { get; set; }

        public PTCastExpr(TokenLocation beg, TokenLocation end, PTExpr target, PTExpr v) : base(beg, end)
        {
            this.SubExpression = v;
            this.TargetType = target;
        }

        public override string ToString()
        {
            return $"cast<{TargetType}>({SubExpression})";
        }

        public override AstExpression CreateAst()
        {
            return new AstCastExpr(this, TargetType.CreateAst(), SubExpression.CreateAst());
        }
    }

    public class PTArrayAccessExpr : PTExpr
    {
        public PTExpr SubExpression { get; set; }
        public PTExpr Indexer { get; set; }

        public PTArrayAccessExpr(TokenLocation beg, TokenLocation end, PTExpr sub, PTExpr index) : base(beg, end)
        {
            this.SubExpression = sub;
            this.Indexer = index;
        }

        public override string ToString()
        {
            return $"{SubExpression}{Indexer}]";
        }

        public override AstExpression CreateAst()
        {
            return new AstArrayAccessExpr(this, SubExpression.CreateAst(), Indexer.CreateAst());
        }
    }

    public class PTStructMemberInitialization
    {
        public PTIdentifierExpr Name { get; set; }
        public PTExpr Value { get; set; }
    }

    public class PTStructValueExpr : PTExpr
    {
        public PTExpr Type { get; }
        public List<PTStructMemberInitialization> Initializers { get; }

        public PTStructValueExpr(TokenLocation beg, TokenLocation end, PTExpr type, List<PTStructMemberInitialization> inits) : base(beg, end)
        {
            this.Type = type;
            this.Initializers = inits;
        }

        public override string ToString()
        {
            var i = string.Join(", ", Initializers.Select(m => m.Name != null ? $"{m.Name} = {m.Value}" : m.Value.ToString()));
            return $"{Type} {{ {i} }}";
        }

        public override AstExpression CreateAst()
        {
            var inits = Initializers.Select(i => new AstStructMemberInitialization(i, i.Name?.Name, i.Value.CreateAst())).ToArray();
            return new AstStructValueExpr(this, Type.CreateAst(), inits);
        }
    }

    public class PTArrayExpression : PTExpr
    {
        public List<PTExpr> Values { get; set; }

        public PTArrayExpression(TokenLocation beg, TokenLocation end, List<PTExpr> values) : base(beg, end)
        {
            this.Values = values;
        }

        public override AstExpression CreateAst()
        {
            var vs = Values.Select(v => v.CreateAst()).ToList();
            return new AstArrayExpression(this, vs);
        }
    }


}
