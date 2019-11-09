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
        private List<AstConstantDeclaration> mAllGlobalConstants = new List<AstConstantDeclaration>();
        private List<AstTraitTypeExpr> mAllTraits = new List<AstTraitTypeExpr>();
        private List<AstStructTypeExpr> mAllStructs = new List<AstStructTypeExpr>();
        private List<AstEnumTypeExpr> mAllEnums = new List<AstEnumTypeExpr>();
        private List<AstFuncExpr> mAllFunctions = new List<AstFuncExpr>();
        private List<AstVariableDecl> mAllGlobalVariables = new List<AstVariableDecl>();
        private List<AstUsingStmt> mAllGlobalUses = new List<AstUsingStmt>();
        private List<AstImplBlock> mAllImpls = new List<AstImplBlock>();

        private Queue<AstImplBlock> mUnresolvedImpls = new Queue<AstImplBlock>();
        private Queue<AstStructTypeExpr> mUnresolvedStructs = new Queue<AstStructTypeExpr>();
        private Queue<AstEnumTypeExpr> mUnresolvedEnums = new Queue<AstEnumTypeExpr>();
        private Queue<AstTraitTypeExpr> mUnresolvedTraits = new Queue<AstTraitTypeExpr>();
        private Queue<AstFuncExpr> mUnresolvedFunctions = new Queue<AstFuncExpr>();


        public IEnumerable<AstFuncExpr> Functions => mAllFunctions;
        public IEnumerable<AstVariableDecl> Variables => mAllGlobalVariables;
        public IEnumerable<AstTraitTypeExpr> Traits => mAllTraits;
        //

        private void AddTrait(AstTraitTypeExpr trait)
        {
            mAllTraits.Add(trait);
            mUnresolvedTraits.Enqueue(trait);
        }

        private void AddEnum(AstEnumTypeExpr en)
        {
            mAllEnums.Add(en);
            mUnresolvedEnums.Enqueue(en);
        }

        private void AddStruct(AstStructTypeExpr str)
        {
            mAllStructs.Add(str);
            mUnresolvedStructs.Enqueue(str);
        }

        private void AddFunction(AstFuncExpr func)
        {
            mAllFunctions.Add(func);
            mUnresolvedFunctions.Enqueue(func);
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
                var.TypeExpr.AttachTo(var);
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
    }
}
