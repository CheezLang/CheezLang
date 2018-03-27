using Cheez.Parsing;
using Cheez.Visitor;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class NumberExpression : Expression
    {
        private NumberData mData;
        public NumberData Data => mData;

        public NumberExpression(LocationInfo loc, NumberData data) : base(loc)
        {
            mData = data;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitNumberExpression(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitNumberExpression(this, data);
        }
    }
}
