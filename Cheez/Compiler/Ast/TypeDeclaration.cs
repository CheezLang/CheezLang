using System.Collections.Generic;
using System.Diagnostics;
using Cheez.Compiler.Visitor;

namespace Cheez.Compiler.Ast
{
    public class AstMemberDecl
    {
        public ParseTree.PTMemberDecl ParseTreeNode { get; set; }

        public string Name => ParseTreeNode.Name.Name;
        public CheezType Type { get; set; }

        public AstMemberDecl(ParseTree.PTMemberDecl node)
        {
            ParseTreeNode = node;
        }
    }

    public class AstTypeDecl : AstStatement
    {
        public ParseTree.PTTypeDecl ParseTreeNode { get; set; }

        public string Name => ParseTreeNode.Name.Name;
        public List<AstMemberDecl> Members { get; }

        public AstTypeDecl(ParseTree.PTTypeDecl node, List<AstMemberDecl> members) : base()
        {
            ParseTreeNode = node;
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeDeclaration(this, data);
        }
    }
}
