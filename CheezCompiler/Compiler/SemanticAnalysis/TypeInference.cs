using System;
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

        private void InferTypes(AstExpression expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies = null, HashSet<AstSingleVariableDecl> allDependencies = null)
        {
            switch (expr)
            {
                case AstTypeExpr t:
                    throw new NotImplementedException();

                case AstNumberExpr n:
                    InferTypesNumberExpr(n, expected);
                    break;

                case AstStringLiteral s:
                    InferTypesStringLiteral(s, expected);
                    break;

                case AstCharLiteral ch:
                    InferTypesCharLiteral(ch, expected);
                    break;

                case AstIdExpr i:
                    InferTypesIdExpr(i, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstBinaryExpr b:
                    InferTypesBinaryExpr(b, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstStructValueExpr s:
                    InferTypeStructValueExpr(s, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstUnaryExpr u:
                    InferTypeUnaryExpr(u, expected, unresolvedDependencies, allDependencies);
                    break;

                case AstCallExpr c:
                    InferTypeCallExpr(c, expected, unresolvedDependencies, allDependencies);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void InferTypeCallExpr(AstCallExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
        }

        private void InferTypeUnaryExpr(AstUnaryExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
        }

        private void InferTypeStructValueExpr(AstStructValueExpr expr, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            if (expr.TypeExpr != null)
            {
                expr.TypeExpr.Scope = expr.Scope;
                expr.TypeExpr.Type = ResolveType(expr.TypeExpr);
                expr.Type = expr.TypeExpr.Type;
            }
            else
            {
                expr.Type = expected;
            }

            if (expr.Type == null)
            {
                ReportError(expr, $"Failed to infer type for expression");
                expr.Type = CheezType.Error;
                return;
            }
            else if (expr.Type == CheezType.Error)
            {
                return;
            }


        }

        private void InferTypesBinaryExpr(AstBinaryExpr b, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
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

        private void InferTypesIdExpr(AstIdExpr i, CheezType expected, HashSet<AstSingleVariableDecl> unresolvedDependencies, HashSet<AstSingleVariableDecl> allDependencies)
        {
            var sym = i.Scope.GetSymbol(i.Name);
            if (sym == null)
            {
                ReportError(i, $"Unknown symbol '{i.Name}'");
                i.Type = CheezType.Error;
                return;
            }

            i.Symbol = sym;

            if (sym is AstSingleVariableDecl var)
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

        private void InferTypesCharLiteral(AstCharLiteral s, CheezType expected)
        {
            s.Type = CheezType.Char;
            s.CharValue = s.RawValue[0];
            s.Value = s.CharValue;
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
