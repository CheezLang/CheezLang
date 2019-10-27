using System;
using System.IO;
using System.Linq;
using System.Text;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Util;

namespace Cheez.Visitors
{
    public class AnalysedAstPrinter : VisitorBase<string, int>
    {
        public void PrintWorkspace(Workspace workspace, TextWriter writer)
        {
            foreach (var s in workspace.Statements)
            {
                writer.WriteLine(s.Accept(this, 0));
                writer.WriteLine();
            }
        }

        public string VisitDirective(AstDirective dir)
        {
            var name = "#" + dir.Name.Accept(this);
            if (dir.Arguments.Count > 0)
            {
                name += $"({string.Join(", ", dir.Arguments.Select(a => a.Accept(this)))})";
            }
            return name;
        }

        #region Statements

        public override string VisitParameter(AstParameter param, int data = 0)
        {
            StringBuilder result = new StringBuilder();

            if (param.Name != null)
                result.Append(param.Name.Accept(this)).Append(": ");

            result.Append(param.Type?.ToString());

            if (param.DefaultValue != null)
                result.Append(" = ").Append(param.DefaultValue.Accept(this));

            return result.ToString();
        }

        public override string VisitFunctionDecl(AstFunctionDecl function, int indentLevel = 0)
        {
            if (function.IsGeneric)
            {
                var sb = new StringBuilder();

                var body = function.Body?.Accept(this) ?? ";";

                var pars = string.Join(", ", function.Parameters.Select(p => p.Name != null ? $"{p.Name.Accept(this)}: {p.TypeExpr}" : p.Type.ToString()));
                var head = $"fn {function.Name.Accept(this)}";

                head += $"({pars})";

                if (function.ReturnTypeExpr != null)
                    head += $" -> {function.ReturnTypeExpr.TypeExpr.Accept(this)}";

                sb.Append($"{head} {body}".Indent(indentLevel));

                // polies
                if (function.PolymorphicInstances?.Count > 0)
                {
                    sb.AppendLine($"\n// Polymorphic instances for {head}");
                    foreach (var pi in function.PolymorphicInstances)
                    {
                        if (pi.PolymorphicTypes != null)
                        {
                            var args = string.Join(", ", pi.PolymorphicTypes.Select(kv => $"{kv.Key} = {kv.Value}"));
                            sb.AppendLine($"/* {args} */".Indent(4));
                        }
                        if (pi.ConstParameters != null)
                        {
                            var args = string.Join(", ", pi.ConstParameters.Select(kv => $"{kv.Key} = {kv.Value.value}"));
                            sb.AppendLine($"/* {args} */".Indent(4));
                        }
                        sb.AppendLine(pi.Accept(this).Indent(4));
                    }
                }

                return sb.ToString();
            }
            else
            {
                var sb = new StringBuilder();

                var body = function.Body?.Accept(this) ?? ";";

                var pars = string.Join(", ", function.Parameters.Select(p => p.Accept(this)));
                var head = $"fn {function.Name.Accept(this)}";

                head += $"({pars})";

                if (function.ReturnTypeExpr != null)
                    head += $" -> {function.ReturnTypeExpr.Accept(this)}";

                if (function.Directives.Count > 0)
                    head += " " + string.Join(" ", function.Directives.Select(d => VisitDirective(d)));

                sb.Append($"{head} {body}".Indent(indentLevel));
                return sb.ToString();
            }
        }

        public override string VisitReturnStmt(AstReturnStmt ret, int data = 0)
        {
            var sb = new StringBuilder();

            if (ret.Destructions != null)
            {
                foreach (var dest in ret.Destructions)
                {
                    sb.AppendLine($"{dest.Accept(this)};");
                }
            }

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
            if (ret.ReturnValue != null)
                sb.Append(" ").Append(ret.ReturnValue.Accept(this));
            return sb.ToString();
        }

        public override string VisitTypeAliasDecl(AstTypeAliasDecl al, int data = 0)
        {
            return $"typedef {al.Name.Accept(this)} = {al.Type}";
        }

        public override string VisitUsingStmt(AstUsingStmt use, int data = 0)
        {
            return $"use {use.Value.Accept(this)}";
        }

        public string VisitStructMember(AstStructMember m)
        {
            var v = $"{m.Name.Accept(this)}: {m.Type}";
            if (m.IsReadOnly)
                v = "const " + v;
            if (m.IsPublic)
                v = "pub " + v;
            if (m.Initializer != null)
                v += $" = {m.Initializer.Accept(this)}";
            return v;
        }

        public string VisitStructMemberRaw(AstStructMember m)
        {
            var v = $"{m.Name.Accept(this)}: {m.TypeExpr.Accept(this)}";
            if (m.Initializer != null)
                v += $" = {m.Initializer.Accept(this)}";
            return v;
        }

