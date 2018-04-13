using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;

namespace Cheez.Ast
{
    public class ImplBlock : Statement
    {
        public string Target { get; set; }
        public string Trait { get; set; }

        public List<FunctionDeclaration> Functions { get; }

        public ImplBlock(LocationInfo beg, LocationInfo end, string target, List<FunctionDeclaration> functions) : base(beg, end)
        {
            this.Target = target;
            this.Functions = functions;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitImplBlock(this, data);
        }

        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitImplBlock(this, data);
        }
    }
}
