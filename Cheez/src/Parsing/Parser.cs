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

        private Token Expect(TokenType type, bool skipNewLines = true, Func<TokenType, object, string> customErrorMessage = null)
        {
            while (true)
            {
                var tok = mLexer.NextToken();

                if (skipNewLines && tok.type == TokenType.NewLine)
                    continue;

                if (tok.type != type)
                {
                    string message = customErrorMessage != null ? customErrorMessage(tok.type, tok.data) : $"Unexpected token ({tok.type}) {tok.data}, expected {type}";
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

                if (skipNewLines && tok.type == TokenType.NewLine)
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
                case TokenType.KwVar:
                    return ParseVariableDeclaration();
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

        public VariableDeclaration ParseVariableDeclaration()
        {
            var beginning = Expect(TokenType.KwVar);
            var name = Expect(TokenType.Identifier);

            var next = mLexer.PeekToken();

            Token type = null;
            Expression init = null;
            switch (next.type)
            {
                case TokenType.Colon:
                    mLexer.NextToken();
                    type = ParseTypeExpression();
                    next = mLexer.PeekToken();
                    if (next.type == TokenType.Equal)
                        goto case TokenType.Equal;
                    else if (next.type == TokenType.Semicolon || next.type == TokenType.NewLine)
                        break;
                    goto case default;
                    
                case TokenType.Equal:
                    mLexer.NextToken();
                    init = ParseExpression();
                    next = mLexer.PeekToken();
                    if (next.type == TokenType.Semicolon || next.type == TokenType.NewLine)
                        break;
                    goto case default;

                case TokenType.Semicolon:
                case TokenType.NewLine:
                    throw new ParsingError(next.location, $"Expected either type annotation or initializer after variable name in declaration, got ({next.type}) {next.data}");

                default:
                    throw new ParsingError(next.location, $"Unexpected token after variable declaration: ({next.type}) {next.data}");
            }

            mLexer.NextToken();

            return new VariableDeclaration(beginning.location, (string)name.data, (string)type?.data, init);
        }


        public PrintStatement ParsePrintStatement()
        {
            var beginning = Expect(TokenType.KwPrint);
            var expr = ParseExpression();
            return new PrintStatement(beginning.location, expr);
        }

        private void SkipNewlines()
        {
            PeekToken(true);
        }

        #region Expression Parsing
        public Token ParseTypeExpression()
        {
            var name = mLexer.NextToken();
            if (name.type != TokenType.Identifier)
                throw new ParsingError(name.location, $"Failed to parse type expression, unexpected token: ({name.type}) {name.data}");
            return name;
        }

        public Expression ParseExpression()
        {
            return ParseAtomicExpression();
        }

        private Expression ParseAtomicExpression()
        {
            var token = mLexer.NextToken();
            switch (token.type)
            {
                case TokenType.Identifier:
                    return new IdentifierExpression(token.location, (string)token.data);

                case TokenType.StringLiteral:
                    return new StringLiteral(token.location, (string)token.data);

                case TokenType.OpenParen:
                    SkipNewlines();
                    var sub = ParseExpression();
                    Expect(TokenType.ClosingParen, customErrorMessage: (t, d) => $"Expected open paren '(' at end of group expression, got ({t}) {d}");
                    return sub;

                default:
                    throw new ParsingError(token.location, $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
            }
        }
        #endregion

        public FunctionDeclaration ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn);

            var name = Expect(TokenType.Identifier, customErrorMessage: (t, d) => $"Expected identifier at beginnig of function declaration, got ({t}) {d}");
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
