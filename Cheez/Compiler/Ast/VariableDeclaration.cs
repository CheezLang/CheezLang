using System.Diagnostics;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;

namespace Cheez.Compiler.Ast
{
    public interface IVariableDecl : INamed
    {
        CheezType VarType { get; }
    }

    public class AstVariableDecl : AstStatement, IVariableDecl
    {
        public ParseTree.PTVariableDecl ParseTreeNode { get; }

        public string Name { get; set; }
        public CheezType VarType { get; set; }
        public AstExpression Initializer { get; set; }
        public Scope SubScope { get; set; }

        public AstVariableDecl(ParseTree.PTVariableDecl node, string name, AstExpression init) : base()
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Initializer = init;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitVariableDeclaration(this, data);
        }

        public override string ToString()
        {
            return $"var {Name}";
        }
    }
}
