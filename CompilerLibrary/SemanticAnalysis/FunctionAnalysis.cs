using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Visitors;

namespace Cheez
{
    public partial class Workspace
    {
        private void AnalyseFunctions(List<AstFuncExpr> newInstances)
        {
            int i = 0;
            while (i < MaxPolyFuncResolveStepCount && newInstances.Count != 0)
            {
                foreach (var instance in newInstances)
                {
                    AnalyseFunction(instance);
                }
                newInstances.Clear();

                i++;
            }

            if (i == MaxPolyFuncResolveStepCount)
            {
                var details = newInstances.Select(str => ("Here:", str.Location)).ToList();
                ReportError($"Detected a potential infinite loop in polymorphic function declarations after {MaxPolyFuncResolveStepCount} steps", details);
            }
        }

        private void AnalyseFunction(AstFuncExpr func)
        {
            if (func.IsAnalysed)
                return;
            func.IsAnalysed = true;

            Log($"Analysing function {func.Name}", $"impl = {func.ImplBlock?.Accept(new SignatureAstPrinter())}", $"poly = {func.IsGeneric}");
            PushLogScope();

            var prevCurrentFunction = currentFunction;
            currentFunction = func;
            try
            {
                if (func.SelfType != SelfParamType.None)
                {
                    var p = func.Parameters[0];
                    if (p.Name == null)
                    {
                        p.Name = new AstIdExpr("self", false, p.Location);
                    }

                    if (func.ImplBlock.TargetType is StructType @struct)
                    {
                        ComputeStructMembers(@struct.Declaration);
                        foreach (var m in @struct.Declaration.Members)
                        {
                            AstExpression expr = new AstDotExpr(new AstSymbolExpr(p), new AstIdExpr(m.Name, false, p.Location), p.Location);
                            expr.AttachTo(func, func.SubScope);
                            expr = InferType(expr, m.Type);

                            // define use if no parameter has the same name
                            if (!func.Parameters.Any(pa => pa.Name?.Name == m.Name))
                            {
                                var (ok, other) = func.SubScope.DefineUse(m.Name, expr, false, out var use);

                                if (!ok)
                                {
                                    ReportError(p, $"A symbol with name '{m.Name}' already exists", ("Other here:", other));
                                }
                            }
                        }
                    }
                }

                if (func.IsGeneric)
                    return;

                if (func.TryGetDirective("linkname", out var ln))
                {
                    if (ln.Arguments.Count != 1)
                    {
                        ReportError(ln, $"#linkname requires exactly one argument!");
                    }
                    else
                    {
                        var arg = ln.Arguments[0];
                        arg.SetFlag(ExprFlags.ValueRequired, true);
                        arg = ln.Arguments[0] = InferType(arg, null);
                        if (!(arg.Value is string))
                        {
                            ReportError(arg, $"Argument to #linkname must be a constant string!");
                        }
                    }
                }

                // define parameters
                foreach (var p in func.Parameters)
                {
                    p.ContainingFunction = func;
                    if (p.Name != null)
                    {
                        var (ok, other) = func.SubScope.DefineSymbol(p);
                        if (!ok)
                        {
                            ReportError(p, $"Duplicate parameter '{p.Name}'", ("Other parameter here:", other));
                        }
                    }

                    if (p.DefaultValue != null)
                    {
                        p.DefaultValue.Scope = func.Scope;
                        p.DefaultValue = InferTypeHelper(p.DefaultValue, p.Type, new TypeInferenceContext { TypeOfExprContext = p.Type });
                        ConvertLiteralTypeToDefaultType(p.DefaultValue, p.Type);
                        p.DefaultValue = CheckType(p.DefaultValue, p.Type);
                        if (p.DefaultValue.Type != p.Type && !p.DefaultValue.Type.IsErrorType)
                        {
                            ReportError(p.DefaultValue,
                                $"The type of the default value ({p.DefaultValue.Type}) does not match the type of the parameter ({p.Type})");
                        }

                    }
                }

                if (func.ReturnTypeExpr != null)
                    func.ReturnTypeExpr.Mutable = true;

                if (func.ReturnTypeExpr?.Name != null)
                {
                    func.ReturnTypeExpr.ContainingFunction = func;
                    func.ReturnTypeExpr.IsReturnParam = true;
                    var (ok, other) = func.SubScope.DefineSymbol(func.ReturnTypeExpr);
                    if (!ok)
                        ReportError(func.ReturnTypeExpr, $"A symbol with name '{func.ReturnTypeExpr.Name.Name}' already exists in current scope", ("Other symbol here:", other));
                }
                else if (func.ReturnTypeExpr != null)
                {
                    func.SubScope.DefineSymbol(func.ReturnTypeExpr, ".ret");
                }
                if (func.ReturnTypeExpr?.TypeExpr is AstTupleExpr t)
                {
                    int index = 0;
                    foreach (var m in t.Types)
                    {
                        if (m.Name == null) continue;
                        m.Mutable = true;
                        AstExpression access = new AstArrayAccessExpr(new AstSymbolExpr(func.ReturnTypeExpr), new AstNumberExpr(index, Location: func.ReturnTypeExpr.Location), func.ReturnTypeExpr.Location);
                        access = InferType(access, null);
                        var (ok, other) = func.SubScope.DefineUse(m.Name.Name, access, false, out var use);
                        if (!ok)
                            ReportError(m, $"A symbol with name '{m.Name.Name}' already exists in current scope", ("Other symbol here:", other));
                        m.Symbol = use;
                        ++index;
                    }
                }

                if (func.FunctionType.IsErrorType || func.FunctionType.IsPolyType)
                    return;

                if (func.Body != null && !func.IsMacroFunction)
                {
                    var errs = PushSilentErrorHandler();
                    func.Body.AttachTo(func, func.SubScope);
                    InferType(func.Body, null);

                    if (func.ReturnTypeExpr != null && !func.Body.GetFlag(ExprFlags.Returns))
                    {
                        // TODO: check that all return values are set
                        var ret = new AstReturnStmt(null, new Location(func.Body.End));
                        ret.Scope = func.Body.SubScope;
                        ret = AnalyseReturnStatement(ret);
                        func.Body.Statements.Add(ret);
                        func.Body.SetFlag(ExprFlags.Returns, true);
                    }

                    PopErrorHandler();

                    if (errs.HasErrors)
                    {
                        if (func.IsPolyInstance && func.InstantiatedAt != null)
                        {
                            ReportError($"Errors in polymorphic function '{func.Name}':");
                            errs.ForwardErrors(CurrentErrorHandler);

                            void ReportSources(AstFuncExpr func, string indent = "")
                            {
                                if (func.InstantiatedAt == null)
                                    return;
                                foreach (var loc in func.InstantiatedAt)
                                    ReportError(loc, indent + $"Failed to instantiate function '{func.Name}'");
                                foreach (var loc in func.InstantiatedBy)
                                {
                                    ReportError(loc, indent + $"Failed to instantiate function '{func.Name}'");
                                    ReportSources(loc, indent + "  ");
                                }
                            }

                            ReportError($"Caused from invocations here:");
                            ReportSources(func);
                        }
                        else
                        {
                            errs.ForwardErrors(CurrentErrorHandler);
                        }
                    }
                    else
                        PassVariableLifetimes(func);

                }
            }
            finally
            {
                currentFunction = prevCurrentFunction;
                PopLogScope();
                Log($"Finished function {func.Name}");
            }
        }