        public override string VisitStructDecl(AstStructDecl str, int data = 0)
        {
            if (str.IsPolymorphic)
            {
                var body = string.Join("\n", str.Members.Select(m => VisitStructMember(m)));
                var head = $"struct {str.Name.Accept(this)}";

                head += "(";
                head += string.Join(", ", str.Parameters.Select(p => $"{p.Name.Accept(this)}: {p.Type}"));
                head += ")";

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
            else
            {
                var body = string.Join("\n", str.Members.Select(m => VisitStructMember(m)));
                var head = $"struct {str.Name.Accept(this)}";

                var sb = new StringBuilder();
                sb.Append($"{head} {{ // size: {str.Type?.Size}, alignment: {str.Type?.Alignment}\n{body.Indent(4)}\n}}");

                return sb.ToString();
            }
        }

        public override string VisitTraitDecl(AstTraitDeclaration trait, int data = 0)
        {
            if (trait.IsPolymorphic)
            {
                var sb = new StringBuilder();
                sb.Append($"trait {trait.Name.Accept(this)}(");
                sb.Append(string.Join(", ", trait.Parameters.Select(p => p.Accept(this))));
                sb.AppendLine(") {");

                foreach (var f in trait.Variables)
                    sb.AppendLine(VisitStructMember(f).Indent(4));

                foreach (var f in trait.Functions)
                    sb.AppendLine(f.Accept(this).Indent(4));

                sb.Append("}");

                // polies
                if (trait.PolymorphicInstances?.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"// Polymorphic instances for {trait.Name}");
                    foreach (var pi in trait.PolymorphicInstances)
                    {
                        var args = string.Join(", ", pi.Parameters.Select(p => $"{p.Name.Accept(this)} = {p.Value}"));
                        sb.AppendLine($"// {args}".Indent(4));
                        sb.AppendLine(pi.Accept(this).Indent(4));
                    }
                }

                return sb.ToString().Indent(data);
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine($"trait {trait.Name.Accept(this)} {{");

                foreach (var f in trait.Variables)
                    sb.AppendLine(VisitStructMember(f).Indent(4));

                foreach (var f in trait.Functions)
                    sb.AppendLine(f.Accept(this).Indent(4));

                sb.Append("}");

                return sb.ToString().Indent(data);
            }
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
            if (impl.IsPolymorphic)
            {
                var body = string.Join("\n\n", impl.Functions.Select(f => f.Accept(this, 0)));
                var header = "impl";

                // parameters
                if (impl.Parameters != null)
                    header += "(" + string.Join(", ", impl.Parameters.Select(p => p.Accept(this, 0))) + ")";

                header += " ";
                if (impl.TraitExpr != null)
                    header += impl.Trait + " for ";
                header += impl.TargetType;

                if (impl.Conditions != null)
                    header += " if " + string.Join(", ", impl.Conditions.Select(c => VisitImplCondition(c)));

                var sb = new StringBuilder();
                sb.Append($"{header} {{\n{body.Indent(4)}\n}}");

                // polies
                if (impl.PolyInstances?.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"// Polymorphic instances for {header}");
                    foreach (var pi in impl.PolyInstances)
                    {
                        var args = string.Join(", ", pi.Parameters.Select(p => $"{p.Name.Accept(this)} = {p.Value}"));
                        sb.AppendLine($"// {args}".Indent(4));
                        sb.AppendLine(pi.Accept(this).Indent(4));
                    }
                }

                return sb.ToString();
            }
            else
            {
                var body = string.Join("\n\n", impl.Functions.Select(f => f.Accept(this, 0)));
                var header = "impl";

                // parameters
                if (impl.Parameters != null)
                    header += "(" + string.Join(", ", impl.Parameters.Select(p => p.Accept(this, 0))) + ")";

                header += " ";
                if (impl.TraitExpr != null)
                    header += impl.Trait + " for ";

                header += impl.TargetType;

                if (impl.Conditions != null)
                    header += " if " + string.Join(", ", impl.Conditions.Select(c => VisitImplCondition(c)));

                return $"{header} {{\n{body.Indent(4)}\n}}";
            }
        }

        public override string VisitVariableDecl(AstVariableDecl variable, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("let ");
            if (variable.Constant)
                sb.Append("const ");
            sb.Append(variable.Pattern.Accept(this));
            sb.Append($": {variable.Type}");
            if (variable.Initializer != null)
                sb.Append($" = {variable.Initializer.Accept(this)}");

            return sb.ToString();
        }

