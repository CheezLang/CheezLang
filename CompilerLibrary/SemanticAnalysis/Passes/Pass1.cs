using System;
using System.Collections.Generic;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;

namespace Cheez
{
    public partial class Workspace
    {
        // for semantic analysis
        private List<AstTraitDeclaration> mTraits = new List<AstTraitDeclaration>();
        public List<AstVariableDecl> mVariables = new List<AstVariableDecl>();
        private List<AstTypeAliasDecl> mTypeDefs = new List<AstTypeAliasDecl>();
        public List<AstImplBlock> mImpls = new List<AstImplBlock>();

        public List<AstFunctionDecl> mFunctions = new List<AstFunctionDecl>();
        private List<AstUsingStmt> mGlobalUses = new List<AstUsingStmt>();

        public IEnumerable<AstTraitDeclaration> Traits => mTraits;
        //

        private void Pass1FunctionDeclaration(AstFunctionDecl func)
        {
            var polyNames = new List<string>();
            foreach (var p in func.Parameters)
            {
                CollectPolyTypeNames(p.TypeExpr, polyNames);
                if (p.Name != null)
                    CollectPolyTypeNames(p.Name, polyNames);
            }

            if (func.ReturnTypeExpr != null)
            {
                CollectPolyTypeNames(func.ReturnTypeExpr.TypeExpr, polyNames);
            }

            //if (polyNames.Count > 0)
            //{
            //    func.Type = new GenericFunctionType(func);
            //}
            //else
            //{
            //    func.Type = new FunctionType(func);

            //    if (func.TryGetDirective("varargs", out var varargs))
            //    {
            //        if (varargs.Arguments.Count != 0)
            //        {
            //            ReportError(varargs, $"#varargs takes no arguments!");
            //        }
            //        func.FunctionType.VarArgs = true;
            //    }
            //}

            var res = func.Scope.DefineDeclaration(func);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(func.Name, $"A symbol with name '{func.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1VariableDeclaration(AstVariableDecl var)
        {
            if (var.Initializer == null)
            {
                if (var.GetFlag(StmtFlags.GlobalScope))
                {
                    ReportError(var, $"Global variables must have an initializer");
                    var.Initializer = new AstDefaultExpr(var.Pattern.Location);
                }
            }
            else
            {
                var.Initializer.AttachTo(var);
            }

            if (var.TypeExpr != null)
            {
                var.TypeExpr.AttachTo(var);
            }
            else if (var.GetFlag(StmtFlags.GlobalScope))
            {
                ReportError(var, $"Global variables must have a type annotation");
            }
            MatchPatternWithTypeExpr(var, var.Pattern, var.TypeExpr);

            foreach (var decl in var.SubDeclarations)
            {
                var res = var.Scope.DefineSymbol(decl);

                if (!res.ok)
                {
                    (string, ILocation)? detail = null;
                    if (res.other != null) detail = ("Other declaration here:", res.other);
                    ReportError(decl.Name, $"A symbol with name '{decl.Name.Name}' already exists in current scope", detail);
                }
            }
        }

        private void MatchPatternWithTypeExpr(AstVariableDecl parent, AstExpression pattern, AstExpression type)
        {
            if (pattern is AstIdExpr id)
            {

                var decl = new AstSingleVariableDecl(id, type, parent, pattern);
                decl.Scope = parent.Scope;
                decl.Type = new VarDeclType(decl);
                decl.SetFlag(StmtFlags.GlobalScope, parent.GetFlag(StmtFlags.GlobalScope));
                parent.SubDeclarations.Add(decl);
                id.Symbol = decl;
            }
            else if (pattern is AstTupleExpr tuple)
            {
                AstTupleExpr tupleType = type as AstTupleExpr;

                for (int i = 0; i < tuple.Values.Count; i++)
                {
                    AstExpression tid = tuple.Types[i].Name;
                    var tty = tuple.Types?[i];

                    if (tid == null)
                    {
                        tid = tuple.Values[i];
                        tty = (i < tupleType?.Types?.Count) ? tupleType.Types[i] : null;
                    }
                    else
                    {
                        tuple.Values[i] = tid;
                    }

                    MatchPatternWithTypeExpr(parent, tid, tty?.TypeExpr);
                }
            }
            else
            {
                ReportError(pattern, $"This pattern is not valid here");
            }
        }

        private void Pass1TraitDeclaration(AstTraitDeclaration trait)
        {
            if (trait.Parameters.Count > 0)
            {
                trait.IsPolymorphic = true;
                trait.Type = new GenericTraitType(trait);

                foreach (var p in trait.Parameters)
                {
                    p.Scope = trait.Scope;
                    p.TypeExpr.Scope = trait.Scope;
                }
            }
            else
            {
                trait.Type = new TraitType(trait);
            }
            trait.SubScope = new Scope($"trait {trait.Name.Name}", trait.Scope);

            var res = trait.Scope.DefineDeclaration(trait);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(trait.Name, $"A symbol with name '{trait.Name.Name}' already exists in current scope", detail);
            }
        }

        //private void Pass1EnumDeclaration(AstEnumDecl @enum)
        //{
        //    if (@enum.Parameters?.Count > 0)
        //    {
        //        @enum.IsPolymorphic = true;
        //        @enum.Type = new GenericEnumType(@enum, @enum.Name.Name);

        //        foreach (var p in @enum.Parameters)
        //        {
        //            p.Scope = @enum.Scope;
        //            p.TypeExpr.Scope = @enum.Scope;
        //        }
        //    }
        //    else
        //    {
        //        @enum.Type = new EnumType(@enum);
        //    }
        //    @enum.SubScope = new Scope($"enum {@enum.Name.Name}", @enum.Scope);


        //    var res = @enum.Scope.DefineDeclaration(@enum);
        //    if (!res.ok)
        //    {
        //        (string, ILocation)? detail = null;
        //        if (res.other != null) detail = ("Other declaration here:", res.other);
        //        ReportError(@enum.Name, $"A symbol with name '{@enum.Name.Name}' already exists in current scope", detail);
        //    }
        //}

        private void Pass1Impl(AstImplBlock impl)
        {
            impl.TargetTypeExpr.Scope = impl.SubScope;

            // check if there are parameters
            if (impl.Parameters != null)
            {
                impl.IsPolymorphic = true;
            }
        }

        //private void Pass1StructDeclaration(AstStructDecl @struct)
        //{
        //    ReportError(@struct, $"Not supported anymore, please use new syntax");
        //    return;

        //    if (@struct.HasDirective("copy"))
        //    {
        //        @struct.SetFlag(StmtFlags.IsCopy);
        //    }

        //    if (@struct.Parameters.Count > 0)
        //    {
        //        @struct.IsPolymorphic = true;
        //        //@struct.Type = new GenericStructType(@struct);

        //        foreach (var p in @struct.Parameters)
        //        {
        //            p.Scope = @struct.Scope;
        //            p.TypeExpr.Scope = @struct.Scope;
        //        }
        //    }
        //    else
        //    {
        //        //@struct.Type = new StructType(@struct);
        //    }
        //    @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);

        //    var res = @struct.Scope.DefineDeclaration(@struct);
        //    if (!res.ok)
        //    {
        //        (string, ILocation)? detail = null;
        //        if (res.other != null) detail = ("Other declaration here:", res.other);
        //        ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope", detail);
        //    }
        //}

        private void Pass1Typedef(AstTypeAliasDecl alias)
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
