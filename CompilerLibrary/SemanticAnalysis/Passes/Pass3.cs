using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // calculate sizes of types (enums and structs)
            CalculateEnumAndStructSizes();

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

        private void CalculateSizeOfDecl(AstDecl decl, HashSet<AstDecl> done, HashSet<AstDecl> path)
        {
            if (done.Contains(decl))
                return;

            if (path.Contains(decl))
            {
                ReportError(decl.Name, $"Failed to calculate the size of {decl.Name}");
                path.Remove(decl);
                done.Add(decl);
                return;
            }

            path.Add(decl);

            if (decl is AstEnumDecl @enum)
            {
                foreach (var em in @enum.Members)
                {
                    if (em.AssociatedType != null)
                    {
                        if (em.AssociatedType.Value is StructType s)
                            CalculateSizeOfDecl(s.Declaration, done, path);
                        else if (em.AssociatedType.Value is EnumType e)
                            CalculateSizeOfDecl(e.Declaration, done, path);
                    }
                }

                ((EnumType)@enum.Type).CalculateSize();
            }
            else if (decl is AstStructDecl @struct)
            {
                foreach (var em in @struct.Members)
                {
                    if (em.Type is StructType s)
                        CalculateSizeOfDecl(s.Declaration, done, path);
                    else if (em.Type is EnumType e)
                        CalculateSizeOfDecl(e.Declaration, done, path);
                }

                ((StructType)@struct.Type).CalculateSize();
            }

            path.Remove(decl);
            done.Add(decl);
        }

        private void CalculateEnumAndStructSizes()
        {
            // detect cycles
            var whiteSet = new HashSet<AstDecl>();

            whiteSet.UnionWith(mEnums);
            whiteSet.UnionWith(mStructs);
            whiteSet.UnionWith(mPolyStructs.SelectMany(ps => ps.PolymorphicInstances));


            var done = new HashSet<AstDecl>();
            foreach (var decl in whiteSet)
            {
                CalculateSizeOfDecl(decl, done, new HashSet<AstDecl>());
            }
        }

        private void Pass3Enum(AstEnumDecl @enum)
        {
            @enum.SubScope = new Scope("enum", @enum.Scope);

            var names = new HashSet<string>();

            @enum.TagType = IntType.DefaultType;

            int value = 0;
            foreach (var mem in @enum.Members)
            {
                if (names.Contains(mem.Name.Name))
                    ReportError(mem.Name, $"Duplicate enum member '{mem.Name}'");

                if (mem.AssociatedType != null)
                {
                    @enum.HasAssociatedTypes = true;
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

            if (@enum.HasAssociatedTypes)
            {
                // TODO: check if all values are unique
            }

            //@enum.Scope.DefineBinaryOperator("==", );
            //@enum.Scope.DefineBinaryOperator("!=", );

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
