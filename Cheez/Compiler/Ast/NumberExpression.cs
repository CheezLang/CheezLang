using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstNumberExpr : AstExpression
    {
        public ParseTree.PTNumberExpr ParseTreeNode { get; }
        public override ParseTree.PTExpr GenericParseTreeNode => ParseTreeNode;

        private NumberData mData;
        public NumberData Data => mData;

        public AstNumberExpr(ParseTree.PTNumberExpr node, NumberData data) : base()
        {
            ParseTreeNode = node;
            mData = data;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitNumberExpression(this, data);
        }
    }
}
