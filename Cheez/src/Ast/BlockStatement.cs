using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }

        public BlockStatement(LocationInfo beg, LocationInfo end, List<Statement> statements) : base(beg, end)
        {
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitBlockStatement(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitBlockStatement(this, data);
        }
    }
}
