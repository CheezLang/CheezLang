using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;

namespace Cheez
{
    public partial class Workspace
    {
        private ISymbol GetSymbolOfExpr(AstExpression expr, Scope scope)
        {
            switch (expr)
            {
                case AstIdExpr id: return scope.GetSymbol(id.Name);
                case AstDotExpr dot:
                    {
                        var sym = GetSymbolOfExpr(dot.Left, scope);
                        if (sym == null)
                            return null;

                        switch (sym)
                        {
                            case ModuleSymbol mod:
                                return mod.Scope.GetSymbol(dot.Right.Name);

                            default:
                                return null;
                        }
                    }

                default:
                    return null;
            }
        }

        private void CollectTypeDependencies(AstDecl decl, AstExpression typeExpr)
        {
            switch (typeExpr)
            {
                case AstFuncExpr fun:
                    {
                        foreach (var p in fun.Parameters)
                            CollectTypeDependencies(decl, p.TypeExpr);
                        if (fun.ReturnTypeExpr != null)
                            CollectTypeDependencies(decl, fun.ReturnTypeExpr.TypeExpr);
                        break;
                    }

                case AstTraitTypeExpr str:
                    {
                        if (str.Parameters != null)
                        {
                            foreach (var p in str.Parameters)
                                CollectTypeDependencies(decl, p.TypeExpr);
                        }

                        foreach (var m in str.Declarations)
                        {
                            switch (m)
                            {
                                //case AstVariableDecl v:
                                //    if (v.TypeExpr != null)
                                //        CollectTypeDependencies(decl, v.TypeExpr, DependencyKind.Value); // or type?
                                //    if (v.Initializer != null)
                                //        CollectTypeDependencies(decl, v.Initializer, DependencyKind.Type); // or type?
                                //    break;

                                case AstConstantDeclaration v:
                                    if (v.TypeExpr != null)
                                        CollectTypeDependencies(decl, v.TypeExpr);
                                    CollectTypeDependencies(decl, v.Initializer);
                                    break;
                            }
                        }

                        decl.Dependencies.Remove(decl);
                        break;
                    }

                case AstStructTypeExpr str:
                    {
                        if (str.TryGetDirective("extend", out var dir))
                            foreach (var arg in dir.Arguments)
                                CollectTypeDependencies(decl, arg);

                        if (str.TraitExpr != null)
                            CollectTypeDependencies(decl, str.TraitExpr);

                        foreach (var p in str.Parameters)
                            CollectTypeDependencies(decl, p.TypeExpr);

                        foreach (var m in str.Declarations)
                        {
                            switch (m)
                            {
                                //case AstVariableDecl v:
                                //    if (v.TypeExpr != null)
                                //        CollectTypeDependencies(decl, v.TypeExpr, DependencyKind.Value); // or type?
                                //    if (v.Initializer != null)
                                //        CollectTypeDependencies(decl, v.Initializer, DependencyKind.Type); // or type?
                                //    break;

                                case AstConstantDeclaration v:
                                    if (v.TypeExpr != null)
                                        CollectTypeDependencies(decl, v.TypeExpr);
                                    CollectTypeDependencies(decl, v.Initializer);
                                    break;
                            }
                        }

                        break;
                    }

                case AstIdExpr id:
                    switch (decl.Scope.GetSymbol(id.Name))
                    {
                        case AstDecl d: decl.Dependencies.Add(d); break;
                    }

                    //if (sym is AstDecl d)
                    //{
                    //    if (d is AstVariableDecl sv)
                    //        d = sv;
                    //    decl.Dependencies.Add(d);
                    //}
                    break;

                case AstDotExpr dot:
                    {
                        switch (GetSymbolOfExpr(dot, decl.Scope))
                        {
                            case AstDecl d: decl.Dependencies.Add(d); break;
                        }
                        break;
                    }

                case AstVariableRef vr:
                    decl.Dependencies.Add(vr.Declaration);
                    break;

                case AstConstantRef vr:
                    decl.Dependencies.Add(vr.Declaration);
                    break;

                case AstAddressOfExpr add:
                    CollectTypeDependencies(decl, add.SubExpression);
                    break;

                case AstSliceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstArrayTypeExpr expr:
                    CollectTypeDependencies(decl, expr.SizeExpr);
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstReferenceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstFunctionTypeExpr expr:
                    if (expr.ReturnType != null)
                        CollectTypeDependencies(decl, expr.ReturnType);
                    foreach (var p in expr.ParameterTypes)
                        CollectTypeDependencies(decl, p);
                    break;

                case AstTupleExpr expr:
                    foreach (var p in expr.Types)
                    {
                        if (p.TypeExpr != null)
                            CollectTypeDependencies(decl, p.TypeExpr);
                        if (p.Name != null)
                            CollectTypeDependencies(decl, p.Name);
                        if (p.DefaultValue != null)
                            CollectTypeDependencies(decl, p.DefaultValue);
                    }
                    break;

                case AstCallExpr expr:
                    CollectTypeDependencies(decl, expr.FunctionExpr);
                    foreach (var p in expr.Arguments)
                        CollectTypeDependencies(decl, p.Expr);
                    break;

                case AstArrayAccessExpr expr:
                    {
                        CollectTypeDependencies(decl, expr.SubExpression);

                        foreach (var p in expr.Arguments)
                            CollectTypeDependencies(decl, p);
                        break;
                    }

                case AstArrayExpr arr:
                    {
                        foreach (var val in arr.Values)
                            CollectTypeDependencies(decl, val);
                        break;
                    }

                case AstBinaryExpr b:
                    CollectTypeDependencies(decl, b.Left);
                    CollectTypeDependencies(decl, b.Right);
                    break;

                case AstUnaryExpr u:
                    CollectTypeDependencies(decl, u.SubExpr);
                    break;
            }
        }
    }
}
