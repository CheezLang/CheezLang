using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using System;

namespace Cheez
{
    public partial class Workspace
    {
        private bool Move(AstExpression expr, ILocation location = null)
        {
            switch (expr)
            {
                case AstDotExpr dot:
                    {
                        if (!dot.Type.IsCopy)
                        {
                            ReportError(location ?? expr, $"Can't move out of '{dot}' because type {dot.Type} is not copy");
                            return false;
                        }
                        return true;
                    }

                case AstIdExpr id:
                    {
                        if (expr.Scope.TryGetSymbolStatus(id.Symbol, out var status))
                        {
                            if (status.kind != SymbolStatus.Kind.initialized)
                            {
                                ReportError(location ?? expr, $"Can't move out of '{id.Name}' because it is {status.kind}",
                                    ("Moved here:", status.location));
                                return false;
                            }
                            else
                            {
                                // check if type is move or copy
                                var type = (id.Symbol as ITypedSymbol).Type;
                                if (!type.IsCopy)
                                {
                                    expr.Scope.SetSymbolStatus(id.Symbol, SymbolStatus.Kind.moved, expr);
                                }
                            }
                        }
                        else if (id.Symbol is Using use)
                        {
                            return Move(use.Expr, id.Location);
                        }
                        return true;
                    }

                //case AstStructValueExpr _:
                //    // TODO: move arguments
                //    break;

                default:
                    return true;
            }
        }
    }
}
