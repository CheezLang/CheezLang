using Cheez.Ast;
using System;
using System.Collections.Generic;

namespace Cheez.Parsing
{
    public class Parser
    {
        private Lexer mLexer;

        public Parser(Lexer lex)
        {
            mLexer = lex;
        }

        private Token Expect(TokenType type, bool skipNewLines = true, Func<TokenType, string> customErrorMessage = null)
        {
            while (true)
            {
                var tok = mLexer.NextToken();

                if (skipNewLines && tok.type == TokenType.EOL)
                    continue;

                if (tok.type != type)
                {
                    string message = customErrorMessage != null ? customErrorMessage(tok.type) : $"Unexpected token ({tok.type}) {tok.data}, expected {type}";
                    throw new ParsingError(tok.location, message);
                }

                return tok;
            }
        }

        private Token PeekToken(bool skipNewLines = false)
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (skipNewLines && tok.type == TokenType.EOL)
                {
                    mLexer.NextToken();
                    continue;
                }

                return tok;
            }
        }

        public Statement ParseStatement()
        {
            var token = PeekToken(true);
            switch (token.type)
            {
                case TokenType.KwFn:
                    return ParseFunctionDeclaration();
                case TokenType.KwPrint:
                    return ParsePrintStatement();
                case TokenType.EOF:
                    return null;
                default:
                    {
                        throw new ParsingError(token.location, $"Unexpected token ({token.type}) {token.data}");
                    }
            }
        }

        public PrintStatement ParsePrintStatement()
        {
            var beginning = Expect(TokenType.KwPrint);
            var lit = Expect(TokenType.StringLiteral, skipNewLines: false);
            return new PrintStatement(beginning.location, new StringLiteral(lit.location, (string)lit.data));
        }

        public FunctionDeclaration ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn);

            var name = Expect(TokenType.Identifier, customErrorMessage: t => $"Expected identifier at beginnig of function declaration, got {t}");
            List<Statement> statements = new List<Statement>();

            Expect(TokenType.DoubleColon);
            Expect(TokenType.OpenParen);
            Expect(TokenType.ClosingParen);
            Expect(TokenType.OpenBrace);

            while (true)
            {
                var token = PeekToken(true);
                if (token.type == TokenType.ClosingBrace)
                    break;

                statements.Add(ParseStatement());
            }

            Expect(TokenType.ClosingBrace);

            return new FunctionDeclaration(beginning.location, (string)name.data, statements);
        }
    }
}