        private AstStatement AnalyseStatement(AstStatement stmt, out List<AstStatement> newStatements)
        {
            newStatements = null;
            switch (stmt)
            {
                case AstConstantDeclaration con: return AnalyseConstantDeclaration(con);
                case AstVariableDecl vardecl: newStatements = AnalyseVariableDecl(vardecl).Select(v => v as AstStatement).ToList(); break;
                case AstReturnStmt ret: return AnalyseReturnStatement(ret);
                case AstExprStmt expr: return AnalyseExprStatement(expr);
                case AstAssignment ass: return AnalyseAssignStatement(ass);
                case AstWhileStmt whl: return AnalyseWhileStatement(whl);
                case AstUsingStmt use: return AnalyseUseStatement(use);
                case AstForStmt fo: return AnalyseForStatement(fo);
                case AstDeferStmt def: return AnalyseDeferStatement(def);
            }

            return stmt;
        }

        private AstStatement AnalyseConstantDeclaration(AstConstantDeclaration c)
        {
            if (c.HasDirective("local"))
                c.SetFlag(StmtFlags.IsLocal, true);

            if (c.TypeExpr != null)
            {
                c.TypeExpr.AttachTo(c);
                c.TypeExpr.SetFlag(ExprFlags.ValueRequired, true);
                c.TypeExpr = ResolveTypeNow(c.TypeExpr, out var t);
                c.Type = t;
            }

            c.Initializer.AttachTo(c);
            c.Initializer.SetFlag(ExprFlags.ValueRequired, true);
            c.Initializer = InferType(c.Initializer, c.Type);

            if (c.Type == null)
                c.Type = c.Initializer.Type;
            else
                c.Initializer = CheckType(c.Initializer, c.Type);

            if (!c.Initializer.IsCompTimeValue)
            {
                ReportError(c.Initializer, $"Value of constant declaration must be constant");
                return c;
            }
            c.Value = c.Initializer.Value;

            CheckValueRangeForType(c.Type, c.Value, c.Initializer);

            var (ok, other) = c.GetFlag(StmtFlags.IsLocal) ?
                c.Scope.DefineLocalSymbol(c) :
                c.Scope.DefineSymbol(c);
            if (!ok)
                ReportError(c, $"A symbol with name '{c.Name.Name}' already exists in this scope", ("Other declaration here:", other));
            return c;
        }

