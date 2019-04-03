using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
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
            DefineTypeDeclarations(scope);

            // 2. go through all constant declarations and define them in the scope
            DefineConstantDeclarations(scope);

            // 3. go through all type declarations again and build the dependencies
            BuildDependencies(scope);

            // 4. check for cyclic dependencies and resolve types of typedefs and constant variables
            ResolveMissingTypesOfDeclarations(scope);
        }
        private void ValidatePolymorphicParameterType(ILocation location, CheezType type)
        {
            switch (type)
            {
                case CheezTypeType _:
                    break;

                default:
                    ReportError(location, $"The type {type} is not allowed here");
                    break;
            }
        }

        private void ResolveMissingTypesOfDeclarationsHelper(
            Scope scope,
            AstDecl decl,
            HashSet<AstDecl> whiteSet, 
            HashSet<AstDecl> greySet, 
            HashSet<AstDecl> blackSet)
        {
            whiteSet.Remove(decl);
            greySet.Add(decl);

            foreach (var dep in decl.Dependencies)
            {
                if (greySet.Contains(dep))
                {
                    ReportError(decl.Name, $"Cyclic dependency not allowed", ("Here is the next declaration in the cycle", dep.Name));
                    return;
                }
                else if (whiteSet.Contains(dep))
                {
                    ResolveMissingTypesOfDeclarationsHelper(scope, dep, whiteSet, greySet, blackSet);
                }
            }

            var newPolyDecls = new List<AstDecl>();

            switch (decl)
            {
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
                            ValidatePolymorphicParameterType(p.TypeExpr, p.Type);
                        }
                        break;
                    }

                case AstEnumDecl @enum when @enum.IsPolymorphic:
                    {
                        foreach (var p in @enum.Parameters)
                        {
                            p.TypeExpr = ResolveType(p.TypeExpr, newPolyDecls, out var type);
                            p.Type = type;
                            ValidatePolymorphicParameterType(p.TypeExpr, p.Type);
                        }
                        break;
                    }

                case AstTraitDeclaration trait when trait.IsPolymorphic:
                    {
                        foreach (var p in trait.Parameters)
                        {
                            p.TypeExpr = ResolveType(p.TypeExpr, newPolyDecls, out var type);
                            p.Type = type;
                            ValidatePolymorphicParameterType(p.TypeExpr, p.Type);
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

            whiteSet.UnionWith(scope.StructDeclarations);
            whiteSet.UnionWith(scope.EnumDeclarations);
            whiteSet.UnionWith(scope.TraitDeclarations);
            whiteSet.UnionWith(scope.Typedefs);
            whiteSet.UnionWith(scope.Variables.Where(v => v.Constant).SelectMany(v => v.SubDeclarations));

            while (whiteSet.Count > 0)
            {
                var x = whiteSet.First();
                ResolveMissingTypesOfDeclarationsHelper(scope, x, whiteSet, greySet, blackSet);
            }
        }

        private void BuildDependencies(Scope scope)
        {
            foreach (var @struct in scope.StructDeclarations)
            {
                foreach (var param in @struct.Parameters)
                {
                    CollectTypeDependencies(@struct, param.TypeExpr);
                }
                PrintDependencies(@struct);
            }

            foreach (var @enum in scope.EnumDeclarations)
            {
                foreach (var param in @enum.Parameters)
                {
                    CollectTypeDependencies(@enum, param.TypeExpr);
                }
                PrintDependencies(@enum);
            }

            foreach (var @trait in scope.TraitDeclarations)
            {
                foreach (var param in @trait.Parameters)
                {
                    CollectTypeDependencies(@trait, param.TypeExpr);
                }
                PrintDependencies(@trait);
            }

            foreach (var typedef in scope.Typedefs)
            {
                CollectTypeDependencies(typedef, typedef.TypeExpr);
                PrintDependencies(typedef);
            }

            foreach (var @var in scope.Variables)
            {
                if (!@var.Constant)
                    continue;

                foreach (var sv in @var.SubDeclarations)
                {
                    CollectTypeDependencies(sv, @var.TypeExpr);
                    PrintDependencies(sv);
                }
            }
        }

        private void PrintDependencies(AstDecl decl)
        {
            //Console.WriteLine($"Dependencies of {decl.Name.Name}");
            //foreach (var d in decl.Dependencies)
            //{
            //    Console.WriteLine($"    {d.Name.Name}");
            //}
        }

        private void CollectTypeDependencies(AstDecl decl, AstExpression typeExpr)
        {
            switch (typeExpr)
            {
                case AstIdExpr id:
                    var sym = decl.Scope.GetSymbol(id.Name);
                    if (sym is AstDecl d)
                        decl.Dependencies.Add(d);
                    break;

                case AstAddressOfExpr add:
                    CollectTypeDependencies(decl, add.SubExpression);
                    break;

                case AstSliceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstArrayTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstReferenceTypeExpr expr:
                    CollectTypeDependencies(decl, expr.Target);
                    break;

                case AstFunctionTypeExpr expr:
                    if (expr.ReturnType != null)
                        CollectTypeDependencies(decl, expr.ReturnType);
                    foreach (var p in expr.ParameterTypes)
                        CollectTypeDependencies(decl, p);
                    break;

                case AstCallExpr expr:
                    CollectTypeDependencies(decl, expr.Function);
                    foreach (var p in expr.Arguments)
                        CollectTypeDependencies(decl, p.Expr);
                    break;

                case AstTupleExpr expr:
                    foreach (var p in expr.Values)
                        CollectTypeDependencies(decl, p);
                    break;
            }
        }

        private void DefineConstantDeclarations(Scope scope)
        {
            foreach (var @const in scope.Variables)
            {
                if (@const.Constant)
                {
                    Pass1VariableDeclaration(@const);
                }
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
        }

        public void InsertDeclarationsIntoScope(Scope scope, List<AstStatement> statements)
        {
            foreach (var decl in statements)
            {
                decl.Scope = scope;

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
