using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
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

        /// <summary>
        /// Returns a list of all impl blocks which apply to a given type
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="trait"></param>
        /// <returns></returns>
        private List<AstImplBlock> GetImplsForType(CheezType type, CheezType trait = null)
        {
            var impls = GetImplsForTypeHelper(type);
            if (trait != null)
                return impls.Where(i => i.Trait == trait).ToList();
            return impls.ToList();
        }

        #region Helper Methods

        private HashSet<AstImplBlock> GetImplsForTypeHelper(CheezType type)
        {
            if (type.IsErrorType)
                WellThatsNotSupposedToHappen();

            if (m_typeImplMap == null)
                UpdateTypeImplMap();

            if (m_typeImplMap.TryGetValue(type, out var _list))
                return _list.impls;

            m_typeImplMap[type] = new TypeImplList(mAllImpls);

            UpdateTypeImplMap();

            return m_typeImplMap[type].impls;
        }

        private void UpdateTypeImplMap()
        {
            if (m_typeImplMap == null)
            {
                m_typeImplMap = new Dictionary<CheezType, TypeImplList>();

                foreach (var td in mAllImpls)
                {
                    if (td.TargetType?.IsErrorType ?? true)
                        continue;
                    if (!td.IsPolymorphic && td.TargetType != null && !m_typeImplMap.ContainsKey(td.TargetType))
                        m_typeImplMap[td.TargetType] = new TypeImplList(mAllImpls);
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
            //if (type == CheezType.Type)
            //    return (null, false);

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

        #endregion
    }
}