        private AstStatement AnalyseDeferStatement(AstDeferStmt def)
        {
            def.Deferred.Scope = def.Scope;
            def.Deferred.Parent = def;

            AnalyseStatement(def.Deferred, out var v);
            if (v?.Count() > 0)
                ReportError(def, $"New statements not allowed");

            def.Scope.DefineSymbol(def, GetUniqueName("defer"));

            return def;
        }

        private AstStatement AnalyseForStatement(AstForStmt fo)
        {
            fo.SubScope = new Scope("for", fo.Scope);

            fo.Collection.SetFlag(ExprFlags.ValueRequired, true);
            fo.Collection.AttachTo(fo);
            fo.Collection = InferType(fo.Collection, null);
            ConvertLiteralTypeToDefaultType(fo.Collection, null);

            if (fo.Collection.Type.IsErrorType)
                return fo;

            fo.Body.AttachTo(fo);
            fo.Body.Scope = fo.SubScope;
            fo.Body = InferType(fo.Body, CheezType.Code);

            var fors = fo.Scope.GetForExtensions(fo.Collection.Type);


            var matches = fors.Select(func =>
            {
                var args = new List<AstArgument>
                {
                    new AstArgument(fo.Collection, Location: fo.Collection),
                    new AstArgument(fo.Body, Location: fo.Body)
                };
                if (fo.Arguments != null)
                    args.AddRange(fo.Arguments);

                var par = func.Parameters.Select(p => (p.Name?.Name, p.Type, p.DefaultValue)).ToArray();
                if (CheckAndMatchArgsToParams(args, par, false))
                    return (func, args);
                return (null, null);
            }).Where(a => a.func != null).ToList();

            if (matches.Count == 0)
            {
                var candidates = fors.Select(f => ("Tried this candidate:", f.ParameterLocation));
                ReportError(fo.Collection, $"No for extension matches type '{fo.Collection.Type}'", candidates);
                return fo;
            }
            else if (matches.Count > 1)
            {
                var candidates = matches.Select(f => ("This matches:", f.func.ParameterLocation));
                ReportError(fo.Collection, $"Multiple for extensions match type '{fo.Collection.Type}'", candidates);
                return fo;
            }
            else
            {
                AstVariableDecl CreateLink(AstIdExpr name, AstExpression expr, ILocation location)
                {
                    var link = new AstCompCallExpr(
                        new AstIdExpr("link", false, location),
                        new List<AstArgument> { new AstArgument(expr, Location: expr.Location) },
                        location);

                    var type = mCompiler.ParseExpression($"@typeof(@link({expr}))", new Dictionary<string, AstExpression>
                    {
                        { "it", name }
                    });

                    var varDecl = new AstVariableDecl(name, type, link, true, Location: location);
                    return varDecl;
                }

                var (func, args) = matches[0];
                var code = args[1].Expr;
                var links = new List<AstStatement>();

                var it = new AstIdExpr("it", false, fo.Location);
                var it_index = new AstIdExpr("it_index", false, fo.Location);

                // create links for it and it_index
                if (fo.VarName != null)
                    links.Add(CreateLink(fo.VarName, it, fo.VarName.Location));
                else
                    links.Add(CreateLink(it, it.Clone(), it.Location));

                if (fo.IndexName != null)
                    links.Add(CreateLink(fo.IndexName, it_index, fo.IndexName.Location));
                else
                    links.Add(CreateLink(it_index, it_index.Clone(), it_index.Location));

                // set break and continue
                if (fo.Label != null)
                {
                    var setBreakAndContinue = mCompiler.ParseStatement($"@set_break_and_continue({fo.Label.Name})");
                    links.Add(setBreakAndContinue);
                }

                // set value to null because it is not a code anymore
                code.TypeInferred = false;
                code.Value = null;
                links.Add(new AstExprStmt(code, code.Location));
                args[1].Expr = new AstBlockExpr(links, Location: fo.Body.Location);

                var call = new AstCallExpr(new AstFunctionRef(func, null, fo.Location), args, fo.Location);
                var exprStmt = new AstExprStmt(call, fo.Body.Location);
                exprStmt.Parent = fo.Parent;
                exprStmt.Scope = fo.SubScope;
                var result = AnalyseStatement(exprStmt, out var ns);
                Debug.Assert(ns == null);
                return result;
            }
        }

