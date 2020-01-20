using Cheez.Ast;
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
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private AstExpression ResolveType(AstExpression expr, TypeInferenceContext context, out CheezType type)
        {
            expr = InferTypeHelper(expr, CheezType.Type, context);
            return ResolveTypeHelper(expr, out type);
        }

        private AstExpression ResolveType(AstExpression expr, List<AstDecl> newPolyDecls, out CheezType type)
        {
            expr = InferTypeHelper(expr, CheezType.Type, new TypeInferenceContext
            {
                newPolyDeclarations = newPolyDecls
            });
            return ResolveTypeHelper(expr, out type);
        }

        private AstExpression ResolveTypeNow(AstExpression expr, out CheezType type, bool resolvePolyExprToConcreteType = false, HashSet<AstDecl> dependencies = null, bool forceInfer = false)
        {
            expr = InferType(expr, CheezType.Type, resolvePolyExprToConcreteType, dependencies, forceInfer);
            return ResolveTypeHelper(expr, out type);
        }

        private AstExpression ResolveTypeHelper(AstExpression expr, out CheezType type)
        {
            expr = InferType(expr, CheezType.Type);
            if (expr.Type.IsErrorType)
            {
                type = CheezType.Error;
                expr.Value = CheezType.Error;
                return expr;
            }

            if (expr.Type != CheezType.Type)
            {
                ReportError(expr, $"Expected type");
                type = CheezType.Error;
                expr.Value = CheezType.Error;
                return expr;
            }
            else
            {
                type = expr.Value as CheezType;
            }
            return expr;
        }

        public static void CollectPolyTypes(object param, object arg, Dictionary<string, (CheezType type, object value)> result)
        {
            switch (param)
            {
                case PolyValue p:
                    {
                        if (!result.ContainsKey(p.Name))
                        {
                            result[p.Name] = arg switch
                            {
                                NumberData v when v.Type == NumberData.NumberType.Float => (FloatType.LiteralType, v),
                                NumberData v when v.Type == NumberData.NumberType.Int => (IntType.LiteralType, v),
                                string v => (StringType.StringLiteral, v),
                                bool v => (CheezType.Bool, v),
                                char v => (CharType.LiteralType, v),
                            };
                        }
                        break;
                    }

                case PolyType i:
                    if (i.IsDeclaring && !result.ContainsKey(i.Name))
                    {
                        if (arg is ReferenceType r)
                            result[i.Name] = (CheezType.Type, r.TargetType);
                        else if (arg is CheezType)
                            result[i.Name] = (CheezType.Type, arg);
                        else
                        {
                            result[i.Name] = arg switch
                            {
                                NumberData v when v.Type == NumberData.NumberType.Float => (FloatType.LiteralType, v),
                                NumberData v when v.Type == NumberData.NumberType.Int => (IntType.LiteralType, v),
                                string v => (StringType.StringLiteral, v),
                                bool v => (CheezType.Bool, v),
                                char v => (CharType.LiteralType, v),
                            };
                        }
                    }
                    break;

                case PointerType p:
                    {
                        if (arg is PointerType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        break;
                    }

                case SliceType p:
                    {
                        if (arg is SliceType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        if (arg is ArrayType a)
                            CollectPolyTypes(p.TargetType, a.TargetType, result);
                        break;
                    }

                case ArrayType p:
                    {
                        if (arg is ArrayType t)
                            CollectPolyTypes(p.TargetType, t.TargetType, result);
                        break;
                    }

                case TupleType te:
                    {
                        if (arg is TupleType tt)
                        {
                            if (te.Members.Length == tt.Members.Length)
                            {
                                for (var i = 0; i < te.Members.Length; i++)
                                {
                                    CollectPolyTypes(te.Members[i].type, tt.Members[i].type, result);
                                }
                            }
                        }
                        break;
                    }

                case StructType str:
                    {
                        if (arg is StructType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    //if (str.Arguments[i] is CheezType a && tt.Arguments[i] is CheezType b)
                                        CollectPolyTypes(str.Arguments[i], tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case GenericStructType type:
                    {
                        if (arg is StructType tt)
                        {
                            if (type.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < type.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(type.Arguments[i].value, tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case GenericEnumType type:
                    {
                        if (arg is EnumType tt)
                        {
                            if (type.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < type.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(type.Arguments[i].value, tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case GenericTraitType type:
                    {
                        if (arg is TraitType tt)
                        {
                            if (type.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < type.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(type.Arguments[i].value, tt.Arguments[i], result);
                                }
                            }
                        }
                        break;
                    }

                case TraitType str:
                    {
                        if (arg is TraitType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(str.Arguments[i] as CheezType, tt.Arguments[i] as CheezType, result);
                                }
                            }
                        }
                        break;
                    }

                case EnumType str:
                    {
                        if (arg is EnumType tt)
                        {
                            if (str.Arguments.Length == tt.Arguments.Length)
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(str.Arguments[i] as CheezType, tt.Arguments[i] as CheezType, result);
                                }
                            }
                        }
                        break;
                    }

                case ReferenceType r:
                    {
                        if (arg is ReferenceType r2)
                            CollectPolyTypes(r.TargetType, r2.TargetType, result);
                        else
                            CollectPolyTypes(r.TargetType, arg, result);
                        break;
                    }

                case FunctionType f:
                    {
                        if (arg is FunctionType f2 && f.Parameters.Length == f2.Parameters.Length)
                        {
                            for (int i = 0; i < f.Parameters.Length; i++)
                            {
                                CollectPolyTypes(f.Parameters[i].type, f2.Parameters[i].type, result);
                            }

                            if (f.ReturnType != null && f2.ReturnType != null)
                                CollectPolyTypes(f.ReturnType, f2.ReturnType, result);
                        }
                        break;
                    }

                case RangeType r:
                    {
                        if (arg is RangeType r2)
                            CollectPolyTypes(r.TargetType, r2.TargetType, result);
                        break;
                    }

                case VoidType _:
                case FloatType _:
                case BoolType _:
                case CheezTypeType _:
                case CharType _:
                case StringType _:
                case IntType _: break;
                case CodeType _: break;

                case ErrorType _: break;

                case CheezType t:
                    throw new NotImplementedException();
            }
        }

        // impl
        private AstImplBlock InstantiatePolyImplNew(AstImplBlock decl, Dictionary<string, (CheezType type, object value)> args, ILocation location = null)
        {
            if (decl.Trait?.ToString() == "Iterator")
            {

            }

            AstImplBlock instance = null;

            // check for existing instance
            var concreteTrait = decl.Trait != null ? InstantiatePolyType(decl.Trait, args, location) : null;
            var concreteTarget = InstantiatePolyType(decl.TargetType, args, location);

            foreach (var pi in decl.PolyInstances)
            {
                if (pi.Trait == concreteTrait && pi.TargetType == concreteTarget)
                {
                    instance = pi;
                    break;
                }
            }

            if (instance == null)
            {
                instance = decl.Clone() as AstImplBlock;
                instance.SubScope = new Scope($"impl <poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Conditions = null;
                instance.Template = decl;
                decl.PolyInstances.Add(instance);

                if (instance.Parameters == null)
                {

                }
                Debug.Assert((instance.Parameters?.Count ?? 0) == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[param.Name.Name];
                    param.Type = CheezType.Type;
                    param.Value = arg;
                }

                foreach (var kv in args)
                {
                    instance.SubScope.DefineConstant(kv.Key, kv.Value.type, kv.Value.value);
                }


                if (instance.TraitExpr != null)
                    Pass3TraitImpl(instance);
                else
                    Pass3Impl(instance);

                // @TODO: does this work?
                mUnresolvedImpls.Enqueue(instance);
            }

            return instance;
        }


        // struct
        private AstFuncExpr InstantiatePolyImplFunction(
            GenericFunctionType func,
            Dictionary<string, (CheezType type, object value)> polyTypes,
            ILocation location = null)
        {
            var impl = func.Declaration.ImplBlock;
            var implInstance = InstantiatePolyImplNew(impl, polyTypes, location);
            return implInstance.Functions.FirstOrDefault(f => f.Name == func.Declaration.Name);
        }

        private AstFuncExpr InstantiatePolyFunction(
            GenericFunctionType func,
            Dictionary<string, (CheezType type, object value)> polyTypes,
            Dictionary<string, (CheezType type, object value)> constArgs,
            List<AstFuncExpr> instances = null,
            ILocation location = null)
        {
            AstFuncExpr instance = null;

            if (func.Declaration.TraitFunction != null && func.Declaration.ImplBlock != null && func.Declaration.ImplBlock.Trait != null)
            {
                return InstantiatePolyImplFunction(func, polyTypes, location);
            }

            // check if instance already exists
            foreach (var pi in func.Declaration.PolymorphicInstances)
            {
                bool eq = true;

                if (pi.ConstParameters.Count == constArgs.Count && pi.PolymorphicTypes.Count == polyTypes.Count)
                {
                    foreach (var ca in constArgs)
                    {
                        if (!(pi.ConstParameters.TryGetValue(ca.Key, out var a) && a.type == ca.Value.type && a.value.Equals(ca.Value.value)))
                        {
                            eq = false;
                            break;
                        }
                        else if (a.value is TupleType t1 && ca.Value.value is TupleType t2 && t1.Members.Length == t2.Members.Length)
                        {
                            bool e = true;
                            for (int i = 0; i < t1.Members.Length; i++)
                            {
                                if (t1.Members[i].name != t2.Members[i].name)
                                {
                                    e = false;
                                    break;
                                }
                            }
                            if (!e)
                            {
                                eq = false;
                                break;
                            }
                        }
                    }
                    foreach (var pt in polyTypes)
                    {
                        if (!(pi.PolymorphicTypes.TryGetValue(pt.Key, out var t) && t.value == pt.Value.value))
                        {
                            eq = false;
                            break;
                        }
                    }
                }
                else
                {
                    eq = false;
                }
                
                if (eq)
                {
                    instance = pi;
                    break;
                }
            }

            // instatiate type
            if (instance == null)
            {
                instance = func.Declaration.Clone() as AstFuncExpr;
                instance.Template = func.Declaration;
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.PolymorphicTypes = polyTypes;
                instance.ConstParameters = constArgs;
                instance.ImplBlock = func.Declaration.ImplBlock;
                instance.SelfType = func.Declaration.SelfType;
                func.Declaration.PolymorphicInstances.Add(instance);

                if (instance.ImplBlock != null)
                {
                    var targetType = instance.ImplBlock.TargetType;
                    var inst = InstantiatePolyType(targetType, polyTypes, location) as CheezType;
                    instance.ConstScope.DefineTypeSymbol("Self", inst);
                }

                foreach (var pt in constArgs)
                    instance.ConstScope.DefineConstant(pt.Key, pt.Value.type, pt.Value.value);
                foreach (var pt in polyTypes)
                    instance.ConstScope.DefineConstant(pt.Key, pt.Value.type, pt.Value.value);

                foreach (var p in instance.Parameters)
                {
                    p.Scope = instance.SubScope;
                    p.TypeExpr.Scope = p.Scope;
                    p.TypeExpr = ResolveTypeNow(p.TypeExpr, out var t, true);
                    p.Type = t;
                    //p.Type = InstantiatePolyType(p.Type, polyTypes);

                    if (p.Name?.IsPolymorphic ?? false)
                    {
                        var (type, value) = constArgs[p.Name.Name];
                        if (type != p.Type)
                        {
                            ReportError(p, $"Type of const argument ({type}) does not match type of parameter ({p.Type})");
                        }
                        else
                        {
                            switch (p.Type)
                            {
                                case IntType _:
                                case FloatType _:
                                case BoolType _:
                                case CharType _:
                                case CheezTypeType _:
                                case CodeType _:
                                    break;

                                case CheezType tt when tt == CheezType.String || tt == CheezType.CString:
                                    break;

                                case ErrorType _:
                                    break;

                                default:
                                    ReportError(p.TypeExpr, $"The type '{p.Type}' is not allowed here.");
                                    break;
                            }
                        }
                    }
                }

                // return types
                //if (instance.ReturnTypeExpr != null)
                //{
                //    instance.ReturnTypeExpr.Scope = instance.SubScope;
                //    instance.ReturnTypeExpr.TypeExpr.Scope = instance.SubScope;
                //    instance.ReturnTypeExpr.TypeExpr = ResolveTypeNow(instance.ReturnTypeExpr.TypeExpr, out var t, true);
                //    instance.ReturnTypeExpr.Type = t;
                //}

                //remove constant params
                instance.AddInstantiatedAt(location, currentFunction);
                instance.Parameters = instance.Parameters.Where(p => !(p.Name?.IsPolymorphic ?? false)).ToList();
                instance = InferTypeFuncExpr(instance) as AstFuncExpr;

                if (instances != null)
                    instances.Add(instance);
            }
            else
            {
                instance.AddInstantiatedAt(location, currentFunction);
            }

            return instance;
        }

        private object InstantiatePolyType(object poly, Dictionary<string, (CheezType type, object value)> concreteTypes, ILocation location)
        {
            if (poly is CheezType ttt && !ttt.IsPolyType)
                return poly;

            switch (poly)
            {
                case PolyValue p:
                    if (concreteTypes.TryGetValue(p.Name, out var t1)) return t1.value;
                    return p;

                case PolyType p:
                    if (concreteTypes.TryGetValue(p.Name, out var t)) return t.value;
                    return p;

                case TupleType tuple:
                    {
                        var args = tuple.Members.Select(m => (m.name, InstantiatePolyType(m.type, concreteTypes, location) as CheezType));
                        return TupleType.GetTuple(args.ToArray());
                    }

                case StructType s:
                    {
                        //if (s.Declaration.Template != null)
                        //    throw new Exception("must be null");
                        var args = s.Arguments.Select(a => (CheezType.Type, InstantiatePolyType(a, concreteTypes, location))).ToList();
                        //var args = s.Declaration.Parameters.Select(p => (p.Type, (object)concreteTypes[p.Name.Name])).ToList();
                        var instance = InstantiatePolyStruct(s.Declaration, args, location: location);
                        return instance.StructType;
                    }

                case GenericStructType s:
                    {
                        var args = s.Arguments.Select(a => (CheezType.Type, InstantiatePolyType(a.value, concreteTypes, location))).ToList();
                        //var args = s.Declaration.Parameters.Select(p => (p.Type, (object)concreteTypes[p.Name.Name])).ToList();
                        var instance = InstantiatePolyStruct(s.Declaration, args, location: location);
                        return instance.StructType;
                    }

                case TraitType s:
                    {
                        //if (s.Declaration.Template != null)
                        //    throw new Exception("must be null");

                        if (s.Arguments.Count() != s.Declaration.Parameters.Count())
                            throw new Exception("argument count must match");

                        var args = s.Arguments.Select(a => InstantiatePolyType(a, concreteTypes, location)).ToList();
                        var zipped = args.Zip(s.Declaration.Parameters, (type, param) => (param.Type, (object)type)).ToList();

                        var instance = InstantiatePolyTrait(s.Declaration, zipped, location: location);
                        return instance.TraitType;
                    }

                case EnumType s:
                    {
                        //if (s.Declaration.Template != null)
                        //    throw new Exception("must be null");
                        var args = s.Declaration.Parameters.Select(p => (p.Type, concreteTypes[p.Name.Name].value)).ToList();
                        var instance = InstantiatePolyEnum(s.Declaration, args, location: location);
                        return instance.EnumType;
                    }

                case PointerType p:
                    return PointerType.GetPointerType(InstantiatePolyType(p.TargetType, concreteTypes, location) as CheezType);

                case ReferenceType p:
                    return ReferenceType.GetRefType(InstantiatePolyType(p.TargetType, concreteTypes, location) as CheezType);

                default: throw new NotImplementedException();
            }
        }


        // type expressions
        private void ComputeTypeMembers(CheezType type)
        {
            switch (type)
            {
                case StructType s:
                    ComputeStructMembers(s.Declaration);
                    break;
                case EnumType e:
                    ComputeEnumMembers(e.Declaration);
                    break;

                case TraitType t:
                    ComputeTraitMembers(t.Declaration);
                    break;
            }
        }

    }
}
