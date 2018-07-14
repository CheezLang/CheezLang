using Cheez.Compiler.ParseTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Compiler.Parsing
{
    public class SkipInStackFrame : Attribute
    { }

    public class Parser
    {
        private delegate string ErrorMessageResolver(Token t);
        private delegate PTExpr ExpressionParser(ErrorMessageResolver e);

        private Lexer mLexer;
        private IErrorHandler mErrorHandler;

        private Token lastNonWhitespace = null;
        private Token mCurrentToken = null;
        private Token CurrentToken => mCurrentToken;

        public Parser(Lexer lex, IErrorHandler errHandler)
        {
            mLexer = lex;
            mErrorHandler = errHandler;
        }

        [DebuggerStepThrough]
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
        [DebuggerStepThrough]
        private void SkipNewlines()
        {
            while (true)
            {
                var tok = mLexer.PeekToken();

                if (tok.type == TokenType.EOF)
                    break;

                if (tok.type == TokenType.NewLine)
                {
                    NextToken();
                    continue;
                }

                break;
            }
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private Token NextToken()
        {
            mCurrentToken = mLexer.NextToken();
            if (mCurrentToken.type != TokenType.NewLine)
                lastNonWhitespace = mCurrentToken;
            return mCurrentToken;
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
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
        [DebuggerStepThrough]
        private void ReportError(TokenLocation location, string message)
        {
            string callingFunctionFile, callingFunctionName;
            int callLineNumber;
            (callingFunctionFile, callingFunctionName, callLineNumber) = GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, new Location(location, location), message, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private void ReportError(ILocation location, string message)
        {
            string callingFunctionFile, callingFunctionName;
            int callLineNumber;
            (callingFunctionFile, callingFunctionName, callLineNumber) = GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, location, message, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [DebuggerStepThrough]
        private ErrorMessageResolver ErrMsg(string expect, string where = null)
        {
            return t => $"Expected {expect} {where}";
        }


        [SkipInStackFrame]
        [DebuggerStepThrough]
        private bool Expect(TokenType type, ErrorMessageResolver customErrorMessage)
        {
            var tok = PeekToken();

            if (tok.type != type)
            {
                string message = customErrorMessage?.Invoke(tok) ?? $"Unexpected Token ({tok.type}) {tok.data}, expected {type}";
                ReportError(tok.location, message);
                return false;
            }

            NextToken();
            return true;
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private Token Consume(TokenType type, ErrorMessageResolver customErrorMessage)
        {
            if (!Expect(type, customErrorMessage))
                NextToken();
            return CurrentToken;
        }

        [DebuggerStepThrough]
        private bool CheckToken(TokenType type)
        {
            var next = PeekToken();
            return next.type == type;
        }

        [DebuggerStepThrough]
        private bool CheckTokens(params TokenType[] types)
        {
            var next = PeekToken();
            foreach (var t in types)
            {
                if (next.type == t)
                    return true;
            }
            return false;
        }

        //[DebuggerStepThrough]
        private bool IsExprToken()
        {
            var next = PeekToken();
            switch (next.type)
            {
                case TokenType.KwNew:
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.OpenParen:
                case TokenType.StringLiteral:
                case TokenType.CharLiteral:
                case TokenType.NumberLiteral:
                case TokenType.KwNull:
                case TokenType.KwTrue:
                case TokenType.KwFalse:
                case TokenType.Ampersand:
                case TokenType.Identifier:
                    return true;

                default:
                    return false;
            }
        }

        [DebuggerStepThrough]
        private Token PeekToken()
        {
            return mLexer.PeekToken();
        }

        [SkipInStackFrame]
        private Token ReadToken(bool SkipNewLines = false)
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

        private void RecoverStatement()
        {
            while (true)
            {
                var next = PeekToken();
                switch (next.type)
                {
                    case TokenType.NewLine:
                        NextToken();
                        return;

                    case TokenType.ClosingBrace:
                        return;

                    case TokenType.EOF:
                        return;

                    default:
                        NextToken();
                        break;
                }
            }
        }

        public (bool done, PTStatement stmt) ParseStatement(bool expectNewline = true)
        {
            SkipNewlines();
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.EOF:
                    return (true, null);

                case TokenType.KwMatch:
                    return (false, ParseMatchStatement());

                case TokenType.KwBreak:
                    NextToken();
                    return (false, new PTBreakStmt(token.location));

                case TokenType.KwContinue:
                    NextToken();
                    return (false, new PTContinueStmt(token.location));

                case TokenType.HashIdentifier:
                    {
                        var dir = new PTDirectiveStatement(ParseDirective());
                        if (expectNewline && !Expect(TokenType.NewLine, ErrMsg("\\n", "after directive statement")))
                            RecoverStatement();
                        return (false, dir);
                    }

                case TokenType.KwDefer:
                    {
                        NextToken();
                        var next = PeekToken();
                        if (next.type == TokenType.NewLine || next.type == TokenType.EOF)
                        {
                            ReportError(token.location, "Expected statement after keyword 'defer'");
                            return (false, null);
                        }

                        var s = ParseStatement(expectNewline);
                        if (s.stmt != null)
                            return (false, new PTDeferStatement(token.location, s.stmt));

                        return (false, null);
                    }

                case TokenType.KwReturn:
                    return (false, ParseReturnStatement());
                case TokenType.KwFn:
                    return (false, ParseFunctionDeclaration());
                case TokenType.KwLet:
                    return (false, ParseVariableDeclaration(TokenType.ClosingBrace));
                case TokenType.KwIf:
                    return (false, ParseIfStatement());
                case TokenType.KwWhile:
                    return (false, ParseWhileStatement());
                case TokenType.KwEnum:
                    return (false, ParseEnumDeclaration());
                case TokenType.KwStruct:
                    return (false, ParseStructDeclaration());
                case TokenType.KwImpl:
                    return (false, ParseImplBlock());
                case TokenType.KwTrait:
                    return (false, ParseTraitDeclaration());
                case TokenType.OpenBrace:
                    return (false, ParseBlockStatement());

                case TokenType.KwUsing:
                    return (false, ParseUsingStatement());

                default:
                    {
                        var expr = ParseExpression();
                        if (expr is PTErrorExpr)
                        {
                            NextToken();
                            return (false, null);
                        }
                        if (CheckTokens(TokenType.Equal, TokenType.AddEq, TokenType.SubEq, TokenType.MulEq, TokenType.DivEq, TokenType.ModEq))
                        {
                            var x = NextToken().type;
                            string op = null;
                            switch (x)
                            {
                                case TokenType.AddEq: op = "+"; break;
                                case TokenType.SubEq: op = "-"; break;
                                case TokenType.MulEq: op = "*"; break;
                                case TokenType.DivEq: op = "/"; break;
                                case TokenType.ModEq: op = "%"; break;
                            }
                            SkipNewlines();
                            var val = ParseExpression();
                            if (expectNewline && !Expect(TokenType.NewLine, ErrMsg("\\n", "after assignment")))
                                RecoverStatement();
                            return (false, new PTAssignment(expr.Beginning, val.End, expr, val, op));
                        }
                        else
                        {
                            if (expectNewline && !Expect(TokenType.NewLine, ErrMsg("\\n", "after expression statement")))
                                RecoverStatement();
                            return (false, new PTExprStmt(expr.Beginning, expr.End, expr));
                        }
                    }
            }
        }

        private PTStatement ParseTraitDeclaration()
        {
            TokenLocation beg = null, end = null;
            PTIdentifierExpr name = null;
            var functions = new List<PTFunctionDecl>();
            var parameters = new List<PTParameter>();

            beg = Consume(TokenType.KwTrait, ErrMsg("keyword 'trait'", "at beginning of trait declaration")).location;
            SkipNewlines();

            name = ParseIdentifierExpr(ErrMsg("name", "after keyword 'trait'"));
            SkipNewlines();

            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList();
                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "after name of trait"));
            SkipNewlines();

            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                functions.Add(ParseFunctionDeclaration());
                SkipNewlines();
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of trait")).location;

            return new PTTraitDeclaration(beg, end, name, parameters, functions);
        }

        private PTStatement ParseMatchStatement()
        {
            TokenLocation beg = null, end = null;
            PTExpr value = null;
            List<PTMatchCase> cases = new List<PTMatchCase>();

            beg = Consume(TokenType.KwMatch, ErrMsg("keyword 'match'", "at beginning of match statement")).location;
            SkipNewlines();

            value = ParseExpression();
            SkipNewlines();

            Consume(TokenType.OpenBrace, ErrMsg("{", "after value in match statement"));

            while (true)
            {
                SkipNewlines();
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var v = ParseExpression();

                Consume(TokenType.Arrow, ErrMsg("->", "after value in match case"));

                var s = ParseStatement(false);

                if (s.stmt != null)
                {
                    cases.Add(new PTMatchCase(v.Beginning, s.stmt.End, v, s.stmt));
                }

                next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                {
                    break;
                }
                else if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                {
                    NextToken();
                }
                else
                {
                    ReportError(next.location, $"Unexpected token after match case (found {next.type}, wanted '\n' or ',')");
                }
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of match statement")).location;

            return new PTMatchStmt(beg, end, value, cases);
        }

        private PTStatement ParseEnumDeclaration()
        {
            TokenLocation beginning = null, end = null;
            PTIdentifierExpr name;
            var members = new List<PTEnumMember>();

            beginning = NextToken().location;
            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'enum'"));

            SkipNewlines();
            Consume(TokenType.OpenBrace, ErrMsg("{", "after name in enum declaration"));

            while (true)
            {
                SkipNewlines();

                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var memberName = ParseIdentifierExpr(ErrMsg("identifier", "at enum member"));

                members.Add(new PTEnumMember(memberName, null));

                next = PeekToken();
                if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                {
                    NextToken();
                }
                else if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                {
                    break;
                }
                else
                {
                    NextToken();
                    ReportError(next.location, $"Expected either ',' or '\n' or '}}' after enum member");
                }
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}")).location;
            return new PTEnumDecl(beginning, end, name, members, null);
        }

        private PTStatement ParseUsingStatement()
        {
            var beg = Consume(TokenType.KwUsing, ErrMsg("keyword 'using'", "at beginning of using statement")).location;
            SkipNewlines();
            var expr = ParseExpression(ErrMsg("expression", "after keyword 'using'"));
            //if (!Expect(TokenType.NewLine, ErrMsg("\\n", "after using statement")))
            //    RecoverStatement();

            return new PTUsingStatement(beg, expr);
        }

        private PTReturnStmt ParseReturnStatement()
        {
            var beg = Consume(TokenType.KwReturn, ErrMsg("keyword 'return'", "at beginning of return statement")).location;
            PTExpr returnValue = null;

            if (IsExprToken())
            {
                returnValue = ParseExpression();
            }

            //if (!Expect(TokenType.NewLine, ErrMsg("\\n", "at end of return statement")))
            //    RecoverStatement();

            return new PTReturnStmt(beg, returnValue);
        }

        private PTDirective ParseDirective()
        {
            TokenLocation end = null;
            var args = new List<PTExpr>();

            var name = ParseIdentifierExpr(ErrMsg("identifier", "after # in directive"), TokenType.HashIdentifier);

            end = name.End;

            if (CheckToken(TokenType.OpenParen))
            {
                NextToken();
                SkipNewlines();

                while (true)
                {
                    var next = PeekToken();
                    if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                        break;

                    var expr = ParseExpression();
                    args.Add(expr);
                    SkipNewlines();

                    next = PeekToken();

                    if (next.type == TokenType.Comma)
                    {
                        NextToken();
                        SkipNewlines();
                        continue;
                    }

                    break;
                }

                end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of directive")).location;
            }

            return new PTDirective(end, name, args);
        }

        private List<PTParameter> ParseParameterList()
        {
            var parameters = new List<PTParameter>();

            Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of parameter list"));
            SkipNewlines();

            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                PTIdentifierExpr pname = null;
                PTExpr ptype = null;

                if (next.type != TokenType.Colon)
                {
                    pname = ParseIdentifierExpr(ErrMsg("identifier"));
                    SkipNewlines();
                }

                Consume(TokenType.Colon, ErrMsg(":", "after name in parameter list"));
                SkipNewlines();

                ptype = ParseExpression();

                parameters.Add(new PTParameter(pname.Beginning, pname, ptype));

                SkipNewlines();
                next = PeekToken();
                if (next.type == TokenType.Comma)
                    NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                {
                    NextToken();
                    SkipNewlines();
                    ReportError(next.location, "Expected ',' or ')'");
                }
            }

            Consume(TokenType.ClosingParen, ErrMsg(")", "at end of parameter list"));

            return parameters;
        }

        private PTStructDecl ParseStructDeclaration()
        {
            TokenLocation beg = null, end = null;
            var members = new List<PTMemberDecl>();
            List<PTDirective> directives = new List<PTDirective>();
            PTIdentifierExpr name = null;
            List<PTParameter> parameters = null;

            beg = Consume(TokenType.KwStruct, ErrMsg("keyword 'struct'", "at beginning of struct declaration")).location;
            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'struct'"));
            SkipNewlines();

            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList();
                SkipNewlines();
            }


            while (CheckToken(TokenType.HashIdentifier))
            {
                var dir = ParseDirective();
                if (dir != null)
                    directives.Add(dir);
                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "at beginning of struct body"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var mName = ParseIdentifierExpr(ErrMsg("identifier", "in struct member"));
                SkipNewlines();
                Consume(TokenType.Colon, ErrMsg(":", "after struct member name"));
                SkipNewlines();

                var mType = ParseExpression();

                next = PeekToken();
                if (next.type == TokenType.NewLine)
                {
                    SkipNewlines();
                }
                else if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                {
                    break;
                }
                else
                {
                    NextToken();
                    ReportError(next.location, $"Unexpected token {next} at end of struct member");
                }

                members.Add(new PTMemberDecl(mName, mType));
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of struct declaration")).location;

            return new PTStructDecl(beg, end, name, parameters, members, directives);
        }

        private PTImplBlock ParseImplBlock()
        {
            TokenLocation beg = null, end = null;
            var functions = new List<PTFunctionDecl>();
            PTExpr target = null;
            PTExpr trait = null;

            beg = Consume(TokenType.KwImpl, ErrMsg("keyword 'impl'", "at beginning of impl statement")).location;
            SkipNewlines();

            target = ParseExpression();
            SkipNewlines();

            if (CheckToken(TokenType.KwFor))
            {
                NextToken();
                SkipNewlines();
                trait = target;
                target = ParseExpression();
                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "after type"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                bool isRef = false;
                if (CheckToken(TokenType.KwRef))
                {
                    isRef = true;
                    NextToken();
                    SkipNewlines();
                }

                var f = ParseFunctionDeclaration();
                f.RefSelf = isRef;
                functions.Add(f);

                SkipNewlines();
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of impl statement")).location;

            return new PTImplBlock(beg, end, target, trait, functions);
        }

        private PTBlockStmt ParseBlockStatement()
        {
            List<PTStatement> statements = new List<PTStatement>();
            var beg = Consume(TokenType.OpenBrace, ErrMsg("{", "at beginning of block statement")).location;

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var s = ParseStatement(false);
                if (s.stmt != null)
                {
                    statements.Add(s.stmt);

                    next = PeekToken();

                    if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                        break;

                    switch (s.stmt)
                    {
                        case PTIfStmt _:
                        case PTBlockStmt _:
                            break;

                        default:
                            if (!Expect(TokenType.NewLine, ErrMsg("\\n", "after statement")))
                                RecoverStatement();
                            break;
                    }

                }
                SkipNewlines();
            }

            var end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of block statement")).location;

            return new PTBlockStmt(beg, end, statements);
        }

        private PTExprStmt ParseExpressionStatement()
        {
            var expr = ParseExpression();
            //if (!Expect(TokenType.NewLine, ErrMsg("\\n", "after expression statement")))
            //    RecoverStatement();
            return new PTExprStmt(expr.Beginning, expr.End, expr);
        }

        private PTVariableDecl ParseVariableDeclaration(params TokenType[] delimiters)
        {
            TokenLocation beg = null, end = null;
            List<PTDirective> directives = new List<PTDirective>();
            PTIdentifierExpr name = null;
            PTExpr type = null;
            PTExpr init = null;

            beg = Consume(TokenType.KwLet, ErrMsg("keyword 'let'", "at beginning of variable declaration")).location;

            SkipNewlines();
            while (CheckToken(TokenType.HashIdentifier))
            {
                directives.Add(ParseDirective());
                SkipNewlines();
            }

            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'let'"));
            end = name.End;

            if (CheckToken(TokenType.Colon))
            {
                NextToken();
                SkipNewlines();
                type = ParseExpression();
                end = type.End;
            }

            if (CheckToken(TokenType.Equal))
            {
                NextToken();
                SkipNewlines();
                init = ParseExpression(ErrMsg("expression", "after '=' in variable declaration"));
                end = init.End;
            }

            //if (!Expect(TokenType.NewLine, ErrMsg("\\n", "after variable declaration")))
            //    RecoverStatement();

            return new PTVariableDecl(beg, end, name, type, init, directives);
        }

        private PTWhileStmt ParseWhileStatement()
        {
            TokenLocation beg = null;
            PTExpr condition = null;
            PTBlockStmt body = null;
            PTVariableDecl init = null;
            PTStatement post = null;

            beg = Consume(TokenType.KwWhile, ErrMsg("keyword 'while'", "at beginning of while statement")).location;
            SkipNewlines();

            if (CheckToken(TokenType.KwLet))
            {
                init = ParseVariableDeclaration(TokenType.Semicolon);
                SkipNewlines();
                Consume(TokenType.Semicolon, ErrMsg(";", "after variable declaration in while statement"));
                SkipNewlines();
            }

            condition = ParseExpression(ErrMsg("expression", "after keyword 'while'"));
            SkipNewlines();

            if (CheckToken(TokenType.Semicolon))
            {
                NextToken();
                SkipNewlines();
                var p = ParseStatement(false);
                post = p.stmt;
                SkipNewlines();
            }

            body = ParseBlockStatement();

            return new PTWhileStmt(beg, condition, body, init, post);
        }

        private PTIfStmt ParseIfStatement()
        {
            TokenLocation beg = null, end = null;
            PTExpr condition = null;
            PTStatement ifCase = null;
            PTStatement elseCase = null;
            PTVariableDecl pre = null;

            beg = Consume(TokenType.KwIf, ErrMsg("keyword 'if'", "at beginning of if statement")).location;
            SkipNewlines();

            if (CheckToken(TokenType.KwLet))
            {
                pre = ParseVariableDeclaration(TokenType.Semicolon);
                SkipNewlines();
                Consume(TokenType.Semicolon, ErrMsg(";", "after variable declaration in if statement"));
                SkipNewlines();
            }

            condition = ParseExpression(ErrMsg("expression", "after keyword 'if'"));

            SkipNewlines();
            ifCase = ParseBlockStatement();
            end = ifCase.End;

            SkipNewlines();
            if (CheckToken(TokenType.KwElse))
            {
                NextToken();
                SkipNewlines();

                if (CheckToken(TokenType.KwIf))
                    elseCase = ParseIfStatement();
                else
                    elseCase = ParseBlockStatement();
                end = elseCase.End;
            }

            return new PTIfStmt(beg, end, condition, ifCase, elseCase, pre);
        }

        private PTFunctionDecl ParseFunctionDeclaration()
        {
            TokenLocation beginning = null, end = null;
            PTBlockStmt body = null;
            List<PTFunctionParam> parameters = new List<PTFunctionParam>();
            List<PTDirective> directives = new List<PTDirective>();
            List<PTIdentifierExpr> generics = new List<PTIdentifierExpr>();
            PTExpr returnType = null;

            beginning = NextToken().location;
            SkipNewlines();

            var name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'fn' in function declaration"));
            SkipNewlines();

            // generics
            if (CheckToken(TokenType.Less))
            {
                NextToken();
                SkipNewlines();

                while (true)
                {
                    var next = PeekToken();
                    if (next.type == TokenType.Greater || next.type == TokenType.EOF)
                        break;

                    var gname = ParseIdentifierExpr(null);
                    generics.Add(gname);
                    SkipNewlines();

                    next = PeekToken();

                    if (next.type == TokenType.Comma)
                    {
                        NextToken();
                        SkipNewlines();
                    }
                    else if (next.type == TokenType.Greater || next.type == TokenType.EOF)
                        break;
                    else
                    {
                        NextToken();
                        SkipNewlines();
                        ReportError(next.location, "Expected ',' or '>'");
                    }
                }

                Consume(TokenType.Greater, ErrMsg(">", "at end of generic parameter list"));
                SkipNewlines();
            }

            // parameters
            Consume(TokenType.OpenParen, ErrMsg("(", "after name in function declaration"));
            SkipNewlines();

            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                PTIdentifierExpr pname = null;
                PTExpr ptype = null;

                if (next.type != TokenType.Colon)
                    pname = ParseIdentifierExpr(ErrMsg("identifier"));

                SkipNewlines();
                Consume(TokenType.Colon, ErrMsg(":", "after name in parameter list"));

                SkipNewlines();
                ptype = ParseExpression();

                parameters.Add(new PTFunctionParam(pname.Beginning, ptype.End, pname, ptype));

                SkipNewlines();
                next = PeekToken();
                if (next.type == TokenType.Comma)
                    NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                {
                    NextToken();
                    SkipNewlines();
                    ReportError(next.location, "Expected ',' or ')'");
                }
            }
            Consume(TokenType.ClosingParen, ErrMsg(")", "at end of parameter list"));

            SkipNewlines();

            // return type
            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                SkipNewlines();
                returnType = ParseExpression();
                SkipNewlines();
            }

            while (CheckToken(TokenType.HashIdentifier))
            {
                directives.Add(ParseDirective());
                SkipNewlines();
            }

            if (CheckToken(TokenType.Semicolon))
            {
                end = NextToken().location;
            }
            else
            {
                // implementation
                body = ParseBlockStatement();
                //Consume(TokenType.OpenBrace, ErrMsg("{", "after header in function declaration"));

                //while (true)
                //{
                //    SkipNewlines();
                //    var token = PeekToken();

                //    if (token.type == TokenType.ClosingBrace || token.type == TokenType.EOF)
                //        break;

                //    var stmt = ParseStatement();
                //    if (stmt.stmt != null)
                //        statements.Add(stmt.stmt);
                //}

                //end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of function")).location;
            }

            return new PTFunctionDecl(beginning, end, name, generics, parameters, returnType, body, directives);
        }

        #region Expression Parsing

        private PTExpr ParseFunctionTypeExpr()
        {
            var beginning = Consume(TokenType.KwFn, ErrMsg("keyword 'fn'", "at beginning of function type")).location;
            SkipNewlines();

            Consume(TokenType.OpenParen, ErrMsg("(", "after keyword 'fn'"));
            SkipNewlines();

            List<PTExpr> args = new List<PTExpr>();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                args.Add(ParseExpression());
                SkipNewlines();

                next = PeekToken();
                if (next.type == TokenType.Comma)
                    NextToken();
                else if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;
                else
                {
                    ReportError(next.location, $"Failed to parse function type, expected comma or closing paren, got {next.data} ({next.type})");
                    NextToken();
                }
            }

            var end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of function type parameter list")).location;
            PTExpr returnType = null;
            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                returnType = ParseExpression();
                end = returnType.End;
            }

            return new PTFunctionTypeExpr(beginning, end, returnType, args);
        }

        private PTExpr ParseExpression(ErrorMessageResolver errorMessage = null)
        {
            errorMessage = errorMessage ?? (t => $"Unexpected token '{t}' in expression");

            return ParseOrExpression(errorMessage);
        }

        [DebuggerStepThrough]
        private PTExpr ParseOrExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAndExpression, e,
                (TokenType.KwOr, "or"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseAndExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseComparisonExpression, e,
                (TokenType.KwAnd, "and"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseComparisonExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAddSubExpression, e,
                (TokenType.Less, "<"),
                (TokenType.LessEqual, "<="),
                (TokenType.Greater, ">"),
                (TokenType.GreaterEqual, ">="),
                (TokenType.DoubleEqual, "=="),
                (TokenType.NotEqual, "!="));
        }

        [DebuggerStepThrough]
        private PTExpr ParseAddSubExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseMulDivExpression, e,
                (TokenType.Plus, "+"),
                (TokenType.Minus, "-"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseMulDivExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseUnaryExpression, e,
                (TokenType.Asterisk, "*"),
                (TokenType.ForwardSlash, "/"),
                (TokenType.Percent, "%"));
        }

        [DebuggerStepThrough]
        private PTExpr ParseBinaryLeftAssociativeExpression(ExpressionParser sub, ErrorMessageResolver errorMessage, params (TokenType, string)[] types)
        {
            return ParseLeftAssociativeExpression(sub, errorMessage, type =>
            {
                foreach (var (t, o) in types)
                {
                    if (t == type)
                        return o;
                }

                return null;
            });
        }

        private PTExpr ParseLeftAssociativeExpression(ExpressionParser sub, ErrorMessageResolver errorMessage, Func<TokenType, string> tokenMapping)
        {
            var lhs = sub(errorMessage);
            PTExpr rhs = null;

            while (true)
            {
                var next = PeekToken();

                var op = tokenMapping(next.type);
                if (op == null)
                {
                    return lhs;
                }

                NextToken();
                SkipNewlines();
                rhs = sub(errorMessage);
                lhs = new PTBinaryExpr(lhs.Beginning, rhs.End, op, lhs, rhs);
            }
        }

        private PTExpr ParseUnaryExpression(ErrorMessageResolver errorMessage = null)
        {
            var next = PeekToken();
            if (next.type == TokenType.Ampersand)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new PTAddressOfExpr(next.location, sub.End, sub);
            }
            else if (next.type == TokenType.LessLess)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new PTDereferenceExpr(next.location, sub.End, sub);
            }
            else if (next.type == TokenType.Minus || next.type == TokenType.Plus)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                string op = "";
                switch (next.type)
                {
                    case TokenType.Plus: op = "+"; break;
                    case TokenType.Minus: op = "-"; break;
                }
                return new PTUnaryExpr(next.location, sub.End, op, sub);
            }
            else if (next.type == TokenType.Bang)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new PTUnaryExpr(next.location, sub.End, "!", sub);
            }

            return ParsePostUnaryExpression(errorMessage);
        }

        private PTExpr ParsePostUnaryExpression(ErrorMessageResolver errorMessage)
        {
            var expr = ParseAtomicExpression(errorMessage);

            while (true)
            {
                switch (PeekToken().type)
                {
                    case TokenType.OpenParen:
                        {
                            NextToken();
                            SkipNewlines();
                            List<PTExpr> args = new List<PTExpr>();
                            while (true)
                            {
                                var next = PeekToken();
                                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                                    break;
                                args.Add(ParseExpression());
                                SkipNewlines();

                                next = PeekToken();
                                if (next.type == TokenType.Comma)
                                {
                                    NextToken();
                                    SkipNewlines();
                                }
                                else if (next.type == TokenType.ClosingParen)
                                    break;
                                else
                                {
                                    NextToken();
                                    ReportError(next.location, $"Failed to parse function call, expected ',' or ')'");
                                    //RecoverExpression();
                                }
                            }
                            var end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of function call")).location;

                            expr = new PTCallExpr(expr.Beginning, end, expr, args);
                        }
                        break;

                    case TokenType.OpenBracket:
                        {
                            NextToken();
                            SkipNewlines();

                            if (CheckToken(TokenType.ClosingBracket))
                            {
                                var c = NextToken();
                                expr = new PTArrayTypeExpr(expr.Beginning, c.location, expr);
                            }
                            else
                            {
                                var index = ParseExpression(errorMessage);
                                SkipNewlines();
                                var end = Consume(TokenType.ClosingBracket, ErrMsg("]", "at end of [] operator")).location;
                                expr = new PTArrayAccessExpr(expr.Beginning, end, expr, index);
                            }
                        }
                        break;

                    case TokenType.Period:
                        {
                            NextToken();
                            SkipNewlines();
                            var right = ParseIdentifierExpr(ErrMsg("identifier", "after ."));

                            expr = new PTDotExpr(expr.Beginning, right.End, expr, right, false);
                            break;
                        }

                    case TokenType.DoubleColon:
                        {
                            NextToken();
                            SkipNewlines();
                            var right = ParseIdentifierExpr(ErrMsg("identifier", "after ."));

                            expr = new PTDotExpr(expr.Beginning, right.End, expr, right, true);
                            break;
                        }

                    case TokenType.Ampersand:
                        {
                            NextToken();
                            expr = new PTPointerTypeExpr(expr.Beginning, CurrentToken.location, expr);
                            break;
                        }

                    default:
                        return expr;
                }
            }
        }

        private List<PTExpr> ParseArgumentList(out TokenLocation end)
        {
            Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of argument list"));

            List<PTExpr> args = new List<PTExpr>();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;
                args.Add(ParseExpression());
                SkipNewlines();

                next = PeekToken();
                if (next.type == TokenType.Comma)
                    NextToken();
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                {
                    NextToken();
                    ReportError(next.location, $"Failed to parse argument list, expected ',' or ')'");
                    //RecoverExpression();
                }
            }
            end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of argument list")).location;

            return args;
        }

        private PTExpr ParseAtomicExpression(ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.KwFn:
                    return ParseFunctionTypeExpr();

                case TokenType.KwNew:
                    return ParseStructValue();

                case TokenType.KwNull:
                    NextToken();
                    return new PTNullExpr(token.location);

                case TokenType.AtSignIdentifier:
                    {
                        NextToken();
                        var args = ParseArgumentList(out var end);
                        return new PTCompCallExpr(end, new PTIdentifierExpr(token.location, (string)token.data, false), args);
                    }

                case TokenType.OpenBracket:
                    {
                        NextToken();
                        var values = new List<PTExpr>();

                        while (true)
                        {
                            SkipNewlines();
                            var next = PeekToken();

                            if (next.type == TokenType.ClosingBracket || next.type == TokenType.EOF)
                                break;

                            values.Add(ParseExpression());

                            next = PeekToken();

                            if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                            {
                                NextToken();
                            }
                            else if (next.type == TokenType.ClosingBracket)
                            {
                                break;
                            }
                            else
                            {
                                ReportError(next.location, "Unexpected token in array expression");
                            }
                        }

                        var end = Consume(TokenType.ClosingBracket, ErrMsg("]", "at end of array expression")).location;
                        return new PTArrayExpression(token.location, end, values);
                    }

                case TokenType.DollarIdentifier:
                    NextToken();
                    return new PTIdentifierExpr(token.location, (string)token.data, true);

                case TokenType.Identifier:
                    NextToken();
                    return new PTIdentifierExpr(token.location, (string)token.data, false);

                case TokenType.StringLiteral:
                    NextToken();
                    return new PTStringLiteral(token.location, (string)token.data, false);

                case TokenType.CharLiteral:
                    NextToken();
                    return new PTStringLiteral(token.location, (string)token.data, true);

                case TokenType.NumberLiteral:
                    NextToken();
                    return new PTNumberExpr(token.location, (NumberData)token.data);

                case TokenType.KwTrue:
                    NextToken();
                    return new PTBoolExpr(token.location, true);

                case TokenType.KwFalse:
                    NextToken();
                    return new PTBoolExpr(token.location, false);

                //case TokenType.Less:
                //    {
                //        NextToken();
                //        SkipNewlines();
                //        var type = ParseExpression();
                //        Consume(TokenType.Greater, ErrMsg(">", "after type in cast"));
                //        SkipNewlines();
                //        var e = ParseUnaryExpression(errorMessage);
                //        return new PTCastExpr(token.location, e.End, type, e);
                //    }

                case TokenType.OpenParen:
                    NextToken();
                    SkipNewlines();
                    var sub = ParseExpression(errorMessage);
                    SkipNewlines();
                    sub.Beginning = token.location;
                    sub.End = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of () expression")).location;

                    if (IsExprToken())
                    {
                        var type = sub;
                        sub = ParseUnaryExpression();
                        return new PTCastExpr(type.Beginning, sub.End, type, sub);
                    }
                    return sub;

                default:
                    //NextToken();
                    ReportError(token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
                    return ParseEmptyExpression();
            }
        }

        private PTExpr ParseStructValue()
        {
            TokenLocation beg = null, end = null;
            List<PTStructMemberInitialization> members = new List<PTStructMemberInitialization>();
            PTExpr type;

            beg = Consume(TokenType.KwNew, ErrMsg("keyword 'new'", "at beginning of struct value expression")).location;

            SkipNewlines();
            type = ParseExpression(ErrMsg("type expression", "after keyword 'new'"));

            SkipNewlines();
            Consume(TokenType.OpenBrace, ErrMsg("{", "after name in struct value"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var value = ParseExpression();
                PTIdentifierExpr memberName = null;
                if (value is PTIdentifierExpr n && CheckToken(TokenType.Equal))
                {
                    memberName = n;
                    NextToken();
                    SkipNewlines();
                    value = ParseExpression();
                }

                members.Add(new PTStructMemberInitialization
                {
                    Beginning = memberName?.Beginning ?? value.Beginning,
                    End = value.End,
                    Name = memberName,
                    Value = value
                });

                next = PeekToken();
                if (next.type == TokenType.NewLine || next.type == TokenType.Comma)
                {
                    NextToken();
                    SkipNewlines();
                }
                else if (next.type == TokenType.ClosingBrace)
                {
                    break;
                }
                else
                {
                    NextToken();
                    ReportError(next.location, $"Expected ',' or '\\n' or '}}'");
                }
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of struct value expression")).location;

            return new PTStructValueExpr(type.Beginning, end, type, members);
        }

        private PTIdentifierExpr ParseIdentifierExpr(ErrorMessageResolver customErrorMessage, TokenType identType = TokenType.Identifier)
        {
            var next = PeekToken();
            if (next.type != identType)
            {
                ReportError(next.location, customErrorMessage?.Invoke(next) ?? "Expected identifier");
                return new PTIdentifierExpr(next.location, "§", false);
            }
            NextToken();
            return new PTIdentifierExpr(next.location, (string)next.data, false);
        }

        private PTExpr ParseEmptyExpression()
        {
            var loc = GetWhitespaceLocation();
            return new PTErrorExpr(loc.beg, loc.end);
        }
        #endregion

    }
}
