using System;
using System.Collections.Generic;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;

namespace Cheez
{
    public partial class Workspace
    {
        private Dictionary<AstWhileStmt, HashSet<(Scope scope, ILocation location)>> mWhileExits =
            new Dictionary<AstWhileStmt, HashSet<(Scope scope, ILocation location)>>();
        private HashSet<AstTempVarExpr> mMovedTempVars = new HashSet<AstTempVarExpr>();

        private Dictionary<CheezType, AstFuncExpr> mTypeDropFuncMap = new Dictionary<CheezType, AstFuncExpr>();
        private AstTraitTypeExpr mTraitDrop;
        private HashSet<CheezType> mTypesWithDestructor = new HashSet<CheezType>();
        
        public IEnumerable<CheezType> TypesWithDestructor => mTypesWithDestructor;

        public AstFuncExpr GetDropFuncForType(CheezType type)
        {
            if (mTypeDropFuncMap.TryGetValue(type, out var f))
                return f;
            return null;
        }

        private void AddLoopExit(AstWhileStmt whl, Scope s, ILocation location)
        {
            if (!mWhileExits.ContainsKey(whl))
            {
                mWhileExits.Add(whl, new HashSet<(Scope scope, ILocation location)>());
            }

            mWhileExits[whl].Add((s, location));
        }

        public bool TypeHasDestructor(CheezType type)
        {
            if (mTypesWithDestructor.Contains(type))
                return true;

            var b = TypeHasDestructorHelper(type);
            if (b)
                mTypesWithDestructor.Add(type);
            return b;
        }

        private bool TypeHasDestructorHelper(CheezType type)
        {
            if (type.IsErrorType)
                WellThatsNotSupposedToHappen();

            if (mTraitDrop == null)
            {
                var sym = GlobalScope.GetSymbol("Drop");
                if (sym is AstConstantDeclaration c && c.Initializer is AstTraitTypeExpr t)
                    mTraitDrop = t;
                else
                    ReportError("There should be a global trait called Drop");
            }

            if (mTypeDropFuncMap.ContainsKey(type))
                return true;

            bool memberNeedsDtor = false;
            if (type is StructType @struct)
            {
                // check if any member needs desctructor
                ComputeStructMembers(@struct.Declaration);
                foreach (var mem in @struct.Declaration.Members)
                {
                    if (TypeHasDestructor(mem.Type))
                        memberNeedsDtor = true;
                }
            }

            if (type is TupleType tuple)
            {
                // check if any member needs desctructor
                foreach (var mem in tuple.Members)
                {
                    if (TypeHasDestructor(mem.type))
                        memberNeedsDtor = true;
                }
            }

            if (type is EnumType @enum)
            {
                // check if any member needs desctructor
                ComputeEnumMembers(@enum.Declaration);
                foreach (var mem in @enum.Declaration.Members)
                {
                    if (mem.AssociatedType != null && TypeHasDestructor(mem.AssociatedType))
                        memberNeedsDtor = true;
                }
            }

            // do this last because we want to visit all members
            var impls = GetImplsForType(type, mTraitDrop.TraitType);
            if (impls.Count > 0)
            {
                if (impls.Count != 1)
                    WellThatsNotSupposedToHappen();
                var impl = impls[0];
                if (impl.Functions.Count != 1)
                    WellThatsNotSupposedToHappen();
                var func = impl.Functions[0];
                mTypeDropFuncMap[type] = func;
                return true;
            }

            return memberNeedsDtor;
        }

        private AstExpression Destruct(AstExpression expr)
        {
            if (!TypeHasDestructor(expr.Type))
                return null;

            var cc = new AstCompCallExpr(new AstIdExpr("destruct", false, expr.Location), new List<AstArgument>
            {
                new AstArgument(expr, Location: expr.Location)
            }, expr.Location);
            cc.Type = CheezType.Void;
            //cc.SetFlag(ExprFlags.IgnoreInCodeGen, true);
            return cc;
        }

