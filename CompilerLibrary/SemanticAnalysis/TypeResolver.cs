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
        private CheezType ResolveType(AstTypeExpr typeExpr, bool poly_from_scope = false)
        {
            List<AstStructDecl> newInstances = new List<AstStructDecl>();
            var t = ResolveTypeHelper(typeExpr, null, newInstances, poly_from_scope);
            ResolveStructs(newInstances);
            return t;
        }

        private CheezType ResolveTypeHelper(AstTypeExpr typeExpr, HashSet<AstDecl> deps = null, List<AstStructDecl> instances = null, bool poly_from_scope = false)
        {
            switch (typeExpr)
            {
                case AstErrorTypeExpr _:
                    return CheezType.Error;

                case AstIdTypeExpr i:
                    {
                        if (i.IsPolymorphic && !poly_from_scope)
                            return new PolyType(i.Name, true);

                        var sym = typeExpr.Scope.GetSymbol(i.Name);

                        if (sym == null)
                        {
                            ReportError(typeExpr, $"Unknown symbol '{i.Name}'");
                            return CheezType.Error;
                        }

                        if (sym is TypeSymbol ts)
                        {
                            return ts.Type;
                        }
                        else if (sym is AstDecl decl)
                        {
                            if (deps != null && decl.Type is AbstractType)
                                deps.Add(decl);
                            return decl.Type;
                        }
                        else if (sym is ConstSymbol s && s.Type == CheezType.Type)
                        {
                            return s.Value as CheezType;
                        }
                        else
                        {
                            ReportError(typeExpr, $"'{typeExpr}' is not a valid type");
                        }

                        break;
                    }

                case AstPointerTypeExpr p:
                    {
                        p.Target.Scope = typeExpr.Scope;
                        var subType = ResolveTypeHelper(p.Target, deps, instances);
                        return PointerType.GetPointerType(subType);
                    }

                case AstSliceTypeExpr a:
                    {
                        a.Target.Scope = typeExpr.Scope;
                        var subType = ResolveTypeHelper(a.Target, deps, instances);
                        return SliceType.GetSliceType(subType);
                    }

                case AstArrayTypeExpr arr:
                    {
                        arr.Target.Scope = typeExpr.Scope;
                        var subType = ResolveTypeHelper(arr.Target, deps, instances);

                        arr.SizeExpr.Scope = typeExpr.Scope;
                        InferType(arr.SizeExpr, IntType.DefaultType);

                        if (arr.SizeExpr.Value == null || !(arr.SizeExpr.Type is IntType))
                        {
                            ReportError(arr.SizeExpr, "Index must be a constant int");
                        }
                        else
                        {
                            int v = (int)((NumberData)arr.SizeExpr.Value).IntValue;
                            // TODO: check size of num.Data.IntValue
                            return ArrayType.GetArrayType(subType, v);
                        }
                        return CheezType.Error;
                    }

                case AstFunctionTypeExpr func:
                    {
                        (string name, CheezType type)[] par = new (string, CheezType)[func.ParameterTypes.Count];
                        for (int i = 0; i < par.Length; i++) {
                            func.ParameterTypes[i].Scope = func.Scope;
                            par[i].type = ResolveTypeHelper(func.ParameterTypes[i], deps, instances);
                        }

                        CheezType ret = CheezType.Void;

                        if (func.ReturnType != null)
                        {
                            func.ReturnType.Scope = func.Scope;
                            ret = ResolveTypeHelper(func.ReturnType, deps, instances);
                        }

                        return new FunctionType(par, ret);
                    }

                case AstPolyStructTypeExpr @struct:
                    {
                        @struct.Struct.Scope = @struct.Scope;
                        @struct.Struct.Type = CheezType.Type;
                        @struct.Struct.Value = ResolveTypeHelper(@struct.Struct, deps, instances);

                        foreach (var arg in @struct.Arguments)
                        {
                            arg.Scope = @struct.Scope;
                            arg.Type = CheezType.Type;
                            arg.Value = ResolveTypeHelper(arg, deps, instances);
                        }

                        // instantiate struct
                        var instance = InstantiatePolyStruct(@struct, instances);
                        return instance?.Type ?? CheezType.Error;
                    }

                case AstTupleTypeExpr tuple:
                    {
                        var members = new(string name, CheezType type)[tuple.Members.Count];
                        for (int i = 0; i < members.Length; i++)
                        {
                            var m = tuple.Members[i];
                            m.Scope = tuple.Scope;
                            m.TypeExpr.Scope = tuple.Scope;
                            m.Type = ResolveTypeHelper(m.TypeExpr, deps, instances);

                            members[i] = (m.Name?.Name, m.Type);
                        }

                        return TupleType.GetTuple(members);
                    }
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
        }

        private void CollectPolyTypes(AstExpression expr, AstTypeExpr typeExpr, CheezType type, Dictionary<string, CheezType> result)
        {
            switch (typeExpr)
            {
                case AstIdTypeExpr i:
                    if (i.IsPolymorphic)
                        result[i.Name] = type;
                    break;

                case AstPointerTypeExpr p:
                    {
                        if (type is PointerType t)
                            CollectPolyTypes(expr, p.Target, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{typeExpr}'");
                        break;
                    }

                case AstSliceTypeExpr p:
                    {
                        if (type is SliceType t)
                            CollectPolyTypes(expr, p.Target, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{typeExpr}'");
                        break;
                    }

                case AstArrayTypeExpr p:
                    {
                        if (type is ArrayType t)
                            CollectPolyTypes(expr, p.Target, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{typeExpr}'");
                        break;
                    }

                default: throw new NotImplementedException();
                //case AstFunctionTypeExpr func:
                //    if (func.ReturnType != null) CollectPolyTypes(func.ReturnType, types);
                //    foreach (var p in func.ParameterTypes) CollectPolyTypes(p, types);
                //    break;

                //case AstPolyStructTypeExpr @struct:
                //    foreach (var p in @struct.Arguments) CollectPolyTypes(p, types);
                //    break;
            }
        }

        // struct
        private AstStructDecl InstantiatePolyStruct(AstPolyStructTypeExpr expr, List<AstStructDecl> instances = null)
        {
            var @struct = expr.Struct.Value as GenericStructType;

            if (expr.Arguments.Count != @struct.Declaration.Parameters.Count)
            {
                ReportError(expr, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", @struct.Declaration));
                return null;
            }

            AstStructDecl instance = null;

            // check if instance already exists
            foreach (var pi in @struct.Declaration.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == expr.Arguments.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = expr.Arguments[i];
                    if (param.Value != arg.Value)
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
                instance = @struct.Declaration.Clone() as AstStructDecl;
                instance.SubScope = new Scope($"struct {@struct.Declaration.Name.Name}<poly>", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                @struct.Declaration.PolymorphicInstances.Add(instance);
                instance.Scope.TypeDeclarations.Add(instance);

                Debug.Assert(instance.Parameters.Count == expr.Arguments.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = expr.Arguments[i];
                    param.Type = arg.Type;
                    param.Value = arg.Value;
                }

                instance.Type = new StructType(instance);

                if (instances != null)
                    instances.Add(instance);
            }

            return instance;
        }

        private void ResolveStruct(AstStructDecl @struct, List<AstStructDecl> instances = null)
        {
            // define parameter types
            foreach (var p in @struct.Parameters)
            {
                @struct.SubScope.DefineTypeSymbol(p.Name.Name, p.Value as CheezType);
            }

            // resolve member types
            foreach (var member in @struct.Members)
            {
                member.TypeExpr.Scope = @struct.SubScope;
                member.Type = ResolveTypeHelper(member.TypeExpr, instances: instances);
            }
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
            List<AstFunctionDecl> instances = null)
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
                func.Declaration.PolymorphicInstances.Add(instance);

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
                    p.Type = ResolveType(p.TypeExpr, true);

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
                    instance.ReturnValue.Type = ResolveType(instance.ReturnValue.TypeExpr, true);
                }

                //remove constant params
                instance.Parameters = instance.Parameters.Where(p => !(p.Name?.IsPolymorphic ?? false)).ToList();

                instance.Type = new FunctionType(instance);

                if (instances != null)
                    instances.Add(instance);
            }

            return instance;
        }
    }
}
