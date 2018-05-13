using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.Ast
{
    #region Function Declaration

    public class AstFunctionParameter : ISymbol
    {
        public ParseTree.PTFunctionParam ParseTreeNode { get; }

        public string Name { get; }
        public CheezType Type { get; set; }
        public Scope Scope { get; set; }

        public AstFunctionParameter(ParseTree.PTFunctionParam node)
        {
            ParseTreeNode = node;
            Name = node.Name.Name;
        }

        public AstFunctionParameter(string name, CheezType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"param {Name} : {Type}";
        }
    }

    public class AstFunctionDecl : AstStatement, ISymbol
    {
        public ParseTree.PTFunctionDecl ParseTreeNode { get; set; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public Scope SubScope { get; set; }

        public string Name { get; }
        public List<AstFunctionParameter> Parameters { get; }
        public CheezType ReturnType { get; set; }

        public CheezType Type { get; set; }

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

    #endregion

    #region Type Declaration

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

    public class AstTypeDecl : AstStatement, INamed
    {
        public ParseTree.PTTypeDecl ParseTreeNode { get; set; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public string Name => ParseTreeNode.Name.Name;
        public List<AstMemberDecl> Members { get; }

        public AstTypeDecl(ParseTree.PTTypeDecl node, List<AstMemberDecl> members, Dictionary<string, AstDirective> dirs) : base(dirs)
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

    public class AstImplBlock : AstStatement
    {
        public ParseTree.PTImplBlock ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public CheezType TargetType;
        public string Trait { get; set; }

        public List<AstFunctionDecl> Functions { get; }

        public Scope SubScope { get; set; }

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

    #endregion

    #region Variable Declarion

    public class AstVariableDecl : AstStatement, ISymbol
    {
        public ParseTree.PTVariableDecl ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public string Name { get; set; }
        public CheezType Type { get; set; }
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

    #endregion
}
