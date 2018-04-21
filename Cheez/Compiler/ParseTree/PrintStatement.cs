using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public class PTPrintStmt : PTStatement
    {
        public List<PTExpr> Expressions { get; }
        public PTExpr Seperator { get; }
        public bool NewLine { get; set; }

        public PTPrintStmt(TokenLocation beg, TokenLocation end, List<PTExpr> expr, PTExpr seperator = null, bool NewLine = true) : base(beg, end)
        {
            this.NewLine = NewLine;
            this.Expressions = expr;
            this.Seperator = seperator;
        }

        public override AstStatement CreateAst()
        {
            var expr = Expressions.Select(e => e.CreateAst()).ToList();
            return new AstPrintStmt(this, expr, Seperator?.CreateAst(), NewLine);
        }
    }
}
