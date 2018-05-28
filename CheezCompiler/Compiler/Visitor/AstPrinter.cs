using System.Linq;
using System.Text;
using Cheez.Compiler.Ast;

namespace Cheez.Compiler.Visitor
{
    public class AstPrinter : VisitorBase<string, int>
    {
        public override string VisitFunctionDeclaration(AstFunctionDecl function, int indentLevel = 0)
        {
            var statements = function.Statements.Select(s => s.Accept(this));
            var statementsStr = string.Join("\n", statements);
            return Indent($"fn {function.Name} :: ()\n{{\n{Indent(statementsStr, 4)}\n}}", indentLevel);
        }

        public static string Indent(string s, int level)
        {
            if (level == 0)
                return s;
            return string.Join("\n", s.Split('\n').Select(line => $"{new string(' ', level)}{line}"));
        }

        public override string VisitStringLiteral(AstStringLiteral str, int data = 0)
        {
            return $"\"{str.Value.Replace("`", "``").Replace("\r", "").Replace("\n", "`n").Replace("\"", "`\"")}\"";
        }

        public override string VisitVariableDeclaration(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var ").Append(variable.Name);
            if (variable.Type != null)
                sb.Append(" : ").Append(variable.Type);
            if (variable.Initializer != null)
            {
                sb.Append(" = ");
                sb.Append(variable.Initializer.Accept(this, indentLevel));
            }
            return sb.ToString();
        }

        public override string VisitIdentifierExpression(AstIdentifierExpr ident, int indentLevel = 0)
        {
            return ident.Name;
        }

        //public override string VisitConstantDeclaration(ConstantDeclaration constant, int indentLevel = 0)
        //{
        //    return constant.Name + " = " + constant.Value.Accept(this);
        //}

        public override string VisitAssignment(AstAssignment ass, int indentLevel = 0)
        {
            return ass.Target.Accept(this) + " = " + ass.Value.Accept(this);
        }

        public override string VisitExpressionStatement(AstExprStmt stmt, int indentLevel = 0)
        {
            return stmt.Expr.Accept(this);
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
            return Indent(sb.ToString(), indentLevel);
        }

        public override string VisitBlockStatement(AstBlockStmt block, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var s in block.Statements)
            {
                sb.AppendLine(Indent(s.Accept(this), 4));
            }
            sb.Append("}");
            return Indent(sb.ToString(), indentLevel);
        }
    }
}
