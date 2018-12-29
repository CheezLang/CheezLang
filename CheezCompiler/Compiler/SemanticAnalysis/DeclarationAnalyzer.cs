using Cheez.Compiler.Ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        public CheezType ResolveType(AstTypeExpr typeExpr, HashSet<AstDecl> deps = null)
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
                        var subType = ResolveType(p.Target, deps);
                        return PointerType.GetPointerType(subType);
                    }

                case AstSliceTypeExpr a:
                    {
                        a.Target.Scope = typeExpr.Scope;
                        var subType = ResolveType(a.Target, deps);
                        return SliceType.GetSliceType(subType);
                    }

                case AstArrayTypeExpr arr:
                    {
                        arr.Target.Scope = typeExpr.Scope;
                        var subType = ResolveType(arr.Target, deps);

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
                            returnType = ResolveType(func.ReturnType, deps);
                        }

                        CheezType[] par = new CheezType[func.ParameterTypes.Count];
                        for (int i = 0; i < par.Length; i++) {
                            func.ParameterTypes[i].Scope = func.Scope;
                            par[i] = ResolveType(func.ParameterTypes[i], deps);
                        }

                        return FunctionType.GetFunctionType(returnType, par);
                    }

                case AstPolyStructTypeExpr @struct:
                    {
                        @struct.Struct.Scope = @struct.Scope;
                        @struct.Struct.Type = CheezType.Type;
                        @struct.Struct.Value = ResolveType(@struct.Struct, deps);

                        foreach (var arg in @struct.Arguments)
                        {
                            arg.Scope = @struct.Scope;
                            arg.Type = CheezType.Type;
                            arg.Value = ResolveType(arg, deps);
                        }

                        // instantiate struct
                        var instance = InstantiatePolyStruct(@struct);
                        return instance?.Type ?? CheezType.Error;
                    }
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
        }

        private AstStructDecl InstantiatePolyStruct(AstPolyStructTypeExpr expr)
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
            }

            return instance;
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
        }
    }
}
