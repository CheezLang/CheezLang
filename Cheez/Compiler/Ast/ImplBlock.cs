using Cheez.Compiler.Visitor;
using System.Collections.Generic;

namespace Cheez.Compiler.Ast
{
    public class AstImplBlock : AstStatement
    {
        public ParseTree.PTImplBlock ParseTreeNode { get; }

        public string Target => ParseTreeNode.Target.Name;
        public string Trait { get; set; }

        public List<AstFunctionDecl> Functions { get; }

        public AstImplBlock(ParseTree.PTImplBlock node, List<AstFunctionDecl> functions) : base()
        {
            ParseTreeNode = node;
            this.Functions = functions;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitImplBlock(this, data);
        }
    }
}