        private AstUsingStmt AnalyseUseStatement(AstUsingStmt use)
        {
            use.Value.SetFlag(ExprFlags.ValueRequired, true);
            use.Value.AttachTo(use);
            use.Value = InferType(use.Value, null);

            if (use.Value.Type.IsErrorType)
                return use;

            switch (use.Value.Type)
            {
                case CheezTypeType type:
                    HandleUseType(use);
                    break;

                case StructType str:
                    {
                        var tempVar = use.Value;
                        //if (!tempVar.GetFlag(ExprFlags.IsLValue))
                        {
                            tempVar = new AstTempVarExpr(use.Value, use.Value.GetFlag(ExprFlags.IsLValue));
                            tempVar.Replace(use.Value);
                            tempVar.SetFlag(ExprFlags.IsLValue, true);
                            tempVar = InferType(tempVar, use.Value.Type);
                            use.Value = tempVar;
                        }
                        ComputeStructMembers(str.Declaration);
                        foreach (var mem in str.Declaration.Members)
                        {
                            AstExpression expr = new AstDotExpr(tempVar, new AstIdExpr(mem.Name, false, use.Location), use.Location);
                            //expr = InferType(expr, null);
                            use.Scope.DefineUse(mem.Name, expr, true, out var u);
                        }
                    }
                    break;

                default:
                    ReportError(use, $"Can't use value of type '{use.Value.Type}'");
                    break;
            }

            return use;
        }

