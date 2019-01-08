using System.IO;
using System.Linq;
using System.Text;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Compiler;
using Cheez.Util;

namespace Cheez.Visitors
{
    public class AnalyzedAstPrinter : VisitorBase<string, int>
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

        public override string VisitParameter(AstParameter param, int data = 0)
        {
            if (param.Name != null)
                return $"{param.Name.Accept(this)}: {param.Type}";
            return param.Type?.ToString();
        }

        public override string VisitFunctionDecl(AstFunctionDecl function, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            var body = function.Body?.Accept(this) ?? ";";

            var pars = string.Join(", ", function.Parameters.Select(p => $"{p.Name.Accept(this)}: {p.Type}"));
            var head = $"fn {function.Name.Accept(this)}";

            head += $"({pars})";

            if (function.ReturnValue != null)
                head += $" -> {function.ReturnValue.Accept(this)}";

            sb.Append($"{head} {body}".Indent(indentLevel));

            // polies
            if (function.PolymorphicInstances?.Count > 0)
            {
                sb.AppendLine($"// Polymorphic instances for {head}");
                foreach (var pi in function.PolymorphicInstances)
                {
                    var args = string.Join(", ", pi.PolymorphicTypes.Select(kv => $"{kv.Key} = {kv.Value}"));
                    sb.AppendLine($"// {args}".Indent(4));
                    sb.AppendLine(pi.Accept(this).Indent(4));
                }
            }

            return sb.ToString();
        }

        public override string VisitReturnStmt(AstReturnStmt ret, int data = 0)
        {
            var sb = new StringBuilder();

            if (ret.DeferredStatements.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("// deferred statements");
                foreach (var s in ret.DeferredStatements)
                {
                    sb.AppendLine(s.Accept(this));
                }

                sb.AppendLine("// return");
            }

            sb.Append("return");
            if (ret.ReturnValues.Count != 0)
                sb.Append(" ").Append(string.Join(", ", ret.ReturnValues.Select(rv => rv.Accept(this))));
            return sb.ToString();
        }

        public override string VisitTypeAliasDecl(AstTypeAliasDecl al, int data = 0)
        {
            return $"typedef {al.Name.Accept(this)} = {al.Type}";
        }

        public override string VisitUsingStmt(AstUsingStmt use, int data = 0)
        {
            return $"using {use.Value.Accept(this)}";
        }

        public override string VisitStructDecl(AstStructDecl str, int data = 0)
        {
            var body = string.Join("\n", str.Members.Select(m => $"{m.Name.Accept(this)}: {m.Type}"));
            var head = $"struct {str.Name.Accept(this)}";

            if (str.Parameters?.Count > 0)
            {
                head += "(";
                head += string.Join(", ", str.Parameters.Select(p => $"{p.Name.Accept(this)}: {p.Type}"));
                head += ")";
            }

            var sb = new StringBuilder();
            sb.Append($"{head} {{\n{body.Indent(4)}\n}}");

            // polies
            if (str.PolymorphicInstances?.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"// Polymorphic instances for {head}");
                foreach (var pi in str.PolymorphicInstances)
                {
                    var args = string.Join(", ", pi.Parameters.Select(p => $"{p.Name.Accept(this)} = {p.Value}"));
                    sb.AppendLine($"// {args}".Indent(4));
                    sb.AppendLine(pi.Accept(this).Indent(4));
                }
            }

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
                header += impl.Trait + " for ";
            }

            header += impl.TargetType;

            return $"{header} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitVariableDecl(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("let ");
            sb.Append(variable.Pattern.Accept(this));
            sb.Append($": {variable.Type}");
            if (variable.Initializer != null)
                sb.Append($" = {variable.Initializer.Accept(this)}");
            return sb.ToString();
        }

        public override string VisitIfStmt(AstIfStmt ifs, int indentLevel = 0)
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

        public override string VisitBlockStmt(AstBlockStmt block, int indentLevel = 0)
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

            if (block.DeferredStatements.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("// deferred statements");
                for (int i = block.DeferredStatements.Count - 1; i >= 0; i--)
                {
                    var s = block.DeferredStatements[i];
                    sb.Append(s.Accept(this));

                    if (i > 0)
                        sb.AppendLine();
                }
            }

            return $"{{\n{sb.ToString().Indent(4)}\n}}".Indent(indentLevel);
        }

        public override string VisitAssignmentStmt(AstAssignment ass, int indentLevel = 0)
        {
            return ass.Target.Accept(this) + $" {ass.Operator}= " + ass.Value.Accept(this);
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

        public override string VisitBreakStmt(AstBreakStmt br, int data = 0)
        {
            var sb = new StringBuilder();

            if (br.DeferredStatements.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("// deferred statements");
                foreach (var s in br.DeferredStatements)
                {
                    sb.AppendLine(s.Accept(this));
                }
                sb.AppendLine("// break");
            }

            sb.Append("break");
            return sb.ToString();
        }

        public override string VisitContinueStmt(AstContinueStmt cont, int data = 0)
        {
            var sb = new StringBuilder();

            if (cont.DeferredStatements.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("// deferred statements");
                foreach (var s in cont.DeferredStatements)
                {
                    sb.AppendLine(s.Accept(this));
                }
                sb.AppendLine("// continue");
            }

            sb.Append("continue");
            return sb.ToString();
        }

        #endregion


        #region Expressions
        public override string VisitArrayExpr(AstArrayExpr arr, int data = 0)
        {
            var vals = string.Join(", ", arr.Values.Select(v => v.Accept(this)));
            return $"[{vals}]";
        }

        public override string VisitTypeExpr(AstTypeRef astArrayTypeExpr, int data = 0)
        {
            return astArrayTypeExpr.Type?.ToString();
        }

        public override string VisitCompCallExpr(AstCompCallExpr call, int data = 0)
        {
            var args = call.Arguments.Select(a => a.Accept(this));
            var argsStr = string.Join(", ", args);
            return $"@{call.Name.Accept(this)}({argsStr})";
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
            switch (expr.CharValue)
            {
                case '\0': return "'`0'";
                case '\r': return "'`r'";
                case '\n': return "'`n'";
                case '\t': return "'`t'";
                default: return $"'{expr.CharValue.ToString()}'";
            }
        }

        public override string VisitStringLiteralExpr(AstStringLiteral str, int data = 0)
        {
            string v = str.StringValue;
            v = v.Replace("`", "``").Replace("\r", "`r").Replace("\n", "`n").Replace("\0", "`0");
            return $"\"{v.Replace("\"", "`\"")}\"";
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
            sb.Append(num.Data.Suffix);
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
                return $"cast({cast.Type}) ({cast.SubExpression.Accept(this, 0)})";
            return $"cast ({cast.SubExpression.Accept(this, 0)})";
        }

        public override string VisitDotExpr(AstDotExpr dot, int data = 0)
        {
            return $"{dot.Left.Accept(this, 0)}.{dot.Right.Accept(this)}";
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

            return $"new {str.Type} {body}";
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