        private AstExpression Destruct(ISymbol symbol, ILocation location)
        {
            if (symbol is AstDeferStmt def)
            {
                var block = new AstBlockExpr(new List<AstStatement> { def.Deferred }, location);
                block.Type = CheezType.Void;
                return block;
            }
            else if (symbol is ITypedSymbol tsymbol)
            {
                if (!tsymbol.Type.IsErrorType && !TypeHasDestructor(tsymbol.Type))
                    return null;

                var cc = new AstCompCallExpr(new AstIdExpr("destruct", false, location), new List<AstArgument>
                {
                    new AstArgument(new AstSymbolExpr(tsymbol), Location: location)
                }, location);
                cc.Type = CheezType.Void;
                return cc;
            }

            WellThatsNotSupposedToHappen();
            return null;
        }

        private void PassVariableLifetimes(AstFuncExpr func)
        {
            mWhileExits.Clear();
            mMovedTempVars.Clear();

            func.SubScope.InitSymbolStats();

            foreach (var p in func.Parameters)
            {
                func.SubScope.SetSymbolStatus(p, SymbolStatus.Kind.initialized, p);
            }

            if (func.ReturnTypeExpr?.Name != null)
            {
                func.SubScope.SetSymbolStatus(func.ReturnTypeExpr, SymbolStatus.Kind.uninitialized, func.ReturnTypeExpr.Name);
            }
            if (func.ReturnTypeExpr?.TypeExpr is AstTupleExpr t && t.IsFullyNamed)
            {
                foreach (var m in t.Types)
                {
                    func.SubScope.SetSymbolStatus(m.Symbol, SymbolStatus.Kind.uninitialized, m.Name);
                }
            }

            PassVLExpr(func.Body);

            // destruct params
            if (!func.Body.GetFlag(ExprFlags.Returns))
            {
                for (int i = func.Parameters.Count - 1; i >= 0; i--)
                {
                    var p = func.Parameters[i];
                    if (func.SubScope.TryGetSymbolStatus(p, out var stat) && stat.kind == SymbolStatus.Kind.initialized)
                    {
                        func.Body.AddDestruction(Destruct(p, func.Body.End));
                    }
                }
            }
        }

