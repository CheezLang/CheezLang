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

        public CheezCompiler compiler;

        public abstract AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler);

        public BuiltInMacro(CheezCompiler comp)
        {
            compiler = comp;
        }
    }

    public class BuiltInMacroFormat : BuiltInMacro
    {
        public BuiltInMacroFormat(CheezCompiler comp) : base(comp)
        {
        }

        public override AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler)
        {
            var parser = new Parser(lexer, errorHandler);

            var statements = new List<AstStatement>();

            // let result = String::empty()
            statements.Add(compiler.ParseStatement("let result = String::empty()"));

            // result.appendf("{}", [...])
            while (lexer.PeekToken().type != TokenType.EOF)
            {
                AstExpression expr;
                string format = "";

                var next = lexer.PeekToken();
                if (next.type == TokenType.StringLiteral)
                {
                    expr = parser.ParseExpression();
                }
                else if (next.type == TokenType.OpenBrace)
                {
                    lexer.NextToken();
                    expr = parser.ParseExpression();

                    // check for format
                    next = lexer.PeekToken();
                    if (next.type == TokenType.Colon)
                    {
                        lexer.NextToken();

                        var start = lexer.PeekToken().location;

                        // skip tokens until }
                        while (lexer.PeekToken().type != TokenType.ClosingBrace)
                            lexer.NextToken();

                        var end = lexer.PeekToken().location;

                        format = lexer.Text.Substring(start.index, end.index - start.index).Trim();
                    }

                    // consume }
                    next = lexer.NextToken();
                    if (next.type != TokenType.ClosingBrace)
                    {
                        errorHandler.ReportError(lexer.Text, new Location(next.location), $"Unexpected token {next.type}, expected '}}'");
                    }
                }
                else
                {
                    errorHandler.ReportError(lexer.Text, new Location(next.location), $"Unexpected token {next.type}, expected '{{'");
                    lexer.NextToken();
                    continue;
                }


                var call = compiler.ParseExpression($"§expr::print(result, \"{format}\")", new Dictionary<string, AstExpression>
                {
                    { "expr", expr }
                });

                statements.Add(new AstExprStmt(call, call.Location));
            }

            // result
            statements.Add(compiler.ParseStatement("result"));

            var result = new AstBlockExpr(statements, original.Location);
            return result;
        }
    }

    public class BuiltInMacroForeach : BuiltInMacro
    {
        public BuiltInMacroForeach(CheezCompiler comp) : base(comp)
        {
        }

        public override AstExpression Execute(AstMacroExpr original, ILexer lexer, IErrorHandler errorHandler)
        {
            var parser = new Parser(lexer, errorHandler);

            var name = parser.ConsumeUntil(TokenType.Identifier, Parser.ErrMsg("identifier", "at beginning of foreach loop"));
            parser.SkipNewlines();

            var kwIn = parser.ConsumeUntil(TokenType.Identifier, Parser.ErrMsg("keyword 'in'", "after name in foreach loop"));
            parser.SkipNewlines();

            if (kwIn.data as string != "in")
            {
                errorHandler.ReportError(lexer.Text, new Location(kwIn.location), $"Expected 'in' after name in foreach loop");
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
