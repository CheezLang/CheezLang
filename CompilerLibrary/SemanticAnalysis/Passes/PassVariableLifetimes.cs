#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;

namespace Cheez
{
    internal class SymbolStatus
    {
        public enum Kind
        {
            initialized,
            uninitialized,
            moved
        }

        public int order { get; }
        public ISymbol symbol { get; set; }
        public Kind kind { get; set; }
        public ILocation location { get; set; }
        public bool Owned { get; set; }

        public SymbolStatus(int order, ISymbol symbol, Kind kind, ILocation location, bool owned)
        {
            this.order = order;
            this.symbol = symbol;
            this.kind = kind;
            this.location = location;
            this.Owned = owned;
        }

        public override string ToString() => $"{symbol.Name}: {kind} @ {location} [{location.Beginning}]";
    }
    internal class SymbolStatusTable
    {
        public SymbolStatusTable Parent { get; }
        public AstWhileStmt? Loop { get; set; }
        private Dictionary<ISymbol, SymbolStatus> mSymbolStatus;
        public ISymbol[] SymbolStatuses => mSymbolStatus.Keys.ToArray();
        public IEnumerable<SymbolStatus> AllSymbolStatusesReverseOrdered => Parent != null ?
            mSymbolStatus.Values.Where(v => v.Owned).Concat(Parent.AllSymbolStatusesReverseOrdered) :
            mSymbolStatus.Values.Where(v => v.Owned);
        
        public IEnumerable<SymbolStatus> UnownedSymbolStatuses => mSymbolStatus.Values
                                .Where(v => !v.Owned);
        public IEnumerable<SymbolStatus> OwnedSymbolStatusesReverseOrdered => mSymbolStatus.Values
                                .Where(v => v.Owned)
                                .OrderByDescending(s => s.order);

        public SymbolStatusTable(SymbolStatusTable parent, AstWhileStmt? loop = null)
        {
            this.Parent = parent;
            this.mSymbolStatus = new Dictionary<ISymbol, SymbolStatus>();
            this.Loop = loop;
        }

        public IEnumerable<SymbolStatus> SymbolStatusesLoopReverseOrdered(AstWhileStmt loop)
        {

            foreach (var stat in mSymbolStatus.Values.Where(v => v.Owned))
            {
                yield return stat;
            }

            if (Loop == loop)
                yield break;

            if (Parent != null)
                foreach (var stat in Parent.SymbolStatusesLoopReverseOrdered(loop))
                    yield return stat;
        }

        public void InitSymbolStatus(ISymbol symbol, SymbolStatus.Kind holdsValue, ILocation location)
        {
            if (mSymbolStatus.ContainsKey(symbol))
                throw new Exception();

            var order = mSymbolStatus.Count;
            if (mSymbolStatus.TryGetValue(symbol, out var stat))
                order = stat.order;

            mSymbolStatus[symbol] = new SymbolStatus(order, symbol, holdsValue, location, true);
        }

        public void UpdateSymbolStatus(ISymbol symbol, SymbolStatus.Kind holdsValue, ILocation location)
        {
            if (mSymbolStatus.TryGetValue(symbol, out var status))
            {
                status.kind = holdsValue;
                status.location = location;
            }
            else
            {
                mSymbolStatus[symbol] = new SymbolStatus(mSymbolStatus.Count, symbol, holdsValue, location, false);
            }
        }

        public SymbolStatus GetSymbolStatus(ISymbol symbol) =>
            mSymbolStatus.TryGetValue(symbol, out var stat) ? stat : Parent?.GetSymbolStatus(symbol)!;

        public SymbolStatus? GetLocalSymbolStatus(ISymbol symbol) =>
            mSymbolStatus.TryGetValue(symbol, out var stat) ? stat : null;

        public bool TryGetSymbolStatus(ISymbol symbol, [NotNullWhen(true)] out SymbolStatus? status)
        {
            if (mSymbolStatus.TryGetValue(symbol, out var s))
            {
                status = s;
                return true;
            }
            if (Parent != null)
                return Parent.TryGetSymbolStatus(symbol, out status);
            status = null;
            return false;
        }

