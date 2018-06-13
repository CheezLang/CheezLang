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
        public List<PTDirective> Directives { get; set; }

        public PTStatement(TokenLocation beg, TokenLocation end, List<PTDirective> directives = null)
        {
            this.Beginning = beg;
            this.End = end;
            this.Directives = directives;
        }

        public abstract AstStatement CreateAst();
        
        public Dictionary<string, AstDirective> CreateDirectivesAst()
        {
            return Directives?.Select(d => d.CreateAst()).ToDictionary(d => d.Name);
        }
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

    public class PTDeferStatement : PTStatement
    {
        public PTStatement DeferredStatement { get; set; }

        public PTDeferStatement(TokenLocation beg, PTStatement deferred, List<PTDirective> directives = null) : base(beg, deferred.End, directives)
        {
            this.DeferredStatement = deferred;
        }

        public override AstStatement CreateAst()
        {
            return new AstDeferStmt(this, DeferredStatement.CreateAst(), CreateDirectivesAst());
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
        public PTBlockStmt Body { get; set; }

        public PTWhileStmt(TokenLocation beg, PTExpr cond, PTBlockStmt body) : base(beg, body.End)
        {
            this.Condition = cond;
            this.Body = body;
        }

        public override AstStatement CreateAst()
        {
            return new AstWhileStmt(this, Condition.CreateAst(), Body.CreateAst() as AstBlockStmt);
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

    public class PTUsingStatement : PTStatement
    {
        public PTExpr Value { get; }

        public PTUsingStatement(TokenLocation beg, PTExpr val) : base(beg, val.End)
        {
            Value = val;
        }

        public override AstStatement CreateAst()
        {
            return new AstUsingStmt(this, Value.CreateAst());
        }
    }

    public class PTMatchCase
    {
        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }
        public PTExpr Value { get; set; }
        public PTStatement Body { get; set; }

        public PTMatchCase(TokenLocation beg, TokenLocation end, PTExpr value, PTStatement body)
        {
            this.Beginning = beg;
            this.End = end;
            this.Value = value;
            this.Body = body;
        }

        public AstMatchCase CreateAst()
        {
            return new AstMatchCase(this, Value.CreateAst(), Body.CreateAst());
        }
    }

    public class PTMatchStmt : PTStatement
    {
        public PTExpr Value { get; set; }
        public List<PTMatchCase> Cases { get; set; }

        public PTMatchStmt(TokenLocation beg, TokenLocation end, PTExpr value, List<PTMatchCase> cases) : base(beg, end)
        {
            this.Value = value;
            this.Cases = cases;
        }

        public override AstStatement CreateAst()
        {
            return new AstMatchStmt(this, Value.CreateAst(), Cases.Select(c => c.CreateAst()).ToList());
        }
    }

    public class PTBreakStmt : PTStatement
    {
        public PTBreakStmt(TokenLocation loc) : base(loc, loc)
        {
        }

        public override AstStatement CreateAst()
        {
            return new AstBreakStmt(this);
        }
    }

    public class PTContinueStmt : PTStatement
    {
        public PTContinueStmt(TokenLocation loc) : base(loc, loc)
        {
        }

        public override AstStatement CreateAst()
        {
            return new AstContinueStmt(this);
        }
    }
}