        private void HandleUseType(AstUsingStmt use)
        {
            switch (use.Value.Value as CheezType)
            {
                case EnumType e:
                    {
                        var decl = e.Declaration;
                        ComputeEnumMembers(decl);
                        foreach (var m in decl.Members)
                        {
                            var eve = new AstEnumValueExpr(e.Declaration, m);
                            use.Scope.DefineUse(m.Name, eve, true, out var u);
                        }
                        break;
                    }

                case GenericEnumType e:
                    {
                        var decl = e.Declaration;
                        ComputeEnumMembers(decl);
                        foreach (var m in decl.Members)
                        {
                            var eve = new AstEnumValueExpr(decl, m);
                            use.Scope.DefineUse(m.Name, eve, true, out var u);
                        }
                        break;
                    }

                default:
                    ReportError(use, $"Can't use type '{use.Value.Value}'");
                    break;
            }
        }

        private AstWhileStmt AnalyseWhileStatement(AstWhileStmt whl)
        {
            whl.SubScope = new Scope("loop", whl.Scope);
            whl.SubScope.DefineLoop(whl);
            whl.Body.AttachTo(whl, whl.SubScope);
            InferType(whl.Body, null);

            return whl;
        }

        private (bool isMutable, AstExpression failed) TryBorrowMutable(AstExpression expr, bool deref)
        {
            if (deref)
            {
                switch (expr.Type)
                {
                    case ReferenceType t: return (t.Mutable, expr);
                    case PointerType t: return (t.Mutable, expr);
                    //case SliceType t: return t.Mutable;
                }
            }

            switch (expr)
            {
                case AstIdExpr id:
                    switch (id.Symbol)
                    {
                        case AstVariableDecl decl:
                            return (decl.Mutable, expr);

                        case Using use:
                            return TryBorrowMutable(use.Expr, deref);

                        case AstParameter param:
                            return (param.Mutable, expr);

                        case null:
                            return (false, expr);

                        default:
                            throw new Exception("Not implemented");
                    }

                case AstSymbolExpr sym:
                    switch (sym.Symbol)
                    {
                        case AstVariableDecl decl:
                            return (decl.Mutable, expr);

                        case Using use:
                            return TryBorrowMutable(use.Expr, deref);

                        case AstParameter param:
                            return (param.Mutable, expr);

                        default:
                            throw new Exception("Not implemented");
                    }

                case AstDotExpr dot:
                    return TryBorrowMutable(dot.Left, true);

                case AstDereferenceExpr de:
                    return TryBorrowMutable(de.SubExpression, true);

                case AstArrayAccessExpr acc:
                    return TryBorrowMutable(acc.SubExpression, true);

                case AstTupleExpr tuple:
                    {
                        var subs = tuple.Values.Select(v => TryBorrowMutable(v, deref));
                        if (subs.Any(v => !v.isMutable))
                            return subs.FirstOrDefault(v => !v.isMutable);
                        return (true, expr);
                    }

                case AstTempVarExpr temp:
                    return TryBorrowMutable(temp.Expr, deref);
                    //return true;

                default:
                    ReportError(expr.Location, $"Invalid pattern ({expr.Type})");
                    //throw new Exception("Not implemented");
                    return (true, expr);
            }
        }

        private AstAssignment AnalyseAssignStatement(AstAssignment ass)
        {
            ass.Pattern.SetFlag(ExprFlags.ValueRequired, true);
            ass.Pattern.AttachTo(ass);
            ass.Pattern.SetFlag(ExprFlags.AssignmentTarget, true);
            ass.Pattern.SetFlag(ExprFlags.SetAccess, true);
            ass.Pattern.SetFlag(ExprFlags.RequireInitializedSymbol, ass.Operator != null);
            ass.Pattern = InferType(ass.Pattern, null);

            ass.Value.SetFlag(ExprFlags.ValueRequired, true);
            ass.Value.AttachTo(ass);
            ass.Value = InferType(ass.Value, ass.Pattern.Type, typeOfExprContext: ass.Pattern.Type);
            ConvertLiteralTypeToDefaultType(ass.Value, ass.Pattern.Type);
            if (ass.Value.Type is TraitType)
            {
                ReportError(ass.Value, $"Type {ass.Value.Type} can't be moved or copied");
            }


            if (!ass.Pattern.Type.IsErrorType && !ass.Value.Type.IsErrorType)
            {
                ass.Value = MatchPatternWithExpression(ass, ass.Pattern, ass.Value);
            }

            if (!ass.OnlyGenerateValue)
            {
                var (isMutable, failed) = TryBorrowMutable(ass.Pattern, false);
                if (!isMutable)
                {
                    ReportError(failed.Location, $"Can't assign to non-mutable pattern ({failed.Type}): '{failed}' is immutable");
                }
            }

            return ass;
        }

