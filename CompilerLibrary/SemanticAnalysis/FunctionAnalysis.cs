using System;
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
            if (func.TryGetDirective("linkname", out var ln))
            {
                if (ln.Arguments.Count != 1)
                {
                    ReportError(ln, $"#linkname requires exactly one argument!");
                }
                else
                {
                    var arg = ln.Arguments[0];
                    InferType(arg, null);
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
                    InferType(p.DefaultValue, p.Type);
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
                    var access = new AstArrayAccessExpr(new AstSymbolExpr(func.ReturnValue), new AstNumberExpr(new Extras.NumberData(index)));
                    InferType(access, null);
                    var (ok, other) = func.SubScope.DefineUse(m.Name, access, out var use);
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
                default: throw new NotImplementedException();
            }
        }

        private void AnalyseAssignStatement(AstAssignment ass)
        {
            ass.Pattern.Scope = ass.Scope;
            InferType(ass.Pattern, null);
            if (ass.Pattern.Type != CheezType.Error)
            {
                MatchPatternWithExpression(ass, ass.Pattern, ass.Value);
            }
        }

        private void MatchPatternWithExpression(AstAssignment ass, AstExpression pattern, AstExpression value)
        {
            switch (pattern)
            {
                case AstIdExpr id:
                    {
                        // TODO: check if can be assigned to id (e.g. not const)

                        value.Scope = ass.Scope;
                        InferType(value, id.Type);
                        if (value.Type == CheezType.Error)
                            break;

                        if (value.Type != id.Type)
                        {
                            ReportError(ass, $"Can't assign a value of type {value.Type} to the variable '{id.Name}' of type {id.Type}");
                        }

                        ass.Scope.SetInitialized(id.Symbol);
                        break;
                    }

                case AstTupleExpr t:
                    {
                        if (value is AstTupleExpr v)
                        {
                            if (t.Values.Count != v.Values.Count)
                            {
                                ReportError(ass, $"Can't assign the tuple '{v}' to the pattern '{t}' because the amount of values does not match");
                                return;
                            }

                            // create new assignments for all sub values
                            for (int i = 0; i < t.Values.Count; i++)
                            {
                                var subPat = t.Values[i];
                                var subVal = v.Values[i];
                                var subAss = new AstAssignment(subPat, subVal, null, ass.Location);
                                subAss.Scope = ass.Scope;
                                MatchPatternWithExpression(subAss, subPat, subVal);
                                ass.AddSubAssignment(subAss);
                            }
                        }
                        else
                        {
                            value.Scope = ass.Scope;
                            InferType(value, t.Type);

                            var tmp = new AstTempVarExpr(value);
                            tmp.SetFlag(ExprFlags.IsLValue, true);

                            if (value.Type != t.Type)
                            {
                                ReportError(ass, $"Can't assign a value of type {value.Type} to the pattern '{t}' of type {t.Type}");
                                return;
                            }

                            // create new assignments for all sub values
                            for (int i = 0; i < t.Values.Count; i++)
                            {
                                var subVal = new AstArrayAccessExpr(tmp, new AstNumberExpr(new Extras.NumberData(i)));
                                var subAss = new AstAssignment(t.Values[i], subVal);
                                subAss.Scope = ass.Scope;
                                MatchPatternWithExpression(subAss, t.Values[i], subVal);
                                ass.AddSubAssignment(subAss);
                            }
                        }
                        break;
                    }

                default: throw new NotImplementedException();
            }
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

        private void AnalyseExprStatement(AstExprStmt expr)
        {
            expr.Expr.Scope = expr.Scope;
            InferType(expr.Expr, null);
        }

        private void AnalyseReturnStatement(AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                ret.ReturnValue.Scope = ret.Scope;
                InferType(ret.ReturnValue, null);

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
                        ReportError(ret, $"Return value has to be provided in non void function");
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
