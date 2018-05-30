using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        public AstFunctionParameter Clone()
        {
            return new AstFunctionParameter(ParseTreeNode)
            {
                Scope = this.Scope,
                Type = this.Type
            };
        }
    }

    public interface ITempVariable
    {
        string Name { get; }
        CheezType Type { get; }
    }

    public class AstFunctionDecl : AstStatement, ISymbol
    {
        public ParseTree.PTFunctionDecl ParseTreeNode { get; set; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public Scope SubScope { get; set; }

        public string Name { get; set; }
        public List<AstFunctionParameter> Parameters { get; }
        public CheezType ReturnType { get; set; }

        public List<AstIdentifierExpr> Generics { get; }

        public CheezType Type { get; set; }

        public AstBlockStmt Body { get; private set; }
        //public List<AstStatement> Statements { get; private set; }
        //public bool HasImplementation => Statements != null;

        public List<ITempVariable> LocalVariables { get; } = new List<ITempVariable>();

        public bool RefSelf { get; }
        public bool IsGeneric { get; set; }

        public AstFunctionDecl(ParseTree.PTFunctionDecl node, 
            string name, 
            List<AstIdentifierExpr> generics,
            List<AstFunctionParameter> parameters, 
            AstBlockStmt body = null, 
            Dictionary<string, AstDirective> directives = null, 
            bool refSelf = false)
            : base(directives)
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Generics = generics;
            this.Parameters = parameters;
            this.Body = body;
            this.RefSelf = refSelf;

            if (Generics.Count > 0)
                IsGeneric = true;
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

        public override AstStatement Clone()
        {
            return new AstFunctionDecl(ParseTreeNode,
                Name,
                Generics.Select(g => g.Clone() as AstIdentifierExpr).ToList(),
                Parameters.Select(p => p.Clone()).ToList(),
                Body?.Clone() as AstBlockStmt,
                Directives) // @Tode: clone this too?
            {
                Scope = this.Scope,
                SubScope = this.SubScope?.Clone(),
                Directives = this.Directives,
                mFlags = this.mFlags
            };
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

        public AstMemberDecl Clone()
        {
            return new AstMemberDecl(ParseTreeNode)
            {
                Type = this.Type
            };
        }
    }

    public class AstTypeDecl : AstStatement, INamed
    {
        public ParseTree.PTTypeDecl ParseTreeNode { get; set; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public string Name => ParseTreeNode.Name.Name;
        public List<AstMemberDecl> Members { get; }

        public CheezType Type { get; set; }

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

        public override AstStatement Clone()
        {
            return new AstTypeDecl(ParseTreeNode,
                Members.Select(m => m.Clone()).ToList(),
                Directives) // @Tode: clone this?
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
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

        public override AstStatement Clone()
        {
            return new AstImplBlock(ParseTreeNode, Functions.Select(f => f.Clone() as AstFunctionDecl).ToList())
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    #endregion

    #region Variable Declarion

    public class AstVariableDecl : AstStatement, ISymbol, ITempVariable
    {
        public ParseTree.PTVariableDecl ParseTreeNode { get; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public string Name { get; set; }
        public CheezType Type { get; set; }
        public AstExpression Initializer { get; set; }
        public Scope SubScope { get; set; }

        public AstVariableDecl(ParseTree.PTVariableDecl node, string name, AstExpression init, Dictionary<string, AstDirective> directives = null) : base(directives)
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

        public override AstStatement Clone()
        {
            return new AstVariableDecl(ParseTreeNode,
                Name,
                Initializer?.Clone(),
                Directives) // @Tode: clone this?
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    #endregion

    #region Enum

    public class AstEnumMember
    {
        public ParseTree.PTEnumMember ParseTreeNode { get; set; }

        public string Name { get; }
        public AstExpression Value { get; }
        public CheezType Type { get; set; }

        public AstEnumMember(ParseTree.PTEnumMember node, string name, AstExpression value)
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Value = value;
        }

        public AstEnumMember Clone()
        {
            return new AstEnumMember(ParseTreeNode, Name, Value?.Clone())
            {
                Type = this.Type
            };
        }
    }

    public class AstEnumDecl : AstStatement, INamed
    {
        public ParseTree.PTEnumDecl ParseTreeNode { get; set; }
        public override ParseTree.PTStatement GenericParseTreeNode => ParseTreeNode;

        public string Name { get; }
        public List<AstEnumMember> Members { get; }

        public AstEnumDecl(ParseTree.PTEnumDecl node, string name, List<AstEnumMember> members, Dictionary<string, AstDirective> dirs) : base(dirs)
        {
            ParseTreeNode = node;
            this.Name = name;
            this.Members = members;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitEnumDeclaration(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstEnumDecl(ParseTreeNode, 
                Name,
                Members.Select(m => m.Clone()).ToList(),
                Directives) // @Tode: clone this?
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    #endregion

    #region Type Alias

    public class AstTypeAliasDecl : AstStatement
    {
        public ParseTree.PTTypeAliasDecl ParseTreeNode => GenericParseTreeNode as ParseTree.PTTypeAliasDecl;
        public override ParseTree.PTStatement GenericParseTreeNode { get; }

        public string Name { get; set; }
        public CheezType Type { get; set; }

        public AstTypeAliasDecl(ParseTree.PTStatement node, string name, Dictionary<string, AstDirective> dirs = null) : base(dirs)
        {
            this.GenericParseTreeNode = node;
            this.Name = name;
        }

        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default)
        {
            return visitor.VisitTypeAlias(this, data);
        }

        public override AstStatement Clone()
        {
            return new AstTypeAliasDecl(ParseTreeNode, 
                Name,
                Directives) // @Tode: clone this?
            {
                Scope = this.Scope,
                Directives = this.Directives,
                mFlags = this.mFlags
            };
        }
    }

    #endregion
}
