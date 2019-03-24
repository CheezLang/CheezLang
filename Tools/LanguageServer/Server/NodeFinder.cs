using Cheez;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Parsing;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Visitors;
using System.Collections.Generic;
using System.Linq;

namespace CheezLanguageServer
{
    public class NodeFinderResult
    {
        public Scope Scope { get; }

        public AstExpression Expr { get; }
        public AstStatement Stmt { get; }
        public CheezType Type { get; }

        public NodeFinderResult()
        {
        }

        public NodeFinderResult(Scope s, AstExpression expr = null, AstStatement stmt = null, CheezType type = null)
        {
            Scope = s;
            Expr = expr;
            Stmt = stmt;
            Type = type;
        }

        public virtual Dictionary<string, ISymbol> GetSymbols()
        {
            var symbols = new Dictionary<string, ISymbol>();
            if (Scope != null)
            {
                AddSymbolsToSymbolList(symbols, Scope);
            }
            return symbols;
        }

        protected void AddSymbolsToSymbolList(Dictionary<string, ISymbol> symbols, Scope scope)
        {
            foreach (var kv in scope.Symbols)
            {
                var sym = kv.Value;
                if (!symbols.ContainsKey(sym.Name.Name))
                    symbols.Add(sym.Name.Name, sym);
            }

            if (scope.Parent != null)
                AddSymbolsToSymbolList(symbols, scope.Parent);
        }
    }

    public class NodeFinderResultCallExpr : NodeFinderResult
    {
        public AstCallExpr Call { get; set; }
        public int ArgIndex { get; set; }

        public NodeFinderResultCallExpr(AstCallExpr call, int index) : base(call.Scope)
        {
            this.Call = call;
            this.ArgIndex = index;
        }

        public override Dictionary<string, ISymbol> GetSymbols()
        {
            var syms = base.GetSymbols();
            var funcType = Call.Function.Type as FunctionType;
            var pars = funcType?.Parameters;
            if (pars == null || ArgIndex >= pars.Length)
                return syms;

            var paramType = pars[ArgIndex];

            syms = syms.ToDictionary(kv => kv.Key, kv => kv.Value);
            return syms;
        }
    }

    public class NodeFinder : VisitorBase<NodeFinderResult, int>
    {
        private enum RelativeLocation
        {
            Before,
            Same,
            After,
            Unknown
        }

        public NodeFinderResult FindNode(Workspace w, PTFile file, int line, int character, bool exactMatch)
        {
            int index = GetPosition(file, line, character);

            foreach (var s in w.Statements.Where(s => s.Location.Beginning.file == file.Name))
            {
                var loc = GetRelativeLocation(s.Location, index);
                if (loc == RelativeLocation.Same)
                {
                    return s.Accept(this, index);
                }
            }

            return null;
        }

        #region Helpers

        private RelativeLocation GetRelativeLocation(ILocation loc, int index)
        {
            if (loc == null)
                return RelativeLocation.Unknown;
            if (index < loc.Beginning.index)
                return RelativeLocation.Before;
            if (index < loc.End.end)
                return RelativeLocation.Same;
            return RelativeLocation.After;
        }

        private static int GetPosition(IText text, int line, int character)
        {
            int pos = 0;
            for (; 0 < line; line--)
            {
                var lf = text.Text.IndexOf('\n', pos);
                if (lf < 0)
                {
                    return text.Text.Length;
                }
                pos = lf + 1;
            }
            var linefeed = text.Text.IndexOf('\n', pos);
            var max = 0;
            if (linefeed < 0)
            {
                max = text.Text.Length;
            }
            else
            {
                max = linefeed;
            }
            pos += character;
            return (pos <= max) ? pos : max;
        }

        #endregion

        #region Visitors

        public override NodeFinderResult VisitFunctionDecl(AstFunctionDecl function, int index = 0)
        {
            if (GetRelativeLocation(function.Body.Location, index) == RelativeLocation.Same)
                return function.Body.Accept(this, index);

            return new NodeFinderResult(function.Scope, stmt: function);
        }

        public override NodeFinderResult VisitVariableDecl(AstVariableDecl variable, int index = 0)
        {
            if (GetRelativeLocation(variable.Initializer?.Location, index) == RelativeLocation.Same)
            {
                return variable.Initializer.Accept(this, index);
            }

            if (GetRelativeLocation(variable.TypeExpr.Location, index) == RelativeLocation.Same)
            {
                return new NodeFinderResult(variable.Scope, type: variable.Type);
            }

            return new NodeFinderResult(variable.Scope, stmt: variable);
        }

        public override NodeFinderResult VisitReturnStmt(AstReturnStmt ret, int index = 0)
        {
            if (ret.ReturnValue != null && GetRelativeLocation(ret.ReturnValue.Location, index) == RelativeLocation.Same)
                return ret.ReturnValue.Accept(this, index);

            return new NodeFinderResult(ret.Scope, stmt: ret);
        }

        public override NodeFinderResult VisitIfExpr(AstIfExpr ifs, int i = 0)
        {
            if (GetRelativeLocation(ifs.Condition.Location, i) == RelativeLocation.Same)
                return ifs.Condition.Accept(this, i);

            if (GetRelativeLocation(ifs.IfCase.Location, i) == RelativeLocation.Same)
                return ifs.IfCase.Accept(this, i);
            if (ifs.ElseCase != null && GetRelativeLocation(ifs.ElseCase.Location, i) == RelativeLocation.Same)
                return ifs.ElseCase.Accept(this, i);

            return new NodeFinderResult(ifs.Scope, expr: ifs);
        }

