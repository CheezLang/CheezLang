using Cheez.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Parsing
{
    public class ParsingError : Exception
    {
        public ILocation Location { get; set; }

        public ParsingError(TokenLocation location, string message)
            : base(message)
        {
            this.Location = new Location(location);
        }
    }

    public class Parser
    {
        private Lexer mLexer;
        private ErrorHandler mErrorHandler = new ErrorHandler();

        public bool HasErrors => mErrorHandler.HasErrors;

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

        private Token ConsumeNewLine(Func<TokenType, object, string> customErrorMessage = null)
        {
            var tok = mLexer.NextToken();

            if (tok.type != TokenType.NewLine)
            {
                string message = customErrorMessage != null ? customErrorMessage(tok.type, tok.data) : $"Unexpected token ({tok.type}) {tok.data}, expected new line";
                throw new ParsingError(tok.location, message);
            }

            return tok;
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

        private Token ReadToken(bool SkipNewLines)
        {
            while (true)
            {
                var tok = mLexer.NextToken();

                if (SkipNewLines && tok.type == TokenType.NewLine)
                {
                    continue;
                }

                return tok;
            }
        }

        public Statement ParseStatement()
        {
            try
            {
                SkipNewlines();
                var token = PeekToken(true);
                switch (token.type)
                {
                    case TokenType.KwReturn:
                        return ParseReturnStatement();
                    case TokenType.KwFn:
                        return ParseFunctionDeclaration();
                    case TokenType.KwVar:
                        return ParseVariableDeclaration();
                    case TokenType.KwPrint:
                    case TokenType.KwPrintln:
                        return ParsePrintStatement();
                    case TokenType.KwIf:
                        return ParseIfStatement();
                    case TokenType.EOF:
                        return null;
                    case TokenType.KwStruct:
                        return ParseTypeDeclaration();
                    case TokenType.KwImpl:
                        return ParseImplBlock();
                    case TokenType.OpenBrace:
                        return ParseBlockStatement();

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
            catch (ParsingError err)
            {
                mErrorHandler.ReportError(mLexer, err.Location, err.Message);
                while (mLexer.PeekToken().type != TokenType.NewLine)
                    mLexer.NextToken();

                return null;
            }
        }

        private ReturnStatement ParseReturnStatement()
        {
            var beg = Expect(TokenType.KwReturn, skipNewLines: true).location;
            TokenLocation end = beg;
            Expression returnValue = null;

            if (PeekToken(skipNewLines: false).type != TokenType.NewLine)
            {
                returnValue = ParseExpression();
                end = returnValue.End;
            }

            ConsumeNewLine();

            return new ReturnStatement(beg, end, returnValue);
        }

        private TypeDeclaration ParseTypeDeclaration()
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

        private ImplBlock ParseImplBlock()
        {
            var functions = new List<FunctionDeclarationAst>();
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

        private BlockStatement ParseBlockStatement()
        {
            List<Statement> statements = new List<Statement>();
            var beginning = Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var s = ParseStatement();
                if (s != null)
                    statements.Add(s);
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new BlockStatement(beginning.location, end.location, statements);
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var expr = ParseExpression();
            return new ExpressionStatement(expr.Beginning, expr.End, expr);
        }

        private VariableDeclarationAst ParseVariableDeclaration()
        {
            var beginning = Expect(TokenType.KwVar, skipNewLines: true);
            var name = Expect(TokenType.Identifier, skipNewLines: true);
            TokenLocation end = name.location;

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
                    SkipNewlines();
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

            return new VariableDeclarationAst(beginning.location, end, name.location, (string)name.data, type, init);
        }

        private IfStatement ParseIfStatement()
        {
            Expression condition = null;
            Statement ifCase = null;
            Statement elseCase = null;

            var beginning = Expect(TokenType.KwIf, skipNewLines: true);
            TokenLocation end = beginning.location;

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

        private PrintStatement ParsePrintStatement()
        {
            List<Expression> expr = new List<Expression>();
            Expression seperator = null;

            var beginning = ReadToken(true);
            if (beginning.type != TokenType.KwPrint && beginning.type != TokenType.KwPrintln)
            {
                throw new ParsingError(beginning.location, "[ERROR]");
            }

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

            return new PrintStatement(beginning.location, expr.Last().End, expr, seperator, beginning.type == TokenType.KwPrintln);
        }

        private void SkipNewlines()
        {
            PeekToken(true);
        }

        #region Expression Parsing
        private TypeExpression ParseTypeExpression()
        {
            TypeExpression type = null;
            bool cond = true;
            while (cond)
            {
                var next = mLexer.PeekToken();
                switch (next.type)
                {
                    case TokenType.Identifier:
                        mLexer.NextToken();
                        type = new NamedTypeExression(next.location, next.location, (string)next.data);
                        break;

                    case TokenType.Asterisk:
                        mLexer.NextToken();
                        if (type == null)
                            throw new ParsingError(next.location, "Failed to parse type expression: * must be preceded by an actual type");
                        type = new PointerTypeExpression(type.Beginning, next.location, type);
                        break;

                    case TokenType.OpenBracket:
                        if (type == null)
                            throw new ParsingError(next.location, "Failed to parse type expression: [] must be preceded by an actual type");
                        mLexer.NextToken();
                        next = Expect(TokenType.ClosingBracket, skipNewLines: true);
                        type = new ArrayTypeExpression(type.Beginning, next.location, type);
                        break;

                    default:
                        cond = false;
                        break;
                }
            }
            return type;
        }

        private Expression ParseExpression()
        {
            Func<Expression> muldiv = () => ParseBinaryLeftAssociativeExpression(ParseCallExpression,
                (TokenType.Asterisk, BinaryOperator.Multiply),
                (TokenType.ForwardSlash, BinaryOperator.Divide));

            Func<Expression> addsub = () => ParseBinaryLeftAssociativeExpression(muldiv,
                (TokenType.Plus, BinaryOperator.Add),
                (TokenType.Minus, BinaryOperator.Subtract));

            return addsub();
        }

        private Expression ParseBinaryLeftAssociativeExpression(Func<Expression> sub, params (TokenType, BinaryOperator)[] types)
        {
            return ParseLeftAssociativeExpression(sub, type =>
            {
                foreach (var (t, o) in types)
                {
                    if (t == type)
                        return o;
                }

                return null;
            });
        }

        private Expression ParseLeftAssociativeExpression(Func<Expression> sub, Func<TokenType, BinaryOperator?> tokenMapping)
        {
            var lhs = sub();
            Expression rhs = null;

            while (true)
            {
                var next = PeekToken(skipNewLines: false);

                var op = tokenMapping(next.type);
                if (op == null)
                {
                    return lhs;
                }

                mLexer.NextToken();
                rhs = sub();
                lhs = new BinaryExpression(lhs.Beginning, rhs.End, op.Value, lhs, rhs);
            }
        }
        
        private Expression ParseCallExpression()
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

        private Expression ParseDotExpression()
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
                    sub.Beginning = token.location;
                    sub.End = Expect(TokenType.ClosingParen, skipNewLines: true, customErrorMessage: (t, d) => $"Expected open paren '(' at end of group expression, got ({t}) {d}").location;
                    return sub;

                default:
                    throw new ParsingError(token.location, $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
            }
        }
        #endregion

        private FunctionDeclarationAst ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn, skipNewLines: true);

            var name = Expect(TokenType.Identifier, skipNewLines: true, customErrorMessage: (t, d) => $"Expected identifier at beginnig of function declaration, got ({t}) {d}");
            var nameId = new IdentifierExpression(name.location, name.location, (string)name.data);
            List<Statement> statements = new List<Statement>();
            List<FunctionParameter> parameters = new List<FunctionParameter>();
            TypeExpression returnType = null;
            TokenLocation end = name.location;

            // parameters
            Expect(TokenType.OpenParen, skipNewLines: true);
            while (PeekToken(true).type != TokenType.ClosingParen)
            {
                var pname = Expect(TokenType.Identifier, true);
                Expect(TokenType.Colon, true);
                var tname = ParseTypeExpression();
                parameters.Add(new FunctionParameter(pname.location, tname.End, (string)pname.data, tname));

                var next = PeekToken(true);
                if (next.type == TokenType.Comma)
                    mLexer.NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                    throw new Exception($"Expected comma or closing paren, got {next.data} ({next.type})");
            }
            end = Expect(TokenType.ClosingParen, skipNewLines: true).location;

            // return type
            if (PeekToken(skipNewLines: true).type == TokenType.Colon)
            {
                mLexer.NextToken();
                returnType = ParseTypeExpression();
                end = returnType.End;
            }

            if (PeekToken(false).type == TokenType.NewLine)
            {
                mLexer.NextToken();
                return new FunctionDeclarationAst(beginning.location, end, nameId, parameters, returnType);
            }

            // implementation
            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (true)
            {
                var token = PeekToken(true);
                if (token.type == TokenType.ClosingBrace)
                    break;

                var stmt = ParseStatement();
                if (stmt != null)
                    statements.Add(stmt);
            }

            end = Expect(TokenType.ClosingBrace, skipNewLines: true).location;

            return new FunctionDeclarationAst(beginning.location, end, nameId, parameters, returnType, statements);
        }
    }
}