        private bool PassVLExpr(AstExpression expr)
        {
            if (expr.Type.IsErrorType)
                return false;

            if (expr.Scope.SymbolStatuses == null)
                expr.Scope.InitSymbolStats();

            switch (expr)
            {
                case AstBlockExpr block: return PassVLBlock(block);
                case AstIfExpr e: return PassVLIf(e);
                case AstMatchExpr m: return PassVLMatch(m); 

                case AstCallExpr c:
                    {
                        bool b = true;
                        foreach (var arg in c.Arguments)
                        {
                            if (PassVLExpr(arg.Expr))
                            {
                                if (arg.Index >= c.FunctionType.Parameters.Length)
                                {
                                    b &= Move(arg.Expr);
                                }
                                else if (arg.Index < c.FunctionType.Parameters.Length
                                    && !c.FunctionType.Parameters[arg.Index].type.IsCopy)
                                {
                                    b &= Move(arg.Expr);
                                }
                            }
                            else
                                b = false;
                        }
                        return b;
                    }

                case AstIdExpr id:
                    {
                        if (id.GetFlag(ExprFlags.AssignmentTarget))
                        {
                            return true;
                        }

                        var sym = id.Symbol;

                        if (sym is AstVariableDecl var && var.GetFlag(StmtFlags.GlobalScope))
                        {
                            // do nothing
                        }
                        else if (id.Scope.TryGetSymbolStatus(sym, out var status))
                        {
                            switch (status.kind)
                            {
                                case SymbolStatus.Kind.moved:
                                    ReportError(expr, $"Can't use variable '{sym.Name}' because it has been moved",
                                        ("Moved here:", status.location));
                                    return false;
                                case SymbolStatus.Kind.uninitialized:
                                    ReportError(expr, $"Can't use variable '{sym.Name}' because it is not yet initialized",
                                        ("Declared here:", status.location));
                                    return false;
                            }
                        }

                        return true;
                    }

                case AstCompCallExpr cc when cc.Name.Name == "log_symbol_status":
                    {
                        var id = cc.Arguments[0].Expr as AstIdExpr;
                        var symbol = cc.Scope.GetSymbol(id.Name);

                        if (cc.Scope.TryGetSymbolStatus(symbol, out var status))
                            Console.WriteLine($"[symbol stat] ({id.Location.Beginning}) {status}");
                        else
                            Console.WriteLine($"[symbol stat] ({id.Location.Beginning}) undefined");
                        return true;
                    }

                case AstCompCallExpr cc:
                    {
                        return true;
                    }

                case AstBinaryExpr b:
                    {
                        var l = PassVLExpr(b.Left);
                        var r = PassVLExpr(b.Right);

                        // no need to move here because the types should be primitive
                        // Move(b.Left);
                        // Move(b.Right);
                        return l && r;
                    }

                case AstUnaryExpr u:
                    return PassVLExpr(u.SubExpr);
                    // no need to move here because the types should be primitive
                    // Move(u.SubExpr);


                case AstAddressOfExpr a: return PassVLExpr(a.SubExpression);
                case AstDereferenceExpr d: return PassVLExpr(d.SubExpression);
                case AstArrayExpr a:
                    foreach (var sub in a.Values)
                    {
                        if (!PassVLExpr(sub))
                            return false;
                        if (!Move(sub))
                            return false;
                    }
                    return true;

                case AstCastExpr c:
                    if (!PassVLExpr(c.SubExpression)) return false;

                    // traits only borrow, so we dont move
                    if (c.Type is TraitType)
                        return true;

                    return Move(c.SubExpression);

                case AstArrayAccessExpr c:
                    if (!PassVLExpr(c.SubExpression)) return false;
                    if (!PassVLExpr(c.Arguments[0])) return false;
                    if (!Move(c.Arguments[0]))
                        return false;
                    return true;

                case AstStructValueExpr sv:
                    {
                        bool b = true;
                        foreach (var arg in sv.MemberInitializers)
                        {
                            if (PassVLExpr(arg.Value))
                                b &= Move(arg.Value);
                            else
                                b = false;
                        }

                        return b;
                    }

                case AstDotExpr dot:
                    if (!PassVLExpr(dot.Left))
                        return false;
                    return true;

                case AstRangeExpr r:
                    {
                        if (!PassVLExpr(r.From))
                            return false;
                        if (!Move(r.From))
                            return false;
                        if (!PassVLExpr(r.To))
                            return false;
                        if (!Move(r.To))
                            return false;
                        return true;
                    }

                case AstBreakExpr br:
                    return PassVLBreak(br);

                case AstContinueExpr cont:
                    return PassVLContinue(cont);

                case AstEnumValueExpr e:
                    return PassVLEnumValueExpr(e);

                case AstTupleExpr t:
                    {
                        foreach (var v in t.Values)
                        {
                            if (!PassVLExpr(v))
                                return false;
                            if (!Move(v))
                                return false;
                        }

                        return true;
                    }

                case AstTempVarExpr t:
                    {
                        if (mMovedTempVars.Contains(t))
                            return true;
                        mMovedTempVars.Add(t);
                        if (!PassVLExpr(t.Expr))
                            return false;
                        if (!Move(t.Expr))
                            return false;
                        return true;
                    }

                case AstVariableRef _:
                case AstConstantRef _:
                        return true;

                case AstDefaultExpr _:
                case AstNumberExpr _:
                case AstBoolExpr _:
                case AstStringLiteral _:
                case AstCharLiteral _:
                case AstNullExpr _:
                    return true;

                case AstUfcFuncExpr _:
                    return true;

                case AstEmptyExpr _:
                    return true;

                default:
                    WellThatsNotSupposedToHappen(expr.GetType().ToString());
                    return false;
                }
        }

