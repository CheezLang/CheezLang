using System.Linq;
using System.Text;
using Cheez.Ast;

namespace Cheez.Visitor
{
    public class AstPrinter : VisitorBase<string, int>
    {
        public override string VisitFunctionDeclaration(FunctionDeclaration function, int indentLevel = 0)
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

        public override string VisitPrintStatement(PrintStatement print, int indentLevel = 0)
        {
            string str = string.Join(", ", print.Expressions.Select(e => e.Accept(this)));
            return Indent($"print {str}", indentLevel);
        }

        public override string VisitStringLiteral(StringLiteral str, int data = 0)
        {
            return $"\"{str.Value.Replace("`", "``").Replace("\r", "").Replace("\n", "`n").Replace("\"", "`\"")}\"";
        }

        public override string VisitVariableDeclaration(VariableDeclaration variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var ").Append(variable.Name);
            if (variable.Type != null)
                sb.Append(" : ").Append(variable.Type.Text);
            if (variable.Initializer != null)
            {
                sb.Append(" = ");
                sb.Append(variable.Initializer.Accept(this, indentLevel));
            }
            return sb.ToString();
        }

        public override string VisitIdentifierExpression(IdentifierExpression ident, int indentLevel = 0)
        {
            return ident.Name;
        }

        public override string VisitConstantDeclaration(ConstantDeclaration constant, int indentLevel = 0)
        {
            return constant.Name + " = " + constant.Value.Accept(this);
        }

        public override string VisitAssignment(Assignment ass, int indentLevel = 0)
        {
            return ass.Target.Accept(this) + " = " + ass.Value.Accept(this);
        }

        public override string VisitExpressionStatement(ExpressionStatement stmt, int indentLevel = 0)
        {
            return stmt.Expr.Accept(this);
        }

        public override string VisitNumberExpression(NumberExpression num, int indentLevel = 0)
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

        public override string VisitIfStatement(IfStatement ifs, int indentLevel = 0)
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

        public override string VisitBlockStatement(BlockStatement block, int indentLevel = 0)
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