        public void ApplyInitializedSymbolsToParent()
        {
            foreach (var stat in UnownedSymbolStatuses)
            {
                Parent.UpdateSymbolStatus(stat.symbol, stat.kind, stat.location);
            }
        }
    }

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

            var symStatTable = new SymbolStatusTable(null);

            foreach (var p in func.Parameters)
            {
                symStatTable.InitSymbolStatus(p, SymbolStatus.Kind.initialized, p);
            }

            if (func.ReturnTypeExpr?.Name != null)
            {
                symStatTable.InitSymbolStatus(func.ReturnTypeExpr, SymbolStatus.Kind.uninitialized, func.ReturnTypeExpr.Name);
            }
            if (func.ReturnTypeExpr?.TypeExpr is AstTupleExpr t && t.IsFullyNamed)
            {
                foreach (var m in t.Types)
                {
                    symStatTable.InitSymbolStatus(m.Symbol, SymbolStatus.Kind.uninitialized, m.Name);
                }
            }

            PassVLExpr(func.Body, symStatTable);

            // destruct params
            if (!func.Body.GetFlag(ExprFlags.Returns))
            {
                for (int i = func.Parameters.Count - 1; i >= 0; i--)
                {
                    var p = func.Parameters[i];
                    var stat = symStatTable.GetSymbolStatus(p);
                    if (stat.kind == SymbolStatus.Kind.initialized)
                    {
                        func.Body.AddDestruction(Destruct(p, func.Body.End));
                    }
                }
            }
        }

        private bool PassVLExpr(AstExpression expr, SymbolStatusTable symStatTable)
        {
            if (expr.Type.IsErrorType)
                return false;

            switch (expr)
            {
                case AstBlockExpr block: return PassVLBlock(block, symStatTable);
                case AstIfExpr e: return PassVLIf(e, symStatTable);
                case AstMatchExpr m: return PassVLMatch(m, symStatTable);

                case AstCallExpr c:
                    {
                        bool b = true;
                        foreach (var arg in c.Arguments)
                        {
                            if (PassVLExpr(arg.Expr, symStatTable))
                            {
                                if (arg.Index >= c.FunctionType.Parameters.Length)
                                {
                                    b &= Move(arg.Expr, symStatTable);
                                }
                                else if (arg.Index < c.FunctionType.Parameters.Length
                                    && !c.FunctionType.Parameters[arg.Index].type.IsCopy)
                                {
                                    b &= Move(arg.Expr, symStatTable);
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
                        else if (symStatTable.TryGetSymbolStatus(sym, out var status))
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

                        if (symStatTable.TryGetSymbolStatus(symbol, out var status))
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
                        var l = PassVLExpr(b.Left, symStatTable);
                        var r = PassVLExpr(b.Right, symStatTable);

                        // no need to move here because the types should be primitive
                        // Move(b.Left);
                        // Move(b.Right);
                        return l && r;
                    }

                case AstUnaryExpr u:
                    return PassVLExpr(u.SubExpr, symStatTable);
                    // no need to move here because the types should be primitive
                    // Move(u.SubExpr);


                case AstAddressOfExpr a: return PassVLExpr(a.SubExpression, symStatTable);
                case AstDereferenceExpr d: return PassVLExpr(d.SubExpression, symStatTable);
                case AstArrayExpr a:
                    foreach (var sub in a.Values)
                    {
                        if (!PassVLExpr(sub, symStatTable))
                            return false;
                        if (!Move(sub, symStatTable))
                            return false;
                    }
                    return true;

                case AstCastExpr c:
                    if (!PassVLExpr(c.SubExpression, symStatTable)) return false;

                    // traits only borrow, so we dont move
                    if (c.Type is TraitType)
                        return true;

                    return Move(c.SubExpression, symStatTable);

                case AstArrayAccessExpr c:
                    if (!PassVLExpr(c.SubExpression, symStatTable)) return false;
                    if (!PassVLExpr(c.Arguments[0], symStatTable)) return false;
                    if (!Move(c.Arguments[0], symStatTable))
                        return false;
                    return true;

                case AstStructValueExpr sv:
                    {
                        bool b = true;
                        foreach (var arg in sv.MemberInitializers)
                        {
                            if (PassVLExpr(arg.Value, symStatTable))
                                b &= Move(arg.Value, symStatTable);
                            else
                                b = false;
                        }

                        return b;
                    }

                case AstDotExpr dot:
                    if (!PassVLExpr(dot.Left, symStatTable))
                        return false;
                    return true;

                case AstRangeExpr r:
                    {
                        if (!PassVLExpr(r.From, symStatTable))
                            return false;
                        if (!Move(r.From, symStatTable))
                            return false;
                        if (!PassVLExpr(r.To, symStatTable))
                            return false;
                        if (!Move(r.To, symStatTable))
                            return false;
                        return true;
                    }

                case AstBreakExpr br:
                    return PassVLBreak(br, symStatTable);

                case AstContinueExpr cont:
                    return PassVLContinue(cont, symStatTable);

                case AstEnumValueExpr e:
                    return PassVLEnumValueExpr(e, symStatTable);

                case AstTupleExpr t:
                    {
                        foreach (var v in t.Values)
                        {
                            if (!PassVLExpr(v, symStatTable))
                                return false;
                            if (!Move(v, symStatTable))
                                return false;
                        }

                        return true;
                    }

                case AstTempVarExpr t:
                    {
                        if (mMovedTempVars.Contains(t))
                            return true;
                        mMovedTempVars.Add(t);
                        if (!PassVLExpr(t.Expr, symStatTable))
                            return false;
                        if (!Move(t.Expr, symStatTable))
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

        private bool PassVLStatement(AstStatement stmt, SymbolStatusTable symStatTable)
        {
            switch (stmt)
            {
                case AstVariableDecl var:
                    if (var.Initializer != null)
                    {
                        symStatTable.InitSymbolStatus(var, SymbolStatus.Kind.uninitialized, var);
                        if (!PassVLExpr(var.Initializer, symStatTable))
                            return false;
                        symStatTable.UpdateSymbolStatus(var, SymbolStatus.Kind.initialized, var);
                        if (!Move(var.Initializer, symStatTable))
                            return false;
                    }
                    else
                    {
                        symStatTable.InitSymbolStatus(var, SymbolStatus.Kind.uninitialized, var);
                    }
                    return true;

                case AstWhileStmt whl:
                    if (!PassVLWhile(whl, symStatTable))
                        return false;
                    return true;

                case AstAssignment ass:
                    if (!PassVLAssignment(ass, symStatTable))
                        return false;
                    return true;

                case AstExprStmt es:
                    {
                        if (!PassVLExpr(es.Expr, symStatTable))
                            return false;

                        if (TypeHasDestructor(es.Expr.Type))
                        {
                            var tempVar = new AstTempVarExpr(es.Expr);
                            tempVar.Type = es.Expr.Type;
                            es.Expr = tempVar;
                            es.AddDestruction(Destruct(tempVar));
                        }
                        return true;
                    }

                case AstReturnStmt ret:
                    return PassVLReturn(ret, symStatTable);

                case AstDeferStmt def:
                    {
                        if (!PassVLStatement(def.Deferred, symStatTable))
                            return false;

                        symStatTable.InitSymbolStatus(def, SymbolStatus.Kind.initialized, def);
                        return true;
                    }
            }

            return true;
        }

        private bool PassVLBlock(AstBlockExpr expr, SymbolStatusTable parent)
        {
            SymbolStatusTable? symStatTable = null;
            if (expr.Transparent)
                symStatTable = parent;
            else
                symStatTable = new SymbolStatusTable(parent);

            foreach (var stmt in expr.Statements)
            {
                if (!PassVLStatement(stmt, symStatTable))
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

            if (!expr.Transparent)
            {
                // call constructors
                if (!expr.GetFlag(ExprFlags.Anonymous)
                    && !expr.GetFlag(ExprFlags.Breaks) && !expr.GetFlag(ExprFlags.Returns))
                {
                    foreach (var stat in symStatTable.OwnedSymbolStatusesReverseOrdered)
                    {
                        if (stat.kind == SymbolStatus.Kind.initialized)
                        {
                            expr.AddDestruction(Destruct(stat.symbol, expr.End));
                        }
                    }
                }

                // apply to parent
                foreach (var stat in symStatTable.UnownedSymbolStatuses)
                {
                    parent.UpdateSymbolStatus(stat.symbol, stat.kind, stat.location);
                }
            }

            return true;
        }

        private bool PassVLIf(AstIfExpr expr, SymbolStatusTable parent)
        {
            var symStatTableIf = new SymbolStatusTable(parent);
            var symStatTableElse = new SymbolStatusTable(parent);

            expr.IfCase.SetFlag(ExprFlags.DontApplySymbolStatuses, true);
            expr.ElseCase.SetFlag(ExprFlags.DontApplySymbolStatuses, true);
            var result = PassVLExpr(expr.IfCase, symStatTableIf);
            result &= PassVLExpr(expr.ElseCase, symStatTableElse);

            bool ifReturns = expr.IfCase.GetFlag(ExprFlags.Returns) || expr.IfCase.GetFlag(ExprFlags.Breaks);
            bool elseReturns = expr.ElseCase.GetFlag(ExprFlags.Returns) || expr.ElseCase.GetFlag(ExprFlags.Breaks);
            if (ifReturns && !elseReturns)
            {
                if (expr.ElseCase is AstNestedExpression elseCase)
                    symStatTableElse.ApplyInitializedSymbolsToParent();
            }
            else if (elseReturns && !ifReturns)
            {
                if (expr.IfCase is AstNestedExpression ifCase)
                    symStatTableIf.ApplyInitializedSymbolsToParent();
            }
            else if (!ifReturns && !elseReturns)
            {
                foreach (var sym in parent.SymbolStatuses)
                {
                    var ifStat = symStatTableIf.GetSymbolStatus(sym);
                    var elseStat = symStatTableElse.GetSymbolStatus(sym);

                    if ((ifStat.kind == SymbolStatus.Kind.initialized) ^ (elseStat.kind == SymbolStatus.Kind.initialized))
                    {
                        ReportError(expr.Beginning, $"Symbol '{sym.Name}' is initialized in one case but not the other",
                            ("if-case: " + ifStat.kind, ifStat.location),
                            ("else-case: " + elseStat.kind, elseStat.location));
                        result = false;
                    }
                    else
                    {
                        parent.UpdateSymbolStatus(sym, ifStat.kind, ifStat.location);
                    }
                }
            }

            return result;
        }

        private bool PassVLMatch(AstMatchExpr expr, SymbolStatusTable parent)
        {
            var subStats = new SymbolStatusTable[expr.Cases.Count];
            for (int i = 0; i < expr.Cases.Count; i++)
            {
                subStats[i] = new SymbolStatusTable(parent);
                if (!PassVLExpr(expr.Cases[i].Body, subStats[i]))
                    return false;
            }

            bool result = true;
            // handle initialized symbols
            foreach (var sym in parent.SymbolStatuses)
            {
                var moves = new List<(SymbolStatus.Kind kind, ILocation location)>();
                var inits = new List<ILocation>();

                for (int i = 0; i < expr.Cases.Count; i++)
                {
                    var caseStat = subStats[i].GetSymbolStatus(sym);

                    switch (caseStat.kind)
                    {
                        case SymbolStatus.Kind.initialized:
                            inits.Add(caseStat.location);
                            break;

                        case SymbolStatus.Kind.moved:
                            moves.Add((caseStat.kind, caseStat.location));
                            break;

                        case SymbolStatus.Kind.uninitialized:
                            moves.Add((caseStat.kind, caseStat.location));
                            break;
                    }
                }

                if (inits.Count > 0 && moves.Count == 0)
                {
                    if (inits.Count != expr.Cases.Count)
                        WellThatsNotSupposedToHappen();
                    parent.UpdateSymbolStatus(sym, SymbolStatus.Kind.initialized, inits[0]);
                }
                else if (moves.Count > 0 && inits.Count == 0)
                {
                    if (moves.Count != expr.Cases.Count)
                        WellThatsNotSupposedToHappen();
                    parent.UpdateSymbolStatus(sym, moves[0].kind, moves[0].location);
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

        private bool PassVLBreak(AstBreakExpr br, SymbolStatusTable symStatTable)
        {
            var whl = br.Loop;
            AddLoopExit(whl, br.Scope, br);

            foreach (var stat in symStatTable.SymbolStatusesLoopReverseOrdered(whl))
            {
                if (stat.kind == SymbolStatus.Kind.initialized)
                {
                    br.AddDestruction(Destruct(stat.symbol, br));
                }
            }
            return true;
        }

        private bool PassVLContinue(AstContinueExpr cont, SymbolStatusTable symStatTable)
        {
            var whl = cont.Loop;
            AddLoopExit(whl, cont.Scope, cont);

            foreach (var stat in symStatTable.SymbolStatusesLoopReverseOrdered(whl))
            {
                if (stat.kind == SymbolStatus.Kind.initialized)
                {
                    cont.AddDestruction(Destruct(stat.symbol, cont));
                }
            }

            return true;
        }

        private bool PassVLWhile(AstWhileStmt whl, SymbolStatusTable parent)
        {
            var symStatTable = new SymbolStatusTable(parent, whl);
            if (!PassVLExpr(whl.Body, symStatTable))
                return false;

            if (!whl.Body.GetFlag(ExprFlags.Breaks) && !whl.Body.GetFlag(ExprFlags.Returns))
                AddLoopExit(whl, whl.Body.SubScope, whl.Body.Location.End);

            if (mWhileExits.TryGetValue(whl, out var exits))
            {
                foreach (var sym in parent.SymbolStatuses)
                {
                    var oldStat = parent.GetSymbolStatus(sym);
                    foreach (var exit in exits)
                    {
                        var newStat = symStatTable.GetSymbolStatus(sym);

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

        private bool PassVLAssignment(AstAssignment ass, SymbolStatusTable symStatTable)
        {
            var result = true;
            if (ass.SubAssignments?.Count > 0)
            {
                foreach (var sub in ass.SubAssignments)
                    result &= PassVLAssignment(sub, symStatTable);
                return result;
            }

            if (!PassVLExpr(ass.Pattern, symStatTable))
                return false;

            // destruct pattern if already initialized
            if (ass.Operator == null) {
                if (ass.Pattern is AstIdExpr id)
                {
                    // if it is an id pattern it may not be initialized
                    if (symStatTable.TryGetSymbolStatus(id.Symbol, out var stat)
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
            result &= PassVLExpr(ass.Value, symStatTable);

            // if pattern is an id, update status
            // otherwise it must already be initialized
            {
                if (ass.Pattern is AstIdExpr id && symStatTable.TryGetSymbolStatus(id.Symbol, out var status))
                {
                    symStatTable.UpdateSymbolStatus(id.Symbol, SymbolStatus.Kind.initialized, ass);
                }
            }

            // move value if no operator assignment (otherwise the operator call handles this)=
            if (ass.Operator == null)
                result &= Move(ass.Value, symStatTable);
            return result;
        }

        private bool PassVLEnumValueExpr(AstEnumValueExpr e, SymbolStatusTable symStatTable)
        {
            if (e.Argument != null)
            {
                if (!PassVLExpr(e.Argument, symStatTable))
                    return false;
                return Move(e.Argument, symStatTable);
            }
            return true;
        }

        private bool PassVLReturn(AstReturnStmt ret, SymbolStatusTable symStatTable)
        {
            // move value
            if (ret.ReturnValue != null)
            {
                if (!PassVLExpr(ret.ReturnValue, symStatTable))
                    return false;
                if (!Move(ret.ReturnValue, symStatTable))
                    return false;
            }

            // add destructors
            foreach (var stat in symStatTable.OwnedSymbolStatusesReverseOrdered)
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
                                && symStatTable.TryGetSymbolStatus(m.Symbol, out var stat)
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
                    if (symStatTable.GetSymbolStatus(currentFunction.ReturnTypeExpr).kind != SymbolStatus.Kind.initialized)
                    {
                        // check if maybe is tuple and all tuples have been initialized
                        if (currentFunction.ReturnTypeExpr.TypeExpr is AstTupleExpr t && t.IsFullyNamed)
                        {
                            foreach (var m in t.Types)
                            {
                                if (m.Symbol != null
                                    && symStatTable.TryGetSymbolStatus(m.Symbol, out var stat)
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
