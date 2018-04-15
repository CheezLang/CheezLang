using Cheez.Ast;
using Cheez.Parsing;
using Cheez.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public IScope scope;

        public TypeCheckerData(IScope scope)
        {
            this.scope = scope;
        }
    }

    public class TypeChecker : VisitorBase<(CType type, object ast), TypeCheckerData>
    {
        private Workspace workspace;

        public TypeChecker(Workspace w)
        {
            workspace = w;
        }

        private IScope GetScope(object ast)
        {
            return workspace.GetScope(ast);
        }

        [DebuggerStepThrough]
        public (CType type, object ast) CheckTypes(Statement statement, IScope Scope = null)
        {
            return ((CType, Statement)) statement.Accept(this, new TypeCheckerData
            {
                scope = Scope
            });
        }

        [DebuggerStepThrough]
        private (CType type, Expression ast) CheckTypes(Expression expr, IScope Scope = null)
        {
            return ((CType, Expression)) expr.Accept(this, new TypeCheckerData
            {
                scope = Scope
            });
        }

        //[DebuggerStepThrough]
        //private CType CheckTypes(Expression expr, TypeCheckerData data)
        //{
        //    return expr.Accept(this, data);
        //}

        public override (CType type, object ast) VisitFunctionDeclaration(FunctionDeclaration function, TypeCheckerData data = default)
        {
            var funcScope = workspace.GetFunctionScope(function);
            foreach (var s in function.Statements)
            {
                CheckTypes(s, funcScope);
            }

            return (CType.Void, function);
        }

        public override (CType, object) VisitVariableDeclaration(VariableDeclaration variable, TypeCheckerData data = default)
        {
            var scope = data.scope;

            CType type = null;
            if (variable.Type != null)
            {
                type = scope.Types.GetCType(variable.Type);
                if (type == null)
                {
                    workspace.ReportError(variable.Type, $"Unknown type '{variable.Type}'");
                    return (CType.Void, variable);
                }

                if (variable.Initializer != null)
                {
                    var init = CheckTypes(variable.Initializer, scope);
                    (init.type, variable.Initializer) = CastExpression(init.ast, init.type, type);
                    workspace.SetType(variable.Initializer, init.type);
                    if (init.type != type)
                    {
                        workspace.ReportError(variable.Initializer, $"Type of initialization does not match type of variable. Expected {type}, got {init.type}");
                        return (CType.Void, variable);
                    }
                }
            }
            else
            {
                if (variable.Initializer == null)
                {
                    workspace.ReportError(variable, $"Type of variable must be explictly specified if no initial value is given");
                    return (CType.Void, variable);
                }

                var init = CheckTypes(variable.Initializer, scope);
                if (init.type == IntType.LiteralType)
                {
                    init.type = IntType.DefaultType;
                }
                variable.Initializer = init.ast;
                workspace.SetType(variable.Initializer, init.type);
                type = init.type;
            }

            if (!scope.DefineVariable(variable.Name, variable, type))
            {
                workspace.ReportError(variable.NameLocation, $"Variable '{variable.Name}' already exists in current scope!");
                return (CType.Void, variable);
            }

            workspace.SetType(variable, type);

            return (CType.Void, variable);
        }

        private (CType, Expression) CastExpression(Expression e, CType sourceType, CType targetType)
        {
            if (sourceType == targetType)
                return (targetType, e);
            if (sourceType == IntType.LiteralType && targetType is IntType)
            {
                workspace.SetType(e, targetType);
                return (targetType, e);
            }

            workspace.ReportError(e, $"Can't cast {sourceType} to {targetType}");
            return (sourceType, e);
        }

        public override (CType, object) VisitNumberExpression(NumberExpression lit, TypeCheckerData data = default)
        {
            return (IntType.LiteralType, lit);
        }

        public override (CType type, object ast) VisitIdentifierExpression(IdentifierExpression ident, TypeCheckerData data = default)
        {
            var variable = data.scope.GetVariable(ident.Name);

            if (variable == null)
            {
                workspace.ReportError(ident, $"No variable called '{ident.Name}' exists in current or surrounding scope");
                return (null, ident);
            }

            return (variable?.type, ident);
        }

        public override (CType, object) VisitBinaryExpression(BinaryExpression bin, TypeCheckerData data = default)
        {
            var scope = data.scope;

            var lhs = CheckTypes(bin.Left, scope);
            var rhs = CheckTypes(bin.Right, scope);

            bin.Left = lhs.ast;
            bin.Right = rhs.ast;

            if (bin.Left is NumberExpression n1 && bin.Right is NumberExpression n2)
            {
                return (IntType.LiteralType, new NumberExpression(bin.Beginning, bin.End, OperateLiterals(n1.Data, n2.Data, bin.Operator)));
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
                return (null, bin);
            }

            workspace.SetType(bin.Left, lhs.type);
            workspace.SetType(bin.Right, rhs.type);
            return (lhs.type, bin);
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

        public override (CType, object) VisitCallExpression(CallExpression call, TypeCheckerData data)
        {
            var scope = data.scope;

            if (call.Function is IdentifierExpression id)
            {
                List<CType> argTypes = new List<CType>();
                bool argTypesOk = true;
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var a = call.Arguments[i];
                    var arg = CheckTypes(a, scope);
                    if (arg.type == null || arg.type == CType.Void)
                        argTypesOk = false;
                    call.Arguments[i] = arg.ast;
                    workspace.SetType(arg.ast, arg.type);

                    argTypes.Add(arg.type);
                }

                if (!argTypesOk)
                {
                    workspace.ReportError(call, "Invalid arguments in function call!");
                    return (null, call);
                }

                var func = scope.GetFunction(id.Name, argTypes);
                if (func == null)
                {
                    workspace.ReportError(call, "No function matches call!");
                    return (null, call);
                }

                var type = scope.Types.GetCType(func.ReturnType);
                if (type == null)
                {
                    workspace.ReportError(call, "Return type of function does not exist!");
                    return (null, call);
                }
                
                return (type, call);
            }

            return (null, call);
        }
    }
}
