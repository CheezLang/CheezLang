#nullable enable

using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private void ResolveImports()
        {
            ModuleSymbol? GetModule(Scope scope, string name)
            {
                var sym = scope.GetSymbol(name);
                return sym as ModuleSymbol;
            }

            while (mUnresolvedFiles.Count > 0)
            {
                var file = mUnresolvedFiles.Dequeue();

                // sort all declarations into different lists
                InsertDeclarationsIntoLists(file.Statements);

                // find all imports and resolve them first
                while (mUnresolvedGlobalImportUses.Count > 0)
                {
                    var use = mUnresolvedGlobalImportUses.Dequeue();
                    var import = use.Value as AstImportExpr ?? throw new System.Exception();
                    InferTypeImportExpr(import, file);
                    if (!(import.Value is PTFile importedFile))
                        continue;

                    file.FileScope.AddUsedScope(importedFile.FileScope);
                }

                while (mUnresolvedGlobalImportConstants.Count > 0)
                {
                    var importC = mUnresolvedGlobalImportConstants.Dequeue();
                    if (!(importC.Pattern is AstIdExpr importAs))
                    {
                        ReportError(importC.Pattern, $"Only identifier allowed here.");
                        continue;
                    }
                    var import = importC.Initializer as AstImportExpr ?? throw new System.Exception();
                    InferTypeImportExpr(import, file);

                    if (!(import.Value is PTFile importedFile))
                        continue;

                    var lastName = importAs;
                    var (ok, other) = file.FileScope.DefineSymbol(new ModuleSymbol(importedFile.FileScope, lastName.Name, lastName.Location));
                    if (!ok)
                        ReportError(import, $"Module {lastName} is already imported", ("Other import here:", other));
                }

                while (mUnresolvedGlobalImports.Count > 0)
                {
                    var import = mUnresolvedGlobalImports.Dequeue();
                    InferTypeImportExpr(import, file);

                    if (!(import.Value is PTFile importedFile))
                        continue;

                    var targetScope = file.FileScope;

                    for (int i = 0; i < import.Path.Length - 1; i++)
                    {
                        var name = import.Path[i];

                        var subMod = GetModule(targetScope, name.Name);

                        if (subMod == null)
                        {
                            var subScope = new Scope(name.Name);
                            targetScope.DefineSymbol(new ModuleSymbol(subScope, name.Name, name.Location));
                            targetScope = subScope;
                        }
                        else
                        {
                            targetScope = subMod.Scope;
                        }
                    }

                    var lastName = import.Path.Last();
                    var (ok, other) = targetScope.DefineSymbol(new ModuleSymbol(importedFile.FileScope, lastName.Name, lastName.Location));
                    if (!ok)
                        ReportError(import, $"Module {lastName} is already imported", ("Other import here:", other));
                }
            }
        }

        private void ResolveDeclarations()
        {
            // handle constant declarations
            ResolveConstantDeclarations();

            // handle uses
            foreach (var use in mAllGlobalUses)
                AnalyseUseStatement(use);


            // global variables
            ResolveGlobalVariables();

            // resolve impls (check if is polymorphic, setup scopes, check for self params in functions, etc.)
            foreach (var impl in mAllImpls)
            {
                if (impl.TraitExpr == null)
                    Pass3Impl(impl);
                else
                    Pass3TraitImpl(impl);
            }

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
                decl.SetFlag(StmtFlags.GlobalScope, true);
                switch (decl)
                {
                    case AstConstantDeclaration con when con.Initializer is AstImportExpr:
                        mUnresolvedGlobalImportConstants.Enqueue(con);
                        break;

                    case AstUsingStmt use when use.Value is AstImportExpr:
                        mUnresolvedGlobalImportUses.Enqueue(use);
                        break;

                    case AstExprStmt stmt when stmt.Expr is AstImportExpr import:
                        import.Scope = stmt.Scope;
                        mUnresolvedGlobalImports.Enqueue(import);
                        break;

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
            for (int i = 0; i < mAllGlobalConstants.Count; i++)
            {
                foreach (var sub in SplitConstantDeclaration(mAllGlobalConstants[i]))
                {
                    mAllGlobalConstants.Add(sub);
                }
            }

            foreach (var con in mAllGlobalConstants)
            {
                var (ok, other) = con.Scope.DefineSymbol(con);
                if (!ok)
                    ReportError(con, $"A symbol with name '{con.Name.Name}' already exists in this scope", ("Other declaration here:", other));
            }
            foreach (var con in mAllGlobalConstants)
            {
                if (con.TypeExpr != null)
                    CollectTypeDependencies(con, con.TypeExpr);
                CollectTypeDependencies(con, con.Initializer);
            }

            ResolveMissingTypesOfDeclarations(mAllGlobalConstants);
        }

        private void ResolveGlobalVariables()
        {
            for (int i = 0; i < mAllGlobalVariables.Count; i++)
            {
                foreach (var sub in SplitVariableDeclaration(mAllGlobalVariables[i]))
                {
                    mAllGlobalVariables.Add(sub);
                }
            }

            foreach (var con in mAllGlobalVariables)
            {
                var (ok, other) = con.Scope.DefineSymbol(con);
                if (!ok)
                    ReportError(con, $"A symbol with name '{con.Name.Name}' already exists in this scope", ("Other declaration here:", other));
            }
            foreach (var @var in mAllGlobalVariables)
            {
                if (@var.TypeExpr != null)
                    CollectTypeDependencies(@var, @var.TypeExpr);
                if (@var.Initializer != null)
                    CollectTypeDependencies(@var, @var.Initializer);
            }
            ResolveMissingTypesOfDeclarations(mAllGlobalVariables);
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

                    var details = new List<(string, ILocation?)> { (detail1, null) };
                    details.AddRange(c.Skip(1).Take(c.Count - 2).Select(x => ("Here is the next declaration in the cycle", x.Name.Location))!);
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
                        if (!(c.Pattern is AstIdExpr))
                            ReportError(c.Pattern, $"Only identifier pattern allowed here");

                        if (c.TypeExpr != null)
                        {
                            c.TypeExpr.AttachTo(c);
                            c.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                            c.TypeExpr = ResolveType(c.TypeExpr, newPolyDecls, out var t);
                            c.Type = t;
                        }

                        c.Initializer.AttachTo(c);
                        c.Initializer.SetFlag(ExprFlags.ValueRequired, true);
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
                        v.SetFlag(StmtFlags.GlobalScope, true);
                        ResolveVariableDecl(v);
                        break;
                    }
            }

            greySet.Remove(decl);
        }
    }
}
