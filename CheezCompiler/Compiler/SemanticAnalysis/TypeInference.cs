using System.Collections.Generic;
using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        private void ConvertLiteralTypeToDefaultType(AstExpression expr)
        {
            if (expr.Type == IntType.LiteralType) expr.Type = IntType.DefaultType;
            else if (expr.Type == FloatType.LiteralType) expr.Type = FloatType.DefaultType;
            else if (expr.Type == CheezType.StringLiteral) expr.Type = CheezType.String;
        }

        private void InferTypes(AstExpression expr, CheezType expected, HashSet<AstVariableDecl> unresolvedDependencies = null, HashSet<AstVariableDecl> allDependencies = null)
        {
            switch (expr)
            {
                case AstNumberExpr n:
                    InferTypesNumberExpr(n, expected);
                    break;

                case AstStringLiteral s:
                    InferTypesStringLiteral(s, expected);
                    break;

                case AstIdExpr i:
                    InferTypesIdExpr(i, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstBinaryExpr b:
                    InferTypesBinaryExpr(b, expected, unresolvedDependencies, allDependencies);
                    break;
            }
        }

        private void InferTypesBinaryExpr(AstBinaryExpr b, CheezType expected, HashSet<AstVariableDecl> unresolvedDependencies, HashSet<AstVariableDecl> allDependencies)
        {
            b.Left.Scope = b.Scope;
            b.Right.Scope = b.Scope;

            InferTypes(b.Left, null, unresolvedDependencies, allDependencies);
            InferTypes(b.Right, null, unresolvedDependencies, allDependencies);

            var at = new List<AbstractType>();
            if (b.Left.Type is AbstractType at1) at.Add(at1);
            if (b.Right.Type is AbstractType at2) at.Add(at2);
            if (at.Count > 0)
            {
                b.Type = new CombiType(at);
            }
            else
            {
                // TODO: find matching operator

                // @hack
                b.Type = expected;
            }
        }

        private void InferTypesIdExpr(AstIdExpr i, CheezType expected, HashSet<AstVariableDecl> unresolvedDependencies, HashSet<AstVariableDecl> allDependencies)
        {
            var sym = i.Scope.GetSymbol(i.Name);
            if (sym == null)
            {
                ReportError(i, $"Unknown symbol '{i.Name}'");
                i.Type = CheezType.Error;
                return;
            }

            i.Symbol = sym;

            if (sym is AstVariableDecl var)
            {
                i.Type = var.Type;
                if (i.Type is AbstractType)
                    unresolvedDependencies.Add(var);
                allDependencies.Add(var);
            }
            else
            {
                ReportError(i, $"'{i.Name}' is not a valid variable");
            }
        }

        private void InferTypesStringLiteral(AstStringLiteral s, CheezType expected)
        {
            if (expected == CheezType.String || expected == CheezType.CString) s.Type = expected;
            else s.Type = CheezType.StringLiteral;
        }

        private void InferTypesNumberExpr(AstNumberExpr expr, CheezType expected)
        {
            if (expr.Data.Type == NumberData.NumberType.Int)
            {
                if (expected != null && (expected is IntType || expected is FloatType)) expr.Type = expected;
                else expr.Type = IntType.LiteralType;
                expr.Value = new CheezValue(expr.Type, expr.Data.ToLong());
            }
            else
            {
                if (expected != null && expected is FloatType) expr.Type = expected;
                else expr.Type = FloatType.LiteralType;
                expr.Value = new CheezValue(expr.Type, expr.Data.ToDouble());
            }
        }
    }
}
