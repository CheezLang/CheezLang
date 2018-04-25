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

        public PTStringLiteral(TokenLocation beg, string value) : base(beg, beg)
        {
            this.Value = value;
        }

        public override AstExpression CreateAst()
        {
            return new AstStringLiteral(this, Value);
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
            return new AstBoolExpr(this);
        }
    }

    public class PTDotExpr : PTExpr
    {
        public PTExpr Left { get; set; }
        public PTIdentifierExpr Right { get; set; }

        public PTDotExpr(TokenLocation beg, TokenLocation end, PTExpr left, PTIdentifierExpr right) : base(beg, end)
        {
            this.Left = left;
            this.Right = right;
        }

        public override AstExpression CreateAst()
        {
            return new AstDotExpr(this, Left.CreateAst(), Right.Name);
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

    public class PTBinaryExpr : PTExpr
    {
        public Operator Operator { get; set; }
        public PTExpr Left { get; set; }
        public PTExpr Right { get; set; }

        public PTBinaryExpr(TokenLocation beg, TokenLocation end, Operator op, PTExpr lhs, PTExpr rhs) : base(beg, end)
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

    public class PTIdentifierExpr : PTExpr
    {
        public string Name { get; set; }

        public PTIdentifierExpr(TokenLocation beg, string name) : base(beg, beg)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override AstExpression CreateAst()
        {
            return new AstIdentifierExpr(this, Name);
        }
    }

    public class PTAddressOfExpr : PTExpr
    {
        public PTExpr Variable { get; set; }

        public PTAddressOfExpr(TokenLocation beg, PTExpr v) : base(beg, beg)
        {
            this.Variable = v;
        }

        public override string ToString()
        {
            return $"&{Variable}";
        }

        public override AstExpression CreateAst()
        {
            return new AstAddressOfExpr(this, Variable.CreateAst());
        }
    }

    public class PTCastExpr : PTExpr
    {
        public PTExpr SubExpression { get; set; }
        public PTTypeExpr TargetType { get; set; }

        public PTCastExpr(TokenLocation beg, TokenLocation end, PTTypeExpr target, PTExpr v) : base(beg, end)
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
            return new AstCastExpr(this, SubExpression.CreateAst());
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
    
}
