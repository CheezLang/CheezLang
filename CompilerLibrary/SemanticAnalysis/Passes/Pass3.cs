using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;

namespace Cheez
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class Workspace
    {
        /// <summary>
        /// pass 3: resolve the types of struct members, enum members and impl blocks
        /// </summary>
        private void Pass3()
        {
            // enums
            foreach (var @enum in mEnums)
            {
                Pass3Enum(@enum);
            }

            // structs
            var newInstances = new List<AstStructDecl>();

            newInstances.AddRange(mStructs);

            foreach (var @struct in mPolyStructs)
            {
                newInstances.AddRange(@struct.PolymorphicInstances);
            }

            ResolveStructs(new List<AstStructDecl>(newInstances));
            ResolveStructMembers(newInstances);

            // impls
            foreach (var trait in mTraits)
            {
                Pass3Trait(trait);
            }
            foreach (var impl in mImpls)
            {
                Pass3Impl(impl);
            }
            foreach (var impl in mTraitImpls)
            {
                Pass3TraitImpl(impl);
            }
        }

        private void Pass3Enum(AstEnumDecl @enum)
        {
            @enum.SubScope = new Scope("enum", @enum.Scope);

            var names = new HashSet<string>();

            @enum.TagType = IntType.DefaultType;

            int value = 0;
            bool hasAssociatedTypes = false;
            foreach (var mem in @enum.Members)
            {
                if (names.Contains(mem.Name.Name))
                    ReportError(mem.Name, $"Duplicate enum member '{mem.Name}'");

                if (mem.AssociatedType != null)
                {
                    hasAssociatedTypes = true;
                    mem.AssociatedType.Scope = @enum.SubScope;
                    mem.AssociatedType = ResolveType(mem.AssociatedType, out var t);
                }

                if (mem.Value == null)
                {
                    mem.Value = new AstNumberExpr(value, Location: mem.Name);
                }

                mem.Value.Scope = @enum.SubScope;
                mem.Value = InferType(mem.Value, @enum.TagType);
                ConvertLiteralTypeToDefaultType(mem.Value, @enum.TagType);
                if (!mem.Value.IsCompTimeValue || !(mem.Value.Type is IntType))
                {
                    ReportError(mem.Value, $"The value of an enum member must be a compile time integer");
                }
                else
                {
                    value = (int)((NumberData)mem.Value.Value).IntValue + 1;
                }
            }

            if (hasAssociatedTypes)
            {
                // TODO: check if all values are unique
            }

            //@enum.Type = new EnumType(@enum);
            ((EnumType)@enum.Type).CalculateSize();
        }

        private void Pass3Trait(AstTraitDeclaration trait)
        {
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
            impl.TraitExpr.Scope = impl.Scope;
            impl.Scope.ImplBlocks.Add(impl);

            impl.TraitExpr = ResolveType(impl.TraitExpr, out var type);
            if (type.IsErrorType)
                return;

            if (type is TraitType tt)
            {
                impl.Trait = tt;
            }
            else
            {
                ReportError(impl.TraitExpr, $"{type} is not a trait");
                return;
            }

            impl.TargetTypeExpr.Scope = impl.Scope;
            impl.TargetTypeExpr = ResolveType(impl.TargetTypeExpr, out var t);
            impl.TargetType = t;
            if (impl.TargetTypeExpr.IsPolymorphic)
            {
                ReportError(impl.TargetTypeExpr, $"Polymorphic type is not allowed here");
                return;
            }
            impl.SubScope.DefineTypeSymbol("Self", impl.TargetType);


            // register impl in trait
            if (impl.Trait.Declaration.Implementations.TryGetValue(impl.TargetType, out var otherImpl))
            {
                ReportError(impl.TargetTypeExpr, $"There already exists an implementation of trait {impl.Trait} for type {impl.TargetType}",
                    ("Other implementation here:", otherImpl.TargetTypeExpr));
            }
            impl.Trait.Declaration.Implementations[impl.TargetType] = impl;
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

                        if (fp.Type != tp.Type)
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
            impl.TargetTypeExpr = ResolveType(impl.TargetTypeExpr, out var t);
            impl.TargetType = t;
            impl.Scope.ImplBlocks.Add(impl);

            var polyNames = new List<string>();
            CollectPolyTypeNames(impl.TargetTypeExpr, polyNames);

            foreach (var pn in polyNames)
            {
                impl.SubScope.DefineTypeSymbol(pn, new PolyType(pn));
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