        private bool PassVLStatement(AstStatement stmt, Scope scope)
        {
            switch (stmt)
            {
                case AstVariableDecl var:
                    // dont handle comptime only variables
                    if (var.Type.IsComptimeOnly)
                        return true;

                    if (var.Initializer != null)
                    {
                        scope.SetSymbolStatus(var, SymbolStatus.Kind.initialized, var);
                        if (!PassVLExpr(var.Initializer))
                            return false;
                        if (!Move(var.Initializer))
                            return false;
                    }
                    else
                    {
                        scope.SetSymbolStatus(var, SymbolStatus.Kind.uninitialized, var.Name);
                    }
                    return true;

                case AstWhileStmt whl:
                    if (!PassVLWhile(whl))
                        return false;
                    return true;

                case AstAssignment ass:
                    if (!PassVLAssignment(ass))
                        return false;
                    return true;

                case AstExprStmt es:
                    {
                        if (es.Scope != scope)
                            es.Scope.InitSymbolStats();
                        if (es.Expr.Scope != es.Scope)
                            es.Expr.Scope.InitSymbolStats(es.Scope);
                        if (!PassVLExpr(es.Expr))
                            return false;

                        if (TypeHasDestructor(es.Expr.Type))
                        {
                            var tempVar = new AstTempVarExpr(es.Expr);
                            tempVar.Type = es.Expr.Type;
                            es.Expr = tempVar;
                            es.AddDestruction(Destruct(tempVar));
                        }

                        if (es.Expr.Scope != es.Scope)
                            es.Expr.Scope.ApplyInitializedSymbolsTo(es.Scope);
                        if (es.Scope != scope)
                            es.Scope.ApplyInitializedSymbolsToParent();
                        return true;
                    }

                case AstReturnStmt ret:
                    return PassVLReturn(ret);

                case AstDeferStmt def:
                    {
                        if (!PassVLStatement(def.Deferred, def.Scope))
                            return false;

                        scope.SetSymbolStatus(def, SymbolStatus.Kind.initialized, def);
                        return true;
                    }
            }

            return true;
        }

        private bool PassVLBlock(AstBlockExpr expr)
        {
            var scope = expr.SubScope;
            scope.InitSymbolStats();

            foreach (var stmt in expr.Statements)
            {
                if (!PassVLStatement(stmt, scope))
                    return false;

                // @todo: should we report errors for code after a break or return?
                // right now we do
                // - nmo, 10.09.2019
                //if (stmt.GetFlag(StmtFlags.Breaks) || stmt.GetFlag(StmtFlags.Returns))
                //    break;
            }

            //if (expr.Statements.LastOrDefault() is AstExprStmt es && expr.Type != CheezType.Void)
            //{
            //    if (!Move(es.Expr))
            //        return false;
            //}

            //if (!expr.GetFlag(ExprFlags.Anonymous) && !expr.GetFlag(ExprFlags.DontApplySymbolStatuses))
            if (expr.SubScope != expr.Scope && !expr.GetFlag(ExprFlags.DontApplySymbolStatuses))
                expr.SubScope.ApplyInitializedSymbolsToParent();

            if (expr.Scope.ExportedSymbols != null)
            {
                foreach (var export in expr.Scope.ExportedSymbols)
                {
                    var stat = expr.Scope.GetSymbolStatus(export);
                    expr.Scope.LinkedScope.SetSymbolStatus(export, stat.kind, stat.location);
                }
            }

            // call constructors
            if (!expr.GetFlag(ExprFlags.Anonymous)
                && !expr.GetFlag(ExprFlags.Breaks) && !expr.GetFlag(ExprFlags.Returns))
            {
                foreach (var stat in expr.SubScope.SymbolStatusesReverseOrdered)
                {
                    if (stat.kind == SymbolStatus.Kind.initialized)
                    {
                        expr.AddDestruction(Destruct(stat.symbol, expr.End));
                    }
                }
            }

            // add destructors
            //foreach (var stat in expr.Scope.AllSymbolStatusesReverseOrdered)
            //{
            //    if (stat.kind == SymbolStatus.Kind.initialized)
            //    {
            //        expr.AddDestruction(Destruct(stat.symbol, expr));
            //    }
            //}

            return true;
        }

