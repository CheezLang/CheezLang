using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;

namespace Cheez.Compiler.SemanticAnalysis
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

        public ScopeCreatorResult CreateScopes(AstStatement s, Scope scope)
        {
            return s.Accept(this, new ScopeCreatorArg
            {
                scope = scope
            });
        }

        public override ScopeCreatorResult VisitFunctionDeclaration(AstFunctionDecl function, ScopeCreatorArg data = default)
        {
            data.scope.FunctionDeclarations.Add(function);
            function.Scope = data.scope;

            function.SubScope = new Scope($"fn {function.Name}", data.scope);
            var subScope = function.SubScope;

            foreach (var p in function.Parameters)
            {
                p.Scope = function.SubScope;
            }

            foreach (var s in function.Statements)
            {
                var result = CreateScopes(s, subScope);
                subScope = result.scope ?? subScope;
                s.Scope = subScope;
            }

            return default;
        }

        public override ScopeCreatorResult VisitVariableDeclaration(AstVariableDecl variable, ScopeCreatorArg data = default)
        {
            if (variable.Initializer != null)
                variable.Initializer.Scope = data.scope;
            return new ScopeCreatorResult(new Scope($"var {variable.Name}", data.scope));
        }

        public override ScopeCreatorResult VisitBlockStatement(AstBlockStmt block, ScopeCreatorArg data = default)
        {
            block.Scope = data.scope;
            block.SubScope = new Scope("{}", data.scope);
            var subScope = block.SubScope;

            foreach (var s in block.Statements)
            {
                var result = CreateScopes(s, subScope);
                subScope = result.scope ?? subScope;
                s.Scope = subScope;
            }

            return default;
        }

        public override ScopeCreatorResult VisitIfStatement(AstIfStmt ifs, ScopeCreatorArg data = default)
        {
            ifs.Scope = data.scope;
            CreateScopes(ifs.IfCase, data.scope);

            if (ifs.ElseCase != null)
            {
                CreateScopes(ifs.ElseCase, data.scope);
            }

            return default;
        }
    }
}
