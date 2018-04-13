using Cheez.Ast;
using Cheez.Visitor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cheez.CodeGeneration
{
    public class CppCodeGenerator : VisitorBase<string, int>
    {
        private StringBuilder mFunctionForwardDeclarations = new StringBuilder();
        private StringBuilder mTypeDeclarations = new StringBuilder();
        private string mImplTarget = null;

        private bool mEmitFunctionBody = false;

        public string GenerateCode(Workspace workspace)
        {
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
                sb.AppendLine(td.Accept(this));
            }
            sb.AppendLine();

            sb.AppendLine("// forward declarations");
            foreach (var func in workspace.GlobalScope.FunctionDeclarations)
            {
                sb.AppendLine(func.Accept(this));
            }
            sb.AppendLine();
            
            sb.AppendLine("// global variables");
            foreach (var func in workspace.GlobalScope.VariableDeclarations)
            {
                sb.AppendLine(func.Accept(this));
            }
            sb.AppendLine();

            sb.AppendLine("// function implementations");
            mEmitFunctionBody = true;
            foreach (var func in workspace.GlobalScope.FunctionDeclarations)
            {
                sb.AppendLine(func.Accept(this));
            }
            sb.AppendLine();

            //sb.Append(CreateMainFunction("main")); // @Todo: entry point name
            //sb.AppendLine();

            return sb.ToString();
        }

        private string GetDecoratedName(FunctionDeclaration func)
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

        public override string VisitFunctionDeclaration(FunctionDeclaration function, int indent = 0)
        {
            var sb = new StringBuilder();

            string returnType = function.ReturnType?.Text ?? "void";

            string funcName = GetDecoratedName(function);
            
            sb.Append($"{returnType} {funcName}(");

            if (mImplTarget != null)
            {
                AddImplTargetParam(mImplTarget, sb);
            }

            bool first = true;
            foreach (var p in function.Parameters)
            {
                if (!first)
                    sb.Append(", ");
                sb.Append($"{p.Type} {p.Name}");

                first = false;
            }
            
            sb.Append(")");

            if (mEmitFunctionBody)
            {
                sb.AppendLine(" {");
                foreach (var s in function.Statements)
                {
                    sb.AppendLine(Indent(s.Accept(this), 4));
                }
                sb.Append("}");
            }
            else
            {
                sb.Append(";");
            }

            return Indent(sb.ToString(), indent);
        }

        private void AddImplTargetParam(string target, StringBuilder sb)
        {
            sb.Append(target).Append(" self");
        }

        public override string VisitPrintStatement(PrintStatement print, int indent = 0)
        {
            var sb = new StringBuilder();
            sb.Append("std::cout");

            bool isFirst = true;
            var sepSb = new StringBuilder();
            sepSb.Append(print.Seperator?.Accept(this) ?? "");
            var sep = sepSb.ToString();
            foreach (var e in print.Expressions)
            {
                if (!isFirst && print.Seperator != null)
                    sb.Append(" << ").Append(sep);
                sb.Append(" << ");
                sb.Append(e.Accept(this));

                isFirst = false;
            }

            sb.Append(";");

            return Indent(sb.ToString(), indent);
        }

        public override string VisitExpressionStatement(ExpressionStatement stmt, int data = 0)
        {
            return $"{stmt.Expr.Accept(this)};";
        }

        public override string VisitIdentifierExpression(IdentifierExpression ident, int indent = 0)
        {
            return ident.Name;
        }

        public override string VisitVariableDeclaration(VariableDeclaration variable, int indent = 0)
        {
            var sb = new StringBuilder();
            string type = variable.Type?.Text ?? "auto";
            if (type == "string")
                type = "std::string";
            sb.Append($"{type} {variable.Name}");
            if (variable.Initializer != null)
            {
                sb.Append($" = ");
                sb.Append(variable.Initializer.Accept(this));
            }
            sb.Append(";");

            return Indent(sb.ToString(), indent);
        }

        #region literals
        public override string VisitStringLiteral(StringLiteral str, int sb)
        {
            return $"\"{str.Value.Replace("\r", "").Replace("\n", "\\n").Replace("\"", "\\\"")}\"";
        }
        
        public override string VisitAssignment(Assignment ass, int indent = 0)
        {
            return Indent(ass.Target.Accept(this) + " = " + ass.Value.Accept(this) + ";", indent);
        }
        
        public override string VisitNumberExpression(NumberExpression num, int indent = 0)
        {
            switch (num.Data.Type)
            {
                case Parsing.NumberData.NumberType.Int:
                    return num.Data.StringValue;
            }

            return null;
        }

        public override string VisitIfStatement(IfStatement ifs, int indent = 0)
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

            return Indent(sb.ToString(), indent);
        }

        public override string VisitBlockStatement(BlockStatement block, int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var s in block.Statements)
            {
                sb.AppendLine(Indent(s.Accept(this), 4));
            }
            sb.Append("}");
            return Indent(sb.ToString(), indent);
        }
        #endregion

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

        public override string VisitTypeDeclaration(TypeDeclaration type, int indent = 0)
        {
            var sb = new StringBuilder();

            sb.Append("struct ").Append(type.Name).AppendLine(" {");
            foreach (var m in type.Members)
            {
                sb.Append(Indent(m.Type.Text, 4)).Append(" ").Append(m.Name).AppendLine(";");
            }
            sb.Append("};");

            return Indent(sb.ToString(), indent);
        }

        public override string VisitImplBlock(ImplBlock impl, int indent = 0)
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
                    sb.AppendLine(f.Accept(this, 4));
                }
                sb.Append("}");
                mFunctionForwardDeclarations.AppendLine("}");

                return Indent(sb.ToString(), indent);
            }
            finally
            {
                mImplTarget = null;
            }
        }

        public override string VisitDotExpression(DotExpression dot, int data = 0)
        {
            return dot.Left.Accept(this) + "." + dot.Right;
        }

        public override string VisitCallExpression(CallExpression call, int data = 0)
        {
            var args = string.Join(", ", call.Arguments.Select(a => a.Accept(this)));
            return $"{call.Function.Accept(this)}({args})";
        }
    }
}
