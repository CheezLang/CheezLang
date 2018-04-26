using Cheez.Compiler.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.Parsing
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
        private delegate TokenLocation LocationResolver(Token t);
        private delegate string ErrorMessageResolver(Token t);
        private delegate PTExpr ExpressionParser(LocationResolver t, ErrorMessageResolver e);

        private Lexer mLexer;
        private IErrorHandler mErrorHandler = new ErrorHandler();

        public bool HasErrors => mErrorHandler.HasErrors;

        public Parser(Lexer lex, IErrorHandler errHandler)
        {
            mLexer = lex;
            mErrorHandler = errHandler ?? mErrorHandler;
        }

        private void SkipNewlines()
        {
            PeekToken(true);
        }

        private Token Expect(TokenType type, bool skipNewLines, Func<TokenType, object, string> customErrorMessage = null, LocationResolver customLocation = null)
        {
            while (true)
            {
                var tok = mLexer.NextToken();

                if (tok.type == TokenType.EOF)
                    return tok;

                if (skipNewLines && tok.type == TokenType.NewLine)
                    continue;

                if (tok.type != type)
                {
                    string message = customErrorMessage?.Invoke(tok.type, tok.data) ?? $"Unexpected token ({tok.type}) {tok.data}, expected {type}";
                    var loc = customLocation?.Invoke(tok) ?? tok.location;
                    throw new ParsingError(loc, message);
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

                if (tok.type == TokenType.EOF)
                    return tok;

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

                if (tok.type == TokenType.EOF)
                    return tok;

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

                if (tok.type == TokenType.EOF)
                    return tok;

                if (SkipNewLines && tok.type == TokenType.NewLine)
                {
                    continue;
                }

                return tok;
            }
        }

        public PTStatement ParseStatement()
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
                    case TokenType.KwWhile:
                        return ParseWhileStatement();
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
                                return new PTAssignment(expr.Beginning, val.End, expr, val);
                            }
                            else
                            {
                                return new PTExprStmt(expr.Beginning, expr.End, expr);
                            }
                            throw new ParsingError(token.location, $"Unexpected token ({token.type}) {token.data}");
                        }
                }
            }
            catch (ParsingError err)
            {
                var stackTrace = new System.Diagnostics.StackTrace(err);
                var topFrame = stackTrace.GetFrame(0);
                mErrorHandler.ReportError(mLexer, err.Location, err.Message, err.HelpLink, topFrame.GetMethod().Name, topFrame.GetFileLineNumber());
                while (true)
                {
                    var next = mLexer.PeekToken();

                    if (next.type == TokenType.EOF || next.type == TokenType.NewLine)
                        break;

                    if (next.type != TokenType.NewLine)
                    {
                        mLexer.NextToken();
                    }
                }
                return null;
            }
        }

        private PTReturnStmt ParseReturnStatement()
        {
            var beg = Expect(TokenType.KwReturn, skipNewLines: true).location;
            PTExpr returnValue = null;

            if (PeekToken(skipNewLines: false).type != TokenType.NewLine)
            {
                returnValue = ParseExpression();
            }

            ConsumeNewLine();

            return new PTReturnStmt(beg, returnValue);
        }

        private PTTypeDecl ParseTypeDeclaration()
        {
            var members = new List<PTMemberDecl>();
            var beginnig = Expect(TokenType.KwStruct, skipNewLines: true);
            var name = ParseIdentifierExpr(true);
            List<PTDirective> directives = new List<PTDirective>();

            while (PeekToken(false).type == TokenType.HashTag)
            {
                var hashtag = mLexer.NextToken();
                var dname = ParseIdentifierExpr(false);
                directives.Add(new PTDirective(hashtag.location, dname.End, dname));
            }

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var mName = ParseIdentifierExpr(true);
                Expect(TokenType.Colon, skipNewLines: true);
                var mType = ParseTypeExpression();
                if (PeekToken(skipNewLines: false).type != TokenType.ClosingBrace)
                    Expect(TokenType.NewLine, skipNewLines: false);

                members.Add(new PTMemberDecl(mName, mType));
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new PTTypeDecl(beginnig.location, end.location, name, members, directives);
        }

        private PTImplBlock ParseImplBlock()
        {
            var functions = new List<PTFunctionDecl>();
            var beginnig = Expect(TokenType.KwImpl, skipNewLines: true);
            var target = ParseIdentifierExpr(true);

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var f = ParseFunctionDeclaration();
                functions.Add(f);
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new PTImplBlock(beginnig.location, end.location, target, functions);
        }

        private PTBlockStmt ParseBlockStatement()
        {
            List<PTStatement> statements = new List<PTStatement>();
            var beginning = Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var s = ParseStatement();
                if (s != null)
                    statements.Add(s);
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new PTBlockStmt(beginning.location, end.location, statements);
        }

        private PTExprStmt ParseExpressionStatement()
        {
            var expr = ParseExpression();
            return new PTExprStmt(expr.Beginning, expr.End, expr);
        }

        private PTVariableDecl ParseVariableDeclaration(params TokenType[] delimiters)
        {
            var beginning = Expect(TokenType.KwVar, skipNewLines: true);
            var name = ParseIdentifierExpr(true, (t, d) => $"Expected identifier after 'let' in variable declaration", t => beginning.location);
            TokenLocation end = name.End;

            var next = mLexer.PeekToken();

            PTTypeExpr type = null;
            PTExpr init = null;
            switch (next.type)
            {
                case TokenType.Colon:
                    mLexer.NextToken();
                    type = ParseTypeExpression();
                    next = mLexer.PeekToken();
                    end = type.End;
                    if (next.type == TokenType.Equal)
                        goto case TokenType.Equal;
                    else if (delimiters.Contains(next.type))
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
                    init = ParseExpression(errorMessage: t => $"Expected expression after '='");
                    next = mLexer.PeekToken();
                    end = init.End;
                    if (delimiters.Contains(next.type))
                    {
                        end = next.location;
                        break;
                    }
                    if (next.type == TokenType.NewLine || next.type == TokenType.EOF)
                        break;
                    goto default;

                case TokenType ttt when delimiters.Contains(ttt):
                    break;
                case TokenType.Semicolon:
                case TokenType.NewLine:
                    break;
                //    throw new ParsingError(next.location, $"Expected either type annotation or initializer after variable name in declaration, got ({next.type}) {next.data}");

                default:
                    throw new ParsingError(next.location, $"Unexpected token after variable declaration: ({next.type}) {next.data}");
            }

            mLexer.NextToken();

            return new PTVariableDecl(beginning.location, end, name, type, init);
        }

        private PTWhileStmt ParseWhileStatement()
        {
            var beginning = Expect(TokenType.KwWhile, skipNewLines: true);
            PTVariableDecl varDecl = null;
            PTStatement postAction = null;

            if (PeekToken(true).type == TokenType.KwVar)
            {
                varDecl = ParseVariableDeclaration(TokenType.Comma);
            }

            PTExpr condition = ParseExpression();

            if (PeekToken(false).type == TokenType.NewLine || PeekToken(false).type == TokenType.Comma)
            {
                mLexer.NextToken();
                postAction = ParseStatement();
            }

            PTStatement body = ParseBlockStatement();


            return new PTWhileStmt(beginning.location, body.End, condition, body, varDecl, postAction);
        }

        private PTIfStmt ParseIfStatement()
        {
            PTExpr condition = null;
            PTStatement ifCase = null;
            PTStatement elseCase = null;

            var beginning = Expect(TokenType.KwIf, skipNewLines: true);
            TokenLocation end = beginning.location;

            condition = ParseExpression();

            ifCase = ParseBlockStatement();
            end = ifCase.End;

            if (PeekToken(skipNewLines: true).type == TokenType.KwElse)
            {
                mLexer.NextToken();

                if (PeekToken(skipNewLines: false).type == TokenType.KwIf)
                    elseCase = ParseIfStatement();
                else
                    elseCase = ParseBlockStatement();
                end = elseCase.End;
            }

            return new PTIfStmt(beginning.location, end, condition, ifCase, elseCase);
        }

        private PTPrintStmt ParsePrintStatement()
        {
            List<PTExpr> expr = new List<PTExpr>();
            PTExpr seperator = null;

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

            return new PTPrintStmt(beginning.location, expr.Last().End, expr, seperator, beginning.type == TokenType.KwPrintln);
        }

        private PTFunctionDecl ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn, skipNewLines: true);

            var name = ParseIdentifierExpr(true, (t, d) => $"Expected identifier at beginnig of function declaration, got ({t}) {d}");
            List<PTStatement> statements = new List<PTStatement>();
            List<PTFunctionParam> parameters = new List<PTFunctionParam>();
            PTTypeExpr returnType = null;
            TokenLocation end = name.End;

            // parameters
            Expect(TokenType.OpenParen, skipNewLines: true);
            while (true)
            {
                var token = PeekToken(true);
                if (token.type == TokenType.ClosingParen || token.type == TokenType.EOF)
                    break;

                var pname = ParseIdentifierExpr(true);
                Expect(TokenType.Colon, true);
                var tname = ParseTypeExpression();
                parameters.Add(new PTFunctionParam(pname.Beginning, tname.End, pname, tname));

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
            if (PeekToken(skipNewLines: false).type == TokenType.Colon)
            {
                mLexer.NextToken();
                returnType = ParseTypeExpression();
                end = returnType.End;
            }

            if (PeekToken(false).type == TokenType.NewLine)
            {
                mLexer.NextToken();
                return new PTFunctionDecl(beginning.location, end, name, parameters, returnType);
            }

            // implementation
            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (true)
            {
                var token = PeekToken(true);

                if (token.type == TokenType.EOF)
                {
                    throw new ParsingError(token.location, "Unexpected end of file in function declaration.");
                }

                if (token.type == TokenType.ClosingBrace)
                    break;

                var stmt = ParseStatement();
                if (stmt != null)
                    statements.Add(stmt);
            }

            end = Expect(TokenType.ClosingBrace, skipNewLines: true).location;

            return new PTFunctionDecl(beginning.location, end, name, parameters, returnType, statements);
        }

        #region Expression Parsing

        private PTTypeExpr ParseTypeExpression()
        {
            PTTypeExpr type = null;
            bool cond = true;
            while (cond)
            {
                var next = mLexer.PeekToken();
                switch (next.type)
                {
                    case TokenType.Identifier:
                        mLexer.NextToken();
                        type = new PTNamedTypeExpr(next.location, next.location, (string)next.data);
                        break;

                    case TokenType.Asterisk:
                        mLexer.NextToken();
                        if (type == null)
                            throw new ParsingError(next.location, "Failed to parse type expression: * must be preceded by an actual type");
                        type = new PTPointerTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.OpenBracket:
                        if (type == null)
                            throw new ParsingError(next.location, "Failed to parse type expression: [] must be preceded by an actual type");
                        mLexer.NextToken();
                        next = Expect(TokenType.ClosingBracket, skipNewLines: true);
                        type = new PTArrayTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.NewLine when type == null:
                        throw new ParsingError(next.location, "Expected type expression, found new line");
                    case TokenType.EOF when type == null:
                        throw new ParsingError(next.location, "Expected type expression, found end of file");


                    case TokenType t when type == null:
                        throw new ParsingError(next.location, $"Unexpected token in type expression: {next}");

                    default:
                        cond = false;
                        break;
                }
            }
            return type;
        }

        private PTExpr ParseExpression(LocationResolver location = null, ErrorMessageResolver errorMessage = null)
        {
            location = location ?? (t => t.location);
            errorMessage = errorMessage ?? (t => $"Unexpected token '{t}' in expression");

            ExpressionParser muldiv = (l, e) => ParseBinaryLeftAssociativeExpression(ParseAddressOfOrCallExpression, l, e,
                (TokenType.Asterisk, Operator.Multiply),
                (TokenType.ForwardSlash, Operator.Divide),
                (TokenType.Percent, Operator.Modulo));

            ExpressionParser addsub = (l, e) => ParseBinaryLeftAssociativeExpression(muldiv, l, e,
                (TokenType.Plus, Operator.Add),
                (TokenType.Minus, Operator.Subtract));

            ExpressionParser comparison = (l, e) => ParseBinaryLeftAssociativeExpression(addsub, l, e,
                (TokenType.Less, Operator.Less),
                (TokenType.LessEqual, Operator.LessEqual),
                (TokenType.Greater, Operator.Greater),
                (TokenType.GreaterEqual, Operator.GreaterEqual),
                (TokenType.DoubleEqual, Operator.Equal),
                (TokenType.NotEqual, Operator.NotEqual));

            ExpressionParser and = (l, e) => ParseBinaryLeftAssociativeExpression(comparison, l, e,
                (TokenType.KwAnd, Operator.And));

            ExpressionParser or = (l, e) => ParseBinaryLeftAssociativeExpression(and, l, e,
                (TokenType.KwOr, Operator.Or));


            return or(location, errorMessage);
        }

        private PTExpr ParseBinaryLeftAssociativeExpression(ExpressionParser sub, LocationResolver location, ErrorMessageResolver errorMessage, params (TokenType, Operator)[] types)
        {
            return ParseLeftAssociativeExpression(sub, location, errorMessage, type =>
            {
                foreach (var (t, o) in types)
                {
                    if (t == type)
                        return o;
                }

                return null;
            });
        }

        private PTExpr ParseLeftAssociativeExpression(ExpressionParser sub, LocationResolver location, ErrorMessageResolver errorMessage, Func<TokenType, Operator?> tokenMapping)
        {
            var lhs = sub(location, errorMessage);
            PTExpr rhs = null;

            while (true)
            {
                var next = PeekToken(skipNewLines: false);

                var op = tokenMapping(next.type);
                if (op == null)
                {
                    return lhs;
                }

                mLexer.NextToken();
                rhs = sub(location, errorMessage);
                lhs = new PTBinaryExpr(lhs.Beginning, rhs.End, op.Value, lhs, rhs);
            }
        }

        private PTExpr ParseAddressOfOrCallExpression(LocationResolver location, ErrorMessageResolver errorMessage = null)
        {
            var next = PeekToken(false);
            if (next.type == TokenType.Ampersand)
            {
                mLexer.NextToken();
                return new PTAddressOfExpr(next.location, ParseDotExpression(location, errorMessage));
            }

            return ParseCallExpression(location, errorMessage);
        }

        private PTExpr ParseCallExpression(LocationResolver location, ErrorMessageResolver errorMessage)
        {
            var expr = ParseDotExpression(location, errorMessage);

            while (true)
            {
                switch (PeekToken(false).type)
                {
                    case TokenType.OpenParen:
                        {
                            mLexer.NextToken();
                            List<PTExpr> args = new List<PTExpr>();
                            if (PeekToken(true).type != TokenType.ClosingParen)
                            {
                                while (true)
                                {
                                    args.Add(ParseExpression(location));

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

                            expr = new PTCallExpr(expr.Beginning, end.location, expr, args);
                        }
                        break;

                    case TokenType.OpenBracket:
                        {
                            mLexer.NextToken();
                            var index = ParseExpression(location, errorMessage);
                            var end = Expect(TokenType.ClosingBracket, true, customLocation: location);
                            expr = new PTArrayAccessExpr(expr.Beginning, end.location, expr, index);
                        }
                        break;

                    default:
                        return expr;
                }
            }
        }

        private PTExpr ParseDotExpression(LocationResolver location, ErrorMessageResolver errorMessage)
        {
            var left = ParseAtomicExpression(location, errorMessage);

            while (mLexer.PeekToken().type == TokenType.Period)
            {
                mLexer.NextToken();
                var right = ParseIdentifierExpr(true, (t, o) => $"Right side of '.' has to be an identifier", location);
                left = new PTDotExpr(left.Beginning, right.End, left, right);
            }

            return left;
        }

        private PTExpr ParseAtomicExpression(LocationResolver location, ErrorMessageResolver errorMessage)
        {
            var token = mLexer.NextToken();
            switch (token.type)
            {
                case TokenType.Identifier:
                    return new PTIdentifierExpr(token.location, (string)token.data);

                case TokenType.StringLiteral:
                    return new PTStringLiteral(token.location, (string)token.data);

                case TokenType.NumberLiteral:
                    return new PTNumberExpr(token.location, (NumberData)token.data);

                case TokenType.KwTrue:
                    return new PTBoolExpr(token.location, true);
                case TokenType.KwFalse:
                    return new PTBoolExpr(token.location, false);

                case TokenType.KwCast:
                    {
                        Expect(TokenType.Less, true);
                        var type = ParseTypeExpression();
                        Expect(TokenType.Greater, true);
                        Expect(TokenType.OpenParen, true);
                        var s = ParseExpression(location);
                        var end = Expect(TokenType.ClosingParen, true);
                        return new PTCastExpr(token.location, end.location, type, s);
                    }

                case TokenType.OpenParen:
                    SkipNewlines();
                    var sub = ParseExpression(location);
                    sub.Beginning = token.location;
                    sub.End = Expect(TokenType.ClosingParen, skipNewLines: true, customErrorMessage: (t, d) => $"Expected closing paren ')' at end of group expression, got ({t}) {d}").location;
                    return sub;

                default:
                    throw new ParsingError(location?.Invoke(token) ?? token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
            }
        }

        private PTIdentifierExpr ParseIdentifierExpr(bool SkipNewLines, Func<TokenType, object, string> customErrorMessage = null, LocationResolver customLocation = null)
        {
            var t = Expect(TokenType.Identifier, SkipNewLines, customErrorMessage, customLocation);
            return new PTIdentifierExpr(t.location, (string)t.data);
        }
        #endregion

    }
}