        private bool PassVLIf(AstIfExpr expr)
        {
            expr.SubScope.InitSymbolStats();
            expr.IfCase.SetFlag(ExprFlags.DontApplySymbolStatuses, true);
            expr.ElseCase.SetFlag(ExprFlags.DontApplySymbolStatuses, true);
            var result = PassVLExpr(expr.IfCase);
            result &= PassVLExpr(expr.ElseCase);

            bool ifReturns = expr.IfCase.GetFlag(ExprFlags.Returns) || expr.IfCase.GetFlag(ExprFlags.Breaks);
            bool elseReturns = expr.ElseCase.GetFlag(ExprFlags.Returns) || expr.ElseCase.GetFlag(ExprFlags.Breaks);
            if (ifReturns && !elseReturns)
            {
                if (expr.ElseCase is AstNestedExpression elseCase)
                    elseCase.SubScope.ApplyInitializedSymbolsToParent();
            }
            else if (elseReturns && !ifReturns)
            {
                if (expr.IfCase is AstNestedExpression ifCase)
                    ifCase.SubScope.ApplyInitializedSymbolsToParent();
            }
            else if (!ifReturns && !elseReturns)
            {
                var ifBlock = expr.IfCase as AstNestedExpression;
                var elseBlock = expr.ElseCase as AstNestedExpression;
                foreach (var sym in expr.Scope.SymbolStatuses)
                {
                    if (!(expr.Scope.TryGetSymbolStatus(sym, out var oldStat)))
                        continue;
                    var ifStat = ifBlock?.SubScope?.GetSymbolStatus(sym) ?? oldStat;
                    var elseStat = elseBlock?.SubScope?.GetSymbolStatus(sym) ?? oldStat;

                    if ((ifStat.kind == SymbolStatus.Kind.initialized) ^ (elseStat.kind == SymbolStatus.Kind.initialized))
                    {
                        ReportError(expr.Beginning, $"Symbol '{sym.Name}' is initialized in one case but not the other",
                            ("if-case: " + ifStat.kind, ifStat.location),
                            ("else-case: " + elseStat.kind, elseStat.location));
                        result = false;
                    }
                    else
                    {
                        expr.SubScope.SetSymbolStatus(sym, ifStat.kind, ifStat.location);
                    }
                }
            }
            expr.SubScope.ApplyInitializedSymbolsToParent();

            return result;
        }

        private bool PassVLMatch(AstMatchExpr expr)
        {
            foreach (var cas in expr.Cases)
            {
                cas.SubScope.InitSymbolStats();
                if (!PassVLExpr(cas.Body))
                    return false;
            }

            bool result = true;
            // handle initialized symbols
            foreach (var sym in expr.Scope.SymbolStatuses)
            {
                if (!(expr.Scope.TryGetSymbolStatus(sym, out var oldStat)))
                    continue;

                var moves = new List<(SymbolStatus.Kind kind, ILocation location)>();
                var inits = new List<ILocation>();

                foreach (var cas in expr.Cases)
                {
                    var caseStat = cas.SubScope.GetSymbolStatus(sym);

                    switch (caseStat.kind)
                    {
                        case SymbolStatus.Kind.initialized:
                            if (caseStat.location == oldStat.location)
                                inits.Add(cas.Body.End);
                            else
                                inits.Add(caseStat.location);
                            break;

                        case SymbolStatus.Kind.moved:
                            if (caseStat.location == oldStat.location)
                                moves.Add((caseStat.kind, cas.Body.End));
                            else
                                moves.Add((caseStat.kind, caseStat.location));
                            break;

                        case SymbolStatus.Kind.uninitialized:
                            if (caseStat.location == oldStat.location)
                                moves.Add((caseStat.kind, cas.Body.End));
                            else
                                moves.Add((caseStat.kind, caseStat.location));
                            break;
                    }

                    //allInit &= caseStat.kind == SymbolStatus.Kind.initialized;
                    //allDeinit &= caseStat.kind != SymbolStatus.Kind.initialized;

                    //if (caseStat.kind == SymbolStatus.Kind.initialized && firstInit == null)
                    //    firstInit = caseStat;
                    //else if (caseStat.kind != SymbolStatus.Kind.initialized && firstDeinit == null)
                    //    firstDeinit = caseStat;
                }

                if (inits.Count > 0 && moves.Count == 0)
                {
                    if (inits.Count != expr.Cases.Count)
                        WellThatsNotSupposedToHappen();
                    if (oldStat.kind != SymbolStatus.Kind.initialized)
                        expr.Scope.SetSymbolStatus(sym, SymbolStatus.Kind.initialized, inits[0]);
                }
                else if (moves.Count > 0 && inits.Count == 0)
                {
                    if (moves.Count != expr.Cases.Count)
                        WellThatsNotSupposedToHappen();
                    if (oldStat.kind == SymbolStatus.Kind.initialized)
                        expr.Scope.SetSymbolStatus(sym, moves[0].kind, moves[0].location);
                }
                else
                {
                    var details = moves.Select(m => (m.kind.ToString() + " here:", m.location)).Concat(
                        inits.Select(i => ("initialized here:", i))
                        );
                    ReportError(expr.Beginning, $"Symbol '{sym.Name}' is initialized in some but not all cases", details);
                    result = false;
                }
            }

            return result;
        }

