using Cheez.Compiler.Ast;

namespace Cheez.Compiler
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
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
                }
            }

            foreach (var i in mTraitImpls)
            {
                foreach (var f in i.Functions)
                {
                    f.Scope = i.SubScope;
                    f.ImplBlock = i;
                    Pass4ResolveFunctionSignature(f);
                }
            }
        }

        private void Pass4ResolveFunctionSignature(AstFunctionDecl func)
        {
            foreach (var p in func.ReturnValues)
            {
                if (p.TypeExpr.IsPolymorphic)
                {
                    func.IsGeneric = true;
                    break;
                }
            }
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
                func.Type = new GenericFunctionType(func);
            }
            else
            {
                // return types
                foreach (var p in func.ReturnValues)
                {
                    p.TypeExpr.Scope = func.Scope;
                    p.Type = ResolveType(p.TypeExpr);
                }

                // parameter types
                foreach (var p in func.Parameters)
                {
                    p.TypeExpr.Scope = func.Scope;
                    p.Type = ResolveType(p.TypeExpr);
                }

                func.Type = FunctionType.GetFunctionType(func);
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
