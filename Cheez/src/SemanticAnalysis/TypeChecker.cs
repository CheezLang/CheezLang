using Cheez.Ast;
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
        public CType expectedType;

        public TypeCheckerData(IScope scope, CType et = null)
        {
            this.scope = scope;
            this.expectedType = et;
        }
    }

    public class TypeChecker : VisitorBase<CType, TypeCheckerData>
    {
        private Workspace workspace;

        public TypeChecker(Workspace w)
        {
            workspace = w;
        }

        private IScope CurrentScope(TypeCheckerData data)
        {
            return data.scope ?? workspace.GlobalScope;
        }

        [DebuggerStepThrough]
        public CType CheckTypes(Statement statement)
        {
            return statement.Accept(this);
        }

        [DebuggerStepThrough]
        private CType CheckTypes(Expression expr)
        {
            return expr.Accept(this);
        }
        
        public override CType VisitVariableDeclaration(VariableDeclaration variable, TypeCheckerData data = default)
        {
            var scope = CurrentScope(data);

            CType type = null;
            if (variable.Type != null)
            {
                type = scope.Types.GetCType(variable.Type);
                if (type == null)
                {
                    workspace.ReportError(variable.Type, $"Unknown type: {variable.Type}");
                    return null;
                }
            }
            else
            {
                if (variable.Initializer == null)
                {
                    workspace.ReportError(variable, $"Type of variable must be explictly specified if no initial value is given");
                    return null;
                }

                type = variable.Initializer.Accept(this, data);
            }

            if (variable.Initializer != null)
            {
                var initType = CheckTypes(variable.Initializer);
                if (initType != type)
                {
                    workspace.ReportError(variable.Initializer, $"Type of initialization does not match type of variable. Expected {type}, got {initType}");
                    return null;
                }
            }

            workspace.SetType(variable, type);
            return type;
        }

        public override CType VisitNumberExpression(NumberExpression lit, TypeCheckerData data = default)
        {
            var scope = CurrentScope(data);

            var type = IntType.LiteralType;
            if (data.expectedType != null && data.expectedType is IntType i)
                type = i;
            workspace.SetType(lit, type);
            return type;
        }

        public override CType VisitCallExpression(CallExpression call, TypeCheckerData data)
        {
            var scope = CurrentScope(data);

            if (call.Function is IdentifierExpression id)
            {
                List<CType> argTypes = new List<CType>();
                bool argTypesOk = true;
                foreach (var a in call.Arguments)
                {
                    var atype = a.Accept(this);
                    if (atype == null || atype == CType.Void)
                        argTypesOk = false;
                    argTypes.Add(atype);
                }

                if (!argTypesOk)
                {
                    workspace.ReportError(call, "Invalid arguments in function call!");
                    return null;
                }

                var func = scope.GetFunction(id.Name, argTypes);
                if (func == null)
                {
                    workspace.ReportError(call, "No function matches call!");
                    return null;
                }

                var type = scope.Types.GetCType(func.ReturnType);
                if (type == null)
                {
                    workspace.ReportError(call, "Return type of function does not exist!");
                    return null;
                }

                workspace.SetType(call, type);
                return type;
            }

            return null;
        }
    }
}
