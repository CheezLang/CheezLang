using System.IO;
using System.Linq;
using System.Text;
using Cheez.Compiler.Ast;

namespace Cheez.Compiler.Visitor
{
    public class RawAstPrinter : VisitorBase<string, int>
    {
        private TextWriter _writer;

        public RawAstPrinter(TextWriter writer)
        {
            _writer = writer;
        }

        public void PrintWorkspace(Workspace workspace)
        {
            foreach (var s in workspace.Statements)
            {
                _writer.WriteLine(s.Accept(this));
                _writer.WriteLine();
            }
        }

        public void PrintStatement(AstStatement statement)
        {
            _writer.WriteLine(statement.Accept(this));
        }

        public void PrintExpression(AstExpression expr)
        {
            _writer.WriteLine(expr.Accept(this));
        }

        #region Statements

        public override string VisitFunctionDeclaration(AstFunctionDecl function, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            var body = function.Body?.Accept(this) ?? ";";

            var pars = string.Join(", ", function.Parameters.Select(p => $"{p.Name.Accept(this)}: {p.TypeExpr.Accept(this)}"));
            var head = $"fn {function.Name.Accept(this)}";

            head += $"({pars})";

            if (function.ReturnTypeExpr != null)
            {
                head += $" -> {function.ReturnTypeExpr.Accept(this)}";
            }

            sb.Append($"{head} {body}".Indent(indentLevel));

            return sb.ToString();
        }

        public override string VisitReturnStatement(AstReturnStmt ret, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("return");
            if (ret.ReturnValue != null)
                sb.Append(" ").Append(ret.ReturnValue.Accept(this));
            return sb.ToString();
        }

        public override string VisitTypeAlias(AstTypeAliasDecl al, int data = 0)
        {
            return $"type {al.Name.Accept(this)} = {al.TypeExpr.Accept(this)}";
        }

        public override string VisitUsingStatement(AstUsingStmt use, int data = 0)
        {
            return $"using {use.Value.Accept(this)}";
        }

        public override string VisitStructDeclaration(AstStructDecl str, int data = 0)
        {
            var body = string.Join("\n", str.Members.Select(m => $"{m.Name.Accept(this)}: {m.TypeExpr.Accept(this)}"));
            var head = $"struct {str.Name.Accept(this)}";

            if (str.Parameters?.Count > 0)
            {
                head += "(";
                head += string.Join(", ", str.Parameters.Select(p => $"{p.Name.Accept(this)}: {p.TypeExpr.Accept(this)}"));
                head += ")";
            }

            var sb = new StringBuilder();
            sb.Append($"{head} {{\n{body.Indent(4)}\n}}");

            return sb.ToString();
        }

        public override string VisitTraitDeclaration(AstTraitDeclaration trait, int data = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"trait {trait.Name.Accept(this)} {{");

            foreach (var f in trait.Functions)
            {
                sb.AppendLine(f.Accept(this).Indent(4));
            }

            sb.Append("}");

            return sb.ToString().Indent(data);
        }

        public override string VisitImplBlock(AstImplBlock impl, int data = 0)
        {
            var body = string.Join("\n\n", impl.Functions.Select(f => f.Accept(this, 0)));

            var header = "impl ";

            if (impl.TraitExpr != null)
            {
                header += impl.TraitExpr.Accept(this) + " for ";
            }

            header += impl.TargetTypeExpr.Accept(this);

            return $"{header} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitVariableDeclaration(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("let ").Append(variable.Name.Accept(this));
            if (variable.TypeExpr != null)
                sb.Append($": {variable.TypeExpr.Accept(this)}");
            if (variable.Initializer != null)
                sb.Append($" = {variable.Initializer.Accept(this)}");
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
            if (wh.PreAction != null) sb.Append(wh.PreAction.Accept(this) + "; ");
            sb.Append(wh.Condition.Accept(this));
            if (wh.PostAction != null) sb.Append("; " + wh.PostAction.Accept(this));
            sb.Append(" " + wh.Body.Accept(this));
            return sb.ToString().Indent(indentLevel);
        }

        public override string VisitDeferStatement(AstDeferStmt def, int data = 0)
        {
            return $"defer {def.Deferred.Accept(this)}".Indent(data);
        }

        public override string VisitBlockStatement(AstBlockStmt block, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            {
                int i = 0;
                foreach (var s in block.Statements)
                {
                    if (i > 0) sb.AppendLine();
                    sb.Append(s.Accept(this));
                    ++i;
                }
            }

            return $"{{\n{sb.ToString().Indent(4)}\n}}".Indent(indentLevel);
        }

        public override string VisitAssignment(AstAssignment ass, int indentLevel = 0)
        {
            return ass.Target.Accept(this) + $" {ass.Operator}= " + ass.Value.Accept(this);
        }

        public override string VisitExpressionStatement(AstExprStmt stmt, int indentLevel = 0)
        {
            return stmt.Expr.Accept(this);
        }

