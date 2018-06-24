using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.ParseTree
{
    public class PTParameter : ILocation
    {
        public PTExpr Type { get; set; }
        public PTIdentifierExpr Name { get; set; }

        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public PTParameter(TokenLocation beg, PTIdentifierExpr name, PTExpr type)
        {
            Beginning = beg;
            End = type.End;
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return $"{Name}: {Type}";
        }

        public AstParameter CreateAst()
        {
            return new AstParameter(this, Name?.CreateAst() as AstIdentifierExpr, Type.CreateAst());
        }
    }

    #region Variable Declaration

    public class PTVariableDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; set; }
        public PTExpr Type { get; set; }
        public PTExpr Initializer { get; set; }

        public PTVariableDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, PTExpr type = null, PTExpr init = null, List<PTDirective> directives = null) : base(beg, end, directives)
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
            return new AstVariableDecl(this, Name.CreateAst() as AstIdentifierExpr, Type?.CreateAst(), Initializer?.CreateAst(), dirs);
        }
    }

    #endregion

    #region Function Declaration

    public class PTFunctionParam : ILocation
    {
        public PTExpr Type { get; set; }
        public PTIdentifierExpr Name { get; set; }

        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public ILocation NameLocation => throw new System.NotImplementedException();

        public PTFunctionParam(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, PTExpr type)
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

        public PTExpr ReturnType { get; }

        public PTBlockStmt Body { get; private set; }

        public bool RefSelf { get; set; }

        public PTFunctionDecl(TokenLocation beg,
            TokenLocation end,
            PTIdentifierExpr name,
            List<PTIdentifierExpr> generics,
            List<PTFunctionParam> parameters,
            PTExpr returnType,
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
            var p = Parameters.Select(x => new AstFunctionParameter(x, x.Name.CreateAst() as AstIdentifierExpr, x.Type.CreateAst())).ToList();
            var g = Generics.Select(x => x.CreateAst() as AstIdentifierExpr).ToList();
            var b = Body?.CreateAst() as AstBlockStmt;
            var dirs = CreateDirectivesAst();
            return new AstFunctionDecl(this, Name.CreateAst() as AstIdentifierExpr, g, p, ReturnType?.CreateAst(), b, dirs, RefSelf);
        }
    }

    #endregion

    #region Struct Declaration

    public class PTMemberDecl
    {
        public PTIdentifierExpr Name { get; }
        public PTExpr Type { get; }

        public PTMemberDecl(PTIdentifierExpr name, PTExpr type)
        {
            this.Name = name;
            this.Type = type;
        }

        public AstMemberDecl CreateAst()
        {
            return new AstMemberDecl(this, Name.CreateAst() as AstIdentifierExpr, Type.CreateAst());
        }
    }

    public class PTStructDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; }
        public List<PTMemberDecl> Members { get; }
        public List<PTParameter> Paramenters { get; }

        public PTStructDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTParameter> parameters, List<PTMemberDecl> members, List<PTDirective> directives) : base(beg, end)
        {
            this.Name = name;
            this.Paramenters = parameters;
            this.Members = members;
            this.Directives = directives;
        }

        public override AstStatement CreateAst()
        {
            var p = Paramenters?.Select(pa => pa.CreateAst()).ToList();
            var mems = Members.Select(m => m.CreateAst()).ToList();
            var dirs = CreateDirectivesAst();
            return new AstStructDecl(this, Name.CreateAst() as AstIdentifierExpr, p, mems, dirs);
        }
    }

    public class PTTraitDeclaration : PTStatement
    {
        public PTIdentifierExpr Name { get; set; }
        public List<PTParameter> Paramenters { get; }

        public List<PTFunctionDecl> Functions { get; }

        public PTTraitDeclaration(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTParameter> parameters, List<PTFunctionDecl> functions) : base(beg, end)
        {
            this.Name = name;
            this.Paramenters = parameters;
            this.Functions = functions;
        }

        public override AstStatement CreateAst()
        {
            var p = Paramenters?.Select(pa => pa.CreateAst()).ToList();
            var funcs = Functions.Select(f => (AstFunctionDecl)f.CreateAst()).ToList();
            return new AstTraitDeclaration(this, Name.CreateAst() as AstIdentifierExpr, p, funcs);
        }
    }

    public class PTImplBlock : PTStatement
    {
        public PTExpr Target { get; set; }
        public PTExpr Trait { get; set; }

        public List<PTFunctionDecl> Functions { get; }

        public PTImplBlock(TokenLocation beg, TokenLocation end, PTExpr target, PTExpr trait, List<PTFunctionDecl> functions) : base(beg, end)
        {
            this.Target = target;
            this.Functions = functions;
            this.Trait = trait;
        }

        public override AstStatement CreateAst()
        {
            var funcs = Functions.Select(f => (AstFunctionDecl)f.CreateAst()).ToList();
            return new AstImplBlock(this, Target.CreateAst(), Trait?.CreateAst(), funcs);
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
            return new AstEnumDecl(this, Name.CreateAst() as AstIdentifierExpr, mems, dirs);
        }
    }

    #endregion

    #region Type Alias

    public class PTTypeAliasDecl : PTStatement
    {
        public PTIdentifierExpr Name { get; set; }
        public PTExpr Type { get; set; }

        public PTTypeAliasDecl(TokenLocation beg, PTIdentifierExpr name, PTExpr type, List<PTDirective> directives = null) : base(beg, type.End, directives)
        {
            this.Name = name;
            this.Type = type;
        }

        public override AstStatement CreateAst()
        {
            var dirs = CreateDirectivesAst();
            return new AstTypeAliasDecl(this, Name.CreateAst() as AstIdentifierExpr, Type.CreateAst(), dirs);
        }
    }

    #endregion
}
