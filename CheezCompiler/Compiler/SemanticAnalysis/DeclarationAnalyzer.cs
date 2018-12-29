using Cheez.Compiler.Ast;
using System;
using System.Collections.Generic;

namespace Cheez.Compiler
{
    public partial class Workspace
    {
        public CheezType ResolveType(AstExpression typeExpr)
        {
            switch (typeExpr)
            {
            case AstIdExpr i:
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
                    var t = c.Value as CheezType;
                    return t;
                }
                else
                {
                    ReportError(typeExpr, $"'{typeExpr}' is not a valid type");
                }

                break;
            }

            case AstAmpersandExpr p:
            {
                p.SubExpression.Scope = typeExpr.Scope;
                var subType = ResolveType(p.SubExpression);
                return PointerType.GetPointerType(subType);
            }

            case AstSliceTypeExpr a:
            {
                a.Target.Scope = typeExpr.Scope;
                var subType = ResolveType(a.Target);
                return SliceType.GetSliceType(subType);
            }

            case AstArrayAccessExpr arr:
            {
                throw new NotImplementedException();
                // arr.SubExpression.Scope = typeExpr.Scope;
                // arr.Indexer.Scope = typeExpr.Scope;
                // var subType = ResolveType(arr.SubExpression);
                // var index = ResolveConstantExpression(arr.Indexer);

                // if (index.type is IntType)
                // {
                //     long v = (long)index.value;
                //     return ArrayType.GetArrayType(subType, (int)v);
                // }
                // ReportError(arr.Indexer, "Index must be a constant int");
                // return CheezType.Error;
            }

            case AstCallExpr call:
            {
                throw new NotImplementedException();
                // call.Function.Scope = call.Scope;
                // var subType = ResolveType(call.Function);
                // if (subType is PolyStructType @struct)
                // {
                //     return ResolvePolyStructType(call, @struct);
                // }
                // return CheezType.Error;
            }
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

            // pass 2: poly struct parameter types
            Pass2();

            // pass 3: rest of typedefs
            Pass3();
        }
    }
}
