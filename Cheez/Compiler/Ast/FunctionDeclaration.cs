using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    public class AstFunctionParameter : IVariableDecl
    {
        public ParseTree.PTFunctionParam ParseTreeNode { get; }

        public string Name => ParseTreeNode.Name.Name;
        public CheezType VarType { get; set; }
        public Scope Scope { get; set; }

        public AstFunctionParameter(ParseTree.PTFunctionParam node)
        {
            ParseTreeNode = node;
        }

        public override string ToString()
        {
            return $"param {Name} : {VarType}";
        }
    }

    public class AstFunctionDecl : AstStatement, INamed
    {
        public ParseTree.PTFunctionDecl ParseTreeNode { get; set; }

        public Scope SubScope { get; set; }

        public string Name { get; }
        public List<AstFunctionParameter> Parameters { get; }
        public CheezType ReturnType { get; set; }

        public List<AstStatement> Statements { get; private set; }
        public bool HasImplementation => Statements != null;

        public AstFunctionDecl(ParseTree.PTFunctionDecl node, string name, List<AstFunctionParameter> parameters, List<AstStatement> statements = null)
            : base()
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Parameters = parameters;
            this.Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitFunctionDeclaration(this, data);
        }

        public override string ToString()
        {
            if (ReturnType != null)
                return $"fn {Name}() : {ReturnType}";
            return $"fn {Name}()";
        }
    }
}
