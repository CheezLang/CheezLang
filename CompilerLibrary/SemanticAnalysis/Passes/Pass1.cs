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
        private List<AstStructDecl> mStructs = new List<AstStructDecl>();

        private List<AstTraitDeclaration> mTraits = new List<AstTraitDeclaration>();
        private List<AstEnumDecl> mEnums = new List<AstEnumDecl>();
        private List<AstEnumDecl> mPolyEnums = new List<AstEnumDecl>();
        
        private List<AstVariableDecl> mVariables = new List<AstVariableDecl>();
        private List<AstTypeAliasDecl> mTypeDefs = new List<AstTypeAliasDecl>();
        private List<AstImplBlock> mImpls = new List<AstImplBlock>();
        private List<AstImplBlock> mTraitImpls = new List<AstImplBlock>();

        private List<AstFunctionDecl> mFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mPolyFunctions = new List<AstFunctionDecl>();
        private List<AstFunctionDecl> mFunctionInstances = new List<AstFunctionDecl>();

        private List<AstUsingStmt> mGlobalUses = new List<AstUsingStmt>();

        public IEnumerable<AstTraitDeclaration> Traits => mTraits;
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
                    case AstUsingStmt use:
                        {
                            use.Scope = GlobalScope;
                            mGlobalUses.Add(use);
                            break;
                        }

                    case AstStructDecl @struct:
                        {
                            @struct.Scope = GlobalScope;
                            Pass1StructDeclaration(@struct);

                            if (@struct.IsPolymorphic)
                                mPolyStructs.Add(@struct);
                            else
                                mStructs.Add(@struct);
                            break;
                        }

                    case AstTraitDeclaration @trait:
                        {
                            trait.Scope = GlobalScope;
                            trait.SubScope = new Scope("trait", trait.Scope);
                            Pass1TraitDeclaration(@trait);
                            mTraits.Add(@trait);
                            break;
                        }

                    case AstEnumDecl @enum:
                        {
                            @enum.Scope = GlobalScope;
                            Pass1EnumDeclaration(@enum);
                            if (@enum.IsPolymorphic)
                                mPolyEnums.Add(@enum);
                            else
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
                            else mImpls.Add(impl);
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
                Pass1Typedef(t);
            }

            // variable declaration dependencies
            foreach (var gv in mVariables)
            {
                if (gv.Initializer != null)
                {
                    var deps = new HashSet<AstSingleVariableDecl>();
                    CollectDependencies(gv.Initializer, deps);
                    gv.VarDependencies = deps;
                }
            }
        }

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

            if (polyNames.Count > 0)
            {
                func.Type = new GenericFunctionType(func);
            }
            else
            {
                func.Type = new FunctionType(func);

                if (func.TryGetDirective("varargs", out var varargs))
                {
                    if (varargs.Arguments.Count != 0)
                    {
                        ReportError(varargs, $"#varargs takes no arguments!");
                    }
                    func.FunctionType.VarArgs = true;
                }
            }

            var res = func.Scope.DefineDeclaration(func);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(func.Name, $"A symbol with name '{func.Name.Name}' already exists in current scope", detail);
            }
            else if (!func.IsGeneric)
            {
                func.Scope.FunctionDeclarations.Add(func);
            }
        }

        private void Pass1VariableDeclaration(AstVariableDecl var)
        {
            if (var.Initializer == null)
            {
                var.Initializer = new AstDefaultExpr(var.Pattern.Location);
            }
            var.Initializer.AttachTo(var);

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
                else
                {
                    var.Scope.VariableDeclarations.Add(var);
                }
            }
        }

        private void MatchPatternWithTypeExpr(AstVariableDecl parent, AstExpression pattern, AstExpression type)
        {
            if (pattern is AstIdExpr id)
            {

                var decl = new AstSingleVariableDecl(id, type, parent, parent.Constant, pattern);
                decl.Scope = parent.Scope;
                decl.Type = new VarDeclType(decl);
                parent.SubDeclarations.Add(decl);
                id.Symbol = decl;
            }
            else if (pattern is AstTupleExpr tuple)
            {
                AstTupleExpr tupleType = type as AstTupleExpr;

                for (int i = 0; i < tuple.Values.Count; i++)
                {
                    var tid = tuple.Values[i];
                    var tty = (i < tupleType?.Types?.Count) ? tupleType.Types[i] : null;

                    MatchPatternWithTypeExpr(parent, tid, tty?.TypeExpr);
                }
            }
            else
            {
                ReportError(pattern, $"This pattern is not valid here");
            }
        }

        private void CollectDependencies(AstExpression expr, HashSet<AstSingleVariableDecl> deps)
        {
            switch (expr)
            {
                case AstIdExpr id:
                    var sym = expr.Scope.GetSymbol(id.Name);
                    if (sym is AstSingleVariableDecl sv)
                    {
                        deps.Add(sv);
                    }
                    break;

                case AstCallExpr c:
                    c.Function.Scope = expr.Scope;
                    CollectDependencies(c.Function, deps);
                    foreach (var a in c.Arguments)
                    {
                        a.Scope = expr.Scope;
                        CollectDependencies(a, deps);
                    }
                    break;

                case AstUnaryExpr u:
                    u.SubExpr.Scope = expr.Scope;
                    CollectDependencies(u.SubExpr, deps);
                    break;

                case AstArgument a:
                    a.Expr.Scope = expr.Scope;
                    CollectDependencies(a.Expr, deps);
                    break;

                case AstLiteral _:
                    break;

                case AstBlockExpr b:
                    foreach (var s in b.Statements) CollectDependencies(s, deps);
                    break;

                case AstIfExpr iff:
                    if (iff.PreAction != null)
                        CollectDependencies(iff.PreAction, deps);
                    CollectDependencies(iff.Condition, deps);
                    CollectDependencies(iff.IfCase, deps);
                    if (iff.ElseCase != null)
                        CollectDependencies(iff.ElseCase, deps);
                    break;

                case AstTupleExpr t:
                    foreach (var m in t.Values)
                    {
                        m.Scope = expr.Scope;
                        CollectDependencies(m, deps);
                    }
                    break;

                case AstBinaryExpr b:
                    b.Left.Scope = expr.Scope;
                    b.Right.Scope = expr.Scope;
                    CollectDependencies(b.Left, deps);
                    CollectDependencies(b.Right, deps);
                    break;

                case AstStructValueExpr s:
                    foreach (var v in s.MemberInitializers)
                    {
                        v.Value.Scope = expr.Scope;
                        CollectDependencies(v.Value, deps);
                    }
                    break;

                case AstDotExpr d:
                    {
                        d.Left.Scope = d.Scope;
                        CollectDependencies(d.Left, deps);
                        break;
                    }

                case AstCompCallExpr c:
                    foreach (var a in c.Arguments)
                    {
                        a.Scope = c.Scope;
                        CollectDependencies(a, deps);
                    }
                    break;

                case AstNullExpr _: break;
                case AstDefaultExpr _: break;

                default: throw new NotImplementedException();
            }
        }

        private void CollectDependencies(AstStatement stmt, HashSet<AstSingleVariableDecl> deps)
        {
            switch (stmt)
            {
                case AstVariableDecl vd:
                    if (vd.Initializer != null) CollectDependencies(vd.Initializer, deps);
                    break;

                case AstExprStmt es:
                    CollectDependencies(es.Expr, deps);
                    break;

                default: throw new NotImplementedException();
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
                trait.Scope.TypeDeclarations.Add(trait);
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

        private void Pass1EnumDeclaration(AstEnumDecl @enum)
        {
            if (@enum.Parameters?.Count > 0)
            {
                @enum.IsPolymorphic = true;
                @enum.Type = new GenericEnumType(@enum);

                foreach (var p in @enum.Parameters)
                {
                    p.Scope = @enum.Scope;
                    p.TypeExpr.Scope = @enum.Scope;
                }
            }
            else
            {
                @enum.Scope.TypeDeclarations.Add(@enum);
                @enum.Type = new EnumType(@enum);
            }
            @enum.SubScope = new Scope($"enum {@enum.Name.Name}", @enum.Scope);


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

                foreach (var p in @struct.Parameters)
                {
                    p.Scope = @struct.Scope;
                    p.TypeExpr.Scope = @struct.Scope;
                }
            }
            else
            {
                @struct.Scope.TypeDeclarations.Add(@struct);
                @struct.Type = new StructType(@struct);
            }
            @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);

            var res = @struct.Scope.DefineDeclaration(@struct);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope", detail);
            }
        }

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
