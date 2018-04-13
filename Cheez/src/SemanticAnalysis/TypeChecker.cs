using Cheez.Ast;
using Cheez.Visitor;
using System;
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

        [DebuggerStepThrough]
        public CType CheckTypes(Statement statement)
        {
            return statement.Accept(this, new TypeCheckerData(workspace.GlobalScope));
        }

        [DebuggerStepThrough]
        private CType CheckTypes(Expression expr)
        {
            return expr.Accept(this, new TypeCheckerData(workspace.GlobalScope));
        }
        
        public override CType VisitVariableDeclaration(VariableDeclaration variable, TypeCheckerData data)
        {
            CType type = null;
            if (variable.Type != null)
            {
                type = data.scope.Types.GetCType(variable.Type);
                if (type == null)
                {
                    workspace.ReportError(variable.Type, $"Unknown type: {variable.Type.Text}");
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
                    workspace.ReportError(variable.Initializer, $"Type of initialization does not match type of variable");
                    return null;
                }
            }

            workspace.SetType(variable, type);
            return type;
        }

        public override CType VisitNumberExpression(NumberExpression lit, TypeCheckerData data)
        {
            var type = IntType.LiteralType;
            if (data.expectedType != null && data.expectedType is IntType i)
                type = i;
            workspace.SetType(lit, type);
            return type;
        }
    }
}