        public override string VisitEnumDeclaration(AstEnumDecl en, int data = 0)
        {
            var body = string.Join("\n", en.Members.Select(m => m.Name.Accept(this)));
            return $"enum {en.Name.Accept(this)} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitBreakStatement(AstBreakStmt br, int data = 0) => "break";

        public override string VisitContinueStatement(AstContinueStmt cont, int data = 0) => "continue";

        #endregion


        #region Expressions
        public override string VisitFunctionTypeExpr(AstFunctionTypeExpr func, int data = 0)
        {
            var args = string.Join(", ", func.ParameterTypes.Select(p => p.Accept(this)));
            if (func.ReturnType != null)
                return $"fn({args}) -> {func.ReturnType.Accept(this)}";
            else
                return $"fn({args})";
        }

        public override string VisitArrayExpression(AstArrayExpression arr, int data = 0)
        {
            var vals = string.Join(", ", arr.Values.Select(v => v.Accept(this)));
            return $"[{vals}]";
        }

        public override string VisitTypeExpr(AstTypeExpr astArrayTypeExpr, int data = 0)
        {
            return astArrayTypeExpr.Type.ToString();
        }

        public override string VisitArrayTypeExpr(AstArrayTypeExpr astArrayTypeExpr, int data = 0)
        {
            return $"[]{astArrayTypeExpr.Target.Accept(this)}";
        }

        public override string VisitCompCallExpression(AstCompCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this));
            var argsStr = string.Join(", ", args);
            return $"@{call.Name.Accept(this)}({argsStr})";
        }

        public override string VisitCallExpression(AstCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this));
            var argsStr = string.Join(", ", args);
            var func = call.Function.Accept(this);
            return $"{func}({argsStr})";
        }

        public override string VisitStringLiteral(AstStringLiteral str, int data = 0)
        {
            string v = null;
            if (str.IsChar)
                v = str.CharValue.ToString();
            else
                v = str.StringValue;

            v = v.Replace("`", "``").Replace("\r", "").Replace("\n", "`n");

            if (str.IsChar)
                return $"'{v.Replace("'", "`'")}'";
            else 
                return $"\"{v.Replace("\"", "`\"")}\"";
        }

        public override string VisitIdentifierExpression(AstIdentifierExpr ident, int indentLevel = 0)
        {
            if (ident.IsPolymorphic)
                return '$' + ident.Name;
            return ident.Name;
        }

        public override string VisitBinaryExpression(AstBinaryExpr bin, int data = 0)
        {
            var left = bin.Left.Accept(this, 0);
            var right = bin.Right.Accept(this, 0);
            return $"({left} {bin.Operator} {right})";
        }

        public override string VisitUnaryExpression(AstUnaryExpr bin, int data = 0)
        {
            var sub = bin.SubExpr.Accept(this, 0);
            return bin.Operator + sub;
        }

        public override string VisitNullExpression(AstNullExpr nul, int data = 0) => "null";

        public override string VisitNumberExpression(AstNumberExpr num, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            if (num.Data.IntBase == 2)
                sb.Append("0b");
            else if (num.Data.IntBase == 16)
                sb.Append("0x");
            sb.Append(num.Data.StringValue);
            sb.Append(num.Data.Suffix);
            return sb.ToString();
        }

        public override string VisitAddressOfExpression(AstAddressOfExpr add, int data = 0)
        {
            return "&" + add.SubExpression.Accept(this);
        }

        public override string VisitDereferenceExpression(AstDereferenceExpr deref, int data = 0)
        {
            return "<<" + deref.SubExpression.Accept(this);
        }

        public override string VisitArrayAccessExpression(AstArrayAccessExpr arr, int data = 0)
        {
            var sub = arr.SubExpression.Accept(this);
            var ind = arr.Indexer.Accept(this);
            return $"{sub}[{ind}]";
        }

        public override string VisitBoolExpression(AstBoolExpr bo, int data = 0)
        {
            return bo.BoolValue ? "true" : "false";
        }

        public override string VisitCastExpression(AstCastExpr cast, int data = 0)
        {
            if (cast.TypeExpr != null)
                return $"({cast.TypeExpr.Accept(this, 0)})({cast.SubExpression.Accept(this, 0)})";
            return $"cast ({cast.SubExpression.Accept(this, 0)})";
        }

        public override string VisitDotExpression(AstDotExpr dot, int data = 0)
        {
            return $"{dot.Left.Accept(this, 0)}.{dot.Right.Accept(this)}";
        }

        public override string VisitStructValueExpression(AstStructValueExpr str, int data = 0)
        {
            const int maxOnOneLine = 4;

            var sep = ", ";
            if (str.MemberInitializers.Count() > maxOnOneLine)
                sep = "\n";
            var body = string.Join(sep, str.MemberInitializers.Select(m => m.Name != null ? $"{m.Name.Accept(this)} = {m.Value.Accept(this, 0)}" : m.Value.Accept(this, 0)));

            if (str.MemberInitializers.Count() > maxOnOneLine)
            {
                body = $"{{\n{body.Indent(4)}\n}}";
            }
            else
            {
                body = $"{{ {body} }}";
            }

            return $"new {str.TypeExpr.Accept(this)} {body}";
        }

        public override string VisitEmptyExpression(AstEmptyExpr em, int data = 0)
        {
            int len = 0;
            if (em.Beginning.line == em.End.line)
                len = em.End.end - em.Beginning.index;
            if (len < 1)
                len = 1;
            return new string('§', len);
        }

        #endregion
    }
}
