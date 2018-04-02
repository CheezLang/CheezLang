using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class FunctionParameter
    {
        public int Id { get; }

        public string TypeName { get; set; }
        public string Name { get; set; }

        public FunctionParameter(string name, string type)
        {
            Id = Util.NewId;
            this.Name = name;
            this.TypeName = type;
        }
    }

    public class FunctionDeclaration : Statement
    {
        public string Name { get; }
        public List<FunctionParameter> Parameters { get; }

        public List<Statement> Statements { get; private set; }

        public FunctionDeclaration(LocationInfo beginning, string name, List<FunctionParameter> parameters, List<Statement> statements)
            : base(beginning)
        {
            this.Name = name;
            this.Parameters = parameters;
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
