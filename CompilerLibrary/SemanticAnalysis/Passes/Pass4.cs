using Cheez.Ast;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
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
                    f.SubScope = new Scope("fn", f.Scope);
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
                }
            }

            foreach (var i in mTraitImpls)
            {
                foreach (var f in i.Functions)
                {
                    f.Scope = i.SubScope;
                    f.SubScope = new Scope("fn", f.Scope);
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
                }
            }
        }

        private void Pass4ResolveFunctionSignature(AstFunctionDecl func)
        {
            if (func.ReturnValue != null)
                func.IsGeneric = func.ReturnValue.TypeExpr.IsPolymorphic;

            if (!func.IsGeneric)
            {
                foreach (var p in func.Parameters)
                {
                    if (p.TypeExpr.IsPolymorphic)
                    {
                        func.IsGeneric = true;
                        break;
                    }
                }
            }

            if (func.IsGeneric)
            {
                var polyTypes = new HashSet<string>();

                if (func.ReturnValue != null)
                    CollectPolyTypes(func.ReturnValue.TypeExpr, polyTypes);

                foreach (var p in func.Parameters)
                    CollectPolyTypes(p.TypeExpr, polyTypes);

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
    }
}
