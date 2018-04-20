using Cheez.Ast;
using Cheez.Visitor;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cheez.CodeGeneration
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
        private string mImplTarget = null;

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
                sb.AppendLine(GenerateCode(td, workspace.GlobalScope));
            }
            sb.AppendLine();

            sb.AppendLine("// forward declarations");
            foreach (var func in workspace.GlobalScope.FunctionDeclarations)
            {
                sb.AppendLine(GenerateCode(func, workspace.GlobalScope));
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

            sb.Append(CreateMainFunction("Main")); // @Todo: entry point name
            sb.AppendLine();

            return sb.ToString();
        }

        private string GetDecoratedName(FunctionDeclarationAst func)
        {
            return func.NameExpr.Name;
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

        private void AddImplTargetParam(string target, StringBuilder sb)
        {
            sb.Append(target).Append(" self");
        }

        private string GenerateCode(Statement s, Scope scope, int indent = 0)
        {
            if (s == null)
                return null;
            var ss = workspace.GetScope(s) ?? scope;
            return s.Accept(this, new CppCodeGeneratorArgs
            {
                indent = indent,
                scope = ss
            });
        }

        private string GenerateCode(Expression s, Scope scope, int indent = 0)
        {
            if (s == null)
                return null;
            var ss = workspace.GetScope(s) ?? scope;
            return s.Accept(this, new CppCodeGeneratorArgs
            {
                indent = indent,
                scope = ss
            });
        }

        public override string VisitFunctionDeclaration(FunctionDeclarationAst function, CppCodeGeneratorArgs data)
        {
            var prevScope = nameDecorator.GetCurrentScope();
            var decoratedName = nameDecorator.GetDecoratedName(function);
            nameDecorator.SetCurrentScope(function);

            var sb = new StringBuilder();
            
            string returnType = function.ReturnType?.Accept(this) ?? "void";

            string funcName = GetDecoratedName(function);
            
            sb.Append($"{returnType} {decoratedName}(");

            if (mImplTarget != null)
            {
                AddImplTargetParam(mImplTarget, sb);
            }

            bool first = true;
            foreach (var p in function.Parameters)
            {
                if (!first)
                    sb.Append(", ");
                sb.Append($"{p.Type} {nameDecorator.GetDecoratedName(p)}");

                first = false;
            }
            
            sb.Append(")");

            if (mEmitFunctionBody)
            {
                sb.AppendLine(" {");
                foreach (var s in function.Statements)
                {
                    sb.AppendLine(Indent(GenerateCode(s, null), 4));
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

        public override string VisitPrintStatement(PrintStatement print, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();
            sb.Append("std::cout");

            bool isFirst = true;
            var sepSb = new StringBuilder();
            
            sepSb.Append(GenerateCode(print.Seperator, data.scope) ?? "");
            var sep = sepSb.ToString();
            foreach (var e in print.Expressions)
            {
                if (!isFirst && print.Seperator != null)
                    sb.Append(" << ").Append(sep);
                sb.Append(" << ");
                sb.Append(GenerateCode(e, data.scope));

                isFirst = false;
            }

            if (print.NewLine)
            {
                sb.Append(" << '\\n'");
            }

            sb.Append(";");

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitExpressionStatement(ExpressionStatement stmt, CppCodeGeneratorArgs data)
        {
            return $"{GenerateCode(stmt.Expr, data.scope)};";
        }

        public override string VisitIdentifierExpression(IdentifierExpression ident, CppCodeGeneratorArgs data)
        {
            var v = data.scope.GetVariable(ident.Name);
            return nameDecorator.GetDecoratedName(v.Value.ast);
        }

        public override string VisitVariableDeclaration(VariableDeclarationAst variable, CppCodeGeneratorArgs data)
        {
            var decoratedName = nameDecorator.GetDecoratedName(variable);
            var sb = new StringBuilder();
            string type = GetTypeName(workspace.GetCheezType(variable));
            sb.Append($"{type} {decoratedName}");

            //if (variable.Type is ArrayTypeExpression)
            //    sb.Append("[]");

            if (variable.Initializer != null)
            {
                var initializerScope = workspace.GetScope(variable.Initializer);
                sb.Append($" = ");
                sb.Append(GenerateCode(variable.Initializer, initializerScope));
            }
            sb.Append(";");

            return Indent(sb.ToString(), data.indent);
        }

        public override string VisitTypeDeclaration(TypeDeclaration type, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();

            sb.Append("struct ").Append(type.Name).AppendLine(" {");
            foreach (var m in type.Members)
            {
                sb.Append(Indent(m.Type.Accept(this), 4)).Append(" ").Append(m.Name).AppendLine(";");
            }
            sb.Append("};");

            return Indent(sb.ToString(), data.indent);
        }

        #region literals
        public override string VisitStringLiteral(StringLiteral str, CppCodeGeneratorArgs data)
        {
            return $"\"{str.Value.Replace("\r", "").Replace("\n", "\\n").Replace("\"", "\\\"")}\"";
        }
        
        public override string VisitAssignment(Assignment ass, CppCodeGeneratorArgs data)
        {
            return Indent(ass.Target.Accept(this) + " = " + ass.Value.Accept(this) + ";", data.indent);
        }
        
        public override string VisitNumberExpression(NumberExpression num, CppCodeGeneratorArgs data)
        {
            switch (num.Data.Type)
            {
                case Parsing.NumberData.NumberType.Int:
                    return num.Data.StringValue;
            }

            return null;
        }

        public override string VisitIfStatement(IfStatement ifs, CppCodeGeneratorArgs data)
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

        public override string VisitBlockStatement(BlockStatement block, CppCodeGeneratorArgs data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var s in block.Statements)
            {
                sb.AppendLine(Indent(GenerateCode(s, null), 4));
            }
            sb.Append("}");
            return Indent(sb.ToString(), data.indent);
        }
        #endregion
        
        public override string VisitImplBlock(ImplBlock impl, CppCodeGeneratorArgs data)
        {
            Debug.Assert(mImplTarget == null);
            mImplTarget = impl.Target;
            try
            {
                var sb = new StringBuilder();
                mFunctionForwardDeclarations.Append("namespace ").Append(impl.Target).AppendLine("_impl {");
                sb.Append("namespace ").Append(impl.Target).AppendLine("_impl {");
                foreach (var f in impl.Functions)
                {
                    sb.AppendLine(GenerateCode(f, data.scope, 4));
                }
                sb.Append("}");
                mFunctionForwardDeclarations.AppendLine("}");

                return Indent(sb.ToString(), data.indent);
            }
            finally
            {
                mImplTarget = null;
            }
        }

        public override string VisitReturnStatement(ReturnStatement ret, CppCodeGeneratorArgs data)
        {
            if (ret.ReturnValue != null)
                return $"return {GenerateCode(ret.ReturnValue, data.scope)};";
            return "return;";
        }

        public override string VisitBinaryExpression(BinaryExpression bin, CppCodeGeneratorArgs data)
        {
            var lhs = GenerateCode(bin.Left, data.scope);
            var rhs = GenerateCode(bin.Right, data.scope);

            {
                if (bin.Left is BinaryExpression b && b.Operator.GetPrecedence() < bin.Operator.GetPrecedence())
                    lhs = $"({lhs})";
            }

            {
                if (bin.Right is BinaryExpression b && b.Operator.GetPrecedence() < bin.Operator.GetPrecedence())
                    rhs = $"({rhs})";
            }

            switch (bin.Operator)
            {
                case BinaryOperator.Add:
                    return $"{lhs} + {rhs}";
                case BinaryOperator.Subtract:
                    return $"{lhs} - {rhs}";
                case BinaryOperator.Multiply:
                    return $"{lhs} * {rhs}";
                case BinaryOperator.Divide:
                    return $"{lhs} / {rhs}";
            }

            return "[ERROR]";
        }

        public override string VisitDotExpression(DotExpression dot, CppCodeGeneratorArgs data)
        {
            return dot.Left.Accept(this) + "." + dot.Right;
        }

        public override string VisitCallExpression(CallExpression call, CppCodeGeneratorArgs data)
        {
            var args = string.Join(", ", call.Arguments.Select(a => a.Accept(this)));

            if (call.Function is IdentifierExpression id)
            {
                return $"{call.Function}({args})";
            }

            return $"{GenerateCode(call.Function, data.scope)}({args})";
        }

        private string GetTypeName(CheezType type)
        {
            switch (type)
            {
                case IntType n:
                    return n.ToString();

                case PointerType p:
                     return GetTypeName(p.TargetType) + "*";

                case ArrayType a:
                    return GetTypeName(a.TargetType)+ "*";

                case StringType s:
                    return "string";

                default:
                    return "void";
            }
        }

        public override string VisitTypeExpression(TypeExpression type, CppCodeGeneratorArgs data)
        {
            switch (type)
            {
                case NamedTypeExression n:
                    return n.Name;

                case PointerTypeExpression p:
                    return p.TargetType.Accept(this) + "*";

                case ArrayTypeExpression a:
                    return a.ElementType.Accept(this) + "*";

                default:
                    return "void";
            }
        }

        public static string Indent(string s, int level)
        {
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
    }
}
