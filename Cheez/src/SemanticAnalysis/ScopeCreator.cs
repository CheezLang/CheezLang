using Cheez.Ast;
using Cheez.Visitor;

namespace Cheez.SemanticAnalysis
{
    public struct ScopeCreatorArg
    {
        public Scope scope;
    }

    public struct ScopeCreatorResult
    {
        public Scope scope;

        public ScopeCreatorResult(Scope s)
        {
            scope = s;
        }
    }

    public class ScopeCreator : VisitorBase<ScopeCreatorResult, ScopeCreatorArg>
    {
        private Workspace workspace;

        public ScopeCreator(Workspace ws)
        {
            workspace = ws;
        }

        public ScopeCreatorResult CreateScopes(Statement s, Scope scope)
        {
            return s.Accept(this, new ScopeCreatorArg
            {
                scope = scope
            });
        }

        public override ScopeCreatorResult VisitFunctionDeclaration(FunctionDeclarationAst function, ScopeCreatorArg data = default)
        {
            data.scope.FunctionDeclarations.Add(function);
            workspace.SetScope(function, data.scope);

            var functionScope = new Scope($"fn {function.NameExpr}", data.scope);
            var subScope = functionScope;
            workspace.SetFunctionScope(function, functionScope);

            foreach (var p in function.Parameters)
            {
                workspace.SetScope(p, functionScope);
                workspace.SetVariableData(p, new VariableData { type = VariableType.Parameter });
            }

            foreach (var s in function.Statements)
            {
                var result = CreateScopes(s, subScope);
                subScope = result.scope ?? subScope;
                workspace.SetScope(s, subScope);
            }

            return default;
        }

        public override ScopeCreatorResult VisitVariableDeclaration(VariableDeclarationAst variable, ScopeCreatorArg data = default)
        {
            if (variable.Initializer != null)
                workspace.SetScope(variable.Initializer, data.scope);

            var varScope = new Scope($"var {variable.Name}", data.scope);
            workspace.SetVariableData(variable, new VariableData { type = VariableType.Local });

            return new ScopeCreatorResult(varScope);
        }

        public override ScopeCreatorResult VisitBlockStatement(BlockStatement block, ScopeCreatorArg data = default)
        {
            var blockScope = new Scope("{}", data.scope);
            var subScope = blockScope;

            foreach (var s in block.Statements)
            {
                var result = CreateScopes(s, subScope);
                subScope = result.scope ?? subScope;
                workspace.SetScope(s, subScope);
            }

            return default;
        }

        public override ScopeCreatorResult VisitIfStatement(IfStatement ifs, ScopeCreatorArg data = default)
        {
            var res = CreateScopes(ifs.IfCase, data.scope);
            var scope = res.scope ?? data.scope;
            workspace.SetScope(ifs.IfCase, scope);

            if (ifs.ElseCase != null)
            {
                res = CreateScopes(ifs.ElseCase, data.scope);
                scope = res.scope ?? data.scope;
                workspace.SetScope(ifs.ElseCase, scope);
            }

            return default;
        }
    }
}
