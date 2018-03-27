using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class FunctionDeclaration : Statement
    {
        public string Name { get; private set; }

        public List<Statement> Statements { get; private set; }

        public FunctionDeclaration(LocationInfo beginning, string name, List<Statement> statements)
            : base(beginning)
        {
            this.Name = name;
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitFunctionDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitFunctionDeclaration(this, data);
        }
    }
}
