using System.Linq;
using System.Reflection;
using Cheez.Ast;
using log4net;

namespace Cheez.Visitor
{
    public class AstPrinter : IVisitor<string, int>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string VisitFunctionDeclaration(FunctionDeclaration function, int indentLevel = 0)
        {
            var statements = function.Statements.Select(s => s.Visit(this));
            var statementsStr = string.Join("\n", statements);
            return Indent($"fn {function.Name} :: ()\n{{\n{Indent(statementsStr, 4)}\n}}", indentLevel);
        }

        public static string Indent(string s, int level)
        {
            if (level == 0)
                return s;
            return string.Join("\n", s.Split('\n').Select(line => $"{new string(' ', level)}{line}"));
        }

        public string VisitPrintStatement(PrintStatement print, int indentLevel = 0)
        {
            return Indent($"print {print.Expr.Visit(this)}", indentLevel);
        }

        public string VisitStringLiteral(StringLiteral str, int data = 0)
        {
            return $"\"{str.Value.Replace("`", "``").Replace("\n", "`n").Replace("\"", "`\"")}\"";
        }
    }
}
