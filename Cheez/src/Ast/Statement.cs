using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public abstract class Statement
    {
        public LocationInfo Beginning { get; set; }

        public Statement(LocationInfo loc)
        {
            Beginning = loc;
        }

        public abstract T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D));
        public abstract void Visit<D>(IVoidVisitor<D> visitor, D data = default(D));
    }
}
