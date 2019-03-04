﻿using Cheez.Ast;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
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

            foreach (var i in mImpls)
            {
                foreach (var f in i.Functions)
                {
                    f.Scope = i.SubScope;
                    f.ConstScope = new Scope("$", f.Scope);
                    f.SubScope = new Scope("fn", f.ConstScope);
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
                }
            }

            foreach (var i in mTraitImpls)
            {
                foreach (var f in i.Functions)
                {
                    f.Scope = i.SubScope;
                    f.ConstScope = new Scope("$", f.Scope);
                    f.SubScope = new Scope("fn", f.ConstScope);
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
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
            else if (!func.IsGeneric && !func.HasConstantParameters)
            {
                func.Scope.FunctionDeclarations.Add(func);
            }
        }

        private void ResolveFunctionSignature(AstFunctionDecl func)
        {
            if (func.ReturnValue?.TypeExpr?.IsPolymorphic ?? false)
            {
                ReportError(func.ReturnValue, "The return type of a function can't be polymorphic");
            }

            foreach (var p in func.Parameters)
            {
                if (p.TypeExpr.IsPolymorphic && (p.Name?.IsPolymorphic ?? false))
                {
                    ReportError(p, "A parameter can't be both constant and polymorphic");
                }
                if (p.TypeExpr.IsPolymorphic)
                {
                    func.IsGeneric = true;
                }
                if (p.Name?.IsPolymorphic ?? false)
                {
                    func.HasConstantParameters = true;
                }
            }

            if (func.HasConstantParameters)
            {
                foreach (var p in func.Parameters)
                {
                    p.TypeExpr.Scope = func.SubScope;
                    if (p.Name.IsPolymorphic)
                    {
                        p.Type = ResolveType(p.TypeExpr);
                        switch (p.Type)
                        {
                            case IntType _:
                            case FloatType _:
                            case CheezTypeType _:
                            case BoolType _:
                            case CharType _:
                                break;

                            case ErrorType _:
                                break;

                            default:
                                ReportError(p.TypeExpr, $"The type '{p.Type}' is not allowed here.");
                                break;
                        }
                    }
                }

                func.Type = new ConstParamFunctionType(func);
            }
            else if (func.IsGeneric)
            {
                var polyTypes = new HashSet<string>();

                foreach (var p in func.Parameters)
                {
                    CollectPolyTypes(p.TypeExpr, polyTypes);
                }

                foreach (var pt in polyTypes)
                {
                    func.SubScope.DefineTypeSymbol(pt, new PolyType(pt, false));
                }

                // return types
                if (func.ReturnValue != null)
                {
                    func.ReturnValue.Scope = func.SubScope;
                    func.ReturnValue.TypeExpr.Scope = func.SubScope;
                    func.ReturnValue.Type = ResolveType(func.ReturnValue.TypeExpr);
                }

                // parameter types
                foreach (var p in func.Parameters)
                {
                    p.TypeExpr.Scope = func.SubScope;
                    p.Type = ResolveType(p.TypeExpr);
                }

                func.Type = new GenericFunctionType(func);
            }
            else
            {
                // return types
                if (func.ReturnValue != null)
                {
                    func.ReturnValue.Scope = func.SubScope;
                    func.ReturnValue.TypeExpr.Scope = func.SubScope;
                    func.ReturnValue.Type = ResolveType(func.ReturnValue.TypeExpr);
                }

                // parameter types
                foreach (var p in func.Parameters)
                {
                    p.TypeExpr.Scope = func.SubScope;
                    p.Type = ResolveType(p.TypeExpr);
                }

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
        }
    }
}