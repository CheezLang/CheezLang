using System.IO;
using System.Linq;
using System.Text;
using Cheez.Compiler.Ast;

namespace Cheez.Compiler.Visitor
{
    public class AstPrinter : VisitorBase<string, int>
    {
        public void PrintWorkspace(Workspace workspace, TextWriter writer)
        {
            foreach (var s in workspace.Statements)
            {
                writer.WriteLine(s.Accept(this, 0));
                writer.WriteLine();
            }
        }

        #region Statements

        public override string VisitFunctionDeclaration(AstFunctionDecl function, int indentLevel = 0)
        {
            var body = function.Body?.Accept(this, 0) ?? ";";

            var pars = string.Join(", ", function.Parameters.Select(p => $"{p.Name}: {p.ParseTreeNode.Type}"));
            var head = $"fn {function.Name}";
                
            //if (function.IsGeneric)
            //{
            //    head += $"<{string.Join(", ", function.Generics.Select(g => g.Name))}>";
            //}

            head += $"({pars})";

            if (function.ReturnTypeExpr != null)
            {
                head += $" -> {function.ReturnTypeExpr.Accept(this, 0)}";
            }

            return $"{head} {body}".Indent(indentLevel);
        }

        public override string VisitReturnStatement(AstReturnStmt ret, int data = 0)
        {
            if (ret.ReturnValue != null)
                return $"return {ret.ReturnValue.Accept(this, 0)}";
            return "return";
        }

        public override string VisitTypeAlias(AstTypeAliasDecl al, int data = 0)
        {
            return $"type {al.Name.Name} = {al.TypeExpr.Accept(this, 0)}";
        }

        public override string VisitUsingStatement(AstUsingStmt use, int data = 0)
        {
            return $"using {use.Value.Accept(this, 0)}";
        }

        public override string VisitTypeDeclaration(AstTypeDecl type, int data = 0)
        {
            var body = string.Join("\n", type.Members.Select(m => $"{m.Name}: {m.ParseTreeNode.Type}"));
            return $"struct {type.Name} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitImplBlock(AstImplBlock impl, int data = 0)
        {
            var body = string.Join("\n\n", impl.Functions.Select(f => f.Accept(this, 0)));

            return $"impl {impl.ParseTreeNode.Target} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitVariableDeclaration(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("let ").Append(variable.Name);
            if (variable.TypeExpr != null)
                sb.Append($": {variable.TypeExpr.Accept(this, 0)}");
            if (variable.Initializer != null)
                sb.Append($" = {variable.Initializer.Accept(this, indentLevel)}");
            return sb.ToString();
        }

        public override string VisitIfStatement(AstIfStmt ifs, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.Append("if ");
            sb.Append(ifs.Condition.Accept(this));
            sb.Append(" ");
            sb.Append(ifs.IfCase.Accept(this));
            if (ifs.ElseCase != null)
            {
                sb.Append(" else ");
                sb.Append(ifs.ElseCase.Accept(this));
            }
            return sb.ToString().Indent(indentLevel);
        }

        public override string VisitWhileStatement(AstWhileStmt wh, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.Append("while ");
            sb.Append(wh.Condition.Accept(this));
            sb.Append(" ");
            sb.Append(wh.Body.Accept(this));
            return sb.ToString().Indent(indentLevel);
        }

        public override string VisitBlockStatement(AstBlockStmt block, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var s in block.Statements)
            {
                sb.AppendLine(s.Accept(this).Indent(4));
            }
            sb.Append("}");
            return sb.ToString().Indent(indentLevel);
        }

        public override string VisitAssignment(AstAssignment ass, int indentLevel = 0)
        {
            return ass.Target.Accept(this) + " = " + ass.Value.Accept(this);
        }

        public override string VisitExpressionStatement(AstExprStmt stmt, int indentLevel = 0)
        {
            return stmt.Expr.Accept(this);
        }

