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
        private CompilationUnit mUnit;

        public TypeChecker(CompilationUnit unit)
        {
            mUnit = unit;
        }

        public CType CheckTypes()
        {
            return CheckTypes(mUnit.statement);
        }

        [DebuggerStepThrough]
        private CType CheckTypes(Statement statement)
        {
            return statement.Accept(this);
        }

        [DebuggerStepThrough]
        private CType CheckTypes(Expression expr)
        {
            return expr.Accept(this);
        }
        
        public override CType VisitVariableDeclaration(VariableDeclaration variable, TypeCheckerData data)
        {
            if (variable.TypeName != null)
            {
                var type = data.scope.Types.GetCType(variable.TypeName);
                variable.Type = type ?? throw new WaitForType(variable.TypeName);
            }

            if (variable.Initializer != null)
            {
                var type = CheckTypes(variable.Initializer);
            }

            return CType.Void;
        }

        public override CType VisitNumberExpression(NumberExpression lit, TypeCheckerData data)
        {
            if (data.expectedType != null && data.expectedType is IntType i)
                lit.Type = i;
            else
                lit.Type = IntType.LiteralType;
            return lit.Type;
        }
    }
}
