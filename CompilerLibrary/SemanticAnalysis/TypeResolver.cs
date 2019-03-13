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
                        var subType = ResolveTypeHelper(p.Target, deps, instances, poly_from_scope);
                        return PointerType.GetPointerType(subType);
                    }

                case AstSliceTypeExpr a:
                    {
                        a.Target.Scope = typeExpr.Scope;
                        var subType = ResolveTypeHelper(a.Target, deps, instances, poly_from_scope);
                        return SliceType.GetSliceType(subType);
                    }

                case AstArrayTypeExpr arr:
                    {
                        arr.Target.Scope = typeExpr.Scope;
                        var subType = ResolveTypeHelper(arr.Target, deps, instances, poly_from_scope);

                        arr.SizeExpr.Scope = typeExpr.Scope;
                        arr.SizeExpr = InferType(arr.SizeExpr, IntType.DefaultType);

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
                            par[i].type = ResolveTypeHelper(func.ParameterTypes[i], deps, instances, poly_from_scope);
                        }

                        CheezType ret = CheezType.Void;

                        if (func.ReturnType != null)
                        {
                            func.ReturnType.Scope = func.Scope;
                            ret = ResolveTypeHelper(func.ReturnType, deps, instances, poly_from_scope);
                        }

                        return new FunctionType(par, ret);
                    }

                case AstPolyStructTypeExpr @struct:
                    {
                        @struct.Struct.Scope = @struct.Scope;
                        @struct.Struct.Type = CheezType.Type;
                        @struct.Struct.Value = ResolveTypeHelper(@struct.Struct, deps, instances, poly_from_scope);

                        var strType = @struct.Struct.Value as GenericStructType;

                        if (strType == null)
                        {
                            ReportError(@struct.Struct, $"This type must be a poly struct type but is '{@struct.Struct.Value}'");
                            return CheezType.Error;
                        }

                        bool anyArgIsPoly = false;

                        foreach (var arg in @struct.Arguments)
                        {
                            arg.Scope = @struct.Scope;
                            arg.Type = CheezType.Type;
                            var argType = ResolveTypeHelper(arg, deps, instances, poly_from_scope);
                            arg.Value = argType;

                            if (argType.IsPolyType) anyArgIsPoly = true;
                        }


                        if (anyArgIsPoly)
                        {
                            return new StructType(strType.Declaration, @struct.Arguments.Select(a => a.Value as CheezType).ToArray());
                        }

                        // instantiate struct
                        var args = @struct.Arguments.Select(a => (a.Type, a.Value)).ToList();
                        var instance = InstantiatePolyStruct(strType.Declaration, args, instances, @struct);
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
                            m.Type = ResolveTypeHelper(m.TypeExpr, deps, instances, poly_from_scope);

                            members[i] = (m.Name?.Name, m.Type);
                        }

                        return TupleType.GetTuple(members);
                    }

                case AstExprTypeExpr expr:
                    expr.Expression.Scope = expr.Scope;
                    expr.Expression = InferType(expr.Expression, CheezType.Type);

                    if (expr.Expression.Type == CheezType.Error) return CheezType.Error;

                    if (expr.Expression.Type != CheezType.Type)
                    {
                        ReportError(expr, $"Expected a type, got '{expr.Type}'");
                        return CheezType.Error;
                    }

                    Debug.Assert(expr.Expression.Value != null);
                    return expr.Expression.Value as CheezType;
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
        }

        private void CollectPolyTypes(AstExpression expr, CheezType param, CheezType arg, Dictionary<string, CheezType> result)
        {
            switch (param)
            {
                case PolyType i:
                    if (i.IsDeclaring)
                        result[i.Name] = arg;
                    break;

                case PointerType p:
                    {
                        if (arg is PointerType t)
                            CollectPolyTypes(expr, p.TargetType, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{param}'");
                        break;
                    }

                case SliceType p:
                    {
                        if (arg is SliceType t)
                            CollectPolyTypes(expr, p.TargetType, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{param}'");
                        break;
                    }

                case ArrayType p:
                    {
                        if (arg is ArrayType t)
                            CollectPolyTypes(expr, p.TargetType, t.TargetType, result);
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{param}'");
                        break;
                    }

                case TupleType te:
                    {
                        if (arg is TupleType tt)
                        {
                            if (te.Members.Length != tt.Members.Length)
                            {
                                ReportError(expr, $"The type of the expression does not match the tuple pattern '{param}'");
                            }
                            else
                            {
                                for (var i = 0; i < te.Members.Length; i++)
                                {
                                    CollectPolyTypes(expr, te.Members[i].type, tt.Members[i].type, result);
                                }
                            }
                        }
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{param}'");
                        break;
                    }

                case StructType str:
                    {
                        if (arg is StructType tt)
                        {
                            if (str.Arguments.Length != tt.Arguments.Length)
                            {
                                ReportError(expr, $"The type of the expression does not match the struct pattern '{param}'");
                            }
                            else
                            {
                                for (var i = 0; i < str.Arguments.Length; i++)
                                {
                                    CollectPolyTypes(expr, str.Arguments[i], tt.Arguments[i], result);
                                }
                            }
                        }
                        else
                            ReportError(expr, $"The type of the expression does not match the type pattern '{param}'");
                        break;
                    }

                //default: throw new NotImplementedException();
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
                    ResolveStructs(new List<AstStructDecl> { instance });
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
                instance.RefSelf = func.Declaration.RefSelf;
                instance.SelfParameter = func.Declaration.SelfParameter;
                func.Declaration.PolymorphicInstances.Add(instance);

                if (instance.SelfParameter)
                {
                    var targetType = instance.ImplBlock.TargetType;
                    var inst = InstantiatePolyType(targetType, polyTypes, location);
                    instance.ConstScope.DefineTypeSymbol("self", inst);
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
                    p.Type = ResolveType(p.TypeExpr, true);
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

                                case CheezType t when t == CheezType.String || t == CheezType.CString:
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
                    //instance.ReturnValue.Type = InstantiatePolyType(instance.ReturnValue.Type, polyTypes);
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