        private AstExpression MatchPatternWithExpression(AstAssignment ass, AstExpression pattern, AstExpression value)
        {
            // check for operator set[]
            if (ass.Pattern is AstArrayAccessExpr arr)
            {
                // before we search for operators, make sure that all impls for both arguments have been matched
                GetImplsForType(arr.SubExpression.Type);
                GetImplsForType(arr.Arguments[0].Type);
                GetImplsForType(value.Type);

                var ops = ass.Scope.GetNaryOperators("set[]", arr.SubExpression.Type, arr.Arguments[0].Type, value.Type);
                if (ops.Count == 0)
                {
                    var type = arr.SubExpression.Type;
                    if (type is ReferenceType r)
                        type = r.TargetType;
                    else
                        type = ReferenceType.GetRefType(type, true);
                    ops = ass.Scope.GetNaryOperators("set[]", type, arr.Arguments[0].Type, value.Type);
                }

                if (ops.Count == 0)
                {
                    if (!pattern.TypeInferred)
                    {
                        pattern.SetFlag(ExprFlags.AssignmentTarget, false);
                        ass.Pattern = pattern = InferType(pattern, null);
                    }
                }
                else if (ops.Count == 1)
                {
                    arr.SubExpression = HandleReference(arr.SubExpression, ops[0].ArgTypes[0], null);
                    var args = new List<AstExpression>
                    {
                        arr.SubExpression,
                        arr.Arguments[0],
                        value
                    };
                    var opCall = new AstNaryOpExpr("set[]", args, value.Location);
                    opCall.ActualOperator = ops[0];
                    opCall.Replace(value);
                    ass.OnlyGenerateValue = true;
                    return InferType(opCall, null);
                }
                else
                {
                    ReportError(ass, $"Multiple operators 'set[]' match the types ({arr.SubExpression.Type}, {arr.Arguments[0].Type}, {value.Type})");
                }
            }

            if (ass.Operator != null)
            {
                var assOp = ass.Operator + "=";
                var valType = LiteralTypeToDefaultType(value.Type);

                // before we search for operators, make sure that all impls for both arguments have been matched
                GetImplsForType(pattern.Type);
                GetImplsForType(valType);

                var ops = ass.Scope.GetBinaryOperators(assOp, pattern.Type, valType);
                if (ops.Count == 0)
                {
                    var type = pattern.Type;
                    if (type is ReferenceType r)
                        type = r.TargetType;
                    else
                        type = ReferenceType.GetRefType(type, true);
                    ops = ass.Scope.GetBinaryOperators(assOp, type, valType);
                }

                if (ops.Count == 1)
                {
                    ass.OnlyGenerateValue = true;
                    pattern = HandleReference(pattern, ops[0].LhsType, null);
                    var opCall = new AstBinaryExpr(assOp, pattern, value, value.Location);
                    opCall.Replace(value);
                    return InferType(opCall, null);
                }
                else if (ops.Count > 1)
                {
                    ReportError(ass, $"Multiple operators '{assOp}' match the types {PointerType.GetPointerType(pattern.Type, true)} and {value.Type}");
                }
            }

            switch (pattern)
            {
                case AstIdExpr id:
                    {
                        if (!id.GetFlag(ExprFlags.IsLValue))
                            ReportError(pattern, $"Can't assign to '{id}' because it is not an lvalue");

                        if (ass.Operator != null)
                        {
                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Replace(value);
                            newVal = InferType(newVal, pattern.Type);

                            return newVal;
                        }

                        ConvertLiteralTypeToDefaultType(ass.Value, pattern.Type);

                        return CheckType(ass.Value, ass.Pattern.Type, $"Can't assign a value of type {ass.Value.Type} to a pattern of type {ass.Pattern.Type}");
                    }

                case AstTupleExpr t:
                    {
                        if (value is AstTupleExpr v)
                        {
                            if (t.Values.Count != v.Values.Count)
                            {
                                ReportError(ass, $"Can't assign the tuple '{v}' to the pattern '{t}' because the amount of values does not match");
                                return value;
                            }

                            // create new assignments for all sub values
                            for (int i = 0; i < t.Values.Count; i++)
                            {
                                var subPat = t.Values[i];
                                var subVal = v.Values[i];
                                var subAss = new AstAssignment(subPat, subVal, ass.Operator, ass.Location);
                                subAss.Scope = ass.Scope;
                                subAss.Value = MatchPatternWithExpression(subAss, subPat, subVal);
                                ass.AddSubAssignment(subAss);
                            }
                        }
                        else
                        {
                            var tmp = new AstTempVarExpr(value);
                            tmp.SetFlag(ExprFlags.IsLValue, true);

                            // create new assignments for all sub values
                            for (int i = 0; i < t.Values.Count; i++)
                            {
                                AstExpression subVal = new AstArrayAccessExpr(tmp, new AstNumberExpr(i));
                                subVal.Scope = ass.Scope;
                                subVal = InferType(subVal, t.Values[i].Type);

                                var subAss = new AstAssignment(t.Values[i], subVal, ass.Operator, ass.Location);
                                subAss.Scope = ass.Scope;
                                subAss.Value = MatchPatternWithExpression(subAss, t.Values[i], subVal);
                                ass.AddSubAssignment(subAss);
                            }
                        }
                        break;
                    }

                case AstDereferenceExpr de:
                    {
                        if (!pattern.GetFlag(ExprFlags.IsLValue))
                            ReportError(pattern, $"Can't assign to '{pattern}' because it is not an lvalue");

                        if (ass.Operator != null)
                        {
                            if (!de.SubExpression.GetFlag(ExprFlags.IsLValue))
                            {
                                AstExpression tmp = new AstTempVarExpr(de.SubExpression);
                                tmp.SetFlag(ExprFlags.IsLValue, true);
                                tmp = InferType(tmp, de.SubExpression.Type);

                                de.SubExpression = tmp;
                            }

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Replace(value);
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }

                        ConvertLiteralTypeToDefaultType(ass.Value, pattern.Type);


                        return CheckType(ass.Value, ass.Pattern.Type, $"Can't assign a value of type {value.Type} to a pattern of type {pattern.Type}");
                    }

                case AstDotExpr dot:
                    {
                        if (!pattern.GetFlag(ExprFlags.IsLValue))
                            ReportError(pattern, $"Can't assign to '{pattern}' because it is not an lvalue");

                        if (ass.Operator != null)
                        {
                            AstExpression tmp = new AstTempVarExpr(dot.Left, true);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            //tmp = new AstDereferenceExpr(tmp, tmp.Location);
                            tmp = InferType(tmp, dot.Left.Type);

                            dot.Left = tmp;

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Replace(value);
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }

                        ConvertLiteralTypeToDefaultType(ass.Value, pattern.Type);


                        return CheckType(ass.Value, ass.Pattern.Type, $"Can't assign a value of type {value.Type} to a pattern of type {pattern.Type}");
                    }

                case AstArrayAccessExpr index:
                    {
                        if (!pattern.GetFlag(ExprFlags.IsLValue))
                            ReportError(pattern, $"Can't assign to '{pattern}' because it is not an lvalue");

                        if (ass.Operator != null)
                        {
                            AstExpression tmp = new AstTempVarExpr(index.SubExpression, true);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            tmp = InferType(tmp, index.SubExpression.Type);

                            index.SubExpression = tmp;

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Replace(value);
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }

                        ConvertLiteralTypeToDefaultType(ass.Value, pattern.Type);


                        return CheckType(ass.Value, ass.Pattern.Type, $"Can't assign a value of type {value.Type} to a pattern of type {pattern.Type}");
                    }

                case AstExpression e when e.Type is ReferenceType r:
                    {
                        if (!pattern.GetFlag(ExprFlags.IsLValue))
                            ReportError(pattern, $"Can't assign to '{pattern}' because it is not an lvalue");

                        // TODO: check if can be assigned to id (e.g. not const)
                        if (ass.Operator != null)
                        {
                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Replace(value);
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }

                        ConvertLiteralTypeToDefaultType(ass.Value, pattern.Type);
                        return CheckType(ass.Value, r.TargetType, $"Can't assign a value of type {value.Type} to a pattern of type {pattern.Type}");
                    }

                default: ReportError(pattern, $"Can't assign to '{pattern.Type}', not an lvalue"); break;
            }

            return value;
        }

