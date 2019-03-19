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
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        public AstExpression ResolveType(AstExpression expr, TypeInferenceContext context, out CheezType type)
        {
            expr = InferTypeHelper(expr, CheezType.Type, context);
            return ResolveTypeHelper(expr, out type);
        }

        public AstExpression ResolveType(AstExpression expr, out CheezType type, bool poly_from_scope = false, HashSet<AstDecl> dependencies = null)
        {
            expr = InferType(expr, CheezType.Type, poly_from_scope, dependencies);
            return ResolveTypeHelper(expr, out type);
        }

        public AstExpression ResolveTypeHelper(AstExpression expr, out CheezType type)
        {
            expr = InferType(expr, CheezType.Type);
            if (expr.Type != CheezType.Type)
            {
                ReportError(expr, $"Expected type");
                type = CheezType.Error;
                return expr;
            }
            else
            {
                type = expr.Value as CheezType;
            }
            return expr;
        }

        public static void CollectPolyTypes(CheezType param, CheezType arg, Dictionary<string, CheezType> result)
        {
            switch (param)
            {
                case PolyType i:
                    if (i.IsDeclaring && !result.ContainsKey(i.Name))
                    {
                        if (arg is ReferenceType r)
                            result[i.Name] = r.TargetType;
                        else
                            result[i.Name] = arg;
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
                                    CollectPolyTypes(str.Arguments[i], tt.Arguments[i], result);
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

                case IntType _: break;
            }
        }

        // struct
        private AstStructDecl InstantiatePolyStruct(AstStructDecl decl, List<(CheezType type, object value)> args, List<AstStructDecl> instances = null, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstStructDecl instance = null;

            // check if instance already exists
            foreach (var pi in decl.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == args.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = args[i];
                    if (param.Value != arg.value)
                    {
                        eq = false;
                        break;
                    }
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
                instance = decl.Clone() as AstStructDecl;
                instance.SubScope = new Scope($"struct {decl.Name.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                instance.Template = decl;
                decl.PolymorphicInstances.Add(instance);
                instance.Scope.TypeDeclarations.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;
                }

                instance.Type = new StructType(instance);

                if (instances != null)
                    instances.Add(instance);
                else
                {
                    ResolveStructs(new List<AstStructDecl> { instance });
                    ResolveStructMembers(instance);
                }
            }

            return instance;
        }

        private void ResolveStructMembers(List<AstStructDecl> @structs)
        {
            foreach (var s in @structs)
            {
                ResolveStructMembers(s);
            }
        }

        private void ResolveStructMembers(AstStructDecl @struct)
        {
            foreach (var member in @struct.Members)
            {
                if (member.Initializer == null)
                {
                    member.Initializer = new AstDefaultExpr(member.Name.Location);
                }

                member.Initializer = InferType(member.Initializer, member.Type);
                ConvertLiteralTypeToDefaultType(member.Initializer, member.Type);
                member.Initializer = Cast(member.Initializer, member.Type);

                if (member.Initializer.Type.IsErrorType)
                    continue;
            }
        }

        private void ResolveStruct(AstStructDecl @struct, List<AstStructDecl> instances = null)
        {
            // define parameter types
            foreach (var p in @struct.Parameters)
            {
                @struct.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }

            // resolve member types
            int index = 0;
            foreach (var member in @struct.Members)
            {
                member.Index = index++;
                member.TypeExpr.Scope = @struct.SubScope;
                member.TypeExpr = ResolveType(member.TypeExpr, out var t);
                member.Type = t;
            }

            ((StructType)@struct.Type).CalculateSize();
        }

        private void ResolveStructs(List<AstStructDecl> newInstances)
        {
            var nextInstances = new List<AstStructDecl>();

            int i = 0;
            while (i < MaxPolyStructResolveStepCount && newInstances.Count != 0)
            {
                foreach (var instance in newInstances)
                {
                    ResolveStruct(instance, nextInstances);
                }
                newInstances.Clear();

                var t = newInstances;
                newInstances = nextInstances;
                nextInstances = t;

                i++;
            }

            if (i == MaxPolyStructResolveStepCount)
            {
                var details = newInstances.Select(str => ("Here:", str.Location)).ToList();
                ReportError($"Detected a potential infinite loop in polymorphic struct declarations after {MaxPolyStructResolveStepCount} steps", details);
            }
        }

        // impl
        //private AstStructDecl InstantiatePolyImpl(AstImplBlock impl, List<AstImplBlock> instances = null)
        //{
        //    var target = impl.TargetType;

        //    //if (expr.Arguments.Count != @struct.Declaration.Parameters.Count)
        //    //{
        //    //    ReportError(expr, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", @struct.Declaration));
        //    //    return null;
        //    //}

        //    // check if instance already exists
        //    AstStructDecl instance = null;
        //    //foreach (var pi in @struct.Declaration.PolymorphicInstances)
        //    //{
        //    //    Debug.Assert(pi.Parameters.Count == expr.Arguments.Count);

        //    //    bool eq = true;
        //    //    for (int i = 0; i < pi.Parameters.Count; i++)
        //    //    {
        //    //        var param = pi.Parameters[i];
        //    //        var ptype = param.Type;
        //    //        var pvalue = param.Value;

        //    //        var arg = expr.Arguments[i];
        //    //        var atype = arg.Type;
        //    //        var avalue = arg.Value;

        //    //        if (pvalue != avalue)
        //    //        {
        //    //            eq = false;
        //    //            break;
        //    //        }
        //    //    }

        //    //    if (eq)
        //    //    {
        //    //        instance = pi;
        //    //        break;
        //    //    }
        //    //}

        //    // instatiate type
        //    if (instance == null)
        //    {
        //        instance = impl.Clone() as AstImplBlock;
        //        instance.SubScope = new Scope($"impl {impl.TargetTypeExpr}<poly>", instance.Scope);
        //        impl..PolymorphicInstances.Add(instance);

        //        Debug.Assert(instance.Parameters.Count == expr.Arguments.Count);

        //        for (int i = 0; i < instance.Parameters.Count; i++)
        //        {
        //            var param = instance.Parameters[i];
        //            var arg = expr.Arguments[i];
        //            param.Type = arg.Type;
        //            param.Value = arg.Value;
        //        }

        //        instance.Type = new StructType(instance);

        //        if (instances != null)
        //            instances.Add(instance);
        //    }

        //    return instance;
        //}

        private AstFunctionDecl InstantiatePolyFunction(
            GenericFunctionType func,
            Dictionary<string, CheezType> polyTypes,
            Dictionary<string, (CheezType type, object value)> constArgs,
            List<AstFunctionDecl> instances = null,
            ILocation location = null)
        {
            AstFunctionDecl instance = null;

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

                    }
                    foreach (var pt in polyTypes)
                    {
                        if (!(pi.PolymorphicTypes.TryGetValue(pt.Key, out var t) && t == pt.Value))
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
                instance = func.Declaration.Clone() as AstFunctionDecl;
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.PolymorphicTypes = polyTypes;
                instance.ConstParameters = constArgs;
                instance.ImplBlock = func.Declaration.ImplBlock;
                instance.SelfType = func.Declaration.SelfType;
                instance.SelfParameter = func.Declaration.SelfParameter;
                func.Declaration.PolymorphicInstances.Add(instance);

                if (instance.ImplBlock != null)
                {
                    var targetType = instance.ImplBlock.TargetType;
                    var inst = InstantiatePolyType(targetType, polyTypes, location);
                    instance.ConstScope.DefineTypeSymbol("Self", inst);
                }

                instance.Scope.FunctionDeclarations.Add(instance);

                foreach (var pt in constArgs)
                {
                    instance.ConstScope.DefineConstant(pt.Key, pt.Value.type, pt.Value.value);
                }

                foreach (var pt in polyTypes)
                {
                    instance.ConstScope.DefineTypeSymbol(pt.Key, pt.Value);
                }

                foreach (var p in instance.Parameters)
                {
                    p.Scope = instance.SubScope;
                    p.TypeExpr.Scope = p.Scope;
                    p.TypeExpr = ResolveType(p.TypeExpr, out var t, true);
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
                                case CheezTypeType _:
                                case BoolType _:
                                case CharType _:
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
                if (instance.ReturnValue != null)
                {
                    instance.ReturnValue.Scope = instance.SubScope;
                    instance.ReturnValue.TypeExpr.Scope = instance.SubScope;
                    instance.ReturnValue.TypeExpr = ResolveType(instance.ReturnValue.TypeExpr, out var t, true);
                    instance.ReturnValue.Type = t;
                }

                //remove constant params
                instance.Parameters = instance.Parameters.Where(p => !(p.Name?.IsPolymorphic ?? false)).ToList();

                instance.Type = new FunctionType(instance);

                if (instances != null)
                    instances.Add(instance);
            }

            return instance;
        }

        private CheezType InstantiatePolyType(CheezType poly, Dictionary<string, CheezType> concreteTypes, ILocation location)
        {
            if (!poly.IsPolyType)
                return poly;

            switch (poly)
            {
                case PolyType p:
                    if (concreteTypes.TryGetValue(p.Name, out var t)) return t;
                    return p;

                case StructType s:
                    {
                        if (s.Declaration.Template != null)
                            throw new Exception("must be null");
                        var args = s.Declaration.Parameters.Select(p => (p.Type, (object)concreteTypes[p.Name.Name])).ToList();
                        var instance = InstantiatePolyStruct(s.Declaration, args, location: location);
                        return instance.Type;
                    }

                case PointerType p:
                    return PointerType.GetPointerType(InstantiatePolyType(p.TargetType, concreteTypes, location));

                default: throw new NotImplementedException();
            }
        }
    }
}
