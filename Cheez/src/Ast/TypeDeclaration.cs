using System.Collections.Generic;
using System.Diagnostics;
using Cheez.Parsing;
using Cheez.Visitor;

namespace Cheez.Ast
{
    public class MemberDeclaration
    {
        public string Name { get; }
        public TypeExpression Type { get; }
        public CType CType { get; set; }

        public MemberDeclaration(string name, TypeExpression type)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    public class TypeDeclaration : Statement
    {
        public string Name { get; }
        public List<MemberDeclaration> Members { get; }

        public TypeDeclaration(TokenLocation beg, TokenLocation end, string name, List<MemberDeclaration> members) : base(beg, end)
        {
            this.Name = name;
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default)
        {
            visitor.VisitTypeDeclaration(this, data);
        }
    }
}
