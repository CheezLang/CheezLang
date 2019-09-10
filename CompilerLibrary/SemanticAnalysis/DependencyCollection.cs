using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;

namespace Cheez
{
    public partial class Workspace
    {
        private void CollectTypeDependencies(AstDecl decl, AstExpression typeExpr, DependencyKind type)
        {
            switch (typeExpr)
            {
                case AstIdExpr id:
                    var sym = decl.Scope.GetSymbol(id.Name);
                    if (sym is AstDecl d)
                    {
                        if (d is AstSingleVariableDecl sv)
                            d = sv.VarDeclaration;
                        decl.Dependencies.Add((type, d));
                    }
                    break;

                case AstAddressOfExpr add:
                    CollectTypeDependencies(decl, add.SubExpression, type);
                    break;

                case AstSliceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target, DependencyKind.Type);
                    break;

                case AstArrayTypeExpr expr:
                    CollectTypeDependencies(decl, expr.SizeExpr, DependencyKind.Value);
                    CollectTypeDependencies(decl, expr.Target, DependencyKind.Type);
                    break;

                case AstReferenceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target, DependencyKind.Type);
                    break;

                case AstFunctionTypeExpr expr:
                    if (expr.ReturnType != null)
                        CollectTypeDependencies(decl, expr.ReturnType, DependencyKind.Type);
                    foreach (var p in expr.ParameterTypes)
                        CollectTypeDependencies(decl, p, DependencyKind.Type);
                    break;

                case AstTupleExpr expr:
                    foreach (var p in expr.Values)
                        CollectTypeDependencies(decl, p, type);
                    break;

                case AstCallExpr expr:
                    CollectTypeDependencies(decl, expr.FunctionExpr, type);
                    foreach (var p in expr.Arguments)
                        CollectTypeDependencies(decl, p.Expr, type);
                    break;
            }
        }
    }
}
