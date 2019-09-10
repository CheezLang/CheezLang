using System;
using System.Collections.Generic;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;

namespace Cheez
{
    public partial class Workspace
    {
        private Dictionary<AstWhileStmt, HashSet<(Scope scope, ILocation location)>> mWhileExits =
            new Dictionary<AstWhileStmt, HashSet<(Scope scope, ILocation location)>>();

        private void AddLoopExit(AstWhileStmt whl, Scope s, ILocation location)
        {
            if (!mWhileExits.ContainsKey(whl))
            {
                mWhileExits.Add(whl, new HashSet<(Scope scope, ILocation location)>());
            }

            mWhileExits[whl].Add((s, location));
        }

        private AstExpression Destruct(AstExpression expr)
        {
            //if (!(expr.Type is StructType))
            //    return null;

            var cc = new AstCompCallExpr(new AstIdExpr("destruct", false, expr.Location), new List<AstArgument>
            {
                new AstArgument(expr, Location: expr.Location)
            }, expr.Location);
            cc.Type = CheezType.Void;
            cc.SetFlag(ExprFlags.IgnoreInCodeGen, true);
            return cc;
        }

        private AstExpression Destruct(ITypedSymbol symbol, ILocation location)
        {
            //if (!(symbol.Type is StructType))
            //    return null;

            var cc = new AstCompCallExpr(new AstIdExpr("destruct", false, location), new List<AstArgument>
            {
                new AstArgument(new AstSymbolExpr(symbol), Location: location)
            }, location);
            cc.Type = CheezType.Void;
            cc.SetFlag(ExprFlags.IgnoreInCodeGen, true);
            return cc;
        }

        private void PassVariableLifetimes(AstFunctionDecl func)
        {
            func.SubScope.InitSymbolStats();

            foreach (var p in func.Parameters)
            {
                func.SubScope.SetSymbolStatus(p, SymbolStatus.Kind.initialized, p);
            }

            if (func.ReturnTypeExpr?.Name != null)
            {
                func.SubScope.SetSymbolStatus(func.ReturnTypeExpr, SymbolStatus.Kind.uninitialized, func.ReturnTypeExpr.Name);
            }
            if (func.ReturnTypeExpr?.TypeExpr is AstTupleExpr t)
            {
                foreach (var m in t.Types)
                {
                    if (m.Name == null) continue;
                    // @todo: 
                }
            }

            PassVLExpr(func.Body);

            // destruct params
            foreach (var p in func.Parameters)
            {
                if (func.SubScope.TryGetSymbolStatus(p, out var stat) && stat.kind == SymbolStatus.Kind.initialized)
                {
                    func.Body.AddDestruction(Destruct(p as ITypedSymbol, func.Body.End));
                }
            }
        }

        private bool PassVLExpr(AstExpression expr)
        {
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
                                    && c.FunctionType.Parameters[arg.Index].type.IsCopy)
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

                        if (id.Scope.TryGetSymbolStatus(sym, out var status))
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
                    var l = PassVLExpr(b.Left);
                    var r = PassVLExpr(b.Right);

                    // no need to move here because the types should be primitive
                    // Move(b.Left);
                    // Move(b.Right);
                    return l && r;

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
                    return Move(c.SubExpression);

                case AstArrayAccessExpr c:
                    if (!PassVLExpr(c.SubExpression)) return false;
                    if (!PassVLExpr(c.Indexer)) return false;
                    if (!Move(c.Indexer))
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

                case AstBreakExpr br:
                    return PassVLBreak(br);

                case AstDefaultExpr _:
                case AstNumberExpr _:
                case AstBoolExpr _:
                case AstStringLiteral _:
                case AstCharLiteral _:
                case AstNullExpr _:
                    return true;

                default:
                    WellThatsNotSupposedToHappen(expr.GetType().ToString());
                    return false;
                }
        }

        private bool PassVLBlock(AstBlockExpr expr)
        {
            var scope = expr.SubScope;
            scope.InitSymbolStats();

            foreach (var stmt in expr.Statements)
            {
                switch (stmt)
                {
                    case AstVariableDecl var:
                        foreach (var sv in var.SubDeclarations)
                        {
                            if (sv.Initializer != null)
                            {
                                scope.SetSymbolStatus(sv, SymbolStatus.Kind.initialized, sv);
                                if (!PassVLExpr(sv.Initializer))
                                    return false;
                                if (!Move(sv.Initializer))
                                    return false;
                            }
                            else
                                scope.SetSymbolStatus(sv, SymbolStatus.Kind.uninitialized, sv.Name);
                        }
                        break;

                    case AstWhileStmt whl:
                        if (!PassVLWhile(whl))
                            return false;
                        break;

                    case AstAssignment ass:
                        if (!PassVLAssignment(ass))
                            return false;
                        break;

                    case AstExprStmt es:
                        if (!PassVLExpr(es.Expr))
                            return false;
                        break;
                }

                // @todo: should we report errors for code after a break or return?
                // right now we do
                // - nmo, 10.09.2019
                //if (stmt.GetFlag(StmtFlags.Breaks) || stmt.GetFlag(StmtFlags.Returns))
                //    break;
            }

            if (!expr.GetFlag(ExprFlags.Anonymous) && !expr.GetFlag(ExprFlags.DontApplySymbolStatuses))
                expr.SubScope.ApplyInitializedSymbolsToParent();

            // call constructors
            if (!expr.GetFlag(ExprFlags.Anonymous)
                && !expr.GetFlag(ExprFlags.Breaks) && !expr.GetFlag(ExprFlags.Returns))
            {
                foreach (var sym in expr.SubScope.Symbols)
                {
                    if (expr.SubScope.TryGetSymbolStatus(sym.Value, out var stat) && stat.kind == SymbolStatus.Kind.initialized)
                    {
                        expr.AddDestruction(Destruct(sym.Value as ITypedSymbol, expr.End));
                    }
                }
            }

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
                    cas.SubScope.InitSymbolStats();
                    PassVLExpr(cas.Body);
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
                foreach (var sym in currentScope.Symbols)
                {
                    if (currentScope.TryGetSymbolStatus(sym.Value, out var stat) && stat.kind == SymbolStatus.Kind.initialized)
                    {
                        br.AddDestruction(Destruct(sym.Value as ITypedSymbol, br));
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
                    if (currentNode == null || currentNode is AstFunctionDecl)
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
            {
                if (ass.Pattern is AstIdExpr id)
                {
                    // if it is an id pattern it may not be initialized
                    if (ass.Scope.TryGetSymbolStatus(id.Symbol, out var stat)
                        && stat.kind == SymbolStatus.Kind.initialized)
                        ass.AddDestruction(Destruct(id.Symbol as ITypedSymbol, ass.Pattern));
                }
                else
                {
                    // otherwise it must be initialized, so always destruct it
                    ass.AddDestruction(Destruct(ass.Pattern));
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
    }
}
