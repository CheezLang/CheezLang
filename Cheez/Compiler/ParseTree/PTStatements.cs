using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public abstract class PTStatement : ILocation
    {
        public PTFile SourceFile { get; set; }
        
        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public PTStatement(TokenLocation beg, TokenLocation end)
        {
            this.Beginning = beg;
            this.End = end;
        }

        public abstract AstStatement CreateAst();
    }

    public class PTErrorStmt : PTStatement
    {
        public string Reason { get; set; }

        public PTErrorStmt(TokenLocation beg, string reason) : base(beg, beg)
        {
            this.Reason = reason;
        }

        public override AstStatement CreateAst()
        {
            throw new NotImplementedException();
        }
    }

    public class PTDirectiveStatement : PTStatement
    {
        public PTDirective Directive { get; }

        public PTDirectiveStatement(PTDirective dir) : base(dir.Beginning, dir.End)
        {
            Directive = dir;
        }

        public override AstStatement CreateAst()
        {
            throw new NotImplementedException();
        }
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

    public class PTWhileStmt : PTStatement
    {
        public PTExpr Condition { get; set; }
        public PTStatement Body { get; set; }
        public PTVariableDecl PreAction { get; set; }
        public PTStatement PostAction { get; set; }

        public PTWhileStmt(TokenLocation beg, TokenLocation end, PTExpr cond, PTStatement body, PTVariableDecl pre, PTStatement post) : base(beg, end)
        {
            this.Condition = cond;
            this.Body = body;
            this.PreAction = pre;
            this.PostAction = post;
        }

        public override AstStatement CreateAst()
        {
            return new AstWhileStmt(this, Condition.CreateAst(), Body.CreateAst(), PreAction?.CreateAst(), PostAction?.CreateAst());
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
            return new AstReturnStmt(this, ReturnValue?.CreateAst());
        }
    }
}
