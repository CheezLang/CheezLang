using System.IO;
using System.Linq;
using System.Text;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Util;

namespace Cheez.Visitors
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
            _writer.Write(statement.Accept(this));
        }

        public void PrintExpression(AstExpression expr)
        {
            _writer.Write(expr.Accept(this));
        }

        #region Statements

        public override string VisitParameter(AstParameter param, int data = 0)
        {
            StringBuilder result = new StringBuilder();

            if (param.Name != null)
                result.Append(param.Name.Accept(this)).Append(": ");

            result.Append(param.TypeExpr.Accept(this));

            if (param.DefaultValue != null)
                result.Append(" = ").Append(param.DefaultValue.Accept(this));

            return result.ToString();
        }

        public override string VisitFunctionDecl(AstFunctionDecl function, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            var body = function.Body?.Accept(this) ?? ";";

            var pars = string.Join(", ", function.Parameters.Select(p => p.Accept(this)));
            var head = $"fn {function.Name.Accept(this)}";

            head += $"({pars})";

            if (function.ReturnValue != null)
                head += $" -> {function.ReturnValue.Accept(this)}";

            sb.Append($"{head} {body}".Indent(indentLevel));

            return sb.ToString();
        }

        public override string VisitReturnStmt(AstReturnStmt ret, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("return");
            if (ret.ReturnValue != null)
                sb.Append(" ").Append(ret.ReturnValue.Accept(this));
            return sb.ToString();
        }

        public override string VisitTypeAliasDecl(AstTypeAliasDecl al, int data = 0)
        {
            return $"typedef {al.Name.Accept(this)} = {al.TypeExpr.Accept(this)}";
        }

        public override string VisitUsingStmt(AstUsingStmt use, int data = 0)
        {
            return $"using {use.Value.Accept(this)}";
        }

        public override string VisitStructDecl(AstStructDecl str, int data = 0)
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

        public override string VisitTraitDecl(AstTraitDeclaration trait, int data = 0)
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

        public override string VisitImplDecl(AstImplBlock impl, int data = 0)
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

        public override string VisitVariableDecl(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("let ");
            sb.Append(variable.Pattern.Accept(this));
            if (variable.TypeExpr != null)
                sb.Append($": {variable.TypeExpr.Accept(this)}");
            if (variable.Initializer != null)
                sb.Append($" = {variable.Initializer.Accept(this)}");
            return sb.ToString();
        }

        public override string VisitIfExpr(AstIfExpr ifs, int indentLevel = 0)
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

        public override string VisitWhileStmt(AstWhileStmt wh, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.Append("while ");
            if (wh.PreAction != null) sb.Append(wh.PreAction.Accept(this) + "; ");
            sb.Append(wh.Condition.Accept(this));
            if (wh.PostAction != null) sb.Append("; " + wh.PostAction.Accept(this));
            sb.Append(" " + wh.Body.Accept(this));
            return sb.ToString().Indent(indentLevel);
        }

        public override string VisitDeferStmt(AstDeferStmt def, int data = 0)
        {
            return $"defer {def.Deferred.Accept(this)}".Indent(data);
        }

        public override string VisitBlockExpr(AstBlockExpr block, int indentLevel = 0)
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

        public override string VisitAssignmentStmt(AstAssignment ass, int indentLevel = 0)
        {
            return ass.Pattern.Accept(this) + $" {ass.Operator}= " + ass.Value.Accept(this);
        }

        public override string VisitExpressionStmt(AstExprStmt stmt, int indentLevel = 0)
        {
            return stmt.Expr.Accept(this);
        }

        public override string VisitEnumDecl(AstEnumDecl en, int data = 0)
        {
            var body = string.Join("\n", en.Members.Select(m => m.Name.Accept(this)));
            return $"enum {en.Name.Accept(this)} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitBreakStmt(AstBreakStmt br, int data = 0) => "break";

        public override string VisitContinueStmt(AstContinueStmt cont, int data = 0) => "continue";

        #endregion


        #region Expressions

        public override string VisitSymbolExpr(AstSymbolExpr te, int data = 0)
        {
            return $"@symbol({te.Symbol.Name?.Accept(this) ?? te.Symbol.ToString()})";
        }

        public override string VisitTempVarExpr(AstTempVarExpr te, int data = 0)
        {
            return $"@tempvar({te.Id})";
        }

        public override string VisitTupleExpr(AstTupleExpr expr, int data = 0)
        {
            var members = string.Join(", ", expr.Values.Select(v => v.Accept(this)));
            return "(" + members + ")";
        }

        public override string VisitArrayExpr(AstArrayExpr arr, int data = 0)
        {
            var vals = string.Join(", ", arr.Values.Select(v => v.Accept(this)));
            return $"[{vals}]";
        }

        public override string VisitTypeExpr(AstTypeRef astArrayTypeExpr, int data = 0)
        {
            return astArrayTypeExpr.Type.ToString();
        }

        public override string VisitCompCallExpr(AstCompCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this));
            var argsStr = string.Join(", ", args);
            return $"@{call.Name.Accept(this)}({argsStr})";
        }

        public override string VisitArgumentExpr(AstArgument expr, int data = 0)
        {
            if (expr.Name != null)
                return $"{expr.Name.Accept(this)} = {expr.Expr.Accept(this)}";
            return expr.Expr.Accept(this);
        }

        public override string VisitCallExpr(AstCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this));
            var argsStr = string.Join(", ", args);
            var func = call.Function.Accept(this);
            return $"{func}({argsStr})";
        }

        public override string VisitCharLiteralExpr(AstCharLiteral expr, int data = 0)
        {
            string v = expr.RawValue;
            v = v.Replace("`", "``").Replace("\r", "`r").Replace("\n", "`n").Replace("\0", "`0");
            return $"'{v}'";
        }

        public override string VisitStringLiteralExpr(AstStringLiteral str, int data = 0)
        {
            string v = str.StringValue;
            v = v.Replace("`", "``").Replace("\r", "`r").Replace("\n", "`n").Replace("\0", "`0");
            v = $"\"{v.Replace("\"", "`\"")}\"";
            if (str.Suffix != null) v += str.Suffix;
            return v;
        }

        public override string VisitIdExpr(AstIdExpr ident, int indentLevel = 0)
        {
            if (ident.IsPolymorphic)
                return '$' + ident.Name;
            return ident.Name;
        }

        public override string VisitBinaryExpr(AstBinaryExpr bin, int data = 0)
        {
            var left = bin.Left.Accept(this, 0);
            var right = bin.Right.Accept(this, 0);
            return $"({left} {bin.Operator} {right})";
        }

        public override string VisitUnaryExpr(AstUnaryExpr bin, int data = 0)
        {
            var sub = bin.SubExpr.Accept(this, 0);
            return bin.Operator + sub;
        }

        public override string VisitNullExpr(AstNullExpr nul, int data = 0) => "null";

        public override string VisitNumberExpr(AstNumberExpr num, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            if (num.Data.IntBase == 2)
                sb.Append("0b");
            else if (num.Data.IntBase == 16)
                sb.Append("0x");
            sb.Append(num.Data.StringValue);
            if (num.Suffix != null)
                sb.Append(num.Suffix);
            return sb.ToString();
        }

        public override string VisitAddressOfExpr(AstAddressOfExpr add, int data = 0)
        {
            return "&" + add.SubExpression.Accept(this);
        }

        public override string VisitDerefExpr(AstDereferenceExpr deref, int data = 0)
        {
            return "<<" + deref.SubExpression.Accept(this);
        }

        public override string VisitArrayAccessExpr(AstArrayAccessExpr arr, int data = 0)
        {
            var sub = arr.SubExpression.Accept(this);
            var ind = arr.Indexer.Accept(this);
            return $"{sub}[{ind}]";
        }

        public override string VisitBoolExpr(AstBoolExpr bo, int data = 0)
        {
            return bo.BoolValue ? "true" : "false";
        }

        public override string VisitCastExpr(AstCastExpr cast, int data = 0)
        {
            if (cast.TypeExpr != null)
                return $"cast({cast.TypeExpr.Accept(this, 0)}) ({cast.SubExpression.Accept(this, 0)})";
            return $"cast {cast.SubExpression.Accept(this, 0)}";
        }

        public override string VisitDotExpr(AstDotExpr dot, int data = 0)
        {
            return $"{dot.Left.Accept(this, 0)}{(dot.IsDoubleColon ? "::" : ".")}{dot.Right.Accept(this)}";
        }

        public override string VisitStructValueExpr(AstStructValueExpr str, int data = 0)
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

            if (str.TypeExpr != null)
            {
                return $"new {str.TypeExpr.Accept(this)} {body}";
            }
            else
            {
                return $"new {body}";
            }
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


        #region Type expressions

        public override string VisitExprTypeExpression(AstExprTypeExpr type, int data = 0)
        {
            return type.Expression.Accept(this);
        }

        public override string VisitTupleTypeExpr(AstTupleTypeExpr expr, int data = 0)
        {
            var members = string.Join(", ", expr.Members.Select(m => m.Accept(this)));
            return "(" + members + ")";
        }

        public override string VisitSliceTypeExpr(AstSliceTypeExpr type, int data = 0)
        {
            return $"[]{type.Target.Accept(this)}";
        }

        public override string VisitFunctionTypeExpr(AstFunctionTypeExpr type, int data = 0)
        {
            var args = string.Join(", ", type.ParameterTypes.Select(p => p.Accept(this)));
            if (type.ReturnType != null)
                return $"fn({args}) -> {type.ReturnType.Accept(this)}";
            else
                return $"fn({args})";
        }

        public override string VisitArrayTypeExpr(AstArrayTypeExpr type, int data = 0)
        {
            return $"[{type.SizeExpr.Accept(this)}]{type.Target.Accept(this)}";
        }

        public override string VisitPointerTypeExpr(AstPointerTypeExpr type, int data = 0)
        {
            return $"*{type.Target.Accept(this)}";
        }

        public override string VisitPolyStructTypeExpr(AstPolyStructTypeExpr type, int data = 0)
        {
            string args = string.Join(", ", type.Arguments.Select(a => a.Accept(this)));
            return $"{type.Struct.Accept(this)}({args})";
        }

        public override string VisitIdTypeExpr(AstIdTypeExpr type, int data = 0)
        {
            if (type.IsPolymorphic)
                return $"${type.Name}";
            return type.Name;
        }
        #endregion
    }
}
