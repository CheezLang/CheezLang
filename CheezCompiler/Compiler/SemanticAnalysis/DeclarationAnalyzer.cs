using Cheez.Compiler.Ast;
using System;
using System.Collections.Generic;

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

                //case AstCallExpr call:
                //{
                //    throw new NotImplementedException();
                //    // call.Function.Scope = call.Scope;
                //    // var subType = ResolveType(call.Function);
                //    // if (subType is PolyStructType @struct)
                //    // {
                //    //     return ResolvePolyStructType(call, @struct);
                //    // }
                //    // return CheezType.Error;
                //}
            }

            ReportError(typeExpr, $"Expected type");
            return CheezType.Error;
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
