using Cheez.Compiler.Ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        public CheezType ResolveType(AstTypeExpr typeExpr, HashSet<AstDecl> deps = null, List<AstStructDecl> instances = null)
        {
            switch (typeExpr)
            {
                case AstIdTypeExpr i:
                    {
                        if (i.IsPolymorphic)
                            return new PolyType(i.Name);

                        var sym = typeExpr.Scope.GetSymbol(i.Name, false);

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
                        var subType = ResolveType(p.Target, deps, instances);
                        return PointerType.GetPointerType(subType);
                    }

                case AstSliceTypeExpr a:
                    {
                        a.Target.Scope = typeExpr.Scope;
                        var subType = ResolveType(a.Target, deps, instances);
                        return SliceType.GetSliceType(subType);
                    }

                case AstArrayTypeExpr arr:
                    {
                        arr.Target.Scope = typeExpr.Scope;
                        var subType = ResolveType(arr.Target, deps, instances);

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
                            returnType = ResolveType(func.ReturnType, deps, instances);
                        }

                        CheezType[] par = new CheezType[func.ParameterTypes.Count];
                        for (int i = 0; i < par.Length; i++) {
                            func.ParameterTypes[i].Scope = func.Scope;
                            par[i] = ResolveType(func.ParameterTypes[i], deps, instances);
                        }

                        return FunctionType.GetFunctionType(returnType, par);
                    }

                case AstPolyStructTypeExpr @struct:
                    {
                        @struct.Struct.Scope = @struct.Scope;
                        @struct.Struct.Type = CheezType.Type;
                        @struct.Struct.Value = ResolveType(@struct.Struct, deps, instances);

                        foreach (var arg in @struct.Arguments)
                        {
                            arg.Scope = @struct.Scope;
                            arg.Type = CheezType.Type;
                            arg.Value = ResolveType(arg, deps, instances);
                        }

                        // instantiate struct
                        var instance = InstantiatePolyStruct(@struct, instances);
                        return instance?.Type ?? CheezType.Error;
                    }
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
        }

        private AstStructDecl InstantiatePolyStruct(AstPolyStructTypeExpr expr, List<AstStructDecl> instances = null)
        {
            var @struct = expr.Struct.Value as GenericStructType;

            if (expr.Arguments.Count != @struct.Declaration.Parameters.Count)
            {
                ReportError(expr, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", @struct.Declaration));
                return null;
            }

            // check if instance already exists
            AstStructDecl instance = null;
            foreach (var pi in @struct.Declaration.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == expr.Arguments.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var ptype = param.Type;
                    var pvalue = param.Value;

                    var arg = expr.Arguments[i];
                    var atype = arg.Type;
                    var avalue = arg.Value;

                    if (pvalue != avalue)
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

        public void ResolveStruct(AstStructDecl @struct, List<AstStructDecl> instances = null)
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
                member.Type = ResolveType(member.TypeExpr, instances: instances);
            }
        }

        public void ResolveStructs(List<AstStructDecl> newInstances)
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
    }
}

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    public partial class DeclarationAnalyzer
    {
        private Workspace mWorkspace;

        private List<AstStructDecl> mPolyStructs = new List<AstStructDecl>();
        private List<AstStructDecl> mPolyStructInstances = new List<AstStructDecl>();
        private List<AstStructDecl> mStructs = new List<AstStructDecl>();

        private List<AstTraitDeclaration> mTraits = new List<AstTraitDeclaration>();
        private List<AstEnumDecl> mEnums = new List<AstEnumDecl>();
        private List<AstVariableDecl> mVariables = new List<AstVariableDecl>();
        private List<AstTypeAliasDecl> mTypeDefs = new List<AstTypeAliasDecl>();
        private List<AstImplBlock> mImpls = new List<AstImplBlock>();

        private List<AstFunctionDecl> mFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mPolyFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mFunctionInstances = new List<AstFunctionDecl>();

        public void CollectDeclarations(Workspace ws)
        {
            mWorkspace = ws;

            // pass 1: collect types (structs, enums, traits, simple typedefs)
            Pass1();

            // pass 2: resolve types
            Pass2();

            Pass3();
        }
    }
}
