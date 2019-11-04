using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
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
        private class TypeImplList
        {
            public HashSet<AstImplBlock> impls;
            public List<AstImplBlock> potentialImpls;
            public List<AstImplBlock> temp;

            public TypeImplList(List<AstImplBlock> potentials)
            {
                impls = new HashSet<AstImplBlock>();
                potentialImpls = new List<AstImplBlock>(potentials);
                temp = new List<AstImplBlock>();
            }
        }

        private Dictionary<CheezType, TypeImplList> m_typeImplMap;

        private IEnumerable<Dictionary<string, CheezType>> CheckIfConditionApplies(ImplConditionImplTrait cond, Dictionary<string, CheezType> polies)
        {
            cond.type.Scope = cond.Scope;
            cond.trait.Scope = cond.Scope;

            var ty_expr = cond.type.Clone();
            var tr_expr = cond.trait.Clone();

            ty_expr.Scope = new Scope("temp", ty_expr.Scope);
            tr_expr.Scope = new Scope("temp", tr_expr.Scope);

            foreach (var p in polies)
            {
                ty_expr.Scope.DefineTypeSymbol(p.Key, p.Value);
                tr_expr.Scope.DefineTypeSymbol(p.Key, p.Value);
            }

            ty_expr.SetFlag(ExprFlags.ValueRequired, true);
            var ty = InferType(ty_expr, null, forceInfer: true).Value as CheezType;
            tr_expr.SetFlag(ExprFlags.ValueRequired, true);
            var tr = InferType(tr_expr, null, forceInfer: true).Value as CheezType;

            var matches = GetTraitImplForType(ty, tr, polies);
            return matches;
        }

        private (List<AstImplBlock> impls, bool maybeApplies) ImplAppliesToType(AstImplBlock impl, CheezType type)
        {
            if (type.IsErrorType)
                WellThatsNotSupposedToHappen();

            // can't impl for type 'type', so always return false
            if (type == CheezType.Type)
                return (null, false);

            if (impl.IsPolymorphic)
            {
                if (!CheezType.TypesMatch(impl.TargetType, type))
                    return (null, false);

                var poliesList = new List<Dictionary<string, CheezType>>();
                {
                    var polies = new Dictionary<string, CheezType>();
                    CollectPolyTypes(impl.TargetType, type, polies);
                    poliesList.Add(polies);
                }

                // @TODO: check conditions
                if (impl.Conditions != null)
                {
                    foreach (var cond in impl.Conditions)
                    {
                        var newPoliesList = new List<Dictionary<string, CheezType>>();
                        foreach (var polies in poliesList)
                        {
                            switch (cond)
                            {
                                case ImplConditionImplTrait c:
                                    //foreach (var match in CheckIfConditionApplies(c, polies))
                                    //    newPoliesList.Add(match);
                                    newPoliesList.AddRange(CheckIfConditionApplies(c, polies));
                                    break;

                                case ImplConditionNotYet c:
                                    {
                                        var targetType = InstantiatePolyType(impl.TargetType, polies, c.Location);
                                        var traitType = InstantiatePolyType(impl.Trait, polies, c.Location);
                                        var impls = GetImplsForType(targetType, traitType);
                                        if (impls.Count == 0)
                                            newPoliesList.Add(polies);
                                        break;
                                    }

                                case ImplConditionAny a:
                                    {
                                        var expr = a.Expr.Clone();
                                        expr.AttachTo(impl, new Scope("temp", impl.Scope));

                                        foreach (var p in polies)
                                            expr.Scope.DefineTypeSymbol(p.Key, p.Value);

                                        expr = InferType(expr, CheezType.Bool);

                                        if (!expr.IsCompTimeValue)
                                        {
                                            ReportError(a.Location, $"Expression must be a compile time constant of type bool");
                                        }
                                        else
                                        {
                                            bool val = (bool)expr.Value;
                                            if (val)
                                                newPoliesList.Add(polies);
                                        }

                                        break;
                                    }
                                default: throw new NotImplementedException();
                            }
                        }
                        poliesList = newPoliesList;
                    }
                }

                if (poliesList.Count == 0)
                    return (null, true);

                var result = poliesList.Select(polies =>
                {
                    if (impl.Parameters.Count != polies.Count)
                    {
                        // @TODO: provide location
                        ReportError("failed to infer all impl parameters");
                        return null;
                    }

                    return InstantiatePolyImplNew(impl, polies);
                }).Where(it => it != null).ToList();
                return (result, false);
            }
            else
            {
                return CheezType.TypesMatch(impl.TargetType, type) ? (new List<AstImplBlock> { impl }, false) : (null, false);
            }
        }

        private List<Dictionary<string, CheezType>> GetTraitImplForType(CheezType type, CheezType trait, Dictionary<string, CheezType> polies)
        {
            if (m_typeImplMap.TryGetValue(type, out var _list))
            {
                var result = new List<Dictionary<string, CheezType>>();

                foreach (var impl in _list.impls)
                {
                    if (CheezType.TypesMatch(impl.Trait, trait))
                    {
                        var p = new Dictionary<string, CheezType>(polies);
                        CollectPolyTypes(trait, impl.Trait, p);
                        result.Add(p);
                    }
                }

                return result;
            }
            else if (type.IsPolyType)
            {
                var result = new List<Dictionary<string, CheezType>>();

                foreach (var kv in m_typeImplMap)
                {
                    if (!CheezType.TypesMatch(type, kv.Key))
                        continue;

                    foreach (var impl in kv.Value.impls)
                    {
                        if (impl.Trait == trait)
                        {
                            var p = new Dictionary<string, CheezType>(polies);
                            CollectPolyTypes(type, kv.Key, p);
                            result.Add(p);
                        }
                    }
                }

                return result;
            }

            return new List<Dictionary<string, CheezType>>();
        }

        private List<AstImplBlock> GetImplsForType(CheezType type, CheezType trait = null)
        {
            var impls = GetImplsForTypeHelper(type);
            if (trait != null)
                return impls.Where(i => i.Trait == trait).ToList();
            return impls.ToList();
        }

        private HashSet<AstImplBlock> GetImplsForTypeHelper(CheezType type)
        {
            if (type.IsErrorType)
                WellThatsNotSupposedToHappen();

            if (m_typeImplMap == null)
                UpdateTypeImplMap(GlobalScope);

            if (m_typeImplMap.TryGetValue(type, out var _list))
                return _list.impls;

            m_typeImplMap[type] = new TypeImplList(GlobalScope.Impls);

            UpdateTypeImplMap(GlobalScope);

            return m_typeImplMap[type].impls;
        }

        private void UpdateTypeImplMap(Scope scope)
        {
            if (m_typeImplMap == null)
            {
                m_typeImplMap = new Dictionary<CheezType, TypeImplList>();
                foreach (var td in scope.Typedefs)
                {
                    if (td.Type.IsErrorType)
                        continue;
                    if (!m_typeImplMap.ContainsKey(td.Type))
                        m_typeImplMap[td.Type] = new TypeImplList(scope.Impls);
                }

                foreach (var td in scope.StructDeclarations)
                {
                    if (td.Type.IsErrorType)
                        continue;
                    if (!td.IsPolymorphic && !m_typeImplMap.ContainsKey(td.Type))
                        m_typeImplMap[td.Type] = new TypeImplList(scope.Impls);
                }

                foreach (var td in scope.EnumDeclarations)
                {
                    if (td.Type.IsErrorType)
                        continue;
                    if (!td.IsPolymorphic && !m_typeImplMap.ContainsKey(td.Type))
                        m_typeImplMap[td.Type] = new TypeImplList(scope.Impls);
                }

                foreach (var td in scope.TraitDeclarations)
                {
                    if (td.Type.IsErrorType)
                        continue;
                    if (!td.IsPolymorphic && !m_typeImplMap.ContainsKey(td.Type))
                        m_typeImplMap[td.Type] = new TypeImplList(scope.Impls);
                }

                foreach (var td in scope.Impls)
                {
                    if (td.TargetType?.IsErrorType ?? true)
                        continue;
                    if (!td.IsPolymorphic && td.TargetType != null && !m_typeImplMap.ContainsKey(td.TargetType))
                        m_typeImplMap[td.TargetType] = new TypeImplList(scope.Impls);
                }
            }

            var changes = true;
            while (changes)
            {
                changes = false;

                var mapCopy = new Dictionary<CheezType, TypeImplList>(m_typeImplMap);

                foreach (var kv in mapCopy)
                {
                    var type = kv.Key;
                    var lists = kv.Value;

                    foreach (var impl in lists.potentialImpls)
                    {
                        var (concreteImpls, maybeApplies) = ImplAppliesToType(impl, type);
                        if (concreteImpls != null)
                        {
                            foreach (var concreteImpl in concreteImpls)
                                lists.impls.Add(concreteImpl);
                            changes = true;
                        }
                        else if (maybeApplies)
                        {
                            lists.temp.Add(impl);
                        }
                    }


                    lists.potentialImpls.Clear();

                    // swap lists
                    var tmpList = lists.temp;
                    lists.temp = lists.potentialImpls;
                    lists.potentialImpls = tmpList;
                }
            }
        }

        private void ResolveDeclarations(Scope scope, List<AstStatement> statements)
        {
            // sort all declarations into different lists
            InsertDeclarationsIntoScope(scope, statements);

            // go through all type declarations (structs, traits, enums, typedefs) and define them in the scope
            // go through all constant declarations and define them in the scope
            DefineTypeDeclarations(scope);

            // go through all type declarations again and build the dependencies
            BuildDependencies(scope);

            // check for cyclic dependencies and resolve types of typedefs and constant variables
            ResolveMissingTypesOfDeclarations(scope);

            // compute types of struct members, enum members, trait members
            ComputeTypeMembers(scope);

            // resolve impls (check if is polymorphic, setup scopes, check for self params in functions, etc.)
            foreach (var impl in scope.Impls)
            {
                if (impl.TraitExpr == null)
                    Pass3Impl(impl);
                else
                    Pass3TraitImpl(impl);
            }

            // go through all type declarations and connect impls to types
            UpdateTypeImplMap(GlobalScope);


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

            foreach (var impl in scope.Impls)
                scope.unresolvedImpls.Enqueue(impl);

            while (scope.unresolvedImpls.Count > 0)
            {
                var i = scope.unresolvedImpls.Dequeue();

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
                    ResolveMissingTypesOfDeclarationsHelper(scope, dep, whiteSet, greySet, chain);
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
                            v.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
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

                        v.Initializer.SetFlag(ExprFlags.ValueRequired, true);
                        v.Initializer = InferType(v.Initializer, type);
                        ConvertLiteralTypeToDefaultType(v.Initializer, type);

                        if (v.Initializer.Type.IsErrorType)
                        {
                            Console.WriteLine(v.Pattern);
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
                        func.ConstScope = new Scope($"fn$ {func.Name.Name}", func.Scope);
                        func.SubScope = new Scope($"fn {func.Name.Name}", func.ConstScope);
                        ResolveFunctionSignature(func, newPolyDecls);
                        break;
                    }

                case AstTypeAliasDecl typedef:
                    {
                        typedef.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                        typedef.TypeExpr = ResolveType(typedef.TypeExpr, newPolyDecls, out var type);
                        typedef.Type = type;
                        break;
                    }

                case AstStructDecl @struct when @struct.IsPolymorphic:
                    {
                        foreach (var p in @struct.Parameters)
                        {
                            p.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
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
                            p.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
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
                            p.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
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
        }

        private void ResolveMissingTypesOfDeclarations(Scope scope)
        {
            var whiteSet = new HashSet<AstDecl>();
            var greySet = new HashSet<AstDecl>();
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
                ResolveMissingTypesOfDeclarationsHelper(scope, x, whiteSet, greySet, chain);
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
            foreach (var v in scope.Impls)
                Pass1Impl(v);
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