        public override string VisitIfExpr(AstIfExpr ifs, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.Append("if ");

            if (ifs.PreAction != null)
                sb.Append(ifs.PreAction.Accept(this)).Append(", ");

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
            var sb = new StringBuilder();
            sb.Append("while ");
            if (wh.PreAction != null) sb.Append(wh.PreAction.Accept(this) + ", ");
            sb.Append(wh.Condition.Accept(this));
            if (wh.PostAction != null) sb.Append(", " + wh.PostAction.Accept(this));
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

            if (block.Destructions != null)
            {
                foreach (var dest in block.Destructions)
                {
                    sb.Append($"\n{dest.Accept(this)}");
                }
            }

            var result = $"{{\n{sb.ToString().Indent(4)}\n}}".Indent(indentLevel);

            if (block.GetFlag(ExprFlags.Anonymous))
                result = "#anonymous " + result;
            if (block.GetFlag(ExprFlags.Link))
                result = "#link " + result;
            if (block.GetFlag(ExprFlags.FromMacroExpansion))
                result = "#macro " + result;
            return result;
        }

        public override string VisitAssignmentStmt(AstAssignment ass, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            if (ass.Destructions != null)
            {
                foreach (var dest in ass.Destructions)
                {
                    sb.AppendLine($"{dest.Accept(this)};");
                }
            }

            if (ass.SubAssignments != null)
            {
                sb.Append("// ");
                sb.Append(ass.Pattern.Accept(this) + $" {ass.Operator}= " + ass.Value.Accept(this));
                sb.AppendLine().Append(string.Join("\n", ass.SubAssignments.Select(sa => sa.Accept(this, indentLevel))).Indent(4));
            }
            else
            {
                if (ass.OnlyGenerateValue)
                    return ass.Value.Accept(this);
                sb.Append(ass.Pattern.Accept(this) + $" = " + ass.Value.Accept(this));
            }

            return sb.ToString();
        }

        public override string VisitExpressionStmt(AstExprStmt stmt, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            sb.Append(stmt.Expr.Accept(this));

            if (stmt.Destructions != null)
            {
                foreach (var dest in stmt.Destructions)
                {
                    sb.Append($";\n{dest.Accept(this)}");
                }
            }
            return sb.ToString();
        }

        public string VisitEnumMember(AstEnumMember m)
        {
            var str = m.Name.Accept(this);
            if (m.AssociatedTypeExpr != null)
                str += " : " + m.AssociatedTypeExpr.Value;
            if (m.Value != null)
                str += " = " + m.Value.Accept(this);
            return str;
        }

        public string VisitEnumMemberRaw(AstEnumMember m)
        {
            var str = m.Name.Accept(this);
            if (m.AssociatedTypeExpr != null)
                str += " : " + m.AssociatedTypeExpr.Accept(this);
            if (m.Value != null)
                str += " = " + m.Value.Accept(this);
            return str;
        }