        public override NodeFinderResult VisitBlockExpr(AstBlockExpr block, int i = 0)
        {
            foreach (var s in block.Statements)
            {
                if (GetRelativeLocation(s.Location, i) == RelativeLocation.Same)
                    return s.Accept(this, i);
            }

            return new NodeFinderResult(block.Scope, expr: block);
        }

        public override NodeFinderResult VisitExpressionStmt(AstExprStmt stmt, int data = 0)
        {
            return stmt.Expr.Accept(this, data);
        }

        public override NodeFinderResult VisitAssignmentStmt(AstAssignment ass, int i = 0)
        {
            if (GetRelativeLocation(ass.Value.Location, i) == RelativeLocation.Same)
                return ass.Value.Accept(this, i);

            if (GetRelativeLocation(ass.Pattern.Location, i) == RelativeLocation.Same)
                return ass.Pattern.Accept(this, i);

            return new NodeFinderResult(ass.Scope, stmt: ass);
        }

        public override NodeFinderResult VisitUsingStmt(AstUsingStmt use, int i = 0)
        {
            if (GetRelativeLocation(use.Value.Location, i) == RelativeLocation.Same)
                return use.Value.Accept(this, i);

            return new NodeFinderResult(use.Scope, stmt: use);
        }

        public override NodeFinderResult VisitImplDecl(AstImplBlock impl, int i = 0)
        {
            foreach (var s in impl.Functions)
            {
                if (GetRelativeLocation(s.Location, i) == RelativeLocation.Same)
                    return s.Accept(this, i);
            }

            return new NodeFinderResult(impl.Scope, stmt: impl);
        }

        public override NodeFinderResult VisitWhileStmt(AstWhileStmt ws, int i = 0)
        {
            if (GetRelativeLocation(ws.Condition.Location, i) == RelativeLocation.Same)
                return ws.Condition.Accept(this, i);

            if (GetRelativeLocation(ws.Body.Location, i) == RelativeLocation.Same)
                return ws.Body.Accept(this, i);

            return new NodeFinderResult(ws.Scope, stmt: ws);
        }

        #region Expressions

        public override NodeFinderResult VisitArrayAccessExpr(AstArrayAccessExpr arr, int index = 0)
        {
            if (GetRelativeLocation(arr.SubExpression.Location, index) == RelativeLocation.Same)
                return arr.SubExpression.Accept(this, index);

            if (GetRelativeLocation(arr.Indexer.Location, index) == RelativeLocation.Same)
                return arr.Indexer.Accept(this, index);

            return new NodeFinderResult(arr.Scope, expr: arr);
        }

        public override NodeFinderResult VisitCastExpr(AstCastExpr cast, int i = 0)
        {
            if (GetRelativeLocation(cast.SubExpression.Location, i) == RelativeLocation.Same)
                return cast.SubExpression.Accept(this, i);

            if (GetRelativeLocation(cast.TypeExpr.Location, i) == RelativeLocation.Same)
                return new NodeFinderResult(cast.Scope, type: cast.Type);

            return new NodeFinderResult(cast.Scope, expr: cast);
        }

        public override NodeFinderResult VisitCallExpr(AstCallExpr call, int i = 0)
        {
            foreach (var arg in call.Arguments)
            {
                if (GetRelativeLocation(arg.Location, i) == RelativeLocation.Same)
                    return arg.Accept(this, i);
            }

            if (GetRelativeLocation(call.Function.Location, i) == RelativeLocation.Same)
                return call.Function.Accept(this, i);

            return new NodeFinderResultCallExpr(call, 0);
        }

        public override NodeFinderResult VisitBinaryExpr(AstBinaryExpr bin, int index = 0)
        {
            if (GetRelativeLocation(bin.Left.Location, index) == RelativeLocation.Same)
                return bin.Left.Accept(this, index);
            
            if (GetRelativeLocation(bin.Right.Location, index) == RelativeLocation.Same)
                return bin.Right.Accept(this, index);

            return new NodeFinderResult(bin.Scope, expr: bin);
        }

        public override NodeFinderResult VisitDotExpr(AstDotExpr dot, int i = 0)
        {
            if (GetRelativeLocation(dot.Left.Location, i) == RelativeLocation.Same)
                return dot.Left.Accept(this, i);

            return new NodeFinderResult(dot.Scope, expr: dot);
        }

        public override NodeFinderResult VisitAddressOfExpr(AstAddressOfExpr add, int i = 0)
        {
            if (GetRelativeLocation(add.SubExpression.Location, i) == RelativeLocation.Same)
                return add.SubExpression.Accept(this, i);
            return new NodeFinderResult(add.Scope, expr: add);
        }

        public override NodeFinderResult VisitDerefExpr(AstDereferenceExpr deref, int i = 0)
        {
            if (GetRelativeLocation(deref.SubExpression.Location, i) == RelativeLocation.Same)
                return deref.SubExpression.Accept(this, i);
            return new NodeFinderResult(deref.Scope, expr: deref);
        }

        public override NodeFinderResult VisitBoolExpr(AstBoolExpr bo, int data = 0)
        {
            return new NodeFinderResult(bo.Scope, expr: bo);
        }

        public override NodeFinderResult VisitNumberExpr(AstNumberExpr num, int data = 0)
        {
            return new NodeFinderResult(num.Scope, expr: num);
        }

        public override NodeFinderResult VisitIdExpr(AstIdExpr ident, int data = 0)
        {
            return new NodeFinderResult(ident.Scope, expr: ident);
        }

        public override NodeFinderResult VisitStringLiteralExpr(AstStringLiteral str, int data = 0)
        {
            return new NodeFinderResult(str.Scope, expr: str);
        }

        #endregion

        #endregion
    }
}
