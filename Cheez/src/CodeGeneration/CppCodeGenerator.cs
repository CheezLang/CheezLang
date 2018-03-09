using Cheez.Ast;
using Cheez.Visitor;
using System;
using System.Text;

namespace Cheez.CodeGeneration
{
    public class CppCodeGenerator : IVoidVisitor<StringBuilder>
    {
        private StringBuilder mForwardDeclarations = new StringBuilder();

        public string GenerateCode(Statement[] statements)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#include <string>");
            sb.AppendLine("#include <iostream>");
            sb.AppendLine();


            var stmts = new StringBuilder();
            foreach (var s in statements)
                s.Visit(this, stmts);

            sb.AppendLine("// forward declarations");
            sb.AppendLine(mForwardDeclarations.ToString());
            sb.AppendLine();

            sb.AppendLine("// compiled statements");
            sb.AppendLine(stmts.ToString());
            return sb.ToString();
        }

        public void VisitFunctionDeclaration(FunctionDeclaration function, StringBuilder sb)
        {
            string returnType = null;
            if (function.Name == "main")
            {
                returnType = "int";
            }
            else
            {
                returnType = "void";
            }

            mForwardDeclarations.AppendLine($"{returnType} {function.Name}();");
            sb.AppendLine($"{returnType} {function.Name}() {{");
            foreach (var s in function.Statements)
            {
                sb.Append("    ");
                s.Visit(this, sb);
            }
            sb.AppendLine("}");
        }

        public void VisitPrintStatement(PrintStatement print, StringBuilder sb)
        {
            sb.Append("std::cout << ");
            print.Expr.Visit(this, sb);
            sb.AppendLine(@" << '\n';");
        }

        public void VisitIdentifierExpression(IdentifierExpression ident, StringBuilder sb = null)
        {
            sb.Append(ident.Name);
        }

        public void VisitStringLiteral(StringLiteral str, StringBuilder sb)
        {
            sb.Append('"').Append(str.Value.Replace("\n", "\\n").Replace("\"", "\\\"")).Append('"');
        }

        public void VisitVariableDeclaration(VariableDeclaration variable, StringBuilder sb = null)
        {
            string type = variable.TypeName ?? "auto";
            if (type == "string")
                type = "std::string";
            sb.Append($"{type} {variable.Name}");
            if (variable.Initializer != null)
            {
                sb.Append($" = ");
                variable.Initializer.Visit(this, sb);
        }
            sb.AppendLine(";");
        }
    }
}
