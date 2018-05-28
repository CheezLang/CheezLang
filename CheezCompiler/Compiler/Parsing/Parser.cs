using Cheez.Compiler.ParseTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler.Parsing
{
    public class SkipInStackFrame : Attribute
    { }

    public class Parser
    {
        private delegate TokenLocation LocationResolver(Token t);
        private delegate string ErrorMessageResolver(Token t);
        private delegate PTExpr ExpressionParser(LocationResolver t, ErrorMessageResolver e);

        private Lexer mLexer;
        private IErrorHandler mErrorHandler;

        private Token lastNonWhitespace = null;

        public Parser(Lexer lex, IErrorHandler errHandler)
        {
            mLexer = lex;
            mErrorHandler = errHandler;
        }

        private (TokenLocation beg, TokenLocation end) GetWhitespaceLocation()
        {
            var end = mLexer.PeekToken().location;
            return (new TokenLocation
            {
                file = end.file,
                index = lastNonWhitespace?.location?.end ?? 0,
                end = lastNonWhitespace?.location?.end ?? 0,
                line = lastNonWhitespace?.location?.line ?? end.line,
                lineStartIndex = lastNonWhitespace?.location?.lineStartIndex ?? end.lineStartIndex,
            }, end);
        }

        [SkipInStackFrame]
        private void SkipNewlines()
        {
            PeekToken(true);
        }

        [SkipInStackFrame]
        private Token NextToken()
        {
            var next = mLexer.NextToken();
            if (next.type != TokenType.NewLine)
                lastNonWhitespace = next;
            return next;
        }

        [SkipInStackFrame]
        private string GetCodeLocation([CallerFilePath] string file = "", [CallerMemberName] string func = "", [CallerLineNumber] int line = 0)
        {
            return $"{Path.GetFileName(file)}:{line} - {func}()";
        }

        [SkipInStackFrame]
        private (string function, string file, int line)? GetCallingFunction()
        {
            try
            {
                var trace = new StackTrace(true);
                var frames = trace.GetFrames();

                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    var attribute = method.GetCustomAttributesData().FirstOrDefault(d => d.AttributeType == typeof(SkipInStackFrame));
                    if (attribute != null)
                        continue;

                    return (method.Name, frame.GetFileName(), frame.GetFileLineNumber());
                }
            }
            catch (Exception)
            { }

            return null;
        }

        [SkipInStackFrame]
        private void ReportError(TokenLocation location, string message)
        {
            string callingFunctionFile, callingFunctionName;
            int callLineNumber;
            (callingFunctionFile, callingFunctionName, callLineNumber) = GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, new Location(location, location), message, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        private void ReportError(ILocation location, string message)
        {
            string callingFunctionFile, callingFunctionName;
            int callLineNumber;
            (callingFunctionFile, callingFunctionName, callLineNumber) = GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, location, message, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        private void RecoverStatement()
        {
            while (true)
            {
                var next = mLexer.PeekToken();
                switch (next.type)
                {
                    case TokenType.NewLine:
                    case TokenType.Semicolon:
                        NextToken();
                        return;

                    case TokenType.EOF:
                    case TokenType.ClosingBrace:
                        return;
                }
                NextToken();
            }
        }

        [SkipInStackFrame]
        private void RecoverExpression()
        {
            while (true)
            {
                var next = mLexer.PeekToken();
                switch (next.type)
                {
                    case TokenType.NewLine:
                    case TokenType.EOF:
                    case TokenType.Semicolon:
                    case TokenType.OpenBrace:
                    case TokenType.OpenBracket:
                    case TokenType.OpenParen:
                    case TokenType.ClosingBrace:
                    case TokenType.ClosingBracket:
                    case TokenType.ClosingParen:
                    case TokenType.Comma:
                        return;
                }
                NextToken();
            }
        }

        [SkipInStackFrame]
        private bool Expect(out Token result, TokenType type, bool skipNewLines, ErrorMessageResolver customErrorMessage = null, LocationResolver customLocation = null)
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (skipNewLines && tok.type == TokenType.NewLine)
                {
                    NextToken();
                    continue;
                }

                if (tok.type != type)
                {
                    string message = customErrorMessage?.Invoke(tok) ?? $"Unexpected token ({tok.type}) {tok.data}, expected {type}";
                    var loc = customLocation?.Invoke(tok) ?? tok.location;
                    ReportError(loc, message);
                    result = tok;
                    return false;
                }

                NextToken();
                result = tok;
                return true;
            }
        }

        [SkipInStackFrame]
        private bool Expect(out TokenLocation loc, TokenType type, bool skipNewLines, ErrorMessageResolver customErrorMessage = null, LocationResolver customLocation = null)
        {
            var b = Expect(out Token t, type, skipNewLines, customErrorMessage, customLocation);
            loc = t.location;
            return b;
        }

        [SkipInStackFrame]
        private bool Consume(TokenType type, bool skipNewLines, ErrorMessageResolver customErrorMessage = null, LocationResolver customLocation = null)
        {
            return Expect(out Token t, type, skipNewLines, customErrorMessage, customLocation);
        }

        [SkipInStackFrame]
        private Token Expect(TokenType type, bool skipNewLines, ErrorMessageResolver customErrorMessage = null, LocationResolver customLocation = null)
        {
            Expect(out Token t, type, skipNewLines, customErrorMessage, customLocation);
            return t;
        }

        [SkipInStackFrame]
        private void ConsumeNewLine(Func<TokenType, object, string> customErrorMessage = null)
        {
            var tok = NextToken();

            if (tok.type != TokenType.NewLine)
            {
                string message = customErrorMessage != null ? customErrorMessage(tok.type, tok.data) : $"Unexpected token ({tok.type}) {tok.data}, expected new line";
                //throw new ParsingError(tok.location, message);
                ReportError(tok.location, message);
            }

            //return tok;
        }

        [SkipInStackFrame]
        private Token ConsumeOptionalToken(TokenType type, bool skipNewLines)
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (tok.type == TokenType.EOF)
                    return tok;

                if (skipNewLines && tok.type == TokenType.NewLine)
                {
                    NextToken();
                    continue;
                }

                if (tok.type == type)
                {
                    NextToken();
                    return tok;
                }

                return null;
            }
        }

        [SkipInStackFrame]
        private Token PeekToken(bool skipNewLines = false)
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (tok.type == TokenType.EOF)
                    return tok;

                if (skipNewLines && tok.type == TokenType.NewLine)
                {
                    NextToken();
                    continue;
                }

                return tok;
            }
        }

        [SkipInStackFrame]
        private Token ReadToken(bool SkipNewLines)
        {
            while (true)
            {
                var tok = NextToken();

                if (tok.type == TokenType.EOF)
                    return tok;

                if (SkipNewLines && tok.type == TokenType.NewLine)
                {
                    continue;
                }

                return tok;
            }
        }

        public (bool done, PTStatement stmt) ParseStatement()
        {
            SkipNewlines();
            var token = PeekToken(true);
            switch (token.type)
            {
                case TokenType.EOF:
                    return (true, null);

                case TokenType.HashTag:
                    return (false, new PTDirectiveStatement(ParseDirective(false)));

                case TokenType.KwReturn:
                    return (false, ParseReturnStatement());
                case TokenType.KwFn:
                    return (false, ParseFunctionDeclaration());
                case TokenType.KwVar:
                    return (false, ParseVariableDeclaration(TokenType.ClosingBrace));
                case TokenType.KwType:
                    return (false, ParseTypeAliasStatement());
                case TokenType.KwIf:
                    return (false, ParseIfStatement());
                case TokenType.KwWhile:
                    return (false, ParseWhileStatement());
                case TokenType.KwEnum:
                    return (false, ParseEnumDeclaration());
                case TokenType.KwStruct:
                    return (false, ParseTypeDeclaration());
                case TokenType.KwImpl:
                    return (false, ParseImplBlock());
                case TokenType.OpenBrace:
                    return (false, ParseBlockStatement());

                case TokenType.KwUsing:
                    return (false, ParseUsingStatement());

                default:
                    {
                        var expr = ParseExpression();
                        if (PeekToken(skipNewLines: false).type == TokenType.Equal)
                        {
                            NextToken();
                            var val = ParseExpression();
                            return (false, new PTAssignment(expr.Beginning, val.End, expr, val));
                        }
                        else
                        {
                            return (false, new PTExprStmt(expr.Beginning, expr.End, expr));
                        }
                    }
            }
        }

        private PTStatement ParseEnumDeclaration()
        {
            var beginning = Expect(TokenType.KwEnum, true).location;

            var name = ParseIdentifierExpr(true);
            if (!(name is PTIdentifierExpr))
                return null;

            var members = new List<PTEnumMember>();
            Expect(TokenType.OpenBrace, true);

            while (PeekToken(true).type != TokenType.ClosingBrace)
            {
                var mn = ParseIdentifierExpr(true) as PTIdentifierExpr;
                if (mn == null)
                    continue;

                members.Add(new PTEnumMember(mn, null));

                var next = PeekToken(false);

                if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                {
                    NextToken();
                }
                else if (next.type == TokenType.ClosingBrace)
                {
                    break;
                }
                else
                {
                    ReportError(next.location, $"Expected either comma, new line or closing brace after enum member, got '{next.data}'");
                }
            }

            var end = Expect(TokenType.ClosingBrace, true).location;

            return new PTEnumDecl(beginning, end, name as PTIdentifierExpr, members, null);
        }

        private PTStatement ParseUsingStatement()
        {
            var beginning = Expect(TokenType.KwUsing, true).location;

            var expr = ParseExpression();

            return new PTUsingStatement(beginning, expr);
        }

        private PTReturnStmt ParseReturnStatement()
        {
            Expect(out Token beg, TokenType.KwReturn, skipNewLines: true);
            PTExpr returnValue = null;

            if (PeekToken(skipNewLines: false).type != TokenType.NewLine)
            {
                returnValue = ParseExpression();
            }

            ConsumeNewLine();

            return new PTReturnStmt(beg.location, returnValue);
        }

        private PTDirective ParseDirective(bool skip)
        {
            var beginning = Expect(TokenType.HashTag, skip).location;
            var name = ParseIdentifierExpr(false, t => "Expected identifier after hashtag", t => beginning) as PTIdentifierExpr;
            if (name == null)
                return null;

            var end = name.End;
            var args = new List<PTExpr>();

            if (PeekToken(false).type == TokenType.OpenParen)
            {
                NextToken();

                while (true)
                {
                    var next = PeekToken(true);
                    if (next.type == TokenType.ClosingParen)
                    {
                        break;
                    }

                    var expr = ParseExpression();
                    if (expr != null)
                        args.Add(expr);

                    if (PeekToken(true).type == TokenType.Comma)
                    {
                        NextToken();
                        continue;
                    }

                    break;
                }

                Consume(TokenType.ClosingParen, true);
            }

            return new PTDirective(beginning, end, name, args);
        }

        private PTTypeDecl ParseTypeDeclaration()
        {
            var members = new List<PTMemberDecl>();
            Expect(out Token beginnig, TokenType.KwStruct, true);
            var name = ParseIdentifierExpr(true) as PTIdentifierExpr;
            if (name == null)
                return null;

            List<PTDirective> directives = new List<PTDirective>();

            while (PeekToken(false).type == TokenType.HashTag)
            {
                var dir = ParseDirective(false);
                if (dir != null)
                    directives.Add(dir);
            }

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                var mName = ParseIdentifierExpr(true) as PTIdentifierExpr;
                if (mName == null)
                    return null;

                Expect(TokenType.Colon, skipNewLines: true);
                var mType = ParseTypeExpression();
                if (PeekToken(skipNewLines: false).type != TokenType.ClosingBrace)
                    Expect(TokenType.NewLine, skipNewLines: false);

                members.Add(new PTMemberDecl(mName, mType));
            }

            Expect(out Token end, TokenType.ClosingBrace, skipNewLines: true);

            return new PTTypeDecl(beginnig.location, end.location, name, members, directives);
        }

        private PTImplBlock ParseImplBlock()
        {
            var functions = new List<PTFunctionDecl>();
            var beginnig = Expect(TokenType.KwImpl, skipNewLines: true);
            var target = ParseTypeExpression();

            Expect(TokenType.OpenBrace, skipNewLines: true);

            while (PeekToken(skipNewLines: true).type != TokenType.ClosingBrace)
            {
                bool isRef = false;
                if (PeekToken(true).type == TokenType.KwRef)
                {
                    isRef = true;
                    NextToken();
                }

                var f = ParseFunctionDeclaration();
                f.RefSelf = isRef;
                functions.Add(f);
            }

            var end = Expect(TokenType.ClosingBrace, skipNewLines: true);

            return new PTImplBlock(beginnig.location, end.location, target, functions);
        }

        private PTBlockStmt ParseBlockStatement()
        {
            List<PTStatement> statements = new List<PTStatement>();
            var beginning = Expect(TokenType.OpenBrace, skipNewLines: true);

            while (true)
            {
                var next = PeekToken(skipNewLines: true);
                if (next.type == TokenType.ClosingBrace)
                    break;
                if (next.type == TokenType.EOF)
                {
                    ReportError(next.location, "Unexpected end of file in block statement");
                    return null;
                }

                var s = ParseStatement();
                if (s.stmt != null)
                    statements.Add(s.stmt);
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
            var beginning = Expect(TokenType.KwVar, skipNewLines: true).location;
            List<PTDirective> directives = new List<PTDirective>();

            while (PeekToken(true).type == TokenType.HashTag)
            {
                directives.Add(ParseDirective(true));
            }

            var name = ParseIdentifierExpr(true, t => $"Expected identifier after 'let' in variable declaration", t => beginning) as PTIdentifierExpr;
            if (name == null)
            {
                RecoverStatement();
                return null;
            }

            TokenLocation end = name.End;

            var next = PeekToken();

            PTTypeExpr type = null;
            PTExpr init = null;
            switch (next.type)
            {
                case TokenType.Colon:
                    NextToken();
                    SkipNewlines();
                    type = ParseTypeExpression();
                    if (type == null)
                    {
                        RecoverStatement();
                        return null;
                    }
                    next = PeekToken();
                    end = type.End;
                    if (next.type == TokenType.Equal)
                        goto case TokenType.Equal;
                    else if (delimiters.Contains(next.type))
                    {
                        end = next.location;
                        break;
                    }
                    else if (next.type == TokenType.NewLine)
                    {
                        NextToken();
                        break;
                    }
                    goto default;

                case TokenType.Equal:
                    NextToken();
                    SkipNewlines();
                    init = ParseExpression(errorMessage: t => $"Expected expression after '='");
                    if (init == null)
                    {
                        RecoverStatement();
                        return null;
                    }
                    next = PeekToken();
                    end = init.End;
                    if (delimiters.Contains(next.type))
                    {
                        end = next.location;
                        break;
                    }
                    if (next.type == TokenType.NewLine || next.type == TokenType.EOF)
                    {
                        NextToken();
                        break;
                    }
                    goto default;

                case TokenType ttt when delimiters.Contains(ttt):
                    break;
                case TokenType.Semicolon:
                case TokenType.NewLine:
                    NextToken();
                    break;

                default:
                    ReportError(next.location, $"Unexpected token after variable declaration: ({next.type}) {next.data}");
                    break;
            }

            return new PTVariableDecl(beginning, end, name, type, init, directives);
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

            PTExpr condition = ParseExpression(errorMessage: t => $"Failed to parse while loop condition");

            if (PeekToken(false).type == TokenType.Comma)
            {
                NextToken();
                var s = ParseStatement();
                postAction = s.stmt;
            }

            PTStatement body = ParseBlockStatement();
            if (body == null)
            {
                RecoverStatement();
                return null;
            }

            return new PTWhileStmt(beginning.location, body.End, condition, body, varDecl, postAction);
        }

        private PTIfStmt ParseIfStatement()
        {
            PTExpr condition = null;
            PTStatement ifCase = null;
            PTStatement elseCase = null;

            var beginning = Expect(TokenType.KwIf, skipNewLines: true).location;
            TokenLocation end = beginning;

            condition = ParseExpression(errorMessage: t => $"Failed to parse if statement condition");

            ifCase = ParseBlockStatement();
            if (ifCase == null)
            {
                ifCase = new PTErrorStmt(beginning, "Failed to parse if case");
            }

            end = ifCase.End;

            if (PeekToken(skipNewLines: true).type == TokenType.KwElse)
            {
                NextToken();

                if (PeekToken(skipNewLines: false).type == TokenType.KwIf)
                    elseCase = ParseIfStatement();
                else
                    elseCase = ParseBlockStatement();
                end = elseCase.End;
            }

            return new PTIfStmt(beginning, end, condition, ifCase, elseCase);
        }

        private PTTypeAliasDecl ParseTypeAliasStatement()
        {
            var beg = Expect(TokenType.KwType, true).location;
            
            var name = ParseIdentifierExpr(true) as PTIdentifierExpr;
            if (name == null)
                return null;
            
            if (!Consume(TokenType.Equal, true))
            {
                return null;
            }

            var type = ParseTypeExpression();

            return new PTTypeAliasDecl(beg, type.End, name, type);
        }

        private PTFunctionDecl ParseFunctionDeclaration()
        {
            var beginning = Expect(TokenType.KwFn, skipNewLines: true).location;

            //bool refSelf = false;
            //if (PeekToken(true).type == TokenType.Ampersand)
            //{
            //}

            var nameExpr = ParseIdentifierExpr(true, t => $"Expected identifier at beginnig of function declaration, got ({t.type}) {t.data}");
            if (nameExpr is PTErrorExpr)
            {
                return null;
            }
            var name = nameExpr as PTIdentifierExpr;

            List<PTStatement> statements = new List<PTStatement>();
            List<PTFunctionParam> parameters = new List<PTFunctionParam>();
            List<PTDirective> directives = new List<PTDirective>();
            PTTypeExpr returnType = null;
            TokenLocation end = name.End;

            // parameters
            if (!Consume(TokenType.OpenParen, true))
                return null;

            while (true)
            {
                var token = PeekToken(true);
                if (token.type == TokenType.ClosingParen || token.type == TokenType.EOF)
                    break;

                var pname = ParseIdentifierExpr(true) as PTIdentifierExpr;
                if (pname == null)
                {
                    RecoverExpression();
                    return null;
                }

                Expect(TokenType.Colon, true);
                var tname = ParseTypeExpression();
                if (tname == null)
                {
                    RecoverExpression();
                    continue;
                }

                parameters.Add(new PTFunctionParam(pname.Beginning, tname.End, pname, tname));

                var next = PeekToken(true);
                if (next.type == TokenType.Comma)
                    NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                    throw new Exception($"Expected comma or closing paren, got {next.data} ({next.type})");
            }
            if (!Expect(out end, TokenType.ClosingParen, skipNewLines: true))
                return null;

            // return type
            if (PeekToken(skipNewLines: false).type == TokenType.Arrow)
            {
                NextToken();
                returnType = ParseTypeExpression();
                if (returnType != null)
                    end = returnType.End;
            }

            while (PeekToken(false).type == TokenType.HashTag)
            {
                directives.Add(ParseDirective(false));
            }

            if (PeekToken(false).type == TokenType.Semicolon)
            {
                NextToken();
                return new PTFunctionDecl(beginning, end, name, parameters, returnType, directives: directives);
            }

            // implementation
            if (!Consume(TokenType.OpenBrace, skipNewLines: true))
                return null;

            while (true)
            {
                var token = PeekToken(true);

                if (token.type == TokenType.EOF)
                {
                    break;
                }

                if (token.type == TokenType.ClosingBrace)
                    break;

                var stmt = ParseStatement();
                if (stmt.stmt != null)
                    statements.Add(stmt.stmt);
            }

            if (!Expect(out end, TokenType.ClosingBrace, skipNewLines: true))
                return null;

            return new PTFunctionDecl(beginning, end, name, parameters, returnType, statements);
        }

        #region Expression Parsing

        private PTTypeExpr ParseFunctionTypeExpr()
        {
            var beginning = Expect(TokenType.KwFn, false).location;
            Consume(TokenType.OpenParen, true);

            List<PTTypeExpr> args = new List<PTTypeExpr>();
            if (PeekToken(true).type != TokenType.ClosingParen)
            {
                while (true)
                {
                    args.Add(ParseTypeExpression());

                    var next = PeekToken(true);
                    if (next.type == TokenType.Comma)
                        NextToken();
                    else if (next.type == TokenType.ClosingParen)
                        break;
                    else
                    {
                        ReportError(next.location, $"Failed to parse function type, expected comma or closing paren, got {next.data} ({next.type})");
                        RecoverExpression();
                        return null;
                    }
                }
            }

            var end = Expect(TokenType.ClosingParen, true).location;
            PTTypeExpr returnType = null;
            if (PeekToken(false).type == TokenType.Colon)
            {
                NextToken();
                returnType = ParseTypeExpression();
                end = returnType.End;
            }

            return new PTFunctionTypeExpr(beginning, end, returnType, args);
        }

        private PTTypeExpr ParseTypeExpression()
        {
            PTTypeExpr type = null;
            bool cond = true;
            while (cond)
            {
                var next = PeekToken();
                switch (next.type)
                {
                    case TokenType.KwFn:
                        return ParseFunctionTypeExpr();

                    case TokenType.Identifier:
                        NextToken();
                        type = new PTNamedTypeExpr(next.location, next.location, (string)next.data);
                        break;

                    case TokenType.Asterisk:
                        NextToken();
                        if (type == null)
                        {
                            ReportError(next.location, "Failed to parse type expression: * must be preceded by an actual type");
                            RecoverExpression();
                            type = new PTErrorTypeExpr(next.location, GetCodeLocation() + " TODO");
                        }
                        type = new PTPointerTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.OpenBracket:
                        if (type == null)
                        {
                            ReportError(next.location, "Failed to parse type expression: [] must be preceded by an actual type");
                            RecoverExpression();
                            type = new PTErrorTypeExpr(next.location, GetCodeLocation() + " TODO");
                        }
                        NextToken();
                        next = Expect(TokenType.ClosingBracket, skipNewLines: true);
                        type = new PTArrayTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.NewLine when type == null:
                        {
                            ReportError(next.location, "Expected type expression, found new line");
                            RecoverExpression();
                            return new PTErrorTypeExpr(next.location, GetCodeLocation() + " TODO");
                        }
                    case TokenType.EOF when type == null:
                        {
                            ReportError(next.location, "Expected type expression, found end of file");
                            RecoverExpression();
                            return new PTErrorTypeExpr(next.location, GetCodeLocation() + " TODO");
                        }

                    case TokenType t when type == null:
                        {
                            ReportError(next.location, $"Unexpected token in type expression: {next}");
                            RecoverExpression();
                            return new PTErrorTypeExpr(next.location, GetCodeLocation() + " TODO");
                        }

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
            
            return ParseOrExpression(location, errorMessage);
        }

        [DebuggerStepThrough]
        private PTExpr ParseOrExpression(LocationResolver l, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAndExpression, l, e,
                (TokenType.KwOr, "or"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseAndExpression(LocationResolver l, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseComparisonExpression, l, e,
                (TokenType.KwAnd, "and"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseComparisonExpression(LocationResolver l, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAddSubExpression, l, e,
                (TokenType.Less, "<"),
                (TokenType.LessEqual, "<="),
                (TokenType.Greater, ">"),
                (TokenType.GreaterEqual, ">="),
                (TokenType.DoubleEqual, "=="),
                (TokenType.NotEqual, "!="));
        }

        [DebuggerStepThrough]
        private PTExpr ParseAddSubExpression(LocationResolver l, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseMulDivExpression, l, e,
                (TokenType.Plus, "+"),
                (TokenType.Minus, "-"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseMulDivExpression(LocationResolver l, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAddressOfOrDerefExpression, l, e,
                (TokenType.Asterisk, "*"),
                (TokenType.ForwardSlash, "/"),
                (TokenType.Percent, "%"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseBinaryLeftAssociativeExpression(ExpressionParser sub, LocationResolver location, ErrorMessageResolver errorMessage, params (TokenType, string)[] types)
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

        private PTExpr ParseLeftAssociativeExpression(ExpressionParser sub, LocationResolver location, ErrorMessageResolver errorMessage, Func<TokenType, string> tokenMapping)
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

                NextToken();
                rhs = sub(location, errorMessage);
                lhs = new PTBinaryExpr(lhs.Beginning, rhs.End, op, lhs, rhs);
            }
        }

        private PTExpr ParseAddressOfOrDerefExpression(LocationResolver location, ErrorMessageResolver errorMessage = null)
        {
            var next = PeekToken(false);
            if (next.type == TokenType.Ampersand)
            {
                NextToken();
                var sub = ParseAddressOfOrDerefExpression(location, errorMessage);
                return new PTAddressOfExpr(next.location, sub.End, sub);
            }
            else if (next.type == TokenType.Asterisk)
            {
                NextToken();
                var sub = ParseAddressOfOrDerefExpression(location, errorMessage);
                return new PTDereferenceExpr(next.location, sub.End, sub);
            }
            else if (next.type == TokenType.Minus || next.type == TokenType.Plus)
            {
                NextToken();
                var sub = ParseAddressOfOrDerefExpression(location, errorMessage);
                string op = "";
                switch (next.type)
                {
                    case TokenType.Plus: op = "+"; break;
                    case TokenType.Minus: op = "-"; break;
                }
                return new PTUnaryExpr(next.location, sub.End, op, sub);
            }

            return ParseCallOrDotExpression(location, errorMessage);
        }

        private PTExpr ParseEmptyExpression()
        {
            var loc = GetWhitespaceLocation();
            return new PTErrorExpr(loc.beg, loc.end);
        }

        private PTExpr ParseCallOrDotExpression(LocationResolver location, ErrorMessageResolver errorMessage)
        {
            var expr = ParseAtomicExpression(location, errorMessage);

            while (true)
            {
                switch (PeekToken(false).type)
                {
                    case TokenType.OpenParen:
                        {
                            NextToken();
                            List<PTExpr> args = new List<PTExpr>();
                            if (PeekToken(true).type != TokenType.ClosingParen)
                            {
                                while (true)
                                {
                                    var next = PeekToken(true);
                                    args.Add(ParseExpression(location));

                                    next = PeekToken(true);
                                    if (next.type == TokenType.Comma)
                                        NextToken();
                                    else if (next.type == TokenType.ClosingParen)
                                        break;
                                    else
                                    {
                                        ReportError(next.location, $"Failed to parse function call, expected comma or closing paren, got {next.data} ({next.type})");
                                        RecoverExpression();
                                        return expr;
                                    }
                                }
                            }
                            var end = Expect(TokenType.ClosingParen, true).location;

                            expr = new PTCallExpr(expr.Beginning, end, expr, args);
                        }
                        break;

                    case TokenType.OpenBracket:
                        {
                            NextToken();
                            var index = ParseExpression(location, errorMessage);
                            var end = Expect(TokenType.ClosingBracket, true, customLocation: location).location;
                            expr = new PTArrayAccessExpr(expr.Beginning, end, expr, index);
                        }
                        break;

                    case TokenType.Period:
                        {
                            NextToken();
                            var right = ParseIdentifierExpr(true, t => $"Right side of '.' has to be an identifier", location);

                            if (right is PTErrorExpr)
                            {
                                RecoverExpression();
                                return right;
                            }

                            expr = new PTDotExpr(expr.Beginning, right.End, expr, right as PTIdentifierExpr, false);
                            break;
                        }

                    case TokenType.DoubleColon:
                        {
                            NextToken();
                            var right = ParseIdentifierExpr(true, t => $"Right side of '::' has to be an identifier", location);

                            if (right is PTErrorExpr)
                            {
                                RecoverExpression();
                                return right;
                            }

                            expr = new PTDotExpr(expr.Beginning, right.End, expr, right as PTIdentifierExpr, true);
                            break;
                        }

                    default:
                        return expr;
                }
            }
        }

        private PTExpr ParseStructValue(PTIdentifierExpr name)
        {
            Consume(TokenType.OpenBrace, false);

            List<PTStructMemberInitialization> members = new List<PTStructMemberInitialization>();


            while (true)
            {
                var next = PeekToken(true);

                if (next.type == TokenType.ClosingBrace)
                {
                    break;
                }

                var value = ParseExpression();
                PTIdentifierExpr memberName = null;
                if (value is PTIdentifierExpr n && PeekToken(false).type == TokenType.Equal)
                {
                    memberName = n;
                    NextToken();
                    value = ParseExpression();
                }

                members.Add(new PTStructMemberInitialization
                {
                    Name = memberName,
                    Value = value
                });

                next = PeekToken(false);
                if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                {
                    NextToken();
                }
                else if (next.type == TokenType.ClosingBrace)
                {
                    break;
                }
                else
                {
                    ReportError(next.location, $"Unexpected token '{next.data}', expected comma, new line or closing brace '{{'");
                }
            }

            var end = Expect(TokenType.ClosingBrace, true).location;

            return new PTStructValueExpr(name.Beginning, end, name, members);
        }

        private PTExpr ParseAtomicExpression(LocationResolver location, ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.Identifier:
                    {
                        NextToken();
                        var id = new PTIdentifierExpr(token.location, (string)token.data);
                        if (PeekToken(false).type == TokenType.OpenBrace)
                        {
                            return ParseStructValue(id); 
                        }
                        return id;
                    }

                case TokenType.StringLiteral:
                    NextToken();
                    return new PTStringLiteral(token.location, (string)token.data);

                case TokenType.NumberLiteral:
                    NextToken();
                    return new PTNumberExpr(token.location, (NumberData)token.data);

                case TokenType.KwTrue:
                    NextToken();
                    return new PTBoolExpr(token.location, true);

                case TokenType.KwFalse:
                    NextToken();
                    return new PTBoolExpr(token.location, false);

                case TokenType.Less:
                    {
                        NextToken();
                        var type = ParseTypeExpression();
                        Consume(TokenType.Greater, false);
                        var e = ParseAddressOfOrDerefExpression(location, errorMessage);
                        return new PTCastExpr(token.location, e.End, type, e);
                    }
                case TokenType.KwCast:
                    {
                        NextToken();
                        Expect(TokenType.Less, true);
                        var type = ParseTypeExpression();

                        Expect(TokenType.Greater, true);
                        Expect(TokenType.OpenParen, true);
                        var s = ParseExpression(location, errorMessage);
                        var end = Expect(TokenType.ClosingParen, true).location;
                        return new PTCastExpr(token.location, end, type, s);
                    }

                case TokenType.OpenParen:
                    NextToken();
                    SkipNewlines();
                    var sub = ParseExpression(location, errorMessage);
                    sub.Beginning = token.location;
                    sub.End = Expect(TokenType.ClosingParen, skipNewLines: true, customErrorMessage: t => $"Expected closing paren ')' at end of group expression, got ({t.type}) {t.data}").location;
                    return sub;

                default:
                    RecoverExpression();
                    ReportError(location?.Invoke(token) ?? token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
                    return ParseEmptyExpression();
            }
        }

        private PTExpr ParseIdentifierExpr(bool SkipNewLines, ErrorMessageResolver customErrorMessage = null, LocationResolver customLocation = null)
        {
            if (!Expect(out Token t, TokenType.Identifier, SkipNewLines, customErrorMessage, customLocation))
            {
                RecoverExpression();
                return new PTErrorExpr(PeekToken().location, reason: GetCodeLocation() + " TODO");
            }
            return new PTIdentifierExpr(t.location, (string)t.data);
        }
        #endregion

    }
}
