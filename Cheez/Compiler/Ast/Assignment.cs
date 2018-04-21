using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstAssignment : AstStatement
    {
        public ParseTree.PTAssignment ParseTreeNode { get; }

        public AstExpression Target { get; set; }
        public AstExpression Value { get; set; }

        public AstAssignment(ParseTree.PTAssignment node, AstExpression target, AstExpression value) : base()
        {
            ParseTreeNode = node;
            this.Target = target;
            this.Value = value;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitAssignment(this, data);
        }
    }
}