        private bool PassVLBreak(AstBreakExpr br)
        {
            var whl = br.Loop;
            AddLoopExit(whl, br.Scope, br);

            // @todo: add destructors and deferred expressions
            var currentScope = br.Scope;
            IAstNode currentNode = br;

            while (currentScope != null)
            {
                var stats = currentScope.SymbolStatusesReverseOrdered;
                if (stats != null)
                {
                    foreach (var stat in stats)
                    {
                        if (stat.kind == SymbolStatus.Kind.initialized)
                        {
                            br.AddDestruction(Destruct(stat.symbol, br));
                        }
                    }
                }

                var newScope = currentScope;
                while (newScope == currentScope)
                {
                    currentNode = currentNode.Parent;
                    if (currentNode == br.Loop)
                    {
                        newScope = null;
                        break;
                    }
                    if (currentNode == null || currentNode is AstFuncExpr)
                    {
                        WellThatsNotSupposedToHappen();
                        newScope = null;
                        break;
                    }
                    if (currentNode is AstExpression expr)
                    {
                        newScope = expr.Scope;
                    }
                    else if (currentNode is AstStatement stmt)
                    {
                        newScope = stmt.Scope;
                    }
                }

                currentScope = newScope;
            }

            return true;
        }

        private bool PassVLContinue(AstContinueExpr cont)
        {
            var whl = cont.Loop;
            AddLoopExit(whl, cont.Scope, cont);

            // @todo: add destructors and deferred expressions
            var currentScope = cont.Scope;
            IAstNode currentNode = cont;

            while (currentScope != null)
            {
                var stats = currentScope.SymbolStatusesReverseOrdered;
                if (stats != null)
                {
                    foreach (var stat in stats)
                    {
                        if (stat.kind == SymbolStatus.Kind.initialized)
                        {
                            cont.AddDestruction(Destruct(stat.symbol, cont));
                        }
                    }
                }

                var newScope = currentScope;
                while (newScope == currentScope)
                {
                    var oldCurrent = currentNode;
                    currentNode = currentNode.Parent;
                    if (currentNode == cont.Loop)
                    {
                        newScope = null;
                        break;
                    }
                    if (currentNode == null || currentNode is AstFuncExpr)
                    {
                        WellThatsNotSupposedToHappen();
                        newScope = null;
                        break;
                    }
                    if (currentNode is AstExpression expr)
                    {
                        newScope = expr.Scope;
                    }
                    else if (currentNode is AstStatement stmt)
                    {
                        newScope = stmt.Scope;
                    }
                }

                currentScope = newScope;
            }

            return true;
        }

        private bool PassVLWhile(AstWhileStmt whl)
        {
            if (whl.PreScope != whl.Scope)
                whl.PreScope.InitSymbolStats();
            whl.SubScope.InitSymbolStats();
            if (!PassVLExpr(whl.Body))
                return false;

            if (!whl.Body.GetFlag(ExprFlags.Breaks) && !whl.Body.GetFlag(ExprFlags.Returns))
                AddLoopExit(whl, whl.Body.SubScope, whl.Body.Location.End);

            if (mWhileExits.TryGetValue(whl, out var exits))
            {
                foreach (var sym in whl.Scope.SymbolStatuses)
                {
                    if (!(whl.Scope.TryGetSymbolStatus(sym, out var oldStat)))
                        continue;
                    foreach (var exit in exits)
                    {
                        var newStat = exit.scope.GetSymbolStatus(sym);

                        if ((oldStat.kind == SymbolStatus.Kind.initialized) ^ (newStat.kind == SymbolStatus.Kind.initialized))
                        {
                            ReportError(exit.location,
                                $"Symbol '{sym.Name}' is {oldStat.kind} before the loop but {newStat.kind} at this exit point of the loop",
                                ("before loop: " + oldStat.kind, oldStat.location),
                                ("exit point: " + newStat.kind, newStat.location));
                        }
                    }
                }
            }

            return true;
        }

