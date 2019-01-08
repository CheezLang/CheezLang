using Cheez.Ast.Statements;
using Cheez.Types.Complex;
using System.Collections.Generic;

namespace Cheez
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class Workspace
    {
        /// <summary>
        /// pass 3: resolve the types of struct members
        /// </summary>
        private void Pass3()
        {
            var newInstances = new List<AstStructDecl>();

            newInstances.AddRange(mStructs);

            foreach (var @struct in mPolyStructs)
            {
                newInstances.AddRange(@struct.PolymorphicInstances);
            }

            ResolveStructs(newInstances);

            // impls
            foreach (var impl in mAllImpls)
            {
                Pass3Impl(impl);
            }
            foreach (var impl in mTraitImpls)
            {
                Pass3TraitImpl(impl);
            }
        }

        private void Pass3TraitImpl(AstImplBlock impl)
        {
            impl.TraitExpr.Scope = impl.Scope;
            var type = ResolveType(impl.TraitExpr);
            if (type is TraitType tt)
            {
                impl.Trait = tt;
            }
            else
            {
                ReportError(impl.TraitExpr, $"Has to be a trait, but is {type}");
            }

            impl.TargetTypeExpr.Scope = impl.Scope;
            impl.TargetType = ResolveType(impl.TargetTypeExpr);
            if (impl.TargetTypeExpr.IsPolymorphic)
            {
                ReportError(impl.TargetTypeExpr, $"Polymorphic type is not allowed here");
            }
        }

        private void Pass3Impl(AstImplBlock impl)
        {
            impl.TargetTypeExpr.Scope = impl.Scope;
            impl.TargetType = ResolveType(impl.TargetTypeExpr);

            if (impl.TargetTypeExpr.IsPolymorphic)
            {
            }
            else
            {
                mImpls.Add(impl);
            }
        }
    }
}
