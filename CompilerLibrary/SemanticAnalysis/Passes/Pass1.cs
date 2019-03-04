using System;
using System.Collections.Generic;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;

namespace Cheez
{
    public partial class Workspace
    {
        // for semantic analysis
        private List<AstStructDecl> mPolyStructs = new List<AstStructDecl>();
        private List<AstStructDecl> mPolyStructInstances = new List<AstStructDecl>();
        private List<AstStructDecl> mStructs = new List<AstStructDecl>();

        private List<AstTraitDeclaration> mTraits = new List<AstTraitDeclaration>();
        private List<AstEnumDecl> mEnums = new List<AstEnumDecl>();
        private List<AstVariableDecl> mVariables = new List<AstVariableDecl>();
        private List<AstTypeAliasDecl> mTypeDefs = new List<AstTypeAliasDecl>();
        private List<AstImplBlock> mAllImpls = new List<AstImplBlock>();
        private List<AstImplBlock> mTraitImpls = new List<AstImplBlock>();
        private List<AstImplBlock> mPolyImpls = new List<AstImplBlock>();
        private List<AstImplBlock> mImpls = new List<AstImplBlock>();

        private List<AstFunctionDecl> mFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mPolyFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mFunctionInstances = new List<AstFunctionDecl>();
        //

        /// <summary>
        /// pass 1:
        /// collect types (structs, enums, traits)
        /// </summary>
        private void Pass1()
        {
            foreach (var s in Statements)
            {
                switch (s)
                {
                    case AstStructDecl @struct:
                        {
                            @struct.Scope = GlobalScope;
                            Pass1StructDeclaration(@struct);

                            if (@struct.Parameters.Count > 0)
                                mPolyStructs.Add(@struct);
                            else
                                mStructs.Add(@struct);
                            break;
                        }

                    case AstTraitDeclaration @trait:
                        {
                            trait.Scope = GlobalScope;
                            Pass1TraitDeclaration(@trait);
                            mTraits.Add(@trait);
                            break;
                        }

                    case AstEnumDecl @enum:
                        {
                            @enum.Scope = GlobalScope;
                            Pass1EnumDeclaration(@enum);
                            mEnums.Add(@enum);
                            break;
                        }

                    case AstVariableDecl @var:
                        {
                            @var.Scope = GlobalScope;
                            mVariables.Add(@var);
                            Pass1VariableDeclaration(@var);
                            break;
                        }

                    case AstFunctionDecl func:
                        {
                            func.Scope = GlobalScope;
                            func.ConstScope = new Scope("$", func.Scope);
                            func.SubScope = new Scope("fn", func.ConstScope);
                            mFunctions.Add(func);
                            break;
                        }

                    case AstImplBlock impl:
                        {
                            impl.Scope = GlobalScope;
                            impl.SubScope = new Scope($"impl", impl.Scope);
                            if (impl.TraitExpr != null) mTraitImpls.Add(impl);
                            else mAllImpls.Add(impl);
                            break;
                        }

                    case AstTypeAliasDecl type:
                        {
                            type.Scope = GlobalScope;
                            mTypeDefs.Add(type);
                            break;
                        }
                }
            }

            // typedefs
            foreach (var t in mTypeDefs)
            {
                Pass1TypeAlias(t);
            }
        }

        private void Pass1VariableDeclaration(AstVariableDecl var)
        {
            if (var.Initializer != null)
                var.Initializer.Scope = var.Scope;

            MatchPatternWithTypeExpr(var, var.Pattern, var.TypeExpr, var.Initializer);

            foreach (var decl in var.SubDeclarations)
            {
                var res = var.Scope.DefineSymbol(decl);
                if (!res.ok)
                {
                    (string, ILocation)? detail = null;
                    if (res.other != null) detail = ("Other declaration here:", res.other);
                    ReportError(decl.Name, $"A symbol with name '{decl.Name.Name}' already exists in current scope", detail);
                }
                else
                {
                    var.Scope.VariableDeclarations.Add(var);
                }
            }
        }

        private void MatchPatternWithTypeExpr(AstVariableDecl parent, AstExpression pattern, AstTypeExpr type, AstExpression initializer)
        {
            if (pattern is AstIdExpr id)
            {
                var decl = new AstSingleVariableDecl(id, type, parent, pattern);
                decl.Type = new VarDeclType(decl);
                decl.Initializer = initializer;
                parent.SubDeclarations.Add(decl);
                id.Symbol = decl;
            }
            else if (pattern is AstTupleExpr tuple)
            {
                AstTupleTypeExpr tupleType = type as AstTupleTypeExpr;

                for (int i = 0; i < tuple.Values.Count; i++)
                {
                    var tid = tuple.Values[i];
                    var tty = (i < tupleType?.Members?.Count) ? tupleType.Members[i] : null;
                    AstExpression tin = null;

                    if (initializer is AstTupleExpr tupleInit)
                    {
                        tin = tupleInit.Values[i];
                    }
                    else if (initializer != null)
                    {
                        tin = new AstArrayAccessExpr(initializer, new AstNumberExpr(new Extras.NumberData(i)));
                        tin.Scope = initializer.Scope;
                    }

                    MatchPatternWithTypeExpr(parent, tid, tty?.TypeExpr, tin);
                }
            }
            else
            {
                ReportError(pattern, $"This pattern is not valid here");
            }
        }

        private void Pass1TraitDeclaration(AstTraitDeclaration trait)
        {
            trait.Scope.TypeDeclarations.Add(trait);
            trait.Type = new TraitType(trait);

            var res = trait.Scope.DefineDeclaration(trait);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(trait.Name, $"A symbol with name '{trait.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1EnumDeclaration(AstEnumDecl @enum)
        {
            @enum.Scope.TypeDeclarations.Add(@enum);
            @enum.Type = new EnumType(@enum);


            var res = @enum.Scope.DefineDeclaration(@enum);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(@enum.Name, $"A symbol with name '{@enum.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1StructDeclaration(AstStructDecl @struct)
        {
            if (@struct.Parameters.Count > 0)
            {
                @struct.IsPolymorphic = true;
                @struct.Type = new GenericStructType(@struct);
                @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);
            }
            else
            {
                @struct.Scope.TypeDeclarations.Add(@struct);
                @struct.Type = new StructType(@struct);
                @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);
            }

            var res = @struct.Scope.DefineDeclaration(@struct);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1TypeAlias(AstTypeAliasDecl alias)
        {
            alias.TypeExpr.Scope = alias.Scope;
            alias.Type = new AliasType(alias);

            var res = alias.Scope.DefineDeclaration(alias);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(alias.Name, $"A symbol with name '{alias.Name.Name}' already exists in current scope", detail);
            }
        }
    }
}
