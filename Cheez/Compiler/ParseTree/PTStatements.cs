using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public abstract class PTStatement : ILocation
    {
        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public PTStatement(TokenLocation beg, TokenLocation end)
        {
            this.Beginning = beg;
            this.End = end;
        }

        public abstract AstStatement CreateAst();
    }

    public class PTAssignment : PTStatement
    {
        public PTExpr Target { get; set; }
        public PTExpr Value { get; set; }

        public PTAssignment(TokenLocation beg, TokenLocation end, PTExpr target, PTExpr value) : base(beg, end)
        {
            this.Target = target;
            this.Value = value;
        }

        public override AstStatement CreateAst()
        {
            return new AstAssignment(this, Target.CreateAst(), Value.CreateAst());
        }
    }

    public class PTBlockStmt : PTStatement
    {
        public List<PTStatement> Statements { get; }

        public PTBlockStmt(TokenLocation beg, TokenLocation end, List<PTStatement> statements) : base(beg, end)
        {
            this.Statements = statements;
        }

        public override AstStatement CreateAst()
        {
            var list = Statements.Select(s => s.CreateAst()).ToList();
            return new AstBlockStmt(this, list);
        }
    }

    public class PTExprStmt : PTStatement
    {
        public PTExpr Expr { get; set; }

        public PTExprStmt(TokenLocation beg, TokenLocation end, PTExpr expr) : base(beg, end)
        {
            this.Expr = expr;
        }

        public override AstStatement CreateAst()
        {
            return new AstExprStmt(this, Expr.CreateAst());
        }
    }

    public class PTIfStmt : PTStatement
    {
        public PTExpr Condition { get; set; }
        public PTStatement IfCase { get; set; }
        public PTStatement ElseCase { get; set; }

        public PTIfStmt(TokenLocation beg, TokenLocation end, PTExpr cond, PTStatement ifCase, PTStatement elseCase = null) : base(beg, end)
        {
            this.Condition = cond;
            this.IfCase = ifCase;
            this.ElseCase = elseCase;
        }

        public override AstStatement CreateAst()
        {
            return new AstIfStmt(this, Condition.CreateAst(), IfCase.CreateAst(), ElseCase?.CreateAst());
        }
    }

    public class PTReturnStmt : PTStatement
    {
        public PTExpr ReturnValue { get; set; }

        public PTReturnStmt(TokenLocation beg, PTExpr value) : base(beg, value?.End ?? beg)
        {
            ReturnValue = value;
        }

        public override AstStatement CreateAst()
        {
            return new AstReturnStmt(this, ReturnValue.CreateAst());
        }
    }
}