        public override string VisitEnumDecl(AstEnumDecl en, int data = 0)
        {
            if (en.IsPolymorphic)
            {
                var body = string.Join("\n", en.Members.Select(m => VisitEnumMember(m)));
                var head = $"enum {en.Name.Accept(this)}";

                head += "(";
                head += string.Join(", ", en.Parameters.Select(p => p.Accept(this)));
                head += ")";

                var sb = new StringBuilder();
                sb.Append($"{head} {{\n{body.Indent(4)}\n}}");

                // polies
                if (en.PolymorphicInstances?.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"// Polymorphic instances for {head}");
                    foreach (var pi in en.PolymorphicInstances)
                    {
                        var args = string.Join(", ", pi.Parameters.Select(p => $"{p.Name.Accept(this)} = {p.Value}"));
                        sb.AppendLine($"// {args}".Indent(4));
                        sb.AppendLine(pi.Accept(this).Indent(4));
                    }
                }

                return sb.ToString();
            }
            else
            {
                var body = string.Join("\n", en.Members.Select(m => VisitEnumMember(m)));
                var head = $"enum {en.Name.Accept(this)}";
                return $"{head} {{ // size: {en.Type?.Size}, alignment: {en.Type?.Alignment}\n{body.Indent(4)}\n}}";
            }
        }

        public override string VisitBreakExpr(AstBreakExpr br, int data = 0)
        {
            var sb = new StringBuilder();

            if (br.Destructions != null)
            {
                foreach (var dest in br.Destructions)
                {
                    sb.AppendLine($"{dest.Accept(this)};");
                }
            }

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
            if (br.Label != null)
                sb.Append($" {br.Label.Accept(this)}");
            return sb.ToString();
        }

        public override string VisitContinueExpr(AstContinueExpr cont, int data = 0)
        {
            var sb = new StringBuilder();

            if (cont.Destructions != null)
            {
                foreach (var dest in cont.Destructions)
                {
                    sb.AppendLine($"{dest.Accept(this)};");
                }
            }

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
            if (cont.Label != null)
                sb.Append($" {cont.Label.Accept(this)}");
            return sb.ToString();
        }

        #endregion


        #region Expressions

        public override string VisitRangeExpr(AstRangeExpr expr, int data = 0)
        {
            return $"{expr.From.Accept(this)}..{expr.To.Accept(this)}";
        }

        public override string VisitLambdaExpr(AstLambdaExpr expr, int data = 0)
        {
            var sb = new StringBuilder();
            sb.Append("|");
            sb.Append(string.Join(", ", expr.Parameters.Select(p => p.Accept(this))));
            sb.Append("| ");

            if (expr.FunctionType?.ReturnType != null && expr.FunctionType.ReturnType != CheezType.Void)
                sb.Append($"-> {expr.FunctionType.ReturnType} ");

            sb.Append(expr.Body);

            return sb.ToString();
        }

        public override string VisitEnumValueExpr(AstEnumValueExpr expr, int data = 0)
        {
            if (expr.Argument != null)
                return $"{expr.Type}.{expr.Member.Name.Name}({expr.Argument.Accept(this)})";
            return $"{expr.Type}.{expr.Member.Name.Name}";
        }

        private string VisitMatchCase(AstMatchCase c)
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
            foreach (var c in expr.Cases)
            {
                sb.AppendLine(VisitMatchCase(c).Indent(4));
            }
            sb.Append("}");

            return sb.ToString();
        }

        public override string VisitDefaultExpr(AstDefaultExpr expr, int data = 0)
        {
            return "default";
        }

        public override string VisitUfcFuncExpr(AstUfcFuncExpr expr, int data = 0)
        {
            return $"{expr.SelfArg.Type}::{expr.FunctionDecl.Name.Accept(this)}";
        }

        public override string VisitTempVarExpr(AstTempVarExpr te, int data = 0)
        {
            return $"@tempvar_{te.Id}({te.Expr.Accept(this)})";
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
            return $"{expr.Expr.Accept(this)}";
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
            switch (expr.CharValue)
            {
                case '\0': return "'`0'";
                case '\r': return "'`r'";
                case '\n': return "'`n'";
                case '\t': return "'`t'";
                case '\'': return "'`''";
                case '`': return "'``'";
                default: return $"'{expr.CharValue.ToString()}'";
            }
        }

        public override string VisitStringLiteralExpr(AstStringLiteral str, int data = 0)
        {
            string v = str.StringValue;
            v = v.Replace("`", "``").Replace("\r", "`r").Replace("\n", "`n").Replace("\0", "`0");
            v = $"\"{v.Replace("\"", "`\"")}\"";
            if (str.Suffix != null) v += str.Suffix;
            return v;
        }

        public override string VisitSymbolExpr(AstSymbolExpr te, int data = 0)
        {
            return te.Symbol.Name?.Accept(this) ?? te.Symbol.ToString();
        }

        public override string VisitIdExpr(AstIdExpr ident, int indentLevel = 0)
        {
            if (ident.Symbol is ConstSymbol c)
            {
                if (c.Value is string v)
                {
                    v = v.Replace("`", "``").Replace("\r", "`r").Replace("\n", "`n").Replace("\0", "`0");
                    return $"\"{v.Replace("\"", "`\"")}\"";
                }
                else if (c.Value is NumberData nd)
                {
                    return VisitNumberData(nd);
                }
                return c.Value.ToString();
            }

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

        private string VisitNumberData(NumberData data)
        {
            var sb = new StringBuilder();
            if (data.IntBase == 2)
                sb.Append("0b");
            else if (data.IntBase == 16)
                sb.Append("0x");
            else if (data.IntBase != 10)
                sb.Append($"0base({data.IntBase})");
            sb.Append(data.StringValue);
            return sb.ToString();
        }

        public override string VisitNumberExpr(AstNumberExpr num, int indentLevel = 0)
        {
            return VisitNumberData(num.Data) + (num.Suffix ?? "");
        }

        public override string VisitAddressOfExpr(AstAddressOfExpr add, int data = 0)
        {
            if (add.Reference)
                return $"@ref({add.SubExpression.Accept(this)})";
            return "&" + add.SubExpression.Accept(this);
        }

        public override string VisitDerefExpr(AstDereferenceExpr deref, int data = 0)
        {
            if (deref.Reference)
                return $"@deref({deref.SubExpression.Accept(this)})";
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


        #region Type expressions

        public override string VisitTypeExpr(AstTypeRef astArrayTypeExpr, int data = 0)
        {
            return astArrayTypeExpr.Type?.ToString();
        }

        public override string VisitSliceTypeExpr(AstSliceTypeExpr type, int data = 0)
        {
            return $"[]{type.Target.Accept(this)}";
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
            return $"ref {type.Target.Accept(this)}";
        }
        #endregion
    }
}
