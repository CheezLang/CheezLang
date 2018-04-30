using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Diagnostics;

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
        public List<Scope> AllScopes { get; } = new List<Scope>();

        public ScopeCreator(Workspace ws, Scope globalScope)
        {
            workspace = ws;
            AllScopes.Add(globalScope);
        }

        [DebuggerStepThrough]
        public ScopeCreatorResult CreateScopes(IVisitorAcceptor s, Scope scope)
        {
            if (s == null)
                return default;
            return s.Accept(this, new ScopeCreatorArg
            {
                scope = scope
            });
        }

        private Scope NewScope(string name, Scope parent)
        {
            var s = new Scope(name, parent);
            AllScopes.Add(s);
            return s;
        }

        #region Statements

        public override ScopeCreatorResult VisitTypeDeclaration(AstTypeDecl type, ScopeCreatorArg data = default)
        {
            data.scope.TypeDeclarations.Add(type);
            type.Scope = data.scope;
            

            return default;
        }

        public override ScopeCreatorResult VisitFunctionDeclaration(AstFunctionDecl function, ScopeCreatorArg data = default)
        {
            data.scope.FunctionDeclarations.Add(function);
            function.Scope = data.scope;

            function.SubScope = NewScope($"fn {function.Name}", data.scope);
            var subScope = function.SubScope;

            foreach (var p in function.Parameters)
            {
                p.Scope = function.SubScope;
            }

            if (function.HasImplementation)
            {
                foreach (var s in function.Statements)
                {
                    var result = CreateScopes(s, subScope);
                    subScope = result.scope ?? subScope;
                }
            }

            return default;
        }

        public override ScopeCreatorResult VisitVariableDeclaration(AstVariableDecl variable, ScopeCreatorArg data = default)
        {
            data.scope.VariableDeclarations.Add(variable);
            variable.Scope = data.scope;
            variable.SubScope = NewScope($"var {variable.Name}", data.scope);
            CreateScopes(variable.Initializer, variable.Scope);
            return new ScopeCreatorResult(variable.SubScope);
        }

        public override ScopeCreatorResult VisitBlockStatement(AstBlockStmt block, ScopeCreatorArg data = default)
        {
            block.Scope = data.scope;
            block.SubScope = NewScope("{}", data.scope);
            var subScope = block.SubScope;

            foreach (var s in block.Statements)
            {
                var result = CreateScopes(s, subScope);
                subScope = result.scope ?? subScope;
            }

            return default;
        }

        public override ScopeCreatorResult VisitIfStatement(AstIfStmt ifs, ScopeCreatorArg data = default)
        {
            ifs.Scope = data.scope;
            CreateScopes(ifs.Condition, data.scope);
            CreateScopes(ifs.IfCase, NewScope("if", data.scope));
            CreateScopes(ifs.ElseCase, NewScope("else", data.scope));

            return default;
        }

        public override ScopeCreatorResult VisitWhileStatement(AstWhileStmt ws, ScopeCreatorArg data = default)
        {
            ws.Scope = data.scope;
            var subScope = NewScope("while", data.scope);
            var res = CreateScopes(ws.PreAction, subScope);
            subScope = res.scope ?? subScope;

            CreateScopes(ws.Condition, subScope);
            CreateScopes(ws.Body, subScope);

            CreateScopes(ws.PostAction, subScope);


            return default;
        }

        public override ScopeCreatorResult VisitPrintStatement(AstPrintStmt print, ScopeCreatorArg data = default)
        {
            print.Scope = data.scope;
            foreach (var e in print.Expressions)
            {
                CreateScopes(e, data.scope);
            }
            return default;
        }

        public override ScopeCreatorResult VisitReturnStatement(AstReturnStmt ret, ScopeCreatorArg data = default)
        {
            ret.Scope = data.scope;
            CreateScopes(ret.ReturnValue, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitAssignment(AstAssignment ass, ScopeCreatorArg data = default)
        {
            ass.Scope = data.scope;
            CreateScopes(ass.Target, data.scope);
            CreateScopes(ass.Value, data.scope);
            return default;
        }

        #endregion

        #region Expressions

        public override ScopeCreatorResult VisitAddressOfExpression(AstAddressOfExpr add, ScopeCreatorArg data = default)
        {
            add.Scope = data.scope;
            CreateScopes(add.SubExpression, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitBinaryExpression(AstBinaryExpr bin, ScopeCreatorArg data = default)
        {
            bin.Scope = data.scope;
            CreateScopes(bin.Left, data.scope);
            CreateScopes(bin.Right, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitDotExpression(AstDotExpr dot, ScopeCreatorArg data = default)
        {
            dot.Scope = data.scope;
            CreateScopes(dot.Left, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitExpressionStatement(AstExprStmt stmt, ScopeCreatorArg data = default)
        {
            stmt.Scope = data.scope;
            CreateScopes(stmt.Expr, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitIdentifierExpression(AstIdentifierExpr ident, ScopeCreatorArg data = default)
        {
            ident.Scope = data.scope;
            return default;
        }

        public override ScopeCreatorResult VisitCallExpression(AstCallExpr call, ScopeCreatorArg data = default)
        {
            call.Scope = data.scope;
            CreateScopes(call.Function, data.scope);
            foreach (var arg in call.Arguments)
            {
                CreateScopes(arg, data.scope);
            }
            return default;
        }

        public override ScopeCreatorResult VisitTypeExpression(AstTypeExpr type, ScopeCreatorArg data = default)
        {
            type.Scope = data.scope;
            return default;
        }

        public override ScopeCreatorResult VisitCastExpression(AstCastExpr cast, ScopeCreatorArg data = default)
        {
            cast.Scope = data.scope;
            CreateScopes(cast.SubExpression, data.scope);
            return default;
        }

        public override ScopeCreatorResult VisitArrayAccessExpression(AstArrayAccessExpr arr, ScopeCreatorArg data = default)
        {
            arr.Scope = data.scope;
            CreateScopes(arr.SubExpression, data.scope);
            CreateScopes(arr.Indexer, data.scope);
            return default;
        }

        #endregion
    }
}
