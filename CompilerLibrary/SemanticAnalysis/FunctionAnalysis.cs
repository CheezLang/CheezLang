using System;
using System.Collections.Generic;
using System.Linq;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types.Complex;

namespace Cheez
{
    public partial class Workspace
    {
        private void AnalyzeFunctions(List<AstFunctionDecl> newInstances)
        {
            var nextInstances = new List<AstFunctionDecl>();

            int i = 0;
            while (i < MaxPolyFuncResolveStepCount && newInstances.Count != 0)
            {
                foreach (var instance in newInstances)
                {
                    AnalyzeFunction(instance, nextInstances);
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

        private void AnalyzeFunction(AstFunctionDecl func, List<AstFunctionDecl> instances = null)
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
            }

            if (func.Body != null)
            {
                func.Body.Scope = func.SubScope;
                AnalyzeStatement(func, func.Body);
            }
        }

        private void AnalyzeStatement(AstFunctionDecl func, AstStatement stmt)
        {
            switch (stmt)
            {
                case AstVariableDecl vardecl: AnalyzeVariableDecl(vardecl); break;
                case AstBlockStmt block: AnalyzeBlockStatement(func, block); break;
                case AstReturnStmt ret: AnalyzeReturnStatement(func, ret); break;
                case AstExprStmt expr: AnalyseExprStatement(expr); break;
                case AstAssignment ass: AnalyseAssignStatement(ass); break;
                default: throw new NotImplementedException();
            }
        }

        private void AnalyseAssignStatement(AstAssignment ass)
        {
            //ass.Value.Scope = ass.Scope;
            //InferType(ass.Value, null);

            ass.Pattern.Scope = ass.Scope;
            InferType(ass.Pattern, null);

            MatchPatternWithExpression(ass, ass.Pattern, ass.Value);
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

                        if (value.Type != id.Type)
                        {
                            ReportError(ass, $"Can't assign a value of type {value.Type} to the variable '{id.Name}' of type {id.Type}");
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

        private void AnalyzeVariableDecl(AstVariableDecl vardecl)
        {
            Pass1VariableDeclaration(vardecl);
            Pass6VariableDeclaration(vardecl);
        }

        private void AnalyseExprStatement(AstExprStmt expr)
        {
            expr.Expr.Scope = expr.Scope;
            InferType(expr.Expr, null);
        }

        private void AnalyzeReturnStatement(AstFunctionDecl func, AstReturnStmt ret)
        {
            if (ret.ReturnValue != null)
            {
                ret.ReturnValue.Scope = ret.Scope;
                InferType(ret.ReturnValue, null);

                ConvertLiteralTypeToDefaultType(ret.ReturnValue);

                if (ret.ReturnValue.Type != func.FunctionType.ReturnType)
                {
                    ReportError(ret.ReturnValue,
                        $"The type of the return value ({ret.ReturnValue.Type}) does not match the return type of the function ({func.FunctionType.ReturnType})");
                }
            }
        }

        private void AnalyzeBlockStatement(AstFunctionDecl func, AstBlockStmt block)
        {
            foreach (var stmt in block.Statements)
            {
                stmt.Scope = block.Scope;
                AnalyzeStatement(func, stmt);
            }

            if (block.Statements.LastOrDefault() is AstExprStmt expr)
            {
                // TODO
            }
        }
    }
}
