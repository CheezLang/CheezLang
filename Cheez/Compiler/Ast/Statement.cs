using Cheez.Compiler.Visitor;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public abstract class AstStatement : IVisitorAcceptor
    {
        public int Id { get; }

        public Scope Scope { get; set; }

        public AstStatement()
        {
            this.Id = Util.NewId;
        }

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        [DebuggerStepThrough]
        public abstract T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }
}
