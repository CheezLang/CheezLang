using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public abstract class Statement
    {
        public LocationInfo Beginning { get; set; }

        public Statement(LocationInfo loc)
        {
            Beginning = loc;
        }

        [DebuggerStepThrough]
        public abstract T Visit<T, D>(IVisitor<T, D> visitor, D data = default(D));

        [DebuggerStepThrough]
        public abstract void Visit<D>(IVoidVisitor<D> visitor, D data = default(D));
    }
}
