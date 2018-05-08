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

        #region Expressions

        public override NodeFinderResult VisitBinaryExpression(AstBinaryExpr bin, int index = 0)
        {
            if (GetRelativeLocation(bin.Left.GenericParseTreeNode, index) == RelativeLocation.Same)
                return bin.Left.Accept(this, index);
            
            if (GetRelativeLocation(bin.Right.GenericParseTreeNode, index) == RelativeLocation.Same)
                return bin.Right.Accept(this, index);

            return new NodeFinderResult(bin.Scope, expr: bin);
        }

        public override NodeFinderResult VisitNumberExpression(AstNumberExpr num, int data = 0)
        {
            return new NodeFinderResult(num.Scope, expr: num);
        }

        public override NodeFinderResult VisitIdentifierExpression(AstIdentifierExpr ident, int data = 0)
        {
            return new NodeFinderResult(ident.Scope, expr: ident);
        }

        #endregion

        #endregion
    }
}
