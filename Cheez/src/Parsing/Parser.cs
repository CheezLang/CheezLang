using Cheez.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Parsing
{
    public class Parser
    {
        private Lexer mLexer;

        public Parser(Lexer lex)
        {
            mLexer = lex;
        }

        private Token Expect(TokenType type, bool skipNewLines, Func<TokenType, object, string> customErrorMessage = null)
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

        private Token ConsumeOptionalToken(TokenType type, bool skipNewLines)
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (skipNewLines && tok.type == TokenType.NewLine)
                {
                    mLexer.NextToken();
                    continue;
                }

                if (tok.type == type)
                {
                    mLexer.NextToken();
                    return tok;
                }

                return null;
            }
        }

        private Token PeekToken(bool skipNewLines)
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
            SkipNewlines();
            var token = PeekToken(true);
            switch (token.type)
            {
                case TokenType.KwFn:
                    return ParseFunctionDeclaration();
                case TokenType.KwVar:
                    return ParseVariableDeclaration();
                case TokenType.KwPrint:
                    return ParsePrintStatement();
                case TokenType.KwIf:
                    return ParseIfStatement();
                case TokenType.EOF:
                    return null;
                case TokenType.KwStruct:
                    return ParseTypeDeclaration();
                case TokenType.KwImpl:
                    return ParseImplBlock();

                default:
                    {
                        var expr = ParseExpression();
                        if (PeekToken(skipNewLines: false).type == TokenType.Equal)
                        {
                            mLexer.NextToken();
                            var val = ParseExpression();
                            return new Assignment(expr.Beginning, val.End, expr, val);
                        }
                        else
                        {
                            return new ExpressionStatement(expr.Beginning, expr.End, expr);
                        }
                        throw new ParsingError(token.location, $"Unexpected token ({token.type}) {token.data}");
                    }
            }
        }

        public TypeDeclaration ParseTypeDeclaration()
        {
            var members = new List<MemberDeclaration>();
            var beginnig = Expect(TokenType.KwStruct, skipNewLines: true);
            var name = Expect(TokenType.Identifier, skipNewLines: true);

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var mName = Expect(TokenType.Identifier, skipNewLines: true);
                Expect(TokenType.Colon, skipNewLines: true);
                var mType = ParseTypeExpression();
                if (PeekToken(skipNewLines: false).type != TokenType.ClosingBrace)
                    Expect(TokenType.NewLine, skipNewLines: false);

                members.Add(new MemberDeclaration((string)mName.data, mType));
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new TypeDeclaration(beginnig.location, end.location, (string)name.data, members);
        }

        public ImplBlock ParseImplBlock()
        {
            var functions = new List<FunctionDeclaration>();
            var beginnig = Expect(TokenType.KwImpl, skipNewLines: true);
            var target = Expect(TokenType.Identifier, skipNewLines: true);

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var f = ParseFunctionDeclaration();
                functions.Add(f);
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new ImplBlock(beginnig.location, end.location, (string)target.data, functions);
        }

        public BlockStatement ParseBlockStatement()
        {
            List<Statement> statements = new List<Statement>();
            var beginning = Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                statements.Add(ParseStatement());
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new BlockStatement(beginning.location, end.location, statements);
        }

        public ExpressionStatement ParseExpressionStatement()
        {
            var expr = ParseExpression();
            return new ExpressionStatement(expr.Beginning, expr.End, expr);
        }

        public VariableDeclaration ParseVariableDeclaration()
        {
            var beginning = Expect(TokenType.KwVar, skipNewLines: true);
            var name = Expect(TokenType.Identifier, skipNewLines: true);
            LocationInfo end = name.location;

            var next = mLexer.PeekToken();

            TypeExpression type = null;
            Expression init = null;
            switch (next.type)
            {
                case TokenType.Colon:
                    mLexer.NextToken();
                    type = ParseTypeExpression();
                    next = mLexer.PeekToken();
                    end = type.End;
                    if (next.type == TokenType.Equal)
                        goto case TokenType.Equal;
                    else if (next.type == TokenType.Semicolon)
                    {
                        end = next.location;
                        break;
                    }
                    else if (next.type == TokenType.NewLine)
                        break;
                    goto default;

                case TokenType.Equal:
                    mLexer.NextToken();
                    init = ParseExpression();
                    next = mLexer.PeekToken();
                    end = init.End;
                    if (next.type == TokenType.Semicolon)
                    {
                        end = next.location;
                        break;
                    }
                    if (next.type == TokenType.NewLine || next.type == TokenType.EOF)
                        break;
                    goto default;

                case TokenType.Semicolon:
                case TokenType.NewLine:
                    break;
                //    throw new ParsingError(next.location, $"Expected either type annotation or initializer after variable name in declaration, got ({next.type}) {next.data}");

                default:
                    throw new ParsingError(next.location, $"Unexpected token after variable declaration: ({next.type}) {next.data}");
            }

            mLexer.NextToken();

            return new VariableDeclaration(beginning.location, end, (string)name.data, type, init);
        }

        public IfStatement ParseIfStatement()
        {
            Expression condition = null;
            Statement ifCase = null;
            Statement elseCase = null;

            var beginning = Expect(TokenType.KwIf, skipNewLines: true);
            LocationInfo end = beginning.location;

            condition = ParseExpression();

            ifCase = ParseBlockStatement();
            end = ifCase.End;

            if (PeekToken(skipNewLines: true).type == TokenType.KwElse)
            {
                mLexer.NextToken();
                elseCase = ParseBlockStatement();
                end = elseCase.End;
            }

            return new IfStatement(beginning.location, end, condition, ifCase, elseCase);
        }

        public PrintStatement ParsePrintStatement()
        {
            List<Expression> expr = new List<Expression>();
            Expression seperator = null;

            var beginning = Expect(TokenType.KwPrint, skipNewLines: true);

            var next = PeekToken(skipNewLines: true);
            if (next.type == TokenType.OpenParen)
            {
                mLexer.NextToken();
                seperator = ParseExpression();
                Expect(TokenType.ClosingParen, skipNewLines: true);
            }

            do
            {
                expr.Add(ParseExpression());
            } while (ConsumeOptionalToken(TokenType.Comma, skipNewLines: false) != null);

            return new PrintStatement(beginning.location, expr.Last().End, expr, seperator);
        }

        private void SkipNewlines()
        {
            PeekToken(true);
        }

        #region Expression Parsing
        public TypeExpression ParseTypeExpression()
        {
            var name = mLexer.NextToken();
            if (name.type != TokenType.Identifier)
                throw new ParsingError(name.location, $"Failed to parse type expression, unexpected token: ({name.type}) {name.data}");
            return new TypeExpression(name.location, name.location, (string)name.data);
        }

        public Expression ParseExpression()
        {
            return ParseCallExpression();
        }

        public Expression ParseCallExpression()
        {
            var func = ParseDotExpression();

            if (PeekToken(false).type == TokenType.OpenParen)
            {
                mLexer.NextToken();
                List<Expression> args = new List<Expression>();
                if (PeekToken(true).type != TokenType.ClosingParen)
                {
                    while (true)
                    {
                        args.Add(ParseExpression());

                        var next = PeekToken(true);
                        if (next.type == TokenType.Comma)
                            mLexer.NextToken();
                        else if (next.type == TokenType.ClosingParen)
                            break;
                        else
                            throw new Exception($"Failed to parse function call, expected comma or closing paren, got {next.data} ({next.type})");
                    }
                }
                var end = Expect(TokenType.ClosingParen, true);

                return new CallExpression(func.Beginning, end.location, func, args);
            }

            return func;
        }

        public Expression ParseDotExpression()
        {
            var left = ParseAtomicExpression();

            while (mLexer.PeekToken().type == TokenType.Period)
            {
                mLexer.NextToken();
                var right = Expect(TokenType.Identifier, skipNewLines: true);
                left = new DotExpression(left.Beginning, right.location, left, (string)right.data);
            }

            return left;
        }

        private Expression ParseAtomicExpression()
        {
            var token = mLexer.NextToken();
            switch (token.type)
            {
                case TokenType.Identifier:
                    return new IdentifierExpression(token.location, token.location, (string)token.data);

                case TokenType.StringLiteral:
                    return new StringLiteral(token.location, token.location, (string)token.data);

                case TokenType.NumberLiteral:
                    return new NumberExpression(token.location, token.location, (NumberData)token.data);

                case TokenType.OpenParen:
                    SkipNewlines();
                    var sub = ParseExpression();
                    Expect(TokenType.ClosingParen, skipNewLines: true, customErrorMessage: (t, d) => $"Expected open paren '(' at end of group expression, got ({t}) {d}");
                    return sub;

                default:
                    throw new ParsingError(token.location, $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
            }
        }
        #endregion

        public FunctionDeclaration ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn, skipNewLines: true);

            var name = Expect(TokenType.Identifier, skipNewLines: true, customErrorMessage: (t, d) => $"Expected identifier at beginnig of function declaration, got ({t}) {d}");
            List<Statement> statements = new List<Statement>();
            List<FunctionParameter> parameters = new List<FunctionParameter>();
            TypeExpression returnType = null;

            // parameters
            Expect(TokenType.OpenParen, skipNewLines: true);
            while (PeekToken(true).type != TokenType.ClosingParen)
            {
                var pname = Expect(TokenType.Identifier, true);
                Expect(TokenType.Colon, true);
                var tname = ParseTypeExpression();
                parameters.Add(new FunctionParameter((string)pname.data, tname));

                var next = PeekToken(true);
                if (next.type == TokenType.Comma)
                    mLexer.NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                    throw new Exception($"Expected comma or closing paren, got {next.data} ({next.type})");
            }
            Expect(TokenType.ClosingParen, skipNewLines: true);
            
            // return type
            if (PeekToken(skipNewLines: true).type == TokenType.Colon)
            {
                mLexer.NextToken();
                returnType = ParseTypeExpression();
            }

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (true)
            {
                var token = PeekToken(true);
                if (token.type == TokenType.ClosingBrace)
                    break;

                statements.Add(ParseStatement());
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new FunctionDeclaration(beginning.location, end.location, (string)name.data, parameters, returnType, statements);
        }
    }
}
