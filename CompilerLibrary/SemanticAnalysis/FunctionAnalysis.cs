﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;

namespace Cheez
{
    public partial class Workspace
    {
        private void AnalyseFunctions(List<AstFunctionDecl> newInstances)
        {
            var nextInstances = new List<AstFunctionDecl>();

            int i = 0;
            while (i < MaxPolyFuncResolveStepCount && newInstances.Count != 0)
            {
                foreach (var instance in newInstances)
                {
                    AnalyseFunction(instance, nextInstances);
                }
                newInstances.Clear();

                var t = newInstances;
                newInstances = nextInstances;
                nextInstances = t;

                i++;
            }

            if (i == MaxPolyFuncResolveStepCount)
            {
                var details = newInstances.Select(str => ("Here:", str.Location)).ToList();
                ReportError($"Detected a potential infinite loop in polymorphic function declarations after {MaxPolyFuncResolveStepCount} steps", details);
            }
        }

        private void AnalyseFunction(AstFunctionDecl func, List<AstFunctionDecl> instances = null)
        {
            if (func.SelfParameter)
            {
                var p = func.Parameters[0];
                if (p.Name == null)
                {
                    p.Name = new AstIdExpr("self", false, p.Location);
                }

                if (func.ImplBlock.TargetType is StructType @struct)
                {
                    foreach (var m in @struct.Declaration.Members)
                    {
                        AstExpression expr = new AstDotExpr(new AstSymbolExpr(p), new AstIdExpr(m.Name.Name, false), false);
                        expr.Scope = func.SubScope;
                        expr.Parent = func;
                        expr = InferType(expr, m.Type);

                        var (ok, other) = func.SubScope.DefineUse(m.Name.Name, expr, out var use);

                        if (!ok)
                        {
                            ReportError(p, $"A symbol with name '{m.Name.Name}' already exists", ("Other here:", other));
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
                    p.DefaultValue = InferType(p.DefaultValue, p.Type);
                    ConvertLiteralTypeToDefaultType(p.DefaultValue, p.Type);
                    if (p.DefaultValue.Type != p.Type && !p.DefaultValue.Type.IsErrorType)
                    {
                        ReportError(p.DefaultValue,
                            $"The type of the default value ({p.DefaultValue.Type}) does not match the type of the parameter ({p.Type})");
                    }

                }
            }

            if (func.ReturnValue?.Name != null)
            {
                var (ok, other) = func.SubScope.DefineSymbol(func.ReturnValue);
                if (!ok)
                    ReportError(func.ReturnValue, $"A symbol with name '{func.ReturnValue.Name.Name}' already exists in current scope", ("Other symbol here:", other));
            }
            else
            {
                func.SubScope.DefineSymbol(func.ReturnValue, ".ret");
            }
            if (func.ReturnValue?.TypeExpr is AstTupleTypeExpr t)
            {
                int index = 0;
                foreach (var m in t.Members)
                {
                    if (m.Name == null) continue;
                    AstExpression access = new AstArrayAccessExpr(new AstSymbolExpr(func.ReturnValue), new AstNumberExpr(index));
                    access = InferType(access, null);
                    var (ok, other) = func.SubScope.DefineUse(m.Name.Name, access, out var use);
                    if (!ok)
                        ReportError(m, $"A symbol with name '{m.Name.Name}' already exists in current scope", ("Other symbol here:", other));
                    m.Symbol = use;
                    ++index;
                }
            }

            if (func.FunctionType.IsErrorType || func.FunctionType.IsPolyType)
                return;

            var prevCurrentFunction = currentFunction;
            currentFunction = func;
            if (func.Body != null)
            {
                func.Body.Scope = func.SubScope;
                func.Body.Parent = func;
                InferType(func.Body, func.FunctionType.ReturnType);

                if (func.ReturnValue != null && !func.Body.GetFlag(ExprFlags.Returns))
                {
                    // TODO: check that all return values are set
                    var ret = new AstReturnStmt(null, new Location(func.Body.End));
                    ret.Scope = func.Body.SubScope;
                    AnalyseStatement(ret);
                    func.Body.Statements.Add(ret);
                }
            }

            currentFunction = prevCurrentFunction;
        }

        private void AnalyseStatement(AstStatement stmt)
        {
            switch (stmt)
            {
                case AstVariableDecl vardecl: AnalyseVariableDecl(vardecl); break;
                case AstReturnStmt ret: AnalyseReturnStatement(ret); break;
                case AstExprStmt expr: AnalyseExprStatement(expr); break;
                case AstAssignment ass: AnalyseAssignStatement(ass); break;
                case AstWhileStmt whl: AnalyseWhileStatement(whl); break;
                case AstBreakStmt br: AnalyseBreakStatement(br); break;
                case AstContinueStmt cont: AnalyseContinueStatement(cont); break;
                default: throw new NotImplementedException();
            }
        }

        private AstWhileStmt FindFirstLoop(IAstNode node)
        {
            while (node != null)
            {
                if (node is AstWhileStmt whl)
                {
                    return whl;
                }
                node = node.Parent;
            }

            return null;
        }

        private void AnalyseContinueStatement(AstContinueStmt cont)
        {
            AstWhileStmt loop = FindFirstLoop(cont);
            if (loop == null)
            {
                ReportError(cont, $"continue can only occur inside of loops");
            }

            cont.Loop = loop;
        }

        private void AnalyseBreakStatement(AstBreakStmt br)
        {
            AstWhileStmt loop = FindFirstLoop(br);
            if (loop == null)
            {
                ReportError(br, $"break can only occur inside of loops");
            }

            br.Loop = loop;
        }

        private void AnalyseWhileStatement(AstWhileStmt whl)
        {
            whl.SubScope = new Scope("while", whl.Scope);

            if (whl.PreAction != null)
            {
                // TODO
                whl.PreAction.Scope = whl.Scope;
                whl.PreAction.Parent = whl;
                AnalyseStatement(whl.PreAction);
            }

            whl.Condition.Scope = whl.SubScope;
            whl.Condition.Parent = whl;
            whl.Condition = InferType(whl.Condition, CheezType.Bool);
            ConvertLiteralTypeToDefaultType(whl.Condition);
            if (whl.Condition.Type != CheezType.Bool && !whl.Condition.Type.IsErrorType)
                ReportError(whl.Condition, $"The condition of a while statement must be a bool but is a {whl.Condition.Type}");

            if (whl.PostAction != null)
            {
                whl.PostAction.Scope = whl.SubScope;
                whl.PostAction.Parent = whl;
                AnalyseStatement(whl.PostAction);
            }

            whl.Body.Scope = whl.SubScope;
            whl.Body.Parent = whl;
            InferType(whl.Body, CheezType.Void);
        }

        private void AnalyseAssignStatement(AstAssignment ass)
        {
            ass.Value.Parent = ass;

            ass.Pattern.Scope = ass.Scope;
            ass.Pattern.Parent = ass;
            ass.Pattern = InferType(ass.Pattern, null);

            ass.Value.Scope = ass.Scope;
            ass.Value = InferType(ass.Value, ass.Pattern.Type);


            if (ass.Pattern.Type != CheezType.Error && ass.Value.Type != CheezType.Error)
            {
                ass.Value = MatchPatternWithExpression(ass, ass.Pattern, ass.Value);
            }
        }

        private AstExpression MatchPatternWithExpression(AstAssignment ass, AstExpression pattern, AstExpression value)
        {
            if (ass.Operator != null)
            {
                var assOp = ass.Operator + "=";
                var valType = LiteralTypeToDefaultType(value.Type);
                var ops = ass.Scope.GetOperators(assOp, PointerType.GetPointerType(pattern.Type), valType);
                if (ops.Count == 1)
                {
                    var left = new AstAddressOfExpr(pattern, pattern.Location);
                    var opCall = new AstBinaryExpr(assOp, left, value, value.Location);
                    opCall.Scope = value.Scope;
                    ass.OnlyGenerateValue = true;
                    return InferType(opCall, null);
                }
                else if (ops.Count > 1)
                {
                    ReportError(ass, $"Multiple operators '{assOp}' match the types {PointerType.GetPointerType(pattern.Type)} and {value.Type}");
                }
            }

            switch (pattern)
            {
                case AstIdExpr id:
                    {
                        // TODO: check if can be assigned to id (e.g. not const)
                        ass.Scope.SetInitialized(id.Symbol);

                        if (ass.Operator != null)
                        {
                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Scope = value.Scope;
                            newVal.Parent = value.Parent;
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }

                        if (ass.Value.Type != ass.Pattern.Type && !ass.Pattern.Type.IsErrorType)
                        {
                            ReportError(ass, $"Can't assign a value of type {value.Type} to a pattern of type {pattern.Type}");
                        }
                        break;
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
                        if (ass.Operator != null)
                        {
                            AstExpression tmp = new AstTempVarExpr(de.SubExpression);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            tmp = InferType(tmp, de.SubExpression.Type);

                            de.SubExpression = tmp;

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Scope = value.Scope;
                            newVal.Parent = value.Parent;
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }
                        break;
                    }

                case AstDotExpr dot:
                    {
                        if (ass.Operator != null)
                        {
                            AstExpression tmp = new AstTempVarExpr(dot.Left, true);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            //tmp = new AstDereferenceExpr(tmp, tmp.Location);
                            tmp = InferType(tmp, dot.Left.Type);

                            dot.Left = tmp;

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Scope = value.Scope;
                            newVal.Parent = value.Parent;
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }
                        break;
                    }

                case AstArrayAccessExpr index:
                    {
                        if (ass.Operator != null)
                        {
                            AstExpression tmp = new AstTempVarExpr(index.SubExpression, true);
                            tmp.SetFlag(ExprFlags.IsLValue, true);
                            tmp = InferType(tmp, index.SubExpression.Type);

                            index.SubExpression = tmp;

                            AstExpression newVal = new AstBinaryExpr(ass.Operator, pattern, value, value.Location);
                            newVal.Scope = value.Scope;
                            newVal.Parent = value.Parent;
                            newVal = InferType(newVal, pattern.Type);
                            return newVal;
                        }
                        break;
                    }

                default: ReportError(pattern, $"Can't assign to the pattern '{pattern}', not an lvalue"); break;
            }

            return value;
        }

        private void AnalyseVariableDecl(AstVariableDecl vardecl)
        {
            Pass1VariableDeclaration(vardecl);
            Pass6VariableDeclaration(vardecl);

            if (vardecl.Type is SumType)
            {
                ReportError(vardecl.Pattern, $"Invalid type for variable declaration: {vardecl.Type}");
            }
        }

        private void AnalyseExprStatement(AstExprStmt expr, bool allow_any_expr = false, bool infer_types = true)
        {
            expr.Expr.Parent = expr;

            if (infer_types)
            {
                expr.Expr.Scope = expr.Scope;
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
                        break;

                    default:
                        ReportError(expr.Expr, $"This type of expression is not allowed here");
                        break;
                }
            }

            if (expr.Expr.GetFlag(ExprFlags.Returns))
            {
                expr.SetFlag(StmtFlags.Returns);
            }
        }

        private void AnalyseReturnStatement(AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                ret.ReturnValue.Scope = ret.Scope;
                ret.ReturnValue.Parent = ret;
                ret.ReturnValue = InferType(ret.ReturnValue, currentFunction.FunctionType.ReturnType);

                ConvertLiteralTypeToDefaultType(ret.ReturnValue);

                if (ret.ReturnValue.Type != currentFunction.FunctionType.ReturnType && !ret.ReturnValue.Type.IsErrorType)
                {
                    ReportError(ret.ReturnValue,
                        $"The type of the return value ({ret.ReturnValue.Type}) does not match the return type of the function ({currentFunction.FunctionType.ReturnType})");
                }
            }
            else if (currentFunction.ReturnValue != null)
            {
                // TODO: check wether all return values have been assigned
                var missing = new List<ILocation>();
                if (currentFunction.ReturnValue.Name == null)
                {
                    if (currentFunction.ReturnValue.TypeExpr is AstTupleTypeExpr t)
                    {
                        foreach (var m in t.Members)
                            if (m.Symbol == null || !ret.Scope.IsInitialized(m.Symbol))
                                missing.Add(m);
                    }
                    else
                    {
                        ReportError(ret, $"Not all code paths return a value");
                    }
                }
                else
                {
                    if (!ret.Scope.IsInitialized(currentFunction.ReturnValue))
                    {
                        if (currentFunction.ReturnValue.TypeExpr is AstTupleTypeExpr t && t.IsFullyNamed)
                        {
                            foreach (var m in t.Members)
                                if (!ret.Scope.IsInitialized(m.Symbol))
                                    missing.Add(m);
                        }
                        else
                        {
                            missing.Add(currentFunction.ReturnValue);
                        }
                    }
                }


                if (missing.Count > 0)
                {
                    ReportError(ret, $"Not all return values have been initialized", missing.Select(l => ("This one is not initialized:", l)));
                }
            }

            ret.SetFlag(StmtFlags.Returns);
        }
    }
}
