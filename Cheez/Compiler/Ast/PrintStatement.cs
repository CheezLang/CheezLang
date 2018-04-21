using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstPrintStmt : AstStatement
    {
        public ParseTree.PTPrintStmt ParseTreeNode { get; }

        public List<AstExpression> Expressions { get; }
        public AstExpression Seperator { get; }
        public bool NewLine { get; set; }

        public AstPrintStmt(ParseTree.PTPrintStmt node, List<AstExpression> expr, AstExpression seperator = null, bool NewLine = true) : base()
        {
            ParseTreeNode = node;
            this.NewLine = NewLine;
            this.Expressions = expr;
            this.Seperator = seperator;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitPrintStatement(this, data);
        }
    }
}
