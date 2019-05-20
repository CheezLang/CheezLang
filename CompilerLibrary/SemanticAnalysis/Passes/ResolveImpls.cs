using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using System.Collections.Generic;

namespace Cheez
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class Workspace
    {        
        private void Pass3Trait(AstTraitDeclaration trait)
        {
            foreach (var p in trait.Parameters)
            {
                trait.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }
            trait.SubScope.DefineTypeSymbol("Self", trait.Type);

            foreach (var f in trait.Functions)
            {
                f.IsTraitFunction = true;
                f.Scope = trait.SubScope;
                f.ConstScope = new Scope("$", f.Scope);
                f.SubScope = new Scope("fn", f.ConstScope);

                Pass4ResolveFunctionSignature(f);
                CheckForSelfParam(f);

                if (!f.SelfParameter || f.SelfType == SelfParamType.Value)
                {
                    ReportError(f.Name, $"Trait functions must take a self parameter by reference");
                }

                // TODO: for now don't allow default implemenation
                if (f.Body != null)
                {
                    ReportError(f.Name, $"Trait functions can't have an implementation");
                }
            }
        }

        private void Pass3TraitImpl(AstImplBlock impl)
        {
            impl.TraitExpr.Scope = impl.SubScope;

            CheezType type = null;
            if (!impl.IsPolyInstance)
            {
                var polyNames = new List<string>();
                CollectPolyTypeNames(impl.TargetTypeExpr, polyNames);
                CollectPolyTypeNames(impl.TraitExpr, polyNames);
                foreach (var pn in polyNames)
                    impl.SubScope.DefineTypeSymbol(pn, new PolyType(pn, true));
                impl.TraitExpr = ResolveTypeNow(impl.TraitExpr, out type);
            }
            else
            {
                impl.TraitExpr = ResolveTypeNow(impl.TraitExpr, out type, poly_from_scope: true);
            }

            if (type.IsErrorType)
                return;

            if (type is TraitType tt)
            {
                impl.Trait = tt;
            }
            else
            {
                ReportError(impl.TraitExpr, $"{type} is not a trait");
                impl.Trait = new TraitErrorType();
                return;
            }

            impl.TargetTypeExpr.Scope = impl.SubScope;
            if (!impl.IsPolyInstance)
            {
                impl.TargetTypeExpr = ResolveTypeNow(impl.TargetTypeExpr, out var t);
                impl.TargetType = t;
            }
            else
            {
                impl.TargetTypeExpr = ResolveTypeNow(impl.TargetTypeExpr, out var t, poly_from_scope: true);
                impl.TargetType = t;
            }

            impl.SubScope.DefineTypeSymbol("Self", impl.TargetType);


            // register impl in trait
            if (impl.Trait.Declaration.Implementations.TryGetValue(impl.TargetType, out var otherImpl))
            {
                ReportError(impl.TargetTypeExpr, $"There already exists an implementation of trait {impl.Trait} for type {impl.TargetType}",
                    ("Other implementation here:", otherImpl.TargetTypeExpr));
            }
            impl.Trait.Declaration.Implementations[impl.TargetType] = impl;

            impl.IsPolymorphic = impl.Trait.IsPolyType || impl.TargetType.IsPolyType;
            if (!impl.IsPolymorphic)
                AddTraitForType(impl.TargetType, impl);

            // handle functions
            foreach (var f in impl.Functions)
            {
                f.Scope = impl.SubScope;
                f.ConstScope = new Scope("$", f.Scope);
                f.SubScope = new Scope("fn", f.ConstScope);
                f.ImplBlock = impl;

                Pass4ResolveFunctionSignature(f);
                CheckForSelfParam(f);
                impl.Scope.DefineImplFunction(f);

                if (f.Body == null)
                {
                    ReportError(f.Name, $"Function must have an implementation");
                }
            }

            // match functions against trait functions
            foreach (var traitFunc in impl.Trait.Declaration.Functions)
            {
                // find matching function
                bool found = false;
                foreach (var func in impl.Functions)
                {
                    if (func.Name.Name != traitFunc.Name.Name)
                        continue;

                    func.TraitFunction = traitFunc;
                    found = true;

                    if (func.Parameters.Count != traitFunc.Parameters.Count)
                    {
                        ReportError(func.ParameterLocation, $"This function must take the same parameters as the corresponding trait function", ("Trait function defined here:", traitFunc));
                        continue;
                    }

                    // skip first parameter since it is the self parameter
                    for (int i = 1; i < func.Parameters.Count; i++)
                    {
                        var fp = func.Parameters[i];
                        var tp = traitFunc.Parameters[i];

                        if (!CheezType.TypesMatch(fp.Type, tp.Type))
                        {
                            ReportError(fp.TypeExpr, $"Type of parameter must match the type of the trait functions parameter", ("Trait function parameter type defined here:", tp.TypeExpr));
                        }
                    }

                    // check return type
                }

                if (!found)
                {
                    ReportError(impl.TargetTypeExpr, $"Missing implementation for trait function '{traitFunc.Name.Name}'", ("Trait function defined here:", traitFunc));
                }
            }
        }

        private void Pass3Impl(AstImplBlock impl)
        {
            impl.TargetTypeExpr.Scope = impl.Scope;
            impl.TargetTypeExpr = ResolveTypeNow(impl.TargetTypeExpr, out var t);
            impl.TargetType = t;

            var polyNames = new List<string>();
            CollectPolyTypeNames(impl.TargetTypeExpr, polyNames);

            foreach (var pn in polyNames)
            {
                impl.SubScope.DefineTypeSymbol(pn, new PolyType(pn, true));
            }
            impl.SubScope.DefineTypeSymbol("Self", impl.TargetType);

            foreach (var f in impl.Functions)
            {
                f.Scope = impl.SubScope;
                f.ConstScope = new Scope("$", f.Scope);
                f.SubScope = new Scope("fn", f.ConstScope);
                f.ImplBlock = impl;

                Pass4ResolveFunctionSignature(f);
                CheckForSelfParam(f);
                impl.Scope.DefineImplFunction(f);
            }
        }
    }
}
