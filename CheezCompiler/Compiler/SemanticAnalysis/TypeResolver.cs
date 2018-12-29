using Cheez.Compiler.Ast;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        private CheezType ResolveType(AstTypeExpr typeExpr)
        {
            List<AstStructDecl> newInstances = new List<AstStructDecl>();
            var t = ResolveTypeHelper(typeExpr, null, newInstances);
            ResolveStructs(newInstances);
            return t;
        }

        private CheezType ResolveTypeHelper(AstTypeExpr typeExpr, HashSet<AstDecl> deps = null, List<AstStructDecl> instances = null)
        {
            switch (typeExpr)
            {
                case AstErrorTypeExpr _:
                    return CheezType.Error;

                case AstIdTypeExpr i:
                    {
                        if (i.IsPolymorphic)
                            return new PolyType(i.Name);

                        var sym = typeExpr.Scope.GetSymbol(i.Name);

                        if (sym == null)
                        {
                            ReportError(typeExpr, $"Unknown symbol");
                            return CheezType.Error;
                        }

                        if (sym is CompTimeVariable c && c.Type == CheezType.Type)
                        {
                            var type = c.Value as CheezType;
                            if (deps != null && c.Declaration != null && type is AbstractType)
                                deps.Add(c.Declaration);
                            return type;
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

                        if (arr.SizeExpr is AstNumberExpr num && num.Data.Type == Parsing.NumberData.NumberType.Int)
                        {
                            int v = (int)num.Data.IntValue;
                            // TODO: check size of num.Data.IntValue
                            return ArrayType.GetArrayType(subType, v);
                        }
                        ReportError(arr.SizeExpr, "Index must be a constant int");
                        return CheezType.Error;
                    }

                case AstFunctionTypeExpr func:
                    {
                        CheezType returnType = CheezType.Void;
                        if (func.ReturnType != null)
                        {
                            func.ReturnType.Scope = func.Scope;
                            returnType = ResolveTypeHelper(func.ReturnType, deps, instances);
                        }

                        CheezType[] par = new CheezType[func.ParameterTypes.Count];
                        for (int i = 0; i < par.Length; i++) {
                            func.ParameterTypes[i].Scope = func.Scope;
                            par[i] = ResolveTypeHelper(func.ParameterTypes[i], deps, instances);
                        }

                        return FunctionType.GetFunctionType(returnType, par);
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
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
        }

        private void CollectPolyTypes(AstTypeExpr typeExpr, HashSet<AstIdTypeExpr> types)
        {
            switch (typeExpr)
            {
                case AstIdTypeExpr i:
                    if (i.IsPolymorphic)
                        types.Add(i);
                    break;

                case AstPointerTypeExpr p:
                    CollectPolyTypes(p.Target, types);
                    break;

                case AstSliceTypeExpr p:
                    CollectPolyTypes(p.Target, types);
                    break;

                case AstArrayTypeExpr p:
                    CollectPolyTypes(p.Target, types);
                    break;

                case AstFunctionTypeExpr func:
                    if (func.ReturnType != null)
                        CollectPolyTypes(func.ReturnType, types);
                    foreach (var p in func.ParameterTypes) CollectPolyTypes(p, types);
                    break;

                case AstPolyStructTypeExpr @struct:
                    foreach (var p in @struct.Arguments) CollectPolyTypes(p, types);
                    break;
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


        private void AnalyzeFunction(AstFunctionDecl func, List<AstFunctionDecl> instances = null)
        {
            // TODO
        }

        private void AnalyzeFunctions(List<AstFunctionDecl> newInstances)
        {
            var nextInstances = new List<AstFunctionDecl>();

            int i = 0;
            while (i < MaxPolyFuncResolveStepCount && newInstances.Count != 0)
            {
                foreach (var instance in newInstances)
                {
                    AnalyzeFunction(instance, nextInstances);
                }
                newInstances.Clear();

                var t = newInstances;
                newInstances = nextInstances;
                nextInstances = t;

                i++;
            }

            if (i == MaxPolyFuncResolveStepCount)
            {
                var details = newInstances.Select(str => ("Here:", str.Location)).ToList();
                ReportError($"Detected a potential infinite loop in polymorphic function declarations after {MaxPolyFuncResolveStepCount} steps", details);
            }
        }
    }
}
