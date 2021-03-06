using System;
using System.IO;
using System.Linq;
using System.Text;
using Cheez.Ast;
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
            foreach (var file in workspace.Files)
            {
                _writer.WriteLine($"#file {Path.GetFileName(file.Name)}");
                foreach (var s in file.Statements)
                {
                    _writer.WriteLine(s.Accept(this));
                    _writer.WriteLine();
                }
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

        public string VisitFunctionSignature(AstFuncExpr function)
        {
            var pars = string.Join(", ", function.Parameters.Select(p => p.Accept(this)));
            var head = $"({pars})";

            if (function.ReturnTypeExpr != null)
                head += $" -> {function.ReturnTypeExpr.Accept(this)}";

            return head;
        }

        public override string VisitReturnStmt(AstReturnStmt ret, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("return");
            if (ret.ReturnValue != null)
                sb.Append(" ").Append(ret.ReturnValue.Accept(this));
            return sb.ToString();
        }

        public override string VisitUsingStmt(AstUsingStmt use, int data = 0)
        {
            return $"use {use.Value.Accept(this)}";
        }

        public override string VisitTraitTypeExpr(AstTraitTypeExpr trait, int data = 0)
        {
            var head = $"trait";
            if (trait.Parameters != null && trait.Parameters.Count > 0)
            {
                head += $"({string.Join(", ", trait.Parameters.Select(p => p.Accept(this)))})";
            }
            return head;

            // var sb = new StringBuilder();
            // sb.AppendLine("trait {");

            // foreach (var f in trait.Members)
            //     sb.AppendLine(f.Decl.Accept(this).Indent(4));

            // foreach (var f in trait.Functions)
            //     sb.AppendLine($"{f.Name} :: f.Accept(this)".Indent(4));

            // sb.Append("}");

            // return sb.ToString().Indent(data);
        }

        private string VisitImplCondition(ImplCondition cond)
        {
            switch (cond)
            {
                case ImplConditionImplTrait c:
                    return $"{c.type.Accept(this)} : {c.trait.Accept(this)}";
                case ImplConditionNotYet c:
                    return "#notyet";
                case ImplConditionAny c:
                    return c.Expr.Accept(this);
                default: throw new NotImplementedException();
            }
        }

        public override string VisitImplDecl(AstImplBlock impl, int data = 0)
        {
            var body = string.Join("\n\n", impl.Functions.Select(f => f.Accept(this, 0)));

            var header = "impl";

            // parameters
            if (impl.Parameters != null)
                header += "(" + string.Join(", ", impl.Parameters.Select(p => p.Accept(this, 0))) + ")";

            header += " ";

            if (impl.TraitExpr != null)
                header += impl.TraitExpr.Accept(this) + " for ";

            header += impl.TargetTypeExpr.Accept(this);

            if (impl.Conditions != null)
                header += " if " + string.Join(", ", impl.Conditions.Select(c => VisitImplCondition(c)));

            return $"{header} {{\n{body.Indent(4)}\n}}";
        }

        public override string VisitConstantDeclaration(AstConstantDeclaration decl, int data = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(decl.Pattern.Accept(this));

            if (decl.TypeExpr != null)
                sb.Append($" : {decl.TypeExpr.Accept(this)} ");
            else
                sb.Append(" :");

            sb.Append(": ");
            sb.Append(decl.Initializer.Accept(this));
            return sb.ToString();
        }

        public override string VisitVariableDecl(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(variable.Pattern.Accept(this));

            if (variable.TypeExpr != null)
                sb.Append($" : {variable.TypeExpr.Accept(this)} ");
            else
                sb.Append(" :");

            if (variable.Initializer != null)
            {
                sb.Append("= ");
                sb.Append(variable.Initializer.Accept(this));
            }
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

        public override string VisitForStmt(AstForStmt stmt, int data = 0)
        {
            var result = "";
            result += "for";

            if (stmt.Arguments != null)
            {
                result += "(";
                result += string.Join(", ", stmt.Arguments.Select(a => a.Accept(this)));
                result += ")";
            }

            result += " ";

            if (stmt.VarName != null)
            {
                result += stmt.VarName.Accept(this);
                if (stmt.IndexName != null)
                    result += $", {stmt.IndexName.Accept(this)} ";
                else result += " ";
            }

            result += ": ";
            result += stmt.Collection.Accept(this);
            result += " ";
            result += stmt.Body.Accept(this);

            return result;
        }

        public override string VisitWhileStmt(AstWhileStmt wh, int indentLevel = 0)
        {
            var str = "loop ";
            if (wh.Label != null)
                str += " #label " + wh.Label.Name;
            str += wh.Body.Accept(this);
            return str;
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

        public override string VisitEnumTypeExpr(AstEnumTypeExpr en, int data = 0)
        {
            var body = string.Join("\n", en.Declarations.Select(m => m.Accept(this)));
            var head = $"enum";
            if (en.Parameters != null && en.Parameters.Count > 0)
            {
                head += $"({string.Join(", ", en.Parameters.Select(p => p.Accept(this)))})";
            }
            // return $"{head} {{\n{body.Indent(4)}\n}}";
            return head;
        }

        public override string VisitBreakExpr(AstBreakExpr br, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("break");
            if (br.Label != null)
                sb.Append($" {br.Label.Accept(this)}");
            return sb.ToString();
        }

        public override string VisitContinueExpr(AstContinueExpr cont, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("continue");
            if (cont.Label != null)
                sb.Append($" {cont.Label.Accept(this)}");
            return sb.ToString();
        }

        #endregion


        #region Expression

        public override string VisitGenericExpr(AstGenericExpr expr, int data = 0)
        {
            var args = string.Join(", ", expr.Parameters.Select(p => p.Accept(this)));
            return $"[{args}] {expr.SubExpression.Accept(this)}";
        }

        public override string VisitMoveAssignExpr(AstMoveAssignExpr expr, int data = 0)
        {
            return $"{expr.Target.Accept(this)} <- {expr.Source.Accept(this)}";
        }
        public override string VisitImportExpr(AstImportExpr expr, int data = 0)
        {
            var result = "import ";
            result += string.Join(".", expr.Path.Select(p => p.Accept(this)));

            return result;
        }

        public override string VisitRangeExpr(AstRangeExpr expr, int data = 0)
        {
            return $"{expr.From?.Accept(this)}..{expr.To?.Accept(this)}";
        }

        public override string VisitEnumValueExpr(AstEnumValueExpr expr, int data = 0)
        {
            if (expr.Argument != null)
                return $"{expr.Type}.{expr.Member.Name}({expr.Argument.Accept(this)})";
            return $"{expr.Type}.{expr.Member.Name}";
        }

        public string VisitMatchCase(AstMatchCase c)
        {
            var result = "";

            result += c.Pattern.Accept(this);
            if (c.Condition != null)
            {
                result += " if " + c.Condition.Accept(this);
            }

            result += " -> " + c.Body.Accept(this);

            return result;
        }

        public override string VisitMatchExpr(AstMatchExpr expr, int data = 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"match {expr.SubExpression.Accept(this)} {{");
            foreach (var use in expr.Uses)
                sb.AppendLine(use.Accept(this).Indent(4));
            foreach (var c in expr.Cases)
                sb.AppendLine(VisitMatchCase(c).Indent(4));
            sb.Append("}");

            return sb.ToString();
        }

        public override string VisitDefaultExpr(AstDefaultExpr expr, int data = 0)
        {
            return "default";
        }

        public override string VisitUfcFuncExpr(AstUfcFuncExpr expr, int data = 0)
        {
            return $"{expr.FunctionDecl.Name}";
        }

        public override string VisitSymbolExpr(AstSymbolExpr te, int data = 0)
        {
            return te.Symbol.Name ?? te.Symbol.ToString();
        }

        public override string VisitTempVarExpr(AstTempVarExpr te, int data = 0)
        {
            return $"@tempvar({te.Id})";
        }

        public override string VisitTupleExpr(AstTupleExpr expr, int data = 0)
        {
            var members = string.Join(", ", expr.Types.Select(v =>
            {
                var sb = new StringBuilder();

                if (v.Name != null)
                    sb.Append(v.Name.Name).Append(": ");

                sb.Append(v.TypeExpr.Accept(this));
                if (v.DefaultValue != null)
                    sb.Append(" = ").Append(v.DefaultValue.Accept(this));
                return sb.ToString();
            }));
            return "(" + members + ")";
        }

        public override string VisitArrayExpr(AstArrayExpr arr, int data = 0)
        {
            var vals = string.Join(", ", arr.Values.Select(v => v.Accept(this)));
            return $"[{vals}]";
        }

        public override string VisitTypeExpr(AstTypeRef type, int data = 0)
        {
            return type.Value.ToString();
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
            var func = call.FunctionExpr.Accept(this);
            return $"{func}({argsStr})";
        }

        public override string VisitCharLiteralExpr(AstCharLiteral expr, int data = 0)
        {
            string v = expr.RawValue;
            v = v.Replace("`", "``", StringComparison.InvariantCulture)
                .Replace("\r", "`r", StringComparison.InvariantCulture)
                .Replace("\n", "`n", StringComparison.InvariantCulture)
                .Replace("\0", "`0", StringComparison.InvariantCulture);
            return $"'{v}'";
        }

        public override string VisitStringLiteralExpr(AstStringLiteral str, int data = 0)
        {
            string v = str.StringValue;
            v = v.Replace("`", "``", StringComparison.InvariantCulture)
                .Replace("\r", "`r", StringComparison.InvariantCulture)
                .Replace("\n", "`n", StringComparison.InvariantCulture)
                .Replace("\0", "`0", StringComparison.InvariantCulture);
            v = $"\"{v.Replace("\"", "`\"", StringComparison.InvariantCulture)}\"";
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
            if (add.Reference)
                return $"&{add.SubExpression.Accept(this)}";
            return "^" + add.SubExpression.Accept(this);
        }

        public override string VisitDerefExpr(AstDereferenceExpr deref, int data = 0)
        {
            if (deref.Reference)
                return $"*{deref.SubExpression.Accept(this)}";
            return "*" + deref.SubExpression.Accept(this);
        }

        public override string VisitArrayAccessExpr(AstArrayAccessExpr arr, int data = 0)
        {
            var sub = arr.SubExpression.Accept(this);
            var args = string.Join(", ", arr.Arguments.Select(a => a.Accept(this)));
            return $"{sub}[{args}]";
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
            return $"{dot.Left?.Accept(this, 0)}.{dot.Right.Accept(this)}";
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
        public override string VisitSliceTypeExpr(AstSliceTypeExpr type, int data = 0)
        {
            return $"[]{type.Target.Accept(this)}";
        }

        public override string VisitStructTypeExpr(AstStructTypeExpr expr, int data = default(int)) {
            var body = string.Join("\n", expr.Declarations.Select(m => m.Accept(this)));
            var head = $"struct";
            if (expr.Parameters != null && expr.Parameters.Count > 0)
            {
                head += $"({string.Join(", ", expr.Parameters.Select(p => p.Accept(this)))})";
            }
            // return $"{head} {{\n{body.Indent(4)}\n}}";
            return head;
        }

        public override string VisitFuncExpr(AstFuncExpr expr, int data = default(int)) {
            var pars = string.Join(", ", expr.Parameters.Select(p => p.Accept(this)));
            var head = $"({pars})";

            if (expr.ReturnTypeExpr != null)
                head += $" -> {expr.ReturnTypeExpr.Accept(this)}";

            return head;
        }

        public override string VisitFunctionTypeExpr(AstFunctionTypeExpr type, int data = 0)
        {
            string fn = type.IsFatFunction ? "Fn" : "fn";
            var args = string.Join(", ", type.ParameterTypes.Select(p => p.Accept(this)));
            if (type.ReturnType != null)
                return $"{fn}({args}) -> {type.ReturnType.Accept(this)}";
            else
                return $"{fn}({args})";
        }

        public override string VisitArrayTypeExpr(AstArrayTypeExpr type, int data = 0)
        {
            return $"[{type.SizeExpr.Accept(this)}]{type.Target.Accept(this)}";
        }

        public override string VisitReferenceTypeExpr(AstReferenceTypeExpr type, int data = 0)
        {
            return $"&{type.Target.Accept(this)}";
        }
        #endregion


        public override string VisitDirective(AstDirective dir, int data = 0)
        {
            var name = "#" + dir.Name.Accept(this);
            if (dir.Arguments.Count > 0)
            {
                name += $"({string.Join(", ", dir.Arguments.Select(a => a.Accept(this)))})";
            }
            return name;
        }
    }
}
