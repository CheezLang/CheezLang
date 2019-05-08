using System;
using System.Collections.Generic;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Parsing;

namespace Cheez
{
    public interface IMacro
    {

    }

    public abstract class BuiltInMacro : IMacro
    {
        public AstIdExpr Name => throw new NotImplementedException();
        public ILocation Location => throw new NotImplementedException();

        public abstract AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler);
    }

    public class BuiltInMacroPrint : BuiltInMacro
    {
        public override AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler)
        {
            var parser = new Parser(lexer, errorHandler);

            var statements = new List<AstStatement>();

            // let result = String::empty()
            statements.Add(new AstVariableDecl(
                new AstIdExpr("result", false, original.Location),
                null,
                new AstCallExpr(
                    new AstDotExpr(
                        new AstIdExpr("String", false, original.Location),
                        new AstIdExpr("empty", false, original.Location),
                        true,
                        original.Location
                    ),
                    new List<AstArgument>(),
                    original.Location
                ),
                false,
                Location: original.Location
            ));


            // result.appendf("{}", [...])
            while (lexer.PeekToken().type != TokenType.EOF)
            {
                var expr = parser.ParseExpression();

                var call = Parser.ParseExpression("result.appendf(\"{}\", [§expr])", new Dictionary<string, AstExpression>
                {
                    { "expr", expr }
                }, errorHandler);

                statements.Add(new AstExprStmt(call, call.Location));
            }

            // println
            statements.Add(new AstExprStmt(
                new AstCallExpr(
                    new AstIdExpr("println", false, original.Location),
                    new List<AstArgument>
                    {
                        new AstArgument(
                            new AstIdExpr("result", false, original.Location),
                            null,
                            original.Location
                        )
                    },
                    original.Location
                ),
                original.Location
            ));

            var result = new AstBlockExpr(statements, original.Location);
            return result;
        }
    }

    public class BuiltInMacroForeach : BuiltInMacro
    {
        public override AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler)
        {
            var parser = new Parser(lexer, errorHandler);

            var name = parser.ConsumeUntil(TokenType.Identifier, Parser.ErrMsg("identifier", "at beginning of foreach loop"));
            parser.SkipNewlines();

            var kwIn = parser.ConsumeUntil(TokenType.Identifier, Parser.ErrMsg("keyword 'in'", "after name in foreach loop"));
            parser.SkipNewlines();

            if (kwIn.data as string != "in")
            {
                errorHandler.ReportError(lexer, new Location(kwIn.location), $"Expected 'in' after name in foreach loop");
                return null;
            }

            var iterator = parser.ParseExpression();
            var body = parser.ParseExpression();

            var result = new AstBlockExpr(new List<AstStatement>
            {
                new AstVariableDecl(new AstIdExpr("it", false, iterator.Location), null, iterator, false, Location: new Location(name.location, iterator.End)),
                new AstWhileStmt(new AstBoolExpr(true, original.Location), new AstBlockExpr(new List<AstStatement>
                {
                    new AstExprStmt(
                        new AstMatchExpr(
                            new AstCallExpr(
                                new AstDotExpr(
                                    new AstIdExpr("it", false, iterator.Location),
                                    new AstIdExpr("next", false, iterator.Location), false, iterator.Location),
                                new List<AstArgument>(), iterator.Location),
                            new List<AstMatchCase>
                            {
                                new AstMatchCase(
                                    new AstCallExpr(
                                        new AstIdExpr("Some", false, original.Location),
                                        new List<AstArgument>
                                        {
                                            new AstArgument(new AstIdExpr(name.data as string, true, original.Location), Location: original.Location)
                                        }),
                                    null,
                                    body),
                                new AstMatchCase(
                                    new AstIdExpr("None", false, original.Location),
                                    null,
                                    new AstBlockExpr(new List<AstStatement>
                                    {
                                        new AstBreakStmt(iterator.Location)
                                    }), iterator.Location)
                            }, body.Location), body.Location)
                }, original.Location), null, null, original.Location)
            }, original.Location);

            return result;
        }
    }
}
