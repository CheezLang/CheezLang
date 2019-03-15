using Cheez.Ast;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
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

            foreach (var i in mImpls)
            {
                i.Scope.ImplBlocks.Add(i);
                i.SubScope.DefineTypeSymbol("Self", i.TargetType);

                foreach (var f in i.Functions)
                {
                    f.Scope = i.SubScope;
                    f.ConstScope = new Scope("$", f.Scope);
                    f.SubScope = new Scope("fn", f.ConstScope);
                    f.ImplBlock = i;

                    Pass4ResolveFunctionSignature(f);
                    i.Scope.DefineImplFunction(f);
                }
            }

            foreach (var i in mPolyImpls)
            {
                ReportError(i.TargetTypeExpr, $"Poly impls not implemented yet.");
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

        private bool IsSelfParameter(AstParameter param)
        {
            if (param.TypeExpr is AstIdTypeExpr i && i.Name == "Self")
                return true;

            if (param.TypeExpr is AstReferenceTypeExpr p2 && p2.Target is AstIdTypeExpr i3 && i3.Name == "Self")
                return true;

            return false;
        }

        private void Pass4ResolveFunctionSignature(AstFunctionDecl func)
        {
            ResolveFunctionSignature(func);

            // check for existance of self param
            if (func.ImplBlock != null && func.Parameters.Count > 0 && IsSelfParameter(func.Parameters[0]))
            {
                func.SelfParameter = true;
                if (func.Parameters[0].Type == ReferenceType.GetRefType(func.ImplBlock.TargetType))
                    func.SelfType = SelfParamType.Reference;
                else
                    func.SelfType = SelfParamType.Value;
            }


            var res = func.Scope.DefineDeclaration(func);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(func.Name, $"A symbol with name '{func.Name.Name}' already exists in current scope", detail);
            }
            else if (!func.IsGeneric)
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
            if (func.ReturnValue != null)
            {
                func.ReturnValue.Scope = func.SubScope;
                func.ReturnValue.TypeExpr.Scope = func.SubScope;
                func.ReturnValue.Type = ResolveType(func.ReturnValue.TypeExpr);

                if (func.ReturnValue.Type.IsPolyType)
                    func.IsGeneric = true;
            }

            // parameter types
            foreach (var p in func.Parameters)
            {
                p.TypeExpr.Scope = func.SubScope;
                p.Type = ResolveType(p.TypeExpr);

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
                        bool assOp = false;
                        if (v.EndsWith("="))
                            assOp = true;

                        var targetScope = func.Scope;
                        if (func.ImplBlock != null) targetScope = func.ImplBlock.Scope;

                        if (!assOp && (func.ReturnValue == null || func.ReturnValue.Type == CheezType.Void))
                        {
                            ReportError(op, $"This function cannot be used as operator because it returns void");
                        }
                        else if (func.Parameters.Count == 1)
                        {
                            targetScope.DefineUnaryOperator(v, func);
                        }
                        else if (func.Parameters.Count == 2)
                        {
                            targetScope.DefineBinaryOperator(v, func);
                        }
                        else
                        {
                            ReportError(op, $"This function cannot be used as operator because it takes {func.Parameters.Count} parameters");
                        }
                    }
                    else
                    {
                        ReportError(arg, $"Argument to #op must be a constant string!");
                    }
                }
            }
        }

        private void CollectPolyTypeNames(AstTypeExpr typeExpr, List<string> result)
        {
            switch (typeExpr)
            {
                case AstIdTypeExpr i:
                    if (i.IsPolymorphic)
                        result.Add(i.Name);
                    break;

                case AstPointerTypeExpr p:
                    {
                        CollectPolyTypeNames(p.Target, result);
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

                case AstTupleTypeExpr te:
                    {
                        foreach (var m in te.Members)
                        {
                            CollectPolyTypeNames(m.TypeExpr, result);
                        }

                        break;
                    }

                case AstPolyStructTypeExpr ps:
                    {
                        foreach (var m in ps.Arguments)
                        {
                            CollectPolyTypeNames(m, result);
                        }

                        break;
                    }

                case AstErrorTypeExpr _: break;

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

                default: throw new NotImplementedException();
            }
        }
    }
}
