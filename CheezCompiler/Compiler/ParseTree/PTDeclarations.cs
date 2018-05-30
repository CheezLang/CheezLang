using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    #region Variable Declaration

    public class PTVariableDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; set; }
        public PTTypeExpr Type { get; set; }
        public PTExpr Initializer { get; set; }

        public PTVariableDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, PTTypeExpr type = null, PTExpr init = null, List<PTDirective> directives = null) : base(beg, end, directives)
        {
            this.Name = name;
            this.Type = type;
            this.Initializer = init;
        }

        public override string ToString()
        {
            return $"var {Name}";
        }

        public override AstStatement CreateAst()
        {
            var dirs = CreateDirectivesAst();
            return new AstVariableDecl(this, Name.Name, Initializer?.CreateAst(), dirs);
        }
    }

    #endregion

    #region Function Declaration

    public class PTFunctionParam : ILocation
    {
        public PTTypeExpr Type { get; set; }
        public PTIdentifierExpr Name { get; set; }

        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public ILocation NameLocation => throw new System.NotImplementedException();

        public PTFunctionParam(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, PTTypeExpr type)
        {
            Beginning = beg;
            End = end;
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return $"param {Name} : {Type}";
        }
    }

    public class PTFunctionDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; }
        public List<PTFunctionParam> Parameters { get; }

        public List<PTIdentifierExpr> Generics { get; }

        public PTTypeExpr ReturnType { get; }

        public PTBlockStmt Body { get; private set; }

        public bool RefSelf { get; set; }

        public PTFunctionDecl(TokenLocation beg,
            TokenLocation end,
            PTIdentifierExpr name,
            List<PTIdentifierExpr> generics,
            List<PTFunctionParam> parameters,
            PTTypeExpr returnType,
            PTBlockStmt body = null,
            List<PTDirective> directives = null,
            bool refSelf = false)
            : base(beg, end, directives)
        {
            this.Name = name;
            this.Generics = generics;
            this.Parameters = parameters;
            this.Body = body;
            this.ReturnType = returnType;
            this.RefSelf = refSelf;
        }

        public override string ToString()
        {
            if (ReturnType != null)
                return $"fn {Name}() : {ReturnType}";
            return $"fn {Name}()";
        }

        public override AstStatement CreateAst()
        {
            var p = Parameters.Select(x => new AstFunctionParameter(x)).ToList();
            var g = Generics.Select(x => x.CreateAst() as AstIdentifierExpr).ToList();
            var b = Body?.CreateAst() as AstBlockStmt;
            var dirs = CreateDirectivesAst();
            return new AstFunctionDecl(this, Name.Name, g, p, b, dirs, RefSelf);
        }
    }

    #endregion

    #region Type Declaration

    public class PTMemberDecl
    {
        public PTIdentifierExpr Name { get; }
        public PTTypeExpr Type { get; }

        public PTMemberDecl(PTIdentifierExpr name, PTTypeExpr type)
        {
            this.Name = name;
            this.Type = type;
        }

        public AstMemberDecl CreateAst()
        {
            return new AstMemberDecl(this);
        }
    }

    public class PTTypeDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; }
        public List<PTMemberDecl> Members { get; }

        public PTTypeDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTMemberDecl> members, List<PTDirective> directives) : base(beg, end)
        {
            this.Name = name;
            this.Members = members;
            this.Directives = directives;
        }

        public override AstStatement CreateAst()
        {
            var mems = Members.Select(m => m.CreateAst()).ToList();
            var dirs = CreateDirectivesAst();
            return new AstTypeDecl(this, mems, dirs);
        }
    }

    public class PTImplBlock : PTStatement
    {
        public PTTypeExpr Target { get; set; }
        public PTIdentifierExpr Trait { get; set; }

        public List<PTFunctionDecl> Functions { get; }

        public PTImplBlock(TokenLocation beg, TokenLocation end, PTTypeExpr target, List<PTFunctionDecl> functions) : base(beg, end)
        {
            this.Target = target;
            this.Functions = functions;
        }

        public override AstStatement CreateAst()
        {
            var funcs = Functions.Select(f => (AstFunctionDecl)f.CreateAst()).ToList();
            return new AstImplBlock(this, funcs);
        }
    }

    #endregion

    #region Enum

    public class PTEnumMember
    {
        public PTIdentifierExpr Name { get; }
        public PTExpr Value { get; }

        public PTEnumMember(PTIdentifierExpr name, PTExpr value)
        {
            this.Name = name;
            this.Value = value;
        }

        public AstEnumMember CreateAst()
        {
            return new AstEnumMember(this, Name.Name, Value?.CreateAst());
        }
    }

    public class PTEnumDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; }
        public List<PTEnumMember> Members { get; }

        public PTEnumDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTEnumMember> members, List<PTDirective> directives) : base(beg, end)
        {
            this.Name = name;
            this.Members = members;
            this.Directives = directives;
        }

        public override AstStatement CreateAst()
        {
            var mems = Members.Select(m => m.CreateAst()).ToList();
            var dirs = Directives?.Select(d => d.CreateAst()).ToDictionary(d => d.Name);
            return new AstEnumDecl(this, Name.Name, mems, dirs);
        }
    }

    #endregion

    #region Type Alias

    public class PTTypeAliasDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; set; }
        public PTTypeExpr Type { get; set; }

        public PTTypeAliasDecl(TokenLocation beg, PTIdentifierExpr name, PTTypeExpr type, List<PTDirective> directives = null) : base(beg, type.End, directives)
        {
            this.Name = name;
            this.Type = type;
        }

        public override AstStatement CreateAst()
        {
            var dirs = CreateDirectivesAst();
            return new AstTypeAliasDecl(this, Name.Name, dirs);
        }
    }

    #endregion
}
