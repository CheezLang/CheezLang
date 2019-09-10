using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using System;

namespace Cheez
{
    public partial class Workspace
    {
        private bool Move(AstExpression expr)
        {
            switch (expr)
            {
                case AstIdExpr id:
                    {
                        if (expr.Scope.TryGetSymbolStatus(id.Symbol, out var status))
                        {
                            if (status.kind != SymbolStatus.Kind.initialized)
                            {
                                ReportError(expr, $"Can't move out of '{id.Name}' because it is {status.kind}",
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
