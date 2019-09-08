using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using System;

namespace Cheez
{
    public partial class Workspace
    {
        private void Move(AstExpression expr)
        {
            switch (expr)
            {
                case AstIdExpr id:
                    {
                        switch (id.Symbol)
                        {
                            case AstSingleVariableDecl var when !var.GetFlag(StmtFlags.GlobalScope):
                            case AstParameter p when p.IsReturnParam:
                                {
                                    var status = expr.Scope.GetSymbolStatus(id.Symbol);
                                    if (status.kind != SymbolStatus.Kind.initialized)
                                    {
                                        ReportError(expr, $"Can't move out of '{id.Name}' because it is {status.kind}",
                                            ("Moved here:", status.location));
                                    }
                                    else
                                    {
                                        // check if type is move or copy
                                        var type = (id.Symbol as ITypedSymbol).Type;
                                        if (!type.IsCopy)
                                        {
                                            expr.Scope.SetSymbolStatus(id.Symbol, SymbolStatus.Kind.moved, expr.Location);
                                        }
                                    }
                                    break;
                                }
                        }
                        break;
                    }

                //case AstStructValueExpr _:
                //    // TODO: move arguments
                //    break;

                default:
                    break;
            }
        }
    }
}
