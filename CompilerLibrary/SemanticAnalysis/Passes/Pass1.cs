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
        private List<AstVariableDecl> mVariables = new List<AstVariableDecl>();

        private List<AstFuncExpr> mFunctions = new List<AstFuncExpr>();
        private List<AstUsingStmt> mGlobalUses = new List<AstUsingStmt>();

        public IEnumerable<AstFuncExpr> Functions => mFunctions;
        public IEnumerable<AstVariableDecl> Variables => mVariables;

        public IEnumerable<AstTraitDeclaration> Traits => mTraits;
        //

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

        private static void Pass1Impl(AstImplBlock impl)
        {
            impl.TargetTypeExpr.Scope = impl.SubScope;

            // check if there are parameters
            if (impl.Parameters != null)
            {
                impl.IsPolymorphic = true;
            }
        }
    }
}