        private bool PassVLAssignment(AstAssignment ass)
        {
            var result = true;
            if (ass.SubAssignments?.Count > 0)
            {
                foreach (var sub in ass.SubAssignments)
                    result &= PassVLAssignment(sub);
                return result;
            }

            if (!PassVLExpr(ass.Pattern))
                return false;

            // destruct pattern if already initialized
            if (ass.Operator == null) {
                if (ass.Pattern is AstIdExpr id)
                {
                    // if it is an id pattern it may not be initialized
                    if (ass.Scope.TryGetSymbolStatus(id.Symbol, out var stat)
                        && stat.kind == SymbolStatus.Kind.initialized)
                        ass.AddDestruction(Destruct(id.Symbol, ass.Pattern));
                }
                else
                {
                    // otherwise it must be initialized, so always destruct it
                    // unless it is deref expression

                    switch (ass.Pattern)
                    {
                        //case AstArrayAccessExpr ind when ind.SubExpression.Type is SliceType:
                        case AstDereferenceExpr de when !de.Reference: // deref a pointer
                        case AstArrayAccessExpr ind when ind.SubExpression.Type is PointerType: // index access a pointer
                            break;

                        default:
                            ass.AddDestruction(Destruct(ass.Pattern));
                            break;
                    }
                }
            }

            // value
            result &= PassVLExpr(ass.Value);

            // if pattern is an id, update status
            // otherwise it must already be initialized
            {
                if (ass.Pattern is AstIdExpr id && ass.Scope.TryGetSymbolStatus(id.Symbol, out var status))
                {
                    ass.Scope.SetSymbolStatus(id.Symbol, SymbolStatus.Kind.initialized, ass);
                }
            }

            // move value if no operator assignment (otherwise the operator call handles this)=
            if (ass.Operator == null)
                result &= Move(ass.Value);
            return result;
        }

        private bool PassVLEnumValueExpr(AstEnumValueExpr e)
        {
            if (e.Argument != null)
            {
                if (!PassVLExpr(e.Argument))
                    return false;
                return Move(e.Argument);
            }
            return true;
        }

        private bool PassVLReturn(AstReturnStmt ret)
        {
            // move value
            if (ret.ReturnValue != null)
            {
                if (!PassVLExpr(ret.ReturnValue))
                    return false;
                if (!Move(ret.ReturnValue))
                    return false;
            }

            // add destructors
            foreach (var stat in ret.Scope.AllSymbolStatusesReverseOrdered)
            {
                if (stat.kind == SymbolStatus.Kind.initialized)
                {
                    ret.AddDestruction(Destruct(stat.symbol, ret));
                }
            }

            // check if all return values have been initialized
            if (ret.ReturnValue == null && currentFunction.ReturnTypeExpr != null)
            {
                var missing = new List<ILocation>();
                if (currentFunction.ReturnTypeExpr.Name == null)
                {
                    if (currentFunction.ReturnTypeExpr.TypeExpr is AstTupleExpr t && t.IsFullyNamed)
                    {
                        foreach (var m in t.Types)
                        {
                            if (m.Symbol != null
                                && ret.Scope.TryGetSymbolStatus(m.Symbol, out var stat)
                                && stat.kind == SymbolStatus.Kind.initialized)
                            {
                                // ok
                            }
                            else
                            {
                                ReportError(ret, $"Return value has not been fully initialized",
                                    ("Missing:", m.Location));
                                return false;
                            }
                        }
                    }
                    else
                    {
                        ReportError(ret, $"Return value has not been initialized",
                            ("Missing:", currentFunction.ReturnTypeExpr.Location));
                        return false;
                    }
                }
                else
                {
                    if (ret.Scope.GetSymbolStatus(currentFunction.ReturnTypeExpr).kind != SymbolStatus.Kind.initialized)
                    {
                        // check if maybe is tuple and all tuples have been initialized
                        if (currentFunction.ReturnTypeExpr.TypeExpr is AstTupleExpr t && t.IsFullyNamed)
                        {
                            foreach (var m in t.Types)
                            {
                                if (m.Symbol != null
                                    && ret.Scope.TryGetSymbolStatus(m.Symbol, out var stat)
                                    && stat.kind == SymbolStatus.Kind.initialized)
                                {
                                    // ok
                                }
                                else
                                {
                                    ReportError(ret, $"Return value has not been fully initialized",
                                        ("Missing:", m.Location));
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
