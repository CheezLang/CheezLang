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
    public class SignatureAstPrinter : VisitorBase<string, int>
    {
        private VisitorBase<string, int> rawPrinter;

        public SignatureAstPrinter(bool raw = true)
        {
            if (raw)
                rawPrinter = new RawAstPrinter(new StringWriter());
            else
                rawPrinter = new AnalysedAstPrinter();
        }

        public string VisitDirective(AstDirective dir)
        {
            var name = "#" + dir.Name.Accept(rawPrinter);
            if (dir.Arguments.Count > 0)
            {
                name += $"({string.Join(", ", dir.Arguments.Select(a => a.Accept(rawPrinter)))})";
            }
            return name;
        }

        public override string VisitImplDecl(AstImplBlock impl, int data = 0)
        {
            var header = "impl";

            // parametersu
            var parameters = impl.IsPolyInstance ? impl.Template.Parameters : impl.Parameters;
            if (parameters != null)
                header += "(" + string.Join(", ", parameters.Select(p => p.Accept(rawPrinter, 0))) + ")";

            header += " ";

            if (impl.TraitExpr != null)
                header += TypeToString(impl.TraitExpr) + " for ";

            header += TypeToString(impl.TargetTypeExpr);

            var conditions = impl.IsPolyInstance ? impl.Template.Conditions : impl.Conditions;
            if (conditions != null)
                header += " if " + string.Join(", ", conditions.Select(c => {
                    switch (c)
                    {
                        case ImplConditionImplTrait t: return $"{TypeToString(t.type)} : {TypeToString(t.trait)}";
                        case ImplConditionNotYet t: return "#notyet";
                        case ImplConditionAny a: return a.Expr.Accept(this);
                        default: throw new NotImplementedException();
                    }
                }));

            return header;
        }

        public override string VisitFuncExpr(AstFuncExpr function, int data = 0)
        {
            var pars = string.Join(", ", function.Parameters.Select(p => p.Accept(this)));
            var head = $"{function.Name}({pars})";

            if (function.ReturnTypeExpr != null)
                head += $" -> {function.ReturnTypeExpr.Accept(this)}";

            // @todo
            if (function.Directives.Count > 0)
                head += " " + string.Join(" ", function.Directives.Select(d => VisitDirective(d)));

            return head;
        }

        private string TypeToString(AstExpression typeExpr)
        {
            if (rawPrinter is AnalysedAstPrinter)
                return typeExpr.Value?.ToString() ?? typeExpr.Accept(rawPrinter);
            return typeExpr.Accept(rawPrinter);
        }
    }
}