        private IEnumerable<AstVariableDecl> AnalyseVariableDecl(AstVariableDecl decl)
        {
            foreach (var sub in SplitVariableDeclaration(decl))
                yield return sub;

            decl.ContainingFunction = currentFunction;
            ResolveVariableDecl(decl);

            var (ok, other) = decl.GetFlag(StmtFlags.IsLocal) ?
                decl.Scope.DefineLocalSymbol(decl) :
                decl.Scope.DefineSymbol(decl);
            if (!ok)
                ReportError(decl, $"A symbol with name '{decl.Name.Name}' already exists in this scope", ("Other declaration here:", other));
        }

        private AstExprStmt AnalyseExprStatement(AstExprStmt expr, bool allow_any_expr = false, bool infer_types = true)
        {
            expr.Expr.AttachTo(expr);

            if (infer_types)
            {
                expr.Expr = InferType(expr.Expr, null);
            }

            if (!allow_any_expr)
            {
                switch (expr.Expr)
                {
                    case AstIfExpr _:
                    case AstBlockExpr _:
                    case AstCallExpr _:
                    case AstCompCallExpr _:
                    case AstMatchExpr _:
                    case AstEmptyExpr _:
                    case AstBreakExpr _:
                    case AstContinueExpr _:
                    case AstMoveAssignExpr _:
                        break;

                    default:
                        ReportError(expr.Expr, $"This type of expression is not allowed here");
                        break;
                }
            }

            if (expr.Expr.Type.IsComptimeOnly)
            {
                ReportError(expr.Expr, $"This type of expression is not allowed here");
            }

            expr.SetFlag(StmtFlags.Returns, expr.Expr.GetFlag(ExprFlags.Returns)); 
            expr.SetFlag(StmtFlags.Breaks, expr.Expr.GetFlag(ExprFlags.Breaks));

            return expr;
        }

        private AstReturnStmt AnalyseReturnStatement(AstReturnStmt ret)
        {
            ret.SetFlag(StmtFlags.Returns);

            if (ret.ReturnValue != null)
            {
                ret.ReturnValue.SetFlag(ExprFlags.ValueRequired, true);
                ret.ReturnValue.AttachTo(ret);
                ret.ReturnValue = InferType(
                    ret.ReturnValue,
                    currentFunction.FunctionType.ReturnType,
                    typeOfExprContext: currentFunction.FunctionType.ReturnType);

                ConvertLiteralTypeToDefaultType(ret.ReturnValue, currentFunction.FunctionType.ReturnType);

                if (ret.ReturnValue.Type.IsErrorType)
                    return ret;

                ret.ReturnValue = CheckType(ret.ReturnValue, currentFunction.FunctionType.ReturnType, $"The type of the return value '{ret.ReturnValue.Type}' does not match the return type of the function '{currentFunction.FunctionType.ReturnType}'");

                if (ret.ReturnValue.Type is VoidType) {
                    ReportError(ret, $"Can't return void");
                }
            }
            return ret;
        }
    }
}
