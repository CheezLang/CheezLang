using System;
using System.Diagnostics;
using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class VariableDeclaration : Statement
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public Type Type { get; set; }
        public Expression Initializer { get; set; }

        public VariableDeclaration(LocationInfo loc, string name, string typeName = null, Expression init = null) : base(loc)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.Initializer = init;
        }

        [DebuggerStepThrough]
        public override T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitVariableDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Visit<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitVariableDeclaration(this, data);
        }
    }
}
