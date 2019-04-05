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
        /// <summary>
        /// pass 4: resolve function signatures
        /// </summary>
        private void Pass4()
        {
            foreach (var f in mFunctions)
            {
                Pass4ResolveFunctionSignature(f);
            }
        }

        private void CheckForSelfParam(AstFunctionDecl func)
        {
            if (func.Parameters.Count > 0)
            {
                var param = func.Parameters[0];
                if (param.TypeExpr is AstIdExpr i && i.Name == "Self")
                {
                    func.SelfParameter = true;
                    func.SelfType = SelfParamType.Value;
                }
                else if (param.TypeExpr is AstReferenceTypeExpr p2 && p2.Target is AstIdExpr i3 && i3.Name == "Self")
                {
                    func.SelfParameter = true;
                    func.SelfType = SelfParamType.Reference;
                }
            }
        }

        private void Pass4ResolveFunctionSignature(AstFunctionDecl func)
        {
            ResolveFunctionSignature(func);


            var res = func.Scope.DefineDeclaration(func);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(func.Name, $"A symbol with name '{func.Name.Name}' already exists in current scope", detail);
            }
        }

        private void ResolveFunctionSignature(AstFunctionDecl func)
        {
            if (func.ReturnTypeExpr?.TypeExpr?.IsPolymorphic ?? false)
            {
                ReportError(func.ReturnTypeExpr, "The return type of a function can't be polymorphic");
            }

            var polyNames = new List<string>();
            foreach (var p in func.Parameters)
            {
                CollectPolyTypeNames(p.TypeExpr, polyNames);
                if (p.Name?.IsPolymorphic ?? false)
                    polyNames.Add(p.Name.Name);
            }

            foreach (var pn in polyNames)
            {
                func.ConstScope.DefineTypeSymbol(pn, new PolyType(pn));
            }

            // return types
            if (func.ReturnTypeExpr != null)
            {
                func.ReturnTypeExpr.Scope = func.SubScope;
                func.ReturnTypeExpr.TypeExpr.Scope = func.SubScope;
                func.ReturnTypeExpr.TypeExpr = ResolveTypeNow(func.ReturnTypeExpr.TypeExpr, out var t);
                func.ReturnTypeExpr.Type = t;

                if (func.ReturnTypeExpr.Type.IsPolyType)
                    func.IsGeneric = true;
            }

            // parameter types
            foreach (var p in func.Parameters)
            {
                p.TypeExpr.Scope = func.SubScope;
                p.TypeExpr = ResolveTypeNow(p.TypeExpr, out var t);
                p.Type = t;

                if (p.Type.IsPolyType || (p.Name?.IsPolymorphic ?? false))
                    func.IsGeneric = true;
            }

            if (func.IsGeneric)
            {
                func.Type = new GenericFunctionType(func);
            }
            else
            {
                func.Type = new FunctionType(func);

                if (func.TryGetDirective("varargs", out var varargs))
                {
                    if (varargs.Arguments.Count != 0)
                    {
                        ReportError(varargs, $"#varargs takes no arguments!");
                    }
                    func.FunctionType.VarArgs = true;
                }
            }

            if (func.TryGetDirective("operator", out var op))
            {
                if (op.Arguments.Count != 1)
                {
                    ReportError(op, $"#operator requires exactly one argument!");
                }
                else
                {
                    var arg = op.Arguments[0];
                    arg = op.Arguments[0] = InferType(arg, null);
                    if (arg.Value is string v)
                    {
                        var targetScope = func.Scope;
                        if (func.ImplBlock != null) targetScope = func.ImplBlock.Scope;

                        CheckForValidOperator(v, func, op, targetScope);
                    }
                    else
                    {
                        ReportError(arg, $"Argument to #op must be a constant string!");
                    }
                }
            }
        }

        private void CheckForValidOperator(string op, AstFunctionDecl func, AstDirective directive, Scope targetScope)
        {
            switch (op)
            {
                case "!":
                case "-" when func.Parameters.Count == 1:
                    if (func.ReturnType == CheezType.Void)
                        ReportError(func.Name, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 1)
                        ReportError(func.Name, $"This function must take one parameter in order to use it as operator '{op}'");
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
                        ReportError(func.Name, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 2)
                        ReportError(func.Name, $"This function must take two parameters in order to use it as operator '{op}'");
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
                        ReportError(func.Name, $"This function must take two parameters in order to use it as operator '{op}'");
                    }
                    else
                    {
                        targetScope.DefineBinaryOperator(op, func);
                    }
                    break;

                case "[]":
                    if (func.ReturnType == CheezType.Void)
                        ReportError(func.Name, $"This function must return a value in order to use it as operator '{op}'");
                    else if (func.Parameters.Count != 2)
                        ReportError(func.Name, $"This function must take two parameters in order to use it as operator '{op}'");
                    else
                        targetScope.DefineBinaryOperator(op, func);
                    break;


                case "set[]":
                    if (func.Parameters.Count != 3)
                        ReportError(func.Name, $"This function must take three parameters in order to use it as operator '{op}'");
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

                default: throw new NotImplementedException($"Type {typeExpr}");
            }
        }
    }
}
