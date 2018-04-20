using Cheez.Parsing;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Ast
{
    public class FunctionParameter : IVariableDeclaration
    {
        public int Id { get; }

        public TypeExpression Type { get; set; }
        public string Name { get; set; }

        public TokenLocation Beginning { get; set; }
        public TokenLocation End { get; set; }

        public ILocation NameLocation => throw new System.NotImplementedException();

        public FunctionParameter(TokenLocation beg, TokenLocation end, string name, TypeExpression type)
        {
            Beginning = beg;
            End = end;
            Id = Util.NewId;
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return $"param {Name} : {Type}";
        }
    }

    public class FunctionDeclarationAst : Statement, INamed
    {
        public IdentifierExpression NameExpr { get; }
        public string Name => NameExpr.Name;
        public List<FunctionParameter> Parameters { get; }

        public TypeExpression ReturnType { get; }

        public List<Statement> Statements { get; private set; }

        public bool HasImplementation => Statements != null;

        public FunctionDeclarationAst(TokenLocation beg, TokenLocation end, IdentifierExpression name, List<FunctionParameter> parameters, TypeExpression returnType, List<Statement> statements = null)
            : base(beg, end)
        {
            this.NameExpr = name;
            this.Parameters = parameters;
            this.Statements = statements;
            this.ReturnType = returnType;
        }

        [DebuggerStepThrough]
        public override T Accept<T, D>(IVisitor<T, D> visitor, D data = default(D))
        {
            return visitor.VisitFunctionDeclaration(this, data);
        }

        [DebuggerStepThrough]
        public override void Accept<D>(IVoidVisitor<D> visitor, D data = default(D))
        {
            visitor.VisitFunctionDeclaration(this, data);
        }

        public override string ToString()
        {
            if (ReturnType != null)
                return $"fn {NameExpr}() : {ReturnType}";
            return $"fn {NameExpr}()";
        }
    }
}
