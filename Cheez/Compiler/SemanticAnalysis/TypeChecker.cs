using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.SemanticAnalysis
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
        //public Scope scope;

        public TypeCheckerData(Scope scope)
        {
            //this.scope = scope;
        }
    }

    public struct TypeCheckResult
    {
        public object ast;

        public AstExpression expr => (AstExpression)ast;
        public AstStatement stmt => (AstStatement)ast;

        public TypeCheckResult(object ast)
        {
            this.ast = ast;
        }
    }

    public class TypeChecker : VisitorBase<TypeCheckResult, TypeCheckerData>
    {
        private Workspace workspace;

        public TypeChecker(Workspace w)
        {
            workspace = w;
        }

        [DebuggerStepThrough]
        public TypeCheckResult CheckTypes(AstStatement statement)
        {
            return statement.Accept(this);
        }

        [DebuggerStepThrough]
        private TypeCheckResult CheckTypes(AstExpression expr)
        {
            return expr.Accept(this);
        }

        public override TypeCheckResult VisitExpressionStatement(AstExprStmt stmt, TypeCheckerData data = default)
        {
            var result = CheckTypes(stmt.Expr);
            stmt.Expr = result.expr;
            return new TypeCheckResult(stmt);
        }

        public override TypeCheckResult VisitFunctionDeclaration(AstFunctionDecl function, TypeCheckerData data = default)
        {
            // check parameter types
            {
                foreach (var p in function.Parameters)
                {
                    function.SubScope.DefineVariable(p.Name, p);
                }
            }

            // check body
            foreach (var s in function.Statements)
            {
                CheckTypes(s);
            }

            return default;
        }

        public override TypeCheckResult VisitBlockStatement(AstBlockStmt block, TypeCheckerData data = default)
        {
            foreach (var s in block.Statements)
            {
                CheckTypes(s);
            }

            return default;
        }

        public override TypeCheckResult VisitIfStatement(AstIfStmt ifs, TypeCheckerData data = default)
        {
            // check condition
            {
                var result = CheckTypes(ifs.Condition);
                ifs.Condition = result.expr ?? ifs.Condition;
            }

            // check if case
            {
                var result = CheckTypes(ifs.IfCase);
            }

            return default;
        }

        public override TypeCheckResult VisitPrintStatement(AstPrintStmt print, TypeCheckerData data = default)
        {
            for (int i = 0; i < print.Expressions.Count; i++)
            {
                var result = CheckTypes(print.Expressions[i]);
                if (result.expr.Type == null)
                {
                    continue;
                }
                else if (result.expr.Type == CheezType.Void)
                {
                    workspace.ReportError(print.Expressions[i].GenericParseTreeNode, $"Cannot print value of type '{result.expr.Type}'");
                }
                print.Expressions[i] = result.expr;
            }
            return new TypeCheckResult(print);
        }

        public override TypeCheckResult VisitVariableDeclaration(AstVariableDecl varAst, TypeCheckerData data = default)
        {
            if (varAst.ParseTreeNode.Type != null)
            {
                varAst.VarType = varAst.Scope.GetCheezType(varAst.ParseTreeNode.Type);
                if (varAst.VarType == null)
                {
                    workspace.ReportError(varAst.ParseTreeNode.Type, $"Unknown type '{varAst.VarType}'");
                    return new TypeCheckResult(varAst);
                }

                if (varAst.Initializer != null)
                {
                    varAst.Initializer = CheckTypes(varAst.Initializer).expr;
                    varAst.Initializer = InsertCastExpressionIf(varAst.Initializer, varAst.Initializer.Type, varAst.VarType);
                    if (varAst.Initializer.Type != varAst.VarType)
                    {
                        workspace.ReportError(varAst.ParseTreeNode.Initializer, $"Type of initialization does not match type of variable. Expected {varAst.VarType}, got {varAst.Initializer.Type}");
                        return new TypeCheckResult(varAst);
                    }
                }
            }
            else
            {
                if (varAst.Initializer == null)
                {
                    workspace.ReportError(varAst.ParseTreeNode, $"Type of variable must be explictly specified if no initial value is given");
                    return new TypeCheckResult(varAst);
                }

                varAst.Initializer = CheckTypes(varAst.Initializer).expr;
                if (varAst.Initializer.Type == IntType.LiteralType)
                {
                    varAst.Initializer.Type = IntType.DefaultType;
                }
                varAst.VarType = varAst.Initializer.Type;
            }

            if (!varAst.SubScope.DefineVariable(varAst.Name, varAst))
            {
                workspace.ReportError(varAst.ParseTreeNode.Name, $"Variable '{varAst.Name}' already exists in current scope!");
                return new TypeCheckResult(varAst);
            }

            return new TypeCheckResult(varAst);
        }

        public override TypeCheckResult VisitStringLiteral(AstStringLiteral str, TypeCheckerData data = default)
        {
            str.Type = CheezType.String;
            return new TypeCheckResult(str);
        }

        public override TypeCheckResult VisitNumberExpression(AstNumberExpr lit, TypeCheckerData data = default)
        {
            lit.Type = IntType.LiteralType;
            return new TypeCheckResult(lit);
        }

        public override TypeCheckResult VisitIdentifierExpression(AstIdentifierExpr ident, TypeCheckerData data = default)
        {
            var variable = ident.Scope.GetVariable(ident.Name);

            if (variable == null)
            {
                workspace.ReportError(ident.ParseTreeNode, $"No variable called '{ident.Name}' exists in current or surrounding scope");
                return new TypeCheckResult(ident);
            }

            ident.Type = variable.VarType;
            return new TypeCheckResult(ident);
        }

        public override TypeCheckResult VisitBinaryExpression(AstBinaryExpr bin, TypeCheckerData data = default)
        {
            bin.Left = CheckTypes(bin.Left).expr;
            bin.Right = CheckTypes(bin.Right).expr;

            if (bin.Left.Type == IntType.LiteralType && bin.Right.Type == IntType.LiteralType)
            {
                bin.Left.Type = IntType.GetIntType(8, true);
                bin.Right.Type = IntType.GetIntType(8, true);
                //return new TypeCheckResult(new AstNumberExpr(bin.ParseTreeNode, OperateLiterals(n1.Data, n2.Data, bin.Operator)), IntType.LiteralType);
            }
            else if (bin.Left.Type == IntType.LiteralType)
            {
                bin.Left.Type = bin.Right.Type;
            }
            else if (bin.Right.Type == IntType.LiteralType)
            {
                bin.Right.Type = bin.Left.Type;
            }

            if (bin.Left.Type != bin.Right.Type)
            {
                workspace.ReportError(bin.ParseTreeNode, $"Type of left hand side and right hand side in binary expression do not match. LHS is {bin.Left.Type}, RHS is {bin.Right.Type}");
                return new TypeCheckResult(bin);
            }

            bin.Type = bin.Left.Type;
            return new TypeCheckResult(bin);
        }

        public override TypeCheckResult VisitCallExpression(AstCallExpr call, TypeCheckerData data)
        {
            if (call.Function is AstIdentifierExpr id)
            {
                List<CheezType> argTypes = new List<CheezType>();
                bool argTypesOk = true;
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var a = call.Arguments[i] = CheckTypes(call.Arguments[i]).expr;
                    if (a.Type == null || a.Type == CheezType.Void)
                        argTypesOk = false;

                    argTypes.Add(a.Type);
                }

                if (!argTypesOk)
                {
                    workspace.ReportError(call.ParseTreeNode, "Invalid arguments in function call!");
                    return new TypeCheckResult(call);
                }

                var func = call.Scope.GetFunction(id.Name, argTypes);
                if (func == null)
                {
                    workspace.ReportError(call.ParseTreeNode, "No function matches call!");
                    return new TypeCheckResult(call);
                }
                
                if (func.ReturnType == null)
                {
                    workspace.ReportError(call.ParseTreeNode, "Return type of function does not exist!");
                    return new TypeCheckResult(call);
                }

                // @Temp
                // check if types match
                if (argTypes.Count != func.Parameters.Count)
                {
                    workspace.ReportError(call.ParseTreeNode, $"Wrong number of arguments in function call");
                    return new TypeCheckResult(call);
                }
                else
                {
                    List<AstExpression> args = new List<AstExpression>();
                    foreach (var (givenAst, expectedAst) in call.Arguments.Zip(func.Parameters, (a, b) => (a, b)))
                    {
                        var given = givenAst.Type;
                        var expected = expectedAst.VarType;

                        var result = InsertCastExpressionIf(givenAst, given, expected);
                        args.Add(result);

                        if (result.Type != expected)
                        {
                            workspace.ReportError(givenAst.GenericParseTreeNode, $"Argument types in function call do not match. Expected {expected}, got {given}");
                        }
                    }

                    call.Arguments = args;
                }

                return new TypeCheckResult(call);
            }

            return new TypeCheckResult(call);
        }

        #region Helper Methods

        private AstExpression InsertCastExpressionIf(AstExpression e, CheezType sourceType, CheezType targetType)
        {
            if (sourceType == targetType)
                return e;
            if (sourceType == IntType.LiteralType && targetType is IntType)
            {
                e.Type = targetType;
                return e;
            }

            //workspace.ReportError(e, $"Can't cast {sourceType} to {targetType}");
            return e;
        }

        private double OperateNumbers(double a, double b, Operator op)
        {
            switch (op)
            {
                case Operator.Add:
                    return a + b;
                case Operator.Subtract:
                    return a - b;
                case Operator.Multiply:
                    return a * b;
                case Operator.Divide:
                    return a / b;
                default:
                    throw new Exception();
            }
        }

        private long OperateNumbers(long a, long b, Operator op)
        {
            switch (op)
            {
                case Operator.Add:
                    return a + b;
                case Operator.Subtract:
                    return a - b;
                case Operator.Multiply:
                    return a * b;
                case Operator.Divide:
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

        private NumberData OperateLiterals(NumberData data1, NumberData data2, Operator op)
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