        public override string VisitEnumDeclaration(AstEnumDecl en, int data = 0)
        {
            var body = string.Join("\n", en.Members.Select(m => m.Name));
            return $"enum {en.Name} {{\n{body.Indent(4)}\n}}";
        }

        #endregion


        #region Expressions

        public override string VisitCallExpression(AstCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this, 0));
            var argsStr = string.Join(", ", args);
            var func = call.Function.Accept(this, 0);
            return $"{func}({argsStr})";
        }

        public override string VisitStringLiteral(AstStringLiteral str, int data = 0)
        {
            return $"\"{str.Value.Replace("`", "``").Replace("\r", "").Replace("\n", "`n").Replace("\"", "`\"")}\"";
        }

        public override string VisitIdentifierExpression(AstIdentifierExpr ident, int indentLevel = 0)
        {
            return ident.Name;
        }

        public override string VisitBinaryExpression(AstBinaryExpr bin, int data = 0)
        {
            var left = bin.Left.Accept(this, 0);
            var right = bin.Right.Accept(this, 0);
            return $"{left} {bin.Operator} {right}";
        }

        public override string VisitUnaryExpression(AstUnaryExpr bin, int data = 0)
        {
            var sub = bin.SubExpr.Accept(this, 0);
            return bin.Operator + sub;
        }

        public override string VisitNumberExpression(AstNumberExpr num, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            if (num.Data.IntBase == 2)
                sb.Append('b');
            else if (num.Data.IntBase == 16)
                sb.Append('x');
            sb.Append(num.Data.StringValue);
            sb.Append(num.Data.Suffix);
            return sb.ToString();
        }

        public override string VisitAddressOfExpression(AstAddressOfExpr add, int data = 0)
        {
            return "&" + add.SubExpression.Accept(this, 0);
        }

        public override string VisitDereferenceExpression(AstDereferenceExpr deref, int data = 0)
        {
            return "*" + deref.SubExpression.Accept(this, 0);
        }

        public override string VisitArrayAccessExpression(AstArrayAccessExpr arr, int data = 0)
        {
            var sub = arr.SubExpression.Accept(this, 0);
            var ind = arr.Indexer.Accept(this, 0);
            return $"{sub}[{ind}]";
        }

        public override string VisitBoolExpression(AstBoolExpr bo, int data = 0)
        {
            return bo.Value.ToString();
        }

        public override string VisitCastExpression(AstCastExpr cast, int data = 0)
        {
            return $"<{cast.ParseTreeNode.TargetType.ToString()}>({cast.SubExpression.Accept(this, 0)})";
        }

        public override string VisitDotExpression(AstDotExpr dot, int data = 0)
        {
            return $"{dot.Left.Accept(this, 0)}.{dot.Right}";
        }

        public override string VisitStructValueExpression(AstStructValueExpr str, int data = 0)
        {
            const int maxOnOneLine = 4;

            var sep = ", ";
            if (str.MemberInitializers.Count() > maxOnOneLine)
                sep = "\n";
            var body = string.Join(sep, str.MemberInitializers.Select(m => m.Name != null ? $"{m.Name} = {m.Value.Accept(this, 0)}" : m.Value.Accept(this, 0)));

            if (str.MemberInitializers.Count() > maxOnOneLine)
            {
                body = $"{{\n{body.Indent(4)}\n}}";
            }
            else
            {
                body = $"{{ {body} }}";
            }

            return $"{str.TypeExpr} {body}";
        }

        public override string VisitEmptyExpression(AstEmptyExpr em, int data = 0)
        {
            int len = 0;
            if (em.GenericParseTreeNode.Beginning.line == em.GenericParseTreeNode.End.line)
                len = em.GenericParseTreeNode.End.end - em.GenericParseTreeNode.Beginning.index;
            if (len < 1)
                len = 1;
            return new string('§', len);
        }

        #endregion
    }
}
