using Cheez.Compiler;
using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System.Linq;

namespace CheezLanguageServer
{
    public class NodeFinderResult
    {
        public Scope Scope { get; }

        public AstExpression Expr { get; }
        public AstStatement Stmt { get; }
        public CheezType Type { get; }

        public NodeFinderResult(Scope s, AstExpression expr = null, AstStatement stmt = null, CheezType type = null)
        {
            Scope = s;
            Expr = expr;
            Stmt = stmt;
            Type = type;
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

            foreach (var s in w.Statements.Where(s => s.GenericParseTreeNode.SourceFile == file))
            {
                var loc = GetRelativeLocation(s.GenericParseTreeNode, index);
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

        public override NodeFinderResult VisitFunctionDeclaration(AstFunctionDecl function, int index = 0)
        {
            foreach (var s in function.Statements)
            {
                var loc = GetRelativeLocation(s.GenericParseTreeNode, index);
                if (loc == RelativeLocation.Same)
                    return s.Accept(this, index);
            }

            return new NodeFinderResult(function.Scope, stmt: function);
        }

        public override NodeFinderResult VisitVariableDeclaration(AstVariableDecl variable, int index = 0)
        {
            if (GetRelativeLocation(variable.Initializer?.GenericParseTreeNode, index) == RelativeLocation.Same)
            {
                return variable.Initializer.Accept(this, index);
            }

            if (GetRelativeLocation(variable.ParseTreeNode.Type, index) == RelativeLocation.Same)
            {
                return new NodeFinderResult(variable.Scope, type: variable.Type);
            }

            return new NodeFinderResult(variable.Scope, stmt: variable);
        }

        public override NodeFinderResult VisitPrintStatement(AstPrintStmt print, int index = 0)
        {
            foreach (var sub in print.Expressions)
            {
                if (GetRelativeLocation(sub.GenericParseTreeNode, index) == RelativeLocation.Same)
                    return sub.Accept(this, index);
            }

            return new NodeFinderResult(print.Scope, stmt: print);
        }

        public override NodeFinderResult VisitReturnStatement(AstReturnStmt ret, int index = 0)
        {
            if (ret.ReturnValue != null && GetRelativeLocation(ret.ReturnValue.GenericParseTreeNode, index) == RelativeLocation.Same)
                return ret.ReturnValue.Accept(this, index);

            return new NodeFinderResult(ret.Scope, stmt: ret);
        }

        public override NodeFinderResult VisitIfStatement(AstIfStmt ifs, int i = 0)
        {
            if (GetRelativeLocation(ifs.Condition.GenericParseTreeNode, i) == RelativeLocation.Same)
                return ifs.Condition.Accept(this, i);

            if (GetRelativeLocation(ifs.IfCase.GenericParseTreeNode, i) == RelativeLocation.Same)
                return ifs.IfCase.Accept(this, i);
            if (ifs.ElseCase != null && GetRelativeLocation(ifs.ElseCase.GenericParseTreeNode, i) == RelativeLocation.Same)
                return ifs.ElseCase.Accept(this, i);

            return new NodeFinderResult(ifs.Scope, stmt: ifs);
        }

        public override NodeFinderResult VisitBlockStatement(AstBlockStmt block, int i = 0)
        {
            foreach (var s in block.Statements)
            {
                if (GetRelativeLocation(s.GenericParseTreeNode, i) == RelativeLocation.Same)
                    return s.Accept(this, i);
            }

            return new NodeFinderResult(block.Scope, stmt: block);
        }

        public override NodeFinderResult VisitExpressionStatement(AstExprStmt stmt, int data = 0)
        {
            return stmt.Expr.Accept(this, data);
        }

        public override NodeFinderResult VisitAssignment(AstAssignment ass, int i = 0)
        {
            if (GetRelativeLocation(ass.Value.GenericParseTreeNode, i) == RelativeLocation.Same)
                return ass.Value.Accept(this, i);

            if (GetRelativeLocation(ass.Target.GenericParseTreeNode, i) == RelativeLocation.Same)
                return ass.Target.Accept(this, i);

            return new NodeFinderResult(ass.Scope, stmt: ass);
        }

        #region Expressions

        public override NodeFinderResult VisitCastExpression(AstCastExpr cast, int i = 0)
        {
            if (GetRelativeLocation(cast.SubExpression.GenericParseTreeNode, i) == RelativeLocation.Same)
                return cast.SubExpression.Accept(this, i);

            if (GetRelativeLocation(cast.ParseTreeNode.TargetType, i) == RelativeLocation.Same)
                return new NodeFinderResult(cast.Scope, type: cast.Type);

            return new NodeFinderResult(cast.Scope, expr: cast);
        }

        public override NodeFinderResult VisitCallExpression(AstCallExpr call, int i = 0)
        {
            foreach (var arg in call.Arguments)
            {
                if (GetRelativeLocation(arg.GenericParseTreeNode, i) == RelativeLocation.Same)
                    return arg.Accept(this, i);
            }

            return call.Function.Accept(this, i);
        }

        public override NodeFinderResult VisitBinaryExpression(AstBinaryExpr bin, int index = 0)
        {
            if (GetRelativeLocation(bin.Left.GenericParseTreeNode, index) == RelativeLocation.Same)
                return bin.Left.Accept(this, index);
            
            if (GetRelativeLocation(bin.Right.GenericParseTreeNode, index) == RelativeLocation.Same)
                return bin.Right.Accept(this, index);

            return new NodeFinderResult(bin.Scope, expr: bin);
        }

        public override NodeFinderResult VisitDotExpression(AstDotExpr dot, int i = 0)
        {
            if (GetRelativeLocation(dot.Left.GenericParseTreeNode, i) == RelativeLocation.Same)
                return dot.Left.Accept(this, i);

            return new NodeFinderResult(dot.Scope, expr: dot);
        }

        public override NodeFinderResult VisitAddressOfExpression(AstAddressOfExpr add, int i = 0)
        {
            if (GetRelativeLocation(add.SubExpression.GenericParseTreeNode, i) == RelativeLocation.Same)
                return add.SubExpression.Accept(this, i);
            return new NodeFinderResult(add.Scope, expr: add);
        }

        public override NodeFinderResult VisitDereferenceExpression(AstDereferenceExpr deref, int i = 0)
        {
            if (GetRelativeLocation(deref.SubExpression.GenericParseTreeNode, i) == RelativeLocation.Same)
                return deref.SubExpression.Accept(this, i);
            return new NodeFinderResult(deref.Scope, expr: deref);
        }

        public override NodeFinderResult VisitBoolExpression(AstBoolExpr bo, int data = 0)
        {
            return new NodeFinderResult(bo.Scope, expr: bo);
        }

        public override NodeFinderResult VisitTypeExpression(AstTypeExpr type, int data = 0)
        {
            return new NodeFinderResult(type.Scope, type: type.Type);
        }

        public override NodeFinderResult VisitNumberExpression(AstNumberExpr num, int data = 0)
        {
            return new NodeFinderResult(num.Scope, expr: num);
        }

        public override NodeFinderResult VisitIdentifierExpression(AstIdentifierExpr ident, int data = 0)
        {
            return new NodeFinderResult(ident.Scope, expr: ident);
        }

        public override NodeFinderResult VisitStringLiteral(AstStringLiteral str, int data = 0)
        {
            return new NodeFinderResult(str.Scope, expr: str);
        }

        #endregion

        #endregion
    }
}
