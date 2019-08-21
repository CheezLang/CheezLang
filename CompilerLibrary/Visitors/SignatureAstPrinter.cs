using System;
using System.IO;
using System.Linq;
using System.Text;
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
        
        public override string VisitFunctionDecl(AstFunctionDecl function, int indentLevel = 0)
        {
            var head = $"fn ";

            if (function.ImplBlock != null)
            {
                head += function.ImplBlock.TargetTypeExpr.Accept(rawPrinter) + "::";
            }

            head += function.Name.Accept(rawPrinter);

            var pars = string.Join(", ", function.Parameters.Select(p => p.Accept(rawPrinter)));
            head += $"({pars})";

            if (function.ReturnTypeExpr != null)
                head += $" -> {function.ReturnTypeExpr.Accept(rawPrinter)}";

            if (function.Directives != null)
            {
                head += string.Join(", ", function.Directives.Select(d => {
                    var args = string.Join(", ", d.Arguments.Select(a => a.Accept(rawPrinter)));
                    return $"#{d.Name}({args})";
                }));
            }

            return head;
        }

        public override string VisitStructDecl(AstStructDecl str, int data = 0)
        {
            var head = $"struct {str.Name.Name}";

            if (str.Parameters?.Count > 0)
            {
                head += "(";
                head += string.Join(", ", str.Parameters.Select(p => $"{p.Name.Name}: {p.TypeExpr.Accept(rawPrinter)}"));
                head += ")";
            }

            if (str.Directives != null)
            {
                head += string.Join(", ", str.Directives.Select(d => {
                    var args = string.Join(", ", d.Arguments.Select(a => a.Accept(rawPrinter)));
                    return $"#{d.Name}({args})";
                }));
            }

            return head;
        }

        public override string VisitTraitDecl(AstTraitDeclaration trait, int data = 0)
        {
            var head = $"trait {trait.Name.Name}";
            if (trait.Parameters?.Count > 0)
            {
                head += "(" + string.Join(", ", trait.Parameters.Select(p => $"{p.Name.Name}: {p.TypeExpr.Accept(rawPrinter)}")) + ")";
            }
            return head;
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
                        default: throw new NotImplementedException();
                    }
                }));

            return header;
        }
        
        public override string VisitEnumDecl(AstEnumDecl en, int data = 0)
        {
            var head = $"enum {en.Name.Accept(rawPrinter)}";
            if (en.Parameters != null)
            {
                head += $"({string.Join(", ", en.Parameters.Select(p => p.Accept(rawPrinter)))})";
            }

            if (en.Directives != null)
            {
                head += string.Join(", ", en.Directives.Select(d => {
                    var args = string.Join(", ", d.Arguments.Select(a => a.Accept(rawPrinter)));
                    return $"#{d.Name}({args})";
                }));
            }
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
