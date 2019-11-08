using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using System;
using System.Collections.Generic;

namespace Cheez
{
    /// <summary>
    /// This pass resolves function signatures
    /// </summary>
    public partial class Workspace
    {
        private void CheckForSelfParam(AstFuncExpr func)
        {
            if (func.Parameters.Count > 0)
            {
                var param = func.Parameters[0];
                if (param.TypeExpr is AstIdExpr i && i.Name == "Self")
                {
                    func.SelfType = SelfParamType.Value;
                }
                else if (param.TypeExpr is AstReferenceTypeExpr p2 && p2.Target is AstIdExpr i3 && i3.Name == "Self")
                {
                    func.SelfType = SelfParamType.Reference;
                }
            }
        }

        private void CheckForValidOperator(string op, AstFuncExpr func, AstDirective directive, Scope targetScope)
        {
            switch (op)
            {
                case "!":
                case "-" when func.Parameters.Count == 1:
                    if (func.ReturnType == CheezType.Void)
                        ReportError(func.ParameterLocation, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 1)
                        ReportError(func.ParameterLocation, $"This function must take one parameter in order to use it as operator '{op}'");
                    else
                        targetScope.DefineUnaryOperator(op, func);
                    break;

                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                case "==":
                case "!=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "and":
                case "or":
                    if (func.ReturnType == CheezType.Void)
                        ReportError(func.ParameterLocation, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 2)
                        ReportError(func.ParameterLocation, $"This function must take two parameters in order to use it as operator '{op}'");
                    else
                        targetScope.DefineBinaryOperator(op, func);
                    break;

                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "%=":
                    if (func.Parameters.Count != 2)
                    {
                        ReportError(func.ParameterLocation, $"This function must take two parameters in order to use it as operator '{op}'");
                    }
                    else
                    {
                        targetScope.DefineBinaryOperator(op, func);
                    }
                    break;

                case "[]":
                    if (func.ReturnType == CheezType.Void)
                        ReportError(func.ParameterLocation, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 2)
                        ReportError(func.ParameterLocation, $"This function must take two parameters in order to use it as operator '{op}'");
                    else
                        targetScope.DefineBinaryOperator(op, func);
                    break;


                case "set[]":
                    if (func.Parameters.Count != 3)
                        ReportError(func.ParameterLocation, $"This function must take three parameters in order to use it as operator '{op}'");
                    else
                        targetScope.DefineOperator(op, func);
                    break;

                default:
                    ReportError(directive, $"'{op}' is not an overridable operator");
                    break;
            }
        }

        private void CollectPolyTypeNames(AstExpression typeExpr, List<string> result)
        {
            switch (typeExpr)
            {
                case AstIdExpr i:
                    if (i.IsPolymorphic)
                        result.Add(i.Name);
                    break;

                case AstAddressOfExpr p:
                    {
                        CollectPolyTypeNames(p.SubExpression, result);
                        break;
                    }

                case AstSliceTypeExpr p:
                    {
                        CollectPolyTypeNames(p.Target, result);
                        break;
                    }

                case AstArrayTypeExpr p:
                    {
                        CollectPolyTypeNames(p.Target, result);
                        break;
                    }

                case AstTupleExpr te:
                    {
                        foreach (var m in te.Types)
                        {
                            CollectPolyTypeNames(m.TypeExpr, result);
                        }

                        break;
                    }

                case AstCallExpr ps:
                    {
                        foreach (var m in ps.Arguments)
                        {
                            CollectPolyTypeNames(m.Expr, result);
                        }

                        break;
                    }

                case AstArrayAccessExpr ps:
                    {
                        foreach (var m in ps.Arguments)
                        {
                            CollectPolyTypeNames(m, result);
                        }

                        break;
                    }

                case AstFunctionTypeExpr func:
                    {
                        foreach (var v in func.ParameterTypes)
                            CollectPolyTypeNames(v, result);
                        if (func.ReturnType != null)
                            CollectPolyTypeNames(func.ReturnType, result);
                        break;
                    }

                case AstReferenceTypeExpr r:
                    CollectPolyTypeNames(r.Target, result);
                    break;

                case AstCompCallExpr _:
                    break;

                case AstEmptyExpr _:
                    break;

                case AstRangeExpr r:
                    CollectPolyTypeNames(r.From, result);
                    CollectPolyTypeNames(r.To, result);
                    break;

                case AstNumberExpr _:
                case AstStringLiteral _:
                case AstBoolExpr _:
                    break;

                default: throw new NotImplementedException($"Type {typeExpr}");
            }
        }
    }
}
