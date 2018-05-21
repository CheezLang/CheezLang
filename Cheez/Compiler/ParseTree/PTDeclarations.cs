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

        public PTVariableDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, PTTypeExpr type = null, PTExpr init = null) : base(beg, end)
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
            return new AstVariableDecl(this, Name.Name, Initializer?.CreateAst());
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

        public PTTypeExpr ReturnType { get; }

        public List<PTStatement> Statements { get; private set; }

        public bool RefSelf { get; set; }
        
        public PTFunctionDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTFunctionParam> parameters, PTTypeExpr returnType, List<PTStatement> statements = null, bool refSelf = false)
            : base(beg, end)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Statements = statements;
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
            var s = Statements?.Select(x => x.CreateAst()).ToList();
            return new AstFunctionDecl(this, Name.Name, p, s, RefSelf);
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
        public List<PTDirective> Directives { get; }

        public PTTypeDecl(TokenLocation beg, TokenLocation end, PTIdentifierExpr name, List<PTMemberDecl> members, List<PTDirective> directives) : base(beg, end)
        {
            this.Name = name;
            this.Members = members;
            this.Directives = directives;
        }

        public override AstStatement CreateAst()
        {
            var mems = Members.Select(m => m.CreateAst()).ToList();
            var dirs = Directives.Select(d => d.CreateAst()).ToDictionary(d => d.Name);
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
}
