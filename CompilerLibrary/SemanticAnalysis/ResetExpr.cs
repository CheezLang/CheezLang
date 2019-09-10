using Cheez.Ast.Expressions;
using System;

namespace Cheez
{
    public partial class Workspace
    {
        private void ResetExpr(AstExpression expr, bool hard = false)
        {
            expr.TypeInferred = false;
            expr.Type = null;
            expr.Value = null;

            if (hard)
            {
                expr.Parent = null;
                expr.Scope = null;
            }

            switch (expr)
            {
                case AstBinaryExpr e:
                    ResetExpr(e.Left, hard);
                    ResetExpr(e.Right, hard);
                    break;

                case AstCallExpr e:
                    ResetExpr(e.FunctionExpr, hard);
                    foreach (var arg in e.Arguments)
                        ResetExpr(arg, hard);
                    break;

                default: throw new NotImplementedException();
            }
        }
    }
}
