using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cheez.Compiler.CodeGeneration
{
    public struct CppCodeGeneratorArgs
    {
        public int indent;
        public Scope scope;
    }

    public class CppCodeGenerator : VisitorBase<string, CppCodeGeneratorArgs>
    {
        private StringBuilder mFunctionForwardDeclarations = new StringBuilder();
        private StringBuilder mTypeDeclarations = new StringBuilder();
        private CheezType mImplTarget = null;

        private bool mEmitFunctionBody = false;
        private Workspace workspace;

        private UniqueNameDecorator nameDecorator = new UniqueNameDecorator();

        public string GenerateCode(Workspace ws)
        {
            workspace = ws;
            nameDecorator.SetCurrentScope(ws.GlobalScope);

            var sb = new StringBuilder();
            sb.AppendLine("#include <string>");
            sb.AppendLine("#include <iostream>");
            sb.AppendLine("#include <cstdint>");
            sb.AppendLine(@"
// type defininions
using u8 = uint8_t;
using u16 = uint16_t;
using u32 = uint32_t;
using u64 = uint64_t;

using i8 =  int8_t;
using i16 = int16_t;
using i32 = int32_t;
using i64 = int64_t;

using f32 = float_t;
using f64 = double_t;

using string = const char*;
");
            sb.AppendLine();

            sb.AppendLine("// type declarations");
            foreach (var td in workspace.GlobalScope.TypeDeclarations)
            {
                if (td.HasDirective("DisableCodeGen"))
                    continue;
                sb.AppendLine(GenerateCode(td, workspace.GlobalScope));
            }
            sb.AppendLine();

            sb.AppendLine("// forward declarations");
            foreach (var func in workspace.GlobalScope.FunctionDeclarations)
            {
                var code = GenerateCode(func, workspace.GlobalScope);
                if (string.IsNullOrEmpty(code))
                    continue;
                sb.AppendLine(code);
            }
            sb.AppendLine();
            
            foreach (var impl in workspace.GlobalScope.ImplBlocks)
            {
                var code = GenerateCode(impl, workspace.GlobalScope);
                if (string.IsNullOrEmpty(code))
                    continue;
                sb.AppendLine(code);
            }
            sb.AppendLine();

            sb.AppendLine("// global variables");
            foreach (var varia in workspace.GlobalScope.VariableDeclarations)
            {
                sb.AppendLine(GenerateCode(varia, workspace.GlobalScope));
            }
            sb.AppendLine();

            sb.AppendLine("// function implementations");
            mEmitFunctionBody = true;
            foreach (var func in workspace.GlobalScope.FunctionDeclarations)
            {
                if (func.HasImplementation)
                    sb.AppendLine(GenerateCode(func, workspace.GlobalScope));
            }
            sb.AppendLine();

            foreach (var impl in workspace.GlobalScope.ImplBlocks)
            {
                sb.AppendLine(GenerateCode(impl, workspace.GlobalScope));
            }
            sb.AppendLine();

            sb.Append(CreateMainFunction("Main")); // @Todo: entry point name
            sb.AppendLine();

            return sb.ToString();
        }

        private string GetDecoratedName(AstFunctionDecl func)
        {
            return func.Name;
        }

        public string CreateMainFunction(string entryPoint)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// entry point to the program");
            sb.AppendLine("int main()");
            sb.AppendLine("{");
            sb.AppendLine("    // call user main function");
            sb.AppendLine($"    {entryPoint}();");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void AddImplTargetParam(CheezType target, StringBuilder sb)
        {
            sb.Append(target).Append(" self");
        }

        private string GenerateCode(AstStatement s, Scope scope, int indent = 0)
        {
            if (s == null)
                return null;
            var ss = s.Scope ?? scope;
            return s.Accept(this, new CppCodeGeneratorArgs
            {
                indent = indent,
                scope = ss
            });
        }

        private string GenerateCode(AstExpression s, Scope scope, int indent = 0)
        {
            if (s == null)
                return null;
            var ss = s.Scope ?? scope;
            return s.Accept(this, new CppCodeGeneratorArgs
            {
                indent = indent,
                scope = ss
            });
        }

        public override string VisitFunctionDeclaration(AstFunctionDecl function, CppCodeGeneratorArgs data)
        {
            if (!function.HasImplementation)
                return "";

            var prevScope = nameDecorator.GetCurrentScope();
            var decoratedName = nameDecorator.GetDecoratedName(function);
            nameDecorator.SetCurrentScope(function);

            var sb = new StringBuilder();

            string returnType = GetCTypeName(function.ReturnType) ?? "void";

            string funcName = GetDecoratedName(function);

            sb.Append($"{returnType} {decoratedName}(");

            bool first = true;
            foreach (var p in function.Parameters)
            {
                if (!first)
                    sb.Append(", ");

                if (p.Type is FunctionType f)
                {
                    sb.Append(GetCTypeName(f, nameDecorator.GetDecoratedName(p)));
                }
                else
                {
                    sb.Append($"{p.Type} {nameDecorator.GetDecoratedName(p)}");
                }


                first = false;
            }

            sb.Append(")");

            if (mEmitFunctionBody)
            {
                sb.AppendLine(" {");
                foreach (var s in function.Statements)
                {
                    var c = GenerateCode(s, null);
                    if (c != null)
                    {
                        sb.Append(Indent(c, 4));
                        if (s is AstWhileStmt || s is AstIfStmt)
                        {
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(";");
                        }
                    }

                    if (s is AstReturnStmt)
                        break;
                }
                sb.Append("}");
            }
            else
            {
                sb.Append(";");
            }

            nameDecorator.SetCurrentScope(prevScope);
            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitPrintStatement(AstPrintStmt print, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();
            sb.Append("std::cout");

            bool isFirst = true;
            var sepSb = new StringBuilder();

            sepSb.Append(GenerateCode(print.Separator, null) ?? "");
            var sep = sepSb.ToString();
            foreach (var e in print.Expressions)
            {
                if (!isFirst && print.Separator != null)
                    sb.Append(" << ").Append(sep);
                sb.Append(" << ");
                sb.Append(GenerateCode(e, null));

                isFirst = false;
            }

            if (print.NewLine)
            {
                sb.Append(" << '\\n'");
            }

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitExpressionStatement(AstExprStmt stmt, CppCodeGeneratorArgs data)
        {
            return $"{GenerateCode(stmt.Expr, null)}";
        }

        public override string VisitIdentifierExpression(AstIdentifierExpr ident, CppCodeGeneratorArgs data)
        {
            var v = ident.Scope.GetSymbol(ident.Name);
            var name = nameDecorator.GetDecoratedName(v);
            if (ident.Type is IntType i && i.SizeInBytes == 1)
                return $"+{name}";
            return name;
        }

        public override string VisitAddressOfExpression(AstAddressOfExpr add, CppCodeGeneratorArgs data = default)
        {
            var sub = add.SubExpression.Accept(this, data);
            if (!(add.SubExpression is AstIdentifierExpr))
                sub = $"({sub})";
            return $"&{sub}";
        }

        public override string VisitDereferenceExpression(AstDereferenceExpr deref, CppCodeGeneratorArgs data = default)
        {
            var sub = deref.SubExpression.Accept(this, data);
            if (!(deref.SubExpression is AstIdentifierExpr))
                sub = $"({sub})";
            return $"*{sub}";
        }

        public override string VisitVariableDeclaration(AstVariableDecl variable, CppCodeGeneratorArgs data)
        {
            var decoratedName = nameDecorator.GetDecoratedName(variable);
            var sb = new StringBuilder();

            if (variable.Type is FunctionType f)
            {
                sb.Append(GetCTypeName(f, decoratedName));
            }
            else
            {
                string type = GetCTypeName(variable.Type, decoratedName);
                sb.Append($"{type} {decoratedName}");
            }


            //if (variable.Type is ArrayTypeExpression)
            //    sb.Append("[]");

            if (variable.Initializer != null)
            {
                sb.Append($" = ");
                sb.Append(GenerateCode(variable.Initializer, variable.Initializer.Scope));
            }

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitTypeDeclaration(AstTypeDecl type, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();

            sb.Append("struct ").Append(type.Name).AppendLine(" {");
            foreach (var m in type.Members)
            {
                if (m.Type is FunctionType f)
                {
                    sb.AppendLine(Indent(GetCTypeName(f, m.Name) + ";", 4));
                }
                else
                {
                    string t = GetCTypeName(m.Type, m.Name);
                    sb.AppendLine(Indent($"{t} {m.Name};", 4));
                }
            }
            sb.Append("};");

            return Indent(sb.ToString(), data.indent);
        }



        public override string VisitImplBlock(AstImplBlock impl, CppCodeGeneratorArgs data)
        {
            Debug.Assert(mImplTarget == null);
            mImplTarget = impl.TargetType;
            var targetName = impl.TargetType.ToString();
            if (impl.TargetType is StructType t)
            {
                targetName = nameDecorator.GetDecoratedName(t.Declaration);
            }
            try
            {
                var sb = new StringBuilder();
                sb.Append("namespace ").Append(targetName).AppendLine("_impl {");
                foreach (var f in impl.Functions)
                {
                    sb.AppendLine(GenerateCode(f, null, 4));
                }
                sb.Append("}");

                return Indent(sb.ToString(), data.indent);
            }
            finally
            {
                mImplTarget = null;
            }
        }
        public override string VisitReturnStatement(AstReturnStmt ret, CppCodeGeneratorArgs data)
        {
            if (ret.ReturnValue != null)
                return $"return {GenerateCode(ret.ReturnValue, null)}";
            return "return";
        }

        public override string VisitUsingStatement(AstUsingStmt use, CppCodeGeneratorArgs data = default)
        {
            return null;
        }

        #region literals
        public override string VisitStringLiteral(AstStringLiteral str, CppCodeGeneratorArgs data)
        {
            var s = str.Value.Replace(
                (@"\", @"\\"),
                (@"""", @"\"""),
                ("\0", @"\0"),
                ("\r", @"\r"),
                ("\n", @"\n")
                );
            return $"\"{s}\"";
        }

        public override string VisitAssignment(AstAssignment ass, CppCodeGeneratorArgs data)
        {
            return Indent(ass.Target.Accept(this) + " = " + ass.Value.Accept(this), data.indent);
        }

        public override string VisitNumberExpression(AstNumberExpr num, CppCodeGeneratorArgs data)
        {
            switch (num.Data.Type)
            {
                case Parsing.NumberData.NumberType.Int:
                    return num.Data.StringValue;
            }

            return null;
        }

        public override string VisitWhileStatement(AstWhileStmt ws, CppCodeGeneratorArgs data = default)
        {
            var sb = new StringBuilder();
            sb.Append("for (");

            if (ws.PreAction != null)
            {
                sb.Append(ws.PreAction.Accept(this));
            }

            sb.Append("; ").Append(ws.Condition.Accept(this)).Append("; ");

            if (ws.PostAction != null)
            {
                sb.Append(ws.PostAction.Accept(this));
            }

            sb.Append(") ");
            sb.Append(ws.Body.Accept(this));

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitIfStatement(AstIfStmt ifs, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();
            sb.Append("if (");
            sb.Append(ifs.Condition.Accept(this));
            sb.Append(") ");
            sb.Append(ifs.IfCase.Accept(this));
            if (ifs.ElseCase != null)
            {
                sb.Append(" else ");
                sb.Append(ifs.ElseCase.Accept(this));
            }

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitBlockStatement(AstBlockStmt block, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var s in block.Statements)
            {
                var c = GenerateCode(s, null);
                if (c != null)
                {
                    sb.Append(Indent(c, 4));
                    if (s is AstWhileStmt || s is AstIfStmt)
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine(";");
                    }
                }

                if (s is AstReturnStmt)
                    break;
            }
            sb.Append("}");
            return Indent(sb.ToString(), data.indent);
        }


        #endregion


        public override string VisitBinaryExpression(AstBinaryExpr bin, CppCodeGeneratorArgs data)
        {
            var lhs = GenerateCode(bin.Left, null);
            var rhs = GenerateCode(bin.Right, null);

            {
                //if (bin.Left is AstBinaryExpr b && b.Operator.GetPrecedence() < bin.Operator.GetPrecedence())
                lhs = $"({lhs})";
            }

            {
                //if (bin.Right is AstBinaryExpr b && b.Operator.GetPrecedence() < bin.Operator.GetPrecedence())
                rhs = $"({rhs})";
            }

            switch (bin.Operator)
            {
                case "+":   return $"{lhs} + {rhs}";
                case "-":   return $"{lhs} - {rhs}";
                case "*":   return $"{lhs} * {rhs}";
                case "/":   return $"{lhs} / {rhs}";
                case "%":   return $"{lhs} % {rhs}";

                case "<":   return $"{lhs} < {rhs}";
                case "<=":  return $"{lhs} <= {rhs}";
                case ">":   return $"{lhs} > {rhs}";
                case ">=":  return $"{lhs} >= {rhs}";
                case "==":  return $"{lhs} == {rhs}";
                case "!=":  return $"{lhs} != {rhs}";

                case "&&":  return $"{lhs} && {rhs}";
                case "||":  return $"{lhs} || {rhs}";
            }

            return "[UNKNOWN OPERATOR]";
        }

        public override string VisitBoolExpression(AstBoolExpr bo, CppCodeGeneratorArgs data = default)
        {
            return bo.Value ? "true" : "false";
        }

        public override string VisitDotExpression(AstDotExpr dot, CppCodeGeneratorArgs data)
        {
            if (dot.IsDoubleColon)
            {
                var targetType = dot.Left.Type;
                var targetName = targetType.ToString();
                if (targetType is StructType t)
                {
                    targetName = nameDecorator.GetDecoratedName(t.Declaration);
                }

                return $"{targetName}_impl::{dot.Right}";
            }
            else
            {
                var sub = dot.Left.Accept(this);
                if (!(dot.Left is AstDotExpr || dot.Left is AstIdentifierExpr))
                    sub = $"({sub})";
                return sub + "." + dot.Right;
            }
        }

        public override string VisitCastExpression(AstCastExpr cast, CppCodeGeneratorArgs data = default)
        {
            return $"(({GetCTypeName(cast.Type)})({cast.SubExpression.Accept(this, data)}))";
        }

        public override string VisitCallExpression(AstCallExpr call, CppCodeGeneratorArgs data)
        {
            var args = string.Join(", ", call.Arguments.Select(a => a.Accept(this)));

            //if (call.Function is AstDotExpr d && d.IsDoubleColon)
            //{
                
            //    return "";
            //}
            //else 
            if (call.Function is AstIdentifierExpr id)
            {
                return $"{call.Function}({args})";
            }

            return $"{GenerateCode(call.Function, null)}({args})";
        }

        public override string VisitArrayAccessExpression(AstArrayAccessExpr arr, CppCodeGeneratorArgs data = default)
        {
            string sub = arr.SubExpression.Accept(this, data);
            string index = arr.Indexer.Accept(this, data);

            if (arr.SubExpression is AstBinaryExpr || arr.SubExpression is AstAddressOfExpr)
                sub = $"({sub})";
            return $"{sub}[{index}]";
        }

        #region Helper Methods

        private string GetCTypeName(CheezType type, string name = null)
        {
            switch (type)
            {
                case IntType n:
                    return n.ToString();

                case FloatType f:
                    return f.ToString();

                case BoolType b:
                    return "bool";

                case PointerType p:
                    return GetCTypeName(p.TargetType) + "*";

                case ArrayType a:
                    return GetCTypeName(a.TargetType) + "*";

                case StringType s:
                    return "string";

                case StructType s:
                    return s.Declaration.Name;

                case FunctionType f:
                    return $"{GetCTypeName(f.ReturnType)}(*{name})({string.Join(", ", f.ParameterTypes.Select(pt => GetCTypeName(pt)))})";

                default:
                    return "void";
            }
        }

        public static string Indent(string s, int level)
        {
            if (s == null)
                return "";
            if (level == 0)
                return s;
            return string.Join("\n", s.Split('\n').Select(line => $"{new string(' ', level)}{line}"));
        }

        public static string Indent(int level)
        {
            if (level == 0)
                return "";
            return new string(' ', level);
        }

        #endregion
    }
}
