﻿using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private void ResolveDeclarations(List<AstStatement> statements)
        {
            // sort all declarations into different lists
            InsertDeclarationsIntoLists(statements);

            // handle constant declarations
            ResolveConstantDeclarations();

            // handle uses
            foreach (var use in mAllGlobalUses)
                AnalyseUseStatement(use);

            // go through all type declarations (structs, traits, enums, typedefs) and define them in the scope
            // go through all constant declarations and define them in the scope
            foreach (var v in mAllGlobalVariables)
                Pass1VariableDeclaration(v);

            // global variables
            foreach (var @var in mAllGlobalVariables)
            {
                if (@var.TypeExpr != null)
                    CollectTypeDependencies(@var, @var.TypeExpr);
                if (@var.Initializer != null)
                    CollectTypeDependencies(@var, @var.Initializer);
            }
            // check for cyclic dependencies and resolve types of typedefs and constant variables
            ResolveMissingTypesOfDeclarations(mAllGlobalVariables);

            // resolve impls (check if is polymorphic, setup scopes, check for self params in functions, etc.)
            foreach (var impl in mAllImpls)
            {
                if (impl.TraitExpr == null)
                    Pass3Impl(impl);
                else
                    Pass3TraitImpl(impl);
            }

            // go through all type declarations and connect impls to types
            // @todo: don't think this is necessary here
            // UpdateTypeImplMap(GlobalScope);

            ResolveGlobalDeclarationBodies();

            // compute struct members of remaining structs

            while (true)
            {
                var allTypes = CheezType.TypesWithMissingProperties;
                if (allTypes.Count() == 0)
                    break;

                //Console.WriteLine($"Calculating properties of {allTypes.Count()} types...");

                CheezType.ClearAllTypes();
                foreach (var type in allTypes)
                {
                    //try
                    {
                        if (type is AbstractType || type.IsErrorType || type.IsPolyType)
                            continue; 

                        // force computation of all types sizes
                        GetSizeOfType(type);
                        IsTypeDefaultConstructable(type);
                    }
                    //catch (Exception e)
                    //{
                    //}
                }
            }
        }

        private void InsertDeclarationsIntoLists(List<AstStatement> statements)
        {
            foreach (var decl in statements)
            {
                decl.Scope = GlobalScope;
                switch (decl)
                {
                    case AstConstantDeclaration con:
                        mAllGlobalConstants.Add(con);
                        break;

                    case AstUsingStmt use:
                        mAllGlobalUses.Add(use);
                        break;

                    case AstVariableDecl @var:
                        mAllGlobalVariables.Add(@var);
                        break;

                    case AstImplBlock impl:
                        mAllImpls.Add(impl);
                        break;
                }
            }
        }

        private void ResolveConstantDeclarations()
        {
            foreach (var con in mAllGlobalConstants)
                GlobalScope.DefineSymbol(con);
            foreach (var con in mAllGlobalConstants)
            {
                if (con.TypeExpr != null)
                    CollectTypeDependencies(con, con.TypeExpr);
                CollectTypeDependencies(con, con.Initializer);
            }

            ResolveMissingTypesOfDeclarations(mAllGlobalConstants);
        }

        private void ResolveGlobalDeclarationBodies()
        {
            while (true)
            {
                bool unresolvedStuff = false;

                while (mUnresolvedTraits.Count > 0)
                {
                    unresolvedStuff = true;
                    var t = mUnresolvedTraits.Dequeue();
                    ComputeTraitMembers(t);
                }
                while (mUnresolvedStructs.Count > 0)
                {
                    unresolvedStuff = true;
                    var t = mUnresolvedStructs.Dequeue();
                    ComputeStructMembers(t);
                }
                while (mUnresolvedEnums.Count > 0)
                {
                    unresolvedStuff = true;
                    var t = mUnresolvedEnums.Dequeue();
                    ComputeEnumMembers(t);
                }
                while (mUnresolvedFunctions.Count > 0)
                {
                    unresolvedStuff = true;
                    var t = mUnresolvedFunctions.Dequeue();
                    AnalyseFunction(t);
                }
                while (mUnresolvedImpls.Count > 0)
                {
                    unresolvedStuff = true;
                    var i = mUnresolvedImpls.Dequeue();

                    //if (i.TraitExpr != null && i.Trait == null)
                    if (i.Trait?.IsErrorType ?? false)
                    {
                        // an error has been reported elsewhere, we don't need to analyse the functions
                        continue;
                    }

                    if (i.IsPolymorphic)
                    {
                        continue;
                    }

                    foreach (var f in i.Functions)
                    {
                        AnalyseFunction(f);
                    }
                }

                if (!unresolvedStuff)
                    break;
            }
        }

        private void ResolveMissingTypesOfDeclarations(IEnumerable<AstDecl> declarations)
        {
            var whiteSet = new HashSet<AstDecl>();
            var greySet = new HashSet<AstDecl>();
            var chain = new Dictionary<AstDecl, AstDecl>();

            whiteSet.UnionWith(declarations);

            while (whiteSet.Count > 0)
            {
                var x = whiteSet.First();
                ResolveMissingTypesOfDeclarationsHelper(x, whiteSet, greySet, chain);
            }
        }

        private void ResolveMissingTypesOfDeclarationsHelper(
            AstDecl decl,
            HashSet<AstDecl> whiteSet,
            HashSet<AstDecl> greySet,
            Dictionary<AstDecl, AstDecl> chain)
        {
            whiteSet.Remove(decl);
            greySet.Add(decl);

            foreach (var dep in decl.Dependencies)
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
                    ResolveMissingTypesOfDeclarationsHelper(dep, whiteSet, greySet, chain);
                }
            }

            var newPolyDecls = new List<AstDecl>();

            switch (decl)
            {
                case AstConstantDeclaration c:
                    {
                        if (c.TypeExpr != null)
                        {
                            c.TypeExpr.AttachTo(c);
                            c.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                            c.TypeExpr = ResolveType(c.TypeExpr, newPolyDecls, out var t);
                            c.Type = t;
                        }

                        c.Initializer.AttachTo(c);
                        c.Initializer = InferType(c.Initializer, c.Type);

                        if (c.Type == null)
                            c.Type = c.Initializer.Type;
                        else
                            c.Initializer = CheckType(c.Initializer, c.Type);

                        if (!c.Initializer.IsCompTimeValue)
                        {
                            ReportError(c.Initializer, $"Value of constant declaration must be constant");
                            break;
                        }
                        c.Value = c.Initializer.Value;

                        CheckValueRangeForType(c.Type, c.Value, c.Initializer);
                        break;
                    }

                case AstVariableDecl v:
                    {
                        CheezType type = null;
                        v.Type = CheezType.Error;

                        // type ex
                        if (v.TypeExpr != null)
                        {
                            v.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                            v.TypeExpr = ResolveType(v.TypeExpr, newPolyDecls, out var t);
                            type = v.Type = t;
                        }

                        v.Initializer.SetFlag(ExprFlags.ValueRequired, true);
                        v.Initializer = InferType(v.Initializer, type);
                        ConvertLiteralTypeToDefaultType(v.Initializer, type);

                        if (v.Initializer.Type.IsErrorType)
                        {
                            Console.WriteLine(v.Pattern);
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

                        if (v.TypeExpr == null)
                            v.Type = v.Initializer.Type;

                        AssignTypesAndValuesToSubdecls(v.Pattern, v.Type, v.Initializer);

                        break;
                    }
            }

            greySet.Remove(decl);
        }
    }
}
