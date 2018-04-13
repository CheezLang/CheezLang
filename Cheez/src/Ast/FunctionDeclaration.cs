using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class FunctionParameter
    {
        public int Id { get; }

        public TypeExpression Type { get; set; }
        public string Name { get; set; }

        public FunctionParameter(string name, TypeExpression type)
        {
            Id = Util.NewId;
            this.Name = name;
            this.Type = type;
        }
    }

    public class FunctionDeclaration : Statement
    {
        public string Name { get; }
        public List<FunctionParameter> Parameters { get; }

        public TypeExpression ReturnType { get; }

        public List<Statement> Statements { get; private set; }

        public FunctionDeclaration(LocationInfo beg, LocationInfo end, string name, List<FunctionParameter> parameters, TypeExpression returnType, List<Statement> statements)
            : base(beg, end)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Statements = statements;
            this.ReturnType = returnType;
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
