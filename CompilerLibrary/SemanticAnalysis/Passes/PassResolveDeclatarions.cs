using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private void ResolveDeclarations(Scope scope, List<AstStatement> statements)
        {
            // 1. sort all declarations into different lists
            InsertDeclarationsIntoScope(scope, statements);

            // 2. go through all type declarations (structs, traits, enums, typedefs) and define them in the scope
            // go through all constant declarations and define them in the scope
            DefineTypeDeclarations(scope);

            // 3. go through all type declarations again and build the dependencies
            BuildDependencies(scope);

            // 4. check for cyclic dependencies and resolve types of typedefs and constant variables
            ResolveMissingTypesOfDeclarations(scope);

            // 5. compute types of struct members, enum members, trait members
            ComputeTypeMembers(scope);

            // resolve impls
            foreach (var impl in scope.Impls)
            {
                if (impl.TraitExpr == null)
                    Pass3Impl(impl);
                else
                    Pass3TraitImpl(impl);
            }

            // handle uses
            foreach (var use in scope.Uses)
            {
                AnalyseUseStatement(use);
            }

            // check initializers of non-constant variables declarations
            CheckInitializersOfNonConstantVars(scope);

            // resolve function bodies
            ResolveFunctionBodies(scope);
        }

        private void ResolveFunctionBodies(Scope scope)
        {
            foreach (var func in scope.Functions)
            {
                AnalyseFunction(func);
            }

            foreach (var i in scope.Impls)
            {
                foreach (var f in i.Functions)
                {
                    AnalyseFunction(f);
                }
            }
        }

        private void CheckInitializersOfNonConstantVars(Scope scope)
        {
            foreach (var v in scope.Variables.Where(x => !x.Constant && !x.Type.IsErrorType))
            {
                var type = v.Type;
                v.Initializer = InferType(v.Initializer, type);
                ConvertLiteralTypeToDefaultType(v.Initializer, type);

                if (v.Initializer.Type.IsErrorType)
                {
                    if (v.Constant && !v.Initializer.IsCompTimeValue)
                        ReportError(v.Initializer, $"Initializer must be a constant");
                    break;
                }

                if (v.TypeExpr != null)
                {
                    v.Initializer = HandleReference(v.Initializer, type, null);
                    v.Initializer = CheckType(v.Initializer, type);
                }
                else
                {
                    if (v.Initializer.Type is ReferenceType)
                        v.Initializer = Deref(v.Initializer, null);
                }

                if (v.Constant && !v.Initializer.IsCompTimeValue)
                {
                    ReportError(v.Initializer, $"Initializer must be a constant");
                    break;
                }

                AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);

                if (v.TypeExpr == null)
                    v.Type = v.Initializer.Type;
            }
        }

        private void ComputeTypeMembers(Scope scope)
        {
            var declarations = new List<AstDecl>();

            foreach (var @struct in scope.StructDeclarations)
            {
                if (@struct.IsPolymorphic)
                    declarations.AddRange(@struct.PolymorphicInstances);
                declarations.Add(@struct);
            }

            foreach (var @enum in scope.EnumDeclarations)
            {
                if (@enum.IsPolymorphic)
                    declarations.AddRange(@enum.PolymorphicInstances);
                declarations.Add(@enum);
            }

            foreach (var trait in scope.TraitDeclarations)
            {
                if (trait.IsPolymorphic)
                    declarations.AddRange(trait.PolymorphicInstances);
                declarations.Add(trait);
            }

            ResolveTypeDeclarations(declarations);
        }

        private bool ValidatePolymorphicParameterType(ILocation location, CheezType type)
        {
            switch (type)
            {
                case CheezTypeType _: return true;

                default:
                    ReportError(location, $"The type {type} is not allowed here");
                    return false;
            }
        }

        private void ResolveMissingTypesOfDeclarationsHelper(
            Scope scope,
            AstDecl decl,
            HashSet<AstDecl> whiteSet, 
            HashSet<AstDecl> greySet, 
            HashSet<AstDecl> blackSet,
            Dictionary<AstDecl, AstDecl> chain)
        {
            whiteSet.Remove(decl);
            greySet.Add(decl);

            foreach (var (kind, dep) in decl.Dependencies)
            {
                if (greySet.Contains(dep))
                {
                    // cyclic dependency, report error
                    var c = new List<AstDecl> { decl, dep };
                    var current = dep;
                    while (chain.TryGetValue(current, out var d))
                    {
                        c.Add(d);
                        current = d;
                    }

                    var detail1 = string.Join(" -> ", c.Select(x => x.Name.Name));

                    var details = new List<(string, ILocation)> { (detail1, null) };
                    details.AddRange(c.Skip(1).Take(c.Count - 2).Select(x => ("Here is the next declaration in the cycle", x.Name.Location)));
                    ReportError(decl.Name, $"Cyclic dependency not allowed", details);
                    return;
                }
                else if (whiteSet.Contains(dep))
                {
                    chain[decl] = dep;
                    ResolveMissingTypesOfDeclarationsHelper(scope, dep, whiteSet, greySet, blackSet, chain);
                }
            }

            var newPolyDecls = new List<AstDecl>();

            switch (decl)
            {
                case AstVariableDecl v:
                    {
                        CheezType type = null;
                        v.Type = CheezType.Error;

                        // type ex
                        if (v.TypeExpr != null)
                        {
                            v.TypeExpr = ResolveType(v.TypeExpr, newPolyDecls, out var t);
                            type = v.Type = t;
                        }

                        // this must happen later after we computed the types of struct/enum/trait members
                        // except for const variables, compute them now
                        if (!v.Constant)
                        {
                            AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);
                            break;
                        }

                        v.Initializer = InferType(v.Initializer, type);
                        ConvertLiteralTypeToDefaultType(v.Initializer, type);

                        if (v.Initializer.Type.IsErrorType)
                        {
                            if (v.Constant && !v.Initializer.IsCompTimeValue)
                                ReportError(v.Initializer, $"Initializer must be a constant");
                            break;
                        }

                        if (v.TypeExpr != null)
                        {
                            v.Initializer = HandleReference(v.Initializer, type, null);
                            v.Initializer = CheckType(v.Initializer, type);
                        }
                        else
                        {
                            if (v.Initializer.Type is ReferenceType)
                                v.Initializer = Deref(v.Initializer, null);
                        }

                        if (v.Constant && !v.Initializer.IsCompTimeValue)
                        {
                            ReportError(v.Initializer, $"Initializer must be a constant");
                            break;
                        }

                        AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);

                        if (v.TypeExpr == null)
                            v.Type = v.Initializer.Type;
                        break;
                    }

                case AstFunctionDecl func:
                    {
                        func.ConstScope = new Scope("$", func.Scope);
                        func.SubScope = new Scope("fn", func.ConstScope);
                        ResolveFunctionSignature(func, newPolyDecls);
                        break;
                    }

                case AstTypeAliasDecl typedef:
                    {
                        typedef.TypeExpr = ResolveType(typedef.TypeExpr, newPolyDecls, out var type);
                        typedef.Type = type;
                        break;
                    }

                case AstStructDecl @struct when @struct.IsPolymorphic:
                    {
                        foreach (var p in @struct.Parameters)
                        {
                            p.TypeExpr = ResolveType(p.TypeExpr, newPolyDecls, out var type);
                            p.Type = type;
                            if (!ValidatePolymorphicParameterType(p.TypeExpr, p.Type))
                                continue;

                            switch (p.Type)
                            {
                                case CheezTypeType _:
                                    p.Value = new PolyType(p.Name.Name, true);
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }

                case AstEnumDecl @enum when @enum.IsPolymorphic:
                    {
                        foreach (var p in @enum.Parameters)
                        {
                            p.TypeExpr = ResolveType(p.TypeExpr, newPolyDecls, out var type);
                            p.Type = type;
                            if (!ValidatePolymorphicParameterType(p.TypeExpr, p.Type))
                                continue;

                            switch (p.Type)
                            {
                                case CheezTypeType _:
                                    p.Value = new PolyType(p.Name.Name, true);
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }

                case AstTraitDeclaration trait when trait.IsPolymorphic:
                    {
                        foreach (var p in trait.Parameters)
                        {
                            p.TypeExpr = ResolveType(p.TypeExpr, newPolyDecls, out var type);
                            p.Type = type;
                            if (!ValidatePolymorphicParameterType(p.TypeExpr, p.Type))
                                continue;

                            switch (p.Type)
                            {
                                case CheezTypeType _:
                                    p.Value = new PolyType(p.Name.Name, true);
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }
            }

            greySet.Remove(decl);
            blackSet.Add(decl);
        }

        private void ResolveMissingTypesOfDeclarations(Scope scope)
        {
            var whiteSet = new HashSet<AstDecl>();
            var greySet = new HashSet<AstDecl>();
            var blackSet = new HashSet<AstDecl>();
            var chain = new Dictionary<AstDecl, AstDecl>();

            whiteSet.UnionWith(scope.StructDeclarations);
            whiteSet.UnionWith(scope.EnumDeclarations);
            whiteSet.UnionWith(scope.TraitDeclarations);
            whiteSet.UnionWith(scope.Typedefs);
            whiteSet.UnionWith(scope.Variables);
            whiteSet.UnionWith(scope.Functions);

            while (whiteSet.Count > 0)
            {
                var x = whiteSet.First();
                ResolveMissingTypesOfDeclarationsHelper(scope, x, whiteSet, greySet, blackSet, chain);
            }
        }

        private void BuildDependencies(Scope scope)
        {
            foreach (var @struct in scope.StructDeclarations)
            {
                foreach (var param in @struct.Parameters)
                {
                    CollectTypeDependencies(@struct, param.TypeExpr, DependencyKind.Type);
                }
            }

            foreach (var @enum in scope.EnumDeclarations)
            {
                foreach (var param in @enum.Parameters)
                {
                    CollectTypeDependencies(@enum, param.TypeExpr, DependencyKind.Type);
                }
            }

            foreach (var @trait in scope.TraitDeclarations)
            {
                foreach (var param in @trait.Parameters)
                {
                    CollectTypeDependencies(@trait, param.TypeExpr, DependencyKind.Type);
                }
            }

            foreach (var typedef in scope.Typedefs)
            {
                CollectTypeDependencies(typedef, typedef.TypeExpr, DependencyKind.Type);
            }

            foreach (var @var in scope.Variables)
            {
                CollectTypeDependencies(@var, @var.TypeExpr, DependencyKind.Type);
                CollectTypeDependencies(@var, @var.Initializer, DependencyKind.Value);
                //PrintDependencies(@var);
            }

            foreach (var func in scope.Functions)
            {
                if (func.ReturnTypeExpr != null)
                    CollectTypeDependencies(func, func.ReturnTypeExpr.TypeExpr, DependencyKind.Type);
                foreach (var p in func.Parameters)
                {
                    CollectTypeDependencies(func, p.TypeExpr, DependencyKind.Type);
                    if (p.DefaultValue != null)
                        CollectTypeDependencies(func, p.DefaultValue, DependencyKind.Value);
                }
                //PrintDependencies(func);
            }
        }

        private void PrintDependencies(AstDecl decl)
        {
            Console.WriteLine($"Dependencies of {decl.Name.Name}");
            foreach (var d in decl.Dependencies)
            {
                Console.WriteLine($"    {d.kind}: {d.decl.Name.Name}");
            }
        }

        private void DefineTypeDeclarations(Scope scope)
        {
            foreach (var @struct in scope.StructDeclarations)
                Pass1StructDeclaration(@struct);
            foreach (var @enum in scope.EnumDeclarations)
                Pass1EnumDeclaration(@enum);
            foreach (var @trait in scope.TraitDeclarations)
                Pass1TraitDeclaration(@trait);
            foreach (var @typedef in scope.Typedefs)
                Pass1Typedef(@typedef);
            foreach (var v in scope.Functions)
                Pass1FunctionDeclaration(v);
            foreach (var v in scope.Variables)
                Pass1VariableDeclaration(v);
        }

        public void InsertDeclarationsIntoScope(Scope scope, List<AstStatement> statements)
        {
            foreach (var decl in statements)
            {
                decl.Scope = scope;
                decl.Position = scope.NextPosition();

                switch (decl)
                {
                    case AstUsingStmt use:
                        {
                            scope.Uses.Add(use);
                            break;
                        }

                    case AstStructDecl @struct:
                        {
                            scope.StructDeclarations.Add(@struct);
                            break;
                        }

                    case AstTraitDeclaration @trait:
                        {
                            scope.TraitDeclarations.Add(@trait);
                            break;
                        }

                    case AstEnumDecl @enum:
                        {
                            scope.EnumDeclarations.Add(@enum);
                            break;
                        }

                    case AstVariableDecl @var:
                        {
                            scope.Variables.Add(@var);
                            break;
                        }

                    case AstFunctionDecl func:
                        {
                            scope.Functions.Add(func);
                            break;
                        }

                    case AstImplBlock impl:
                        {
                            impl.SubScope = new Scope($"impl", impl.Scope);
                            scope.Impls.Add(impl);
                            break;
                        }

                    case AstTypeAliasDecl type:
                        {
                            scope.Typedefs.Add(type);
                            break;
                        }
                }
            }
        }
    }
}
