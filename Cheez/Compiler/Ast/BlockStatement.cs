using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstBlockStmt : AstStatement
    {
        public ParseTree.PTBlockStmt ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public List<AstStatement> Statements { get; }
        public Scope SubScope { get; set; }

        public AstBlockStmt(ParseTree.PTBlockStmt node, List<AstStatement> statements) : base()
        {
            ParseTreeNode = node;
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBlockStatement(this, data);
        }
    }
}
