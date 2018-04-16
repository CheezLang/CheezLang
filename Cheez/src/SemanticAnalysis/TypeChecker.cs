using Cheez.Ast;
using Cheez.Parsing;
using Cheez.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.SemanticAnalysis
{
    public class WaitForType : Exception
    {
        public string TypeName { get; private set; }

        public WaitForType(string type)
        {
            this.TypeName = type;
        }
    }

    public struct TypeCheckerData
    {
        public Scope scope;

        public TypeCheckerData(Scope scope)
        {
            this.scope = scope;
        }
    }

    public struct TypeCheckResult
    {
        public Scope scope;
        public object ast;
        public CheezType type;

        public Expression expr => (Expression)ast;
        public Statement stmt => (Statement)ast;

        public TypeCheckResult(object ast, CheezType type = null, Scope scope = null)
        {
            this.scope = scope;
            this.ast = ast;
            this.type = type;
        }
    }

    public class TypeChecker : VisitorBase<TypeCheckResult, TypeCheckerData>
    {
        private Workspace workspace;

        public TypeChecker(Workspace w)
        {
            workspace = w;
        }

        private Scope GetScope(object ast)
        {
            return workspace.GetScope(ast);
        }

        [DebuggerStepThrough]
        public TypeCheckResult CheckTypes(Statement statement, Scope Scope)
        {
            return statement.Accept(this, new TypeCheckerData
            {
                scope = Scope
            });
        }

        [DebuggerStepThrough]
        private TypeCheckResult CheckTypes(Expression expr, Scope Scope)
        {
            return expr.Accept(this, new TypeCheckerData
            {
                scope = Scope
            });
        }

        public override TypeCheckResult VisitExpressionStatement(ExpressionStatement stmt, TypeCheckerData data = default)
        {
            var result = CheckTypes(stmt.Expr, data.scope);
            stmt.Expr = result.expr;
            workspace.SetCheezType(stmt.Expr, result.type);
            return new TypeCheckResult(stmt, CheezType.Void);
        }

        public override TypeCheckResult VisitFunctionDeclaration(FunctionDeclarationAst function, TypeCheckerData data = default)
        {
            var funcScope = new Scope(data.scope);
            var scope = funcScope;
            
            // check parameter types
            {
                foreach (var p in function.Parameters)
                {
                    var type = workspace.GetCheezType(p);
                    funcScope.DefineVariable(p.Name, p, type);
                }
            }

            // check body
            foreach (var s in function.Statements)
            {
                var result = CheckTypes(s, scope);
                scope = result.scope ?? scope;
            }

            return default;
        }

        public override TypeCheckResult VisitPrintStatement(PrintStatement print, TypeCheckerData data = default)
        {
            for (int i = 0; i < print.Expressions.Count; i++)
            {
                var result = CheckTypes(print.Expressions[i], data.scope);
                if (result.type == null || result.type == CheezType.Void)
                {
                    workspace.ReportError(print.Expressions[i], $"Cannot print value of type '{result.type}'");
                }
                print.Expressions[i] = result.expr;
                workspace.SetCheezType(result.expr, result.type);
            }
            return new TypeCheckResult(print);
        }

        public override TypeCheckResult VisitVariableDeclaration(VariableDeclarationAst varAst, TypeCheckerData data = default)
        {
            var scope = data.scope;

            CheezType type = null;
            if (varAst.Type != null)
            {
                type = scope.GetCheezType(varAst.Type);
                if (type == null)
                {
                    workspace.ReportError(varAst.Type, $"Unknown type '{varAst.Type}'");
                    return new TypeCheckResult(varAst, CheezType.Void);
                }

                if (varAst.Initializer != null)
                {
                    var initResult = CheckTypes(varAst.Initializer, scope);
                    var castResult = InsertCastExpressionIf(initResult.expr, initResult.type, type);
                    varAst.Initializer = castResult.expr;
                    workspace.SetCheezType(varAst.Initializer, castResult.type);
                    if (castResult.type != type)
                    {
                        workspace.ReportError(varAst.Initializer, $"Type of initialization does not match type of variable. Expected {type}, got {initResult.type}");
                        return new TypeCheckResult(varAst, CheezType.Void);
                    }
                }
            }
            else
            {
                if (varAst.Initializer == null)
                {
                    workspace.ReportError(varAst, $"Type of variable must be explictly specified if no initial value is given");
                    return new TypeCheckResult(varAst, CheezType.Void);
                }

                var init = CheckTypes(varAst.Initializer, scope);
                if (init.type == IntType.LiteralType)
                {
                    init.type = IntType.DefaultType;
                }
                varAst.Initializer = init.expr;
                workspace.SetCheezType(varAst.Initializer, init.type);
                type = init.type;
            }

            if (!scope.DefineVariable(varAst.Name, varAst, type))
            {
                workspace.ReportError(varAst.NameLocation, $"Variable '{varAst.Name}' already exists in current scope!");
                return new TypeCheckResult(varAst, CheezType.Void);
            }

            workspace.SetCheezType(varAst, type);

            return new TypeCheckResult(varAst, CheezType.Void, new Scope(scope));
        }

        public override TypeCheckResult VisitStringLiteral(StringLiteral str, TypeCheckerData data = default)
        {
            return new TypeCheckResult(str, CheezType.String);
        }

        public override TypeCheckResult VisitNumberExpression(NumberExpression lit, TypeCheckerData data = default)
        {
            return new TypeCheckResult(lit, IntType.LiteralType);
        }

        public override TypeCheckResult VisitIdentifierExpression(IdentifierExpression ident, TypeCheckerData data = default)
        {
            var variable = data.scope.GetVariable(ident.Name);

            if (variable == null)
            {
                workspace.ReportError(ident, $"No variable called '{ident.Name}' exists in current or surrounding scope");
                return new TypeCheckResult(ident);
            }

            return new TypeCheckResult(ident, variable?.type);
        }

        public override TypeCheckResult VisitBinaryExpression(BinaryExpression bin, TypeCheckerData data = default)
        {
            var scope = data.scope;

            var lhs = CheckTypes(bin.Left, scope);
            var rhs = CheckTypes(bin.Right, scope);

            bin.Left = lhs.expr;
            bin.Right = rhs.expr;

            if (bin.Left is NumberExpression n1 && bin.Right is NumberExpression n2)
            {
                return new TypeCheckResult(new NumberExpression(bin.Beginning, bin.End, OperateLiterals(n1.Data, n2.Data, bin.Operator)), IntType.LiteralType);
            }

            if (lhs.type == IntType.LiteralType)
            {
                lhs.type = rhs.type;
            }
            else if (rhs.type == IntType.LiteralType)
            {
                rhs.type = lhs.type;
            }


            if (lhs.type != rhs.type)
            {
                workspace.ReportError(bin, $"Type of left hand side and right hand side in binary expression do not match. LHS is {lhs.type}, RHS is {rhs.type}");
                return new TypeCheckResult(bin);
            }

            workspace.SetCheezType(bin.Left, lhs.type);
            workspace.SetCheezType(bin.Right, rhs.type);
            return new TypeCheckResult(bin, lhs.type);
        }

        public override TypeCheckResult VisitCallExpression(CallExpression call, TypeCheckerData data)
        {
            var scope = data.scope;

            if (call.Function is IdentifierExpression id)
            {
                List<CheezType> argTypes = new List<CheezType>();
                bool argTypesOk = true;
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var a = call.Arguments[i];
                    var arg = CheckTypes(a, scope);
                    if (arg.type == null || arg.type == CheezType.Void)
                        argTypesOk = false;
                    call.Arguments[i] = arg.expr;
                    workspace.SetCheezType(arg.ast, arg.type);

                    argTypes.Add(arg.type);
                }

                if (!argTypesOk)
                {
                    workspace.ReportError(call, "Invalid arguments in function call!");
                    return new TypeCheckResult(call);
                }

                var func = scope.GetFunction(id.Name, argTypes);
                if (func == null)
                {
                    workspace.ReportError(call, "No function matches call!");
                    return new TypeCheckResult(call);
                }

                var type = scope.GetCheezType(func.ReturnType);
                if (type == null)
                {
                    workspace.ReportError(call, "Return type of function does not exist!");
                    return new TypeCheckResult(call);
                }

                // @Temp
                // check if types match
                if (argTypes.Count != func.Parameters.Count)
                {
                    workspace.ReportError(call, $"Wrong number of arguments in function call");
                    return new TypeCheckResult(call);
                }
                else
                {
                    List<Expression> args = new List<Expression>();
                    foreach (var (givenAst, expectedAst) in call.Arguments.Zip(func.Parameters, (a, b) => (a, b)))
                    {
                        var given = workspace.GetCheezType(givenAst);
                        var expected = workspace.GetCheezType(expectedAst);

                        var result = InsertCastExpressionIf(givenAst, given, expected);
                        args.Add(result.expr);
                        workspace.SetCheezType(result.expr, result.type);

                        if (result.type != expected)
                        {
                            workspace.ReportError(givenAst, $"Argument types in function call do not match. Expected {expected}, got {given}");
                        }
                    }

                    call.Arguments = args;
                }

                return new TypeCheckResult(call, type);
            }

            return new TypeCheckResult(call);
        }

        #region Helper Methods

        private TypeCheckResult InsertCastExpressionIf(Expression e, CheezType sourceType, CheezType targetType)
        {
            if (sourceType == targetType)
                return new TypeCheckResult(e, targetType);
            if (sourceType == IntType.LiteralType && targetType is IntType)
            {
                workspace.SetCheezType(e, targetType);
                return new TypeCheckResult(e, targetType);
            }

            //workspace.ReportError(e, $"Can't cast {sourceType} to {targetType}");
            return new TypeCheckResult(e, sourceType);
        }

        private double OperateNumbers(double a, double b, BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Add:
                    return a + b;
                case BinaryOperator.Subtract:
                    return a - b;
                case BinaryOperator.Multiply:
                    return a * b;
                case BinaryOperator.Divide:
                    return a / b;
                default:
                    throw new Exception();
            }
        }

        private long OperateNumbers(long a, long b, BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Add:
                    return a + b;
                case BinaryOperator.Subtract:
                    return a - b;
                case BinaryOperator.Multiply:
                    return a * b;
                case BinaryOperator.Divide:
                    return a / b;
                default:
                    throw new Exception();
            }
        }

        private double ParseNumberDataFloat(NumberData data)
        {
            if (data.Type == NumberData.NumberType.Float)
            {
                if (data.Value != null)
                    return (double)data.Value;
                return double.Parse(data.StringValue);
            }

            if (data.Value != null)
                return (long)data.Value;
            switch (data.IntBase)
            {
                case 10:
                    return long.Parse(data.StringValue);
                case 16:
                    return long.Parse(data.StringValue, System.Globalization.NumberStyles.HexNumber);

                default:
                    throw new Exception();
            }
        }

        private long ParseNumberDataInt(NumberData data)
        {
            if (data.Type == NumberData.NumberType.Float)
            {
                throw new Exception();
            }

            if (data.Value != null)
                return (long)data.Value;
            switch (data.IntBase)
            {
                case 10:
                    return long.Parse(data.StringValue);
                case 16:
                    return long.Parse(data.StringValue, System.Globalization.NumberStyles.HexNumber);

                default:
                    throw new Exception();
            }
        }

        private NumberData OperateLiterals(NumberData data1, NumberData data2, BinaryOperator op)
        {
            if (data1.Type == NumberData.NumberType.Float || data2.Type == NumberData.NumberType.Float)
            {
                double d1 = ParseNumberDataFloat(data1);
                double d2 = ParseNumberDataFloat(data2);
                double result = OperateNumbers(d1, d2, op);
                return new NumberData
                {
                    IntBase = 10,
                    StringValue = result.ToString(),
                    Suffix = "",
                    Type = NumberData.NumberType.Float,
                    Value = result
                };
            }
            {
                long d1 = ParseNumberDataInt(data1);
                long d2 = ParseNumberDataInt(data2);
                long result = OperateNumbers(d1, d2, op);
                return new NumberData
                {
                    IntBase = 10,
                    StringValue = result.ToString(),
                    Suffix = "",
                    Type = NumberData.NumberType.Int,
                    Value = result
                };
            }
        }

        #endregion
    }
}
