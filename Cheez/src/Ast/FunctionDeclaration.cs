using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;

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

        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitFunctionDeclaration(this, data);
        }

        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitFunctionDeclaration(this, data);
        }
    }
}
