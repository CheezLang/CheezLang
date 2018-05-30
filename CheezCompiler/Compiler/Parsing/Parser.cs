﻿using Cheez.Compiler.ParseTree;
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
            mErrorHandler.ReportError(mLexer, new Location(location, location), message, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private void ReportError(ILocation location, string message)
        {
            string callingFunctionFile, callingFunctionName;
            int callLineNumber;
            (callingFunctionFile, callingFunctionName, callLineNumber) = GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, location, message, callingFunctionFile, callingFunctionName, callLineNumber);
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

        public (bool done, PTStatement stmt) ParseStatement()
        {
            SkipNewlines();
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.EOF:
                    return (true, null);

                case TokenType.KwReturn:
                    return (false, ParseReturnStatement());
                case TokenType.KwFn:
                    return (false, ParseFunctionDeclaration());
                case TokenType.KwLet:
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
                    return (false, ParseStructDeclaration());
                case TokenType.KwImpl:
                    return (false, ParseImplBlock());
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
                        if (CheckToken(TokenType.Equal))
                        {
                            NextToken();
                            SkipNewlines();
                            var val = ParseExpression();
                            Consume(TokenType.NewLine, ErrMsg("\\n", "after assignment"));
                            return (false, new PTAssignment(expr.Beginning, val.End, expr, val));
                        }
                        else
                        {
                            Consume(TokenType.NewLine, ErrMsg("\\n", "after expression statement"));
                            return (false, new PTExprStmt(expr.Beginning, expr.End, expr));
                        }
                    }
            }
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
            Consume(TokenType.NewLine, ErrMsg("\\n", "after using statement"));

            return new PTUsingStatement(beg, expr);
        }

        private PTReturnStmt ParseReturnStatement()
        {
            var beg = Consume(TokenType.KwReturn, ErrMsg("keyword 'return'", "at beginning of return statement")).location;
            PTExpr returnValue = null;

            if (!CheckToken(TokenType.NewLine))
            {
                returnValue = ParseExpression();
            }

            Consume(TokenType.NewLine, ErrMsg("\\n", "at end of return statement"));

            return new PTReturnStmt(beg, returnValue);
        }

        private PTDirective ParseDirective()
        {
            TokenLocation beginning = null, end = null;
            var args = new List<PTExpr>();

            beginning = NextToken().location;
            var name = ParseIdentifierExpr(ErrMsg("identifier", "after # in directive"));

            end = name.End;

            return new PTDirective(beginning, end, name, args);
        }

        private PTTypeDecl ParseStructDeclaration()
        {
            TokenLocation beg = null, end = null;
            var members = new List<PTMemberDecl>();
            List<PTDirective> directives = new List<PTDirective>();
            PTIdentifierExpr name = null;

            beg = Consume(TokenType.KwStruct, ErrMsg("keyword 'struct'", "at beginning of struct declaration")).location;
            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'struct'"));


            SkipNewlines();
            while (CheckToken(TokenType.HashTag))
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

                var mType = ParseTypeExpression();

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

            return new PTTypeDecl(beg, end, name, members, directives);
        }

        private PTImplBlock ParseImplBlock()
        {
            TokenLocation beg = null, end = null;
            var functions = new List<PTFunctionDecl>();
            PTTypeExpr target = null;

            beg = Consume(TokenType.KwImpl, ErrMsg("keyword 'impl'", "at beginning of impl statement")).location;

            SkipNewlines();

            target = ParseTypeExpression();

            SkipNewlines();
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

            return new PTImplBlock(beg, end, target, functions);
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

                var s = ParseStatement();
                if (s.stmt != null)
                    statements.Add(s.stmt);
                SkipNewlines();
            }

            var end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of block statement")).location;

            return new PTBlockStmt(beg, end, statements);
        }

        private PTExprStmt ParseExpressionStatement()
        {
            var expr = ParseExpression();
            Consume(TokenType.NewLine, ErrMsg("\\n", "after expression statement"));
            return new PTExprStmt(expr.Beginning, expr.End, expr);
        }

        private PTVariableDecl ParseVariableDeclaration(params TokenType[] delimiters)
        {
            TokenLocation beg = null, end = null;
            List<PTDirective> directives = new List<PTDirective>();
            PTIdentifierExpr name = null;
            PTTypeExpr type = null;
            PTExpr init = null;

            beg = Consume(TokenType.KwLet, ErrMsg("keyword 'let'", "at beginning of variable declaration")).location;

            SkipNewlines();
            while (CheckToken(TokenType.HashTag))
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
                type = ParseTypeExpression();
                end = type.End;
            }

            if (CheckToken(TokenType.Equal))
            {
                NextToken();
                SkipNewlines();
                init = ParseExpression(ErrMsg("expression", "after '=' in variable declaration"));
                end = init.End;
            }

            Consume(TokenType.NewLine, ErrMsg("\\n", "after variable declaration"));

            return new PTVariableDecl(beg, end, name, type, init, directives);
        }

        private PTWhileStmt ParseWhileStatement()
        {
            TokenLocation beg = null;
            PTExpr condition = null;
            PTStatement body = null;

            beg = Consume(TokenType.KwWhile, ErrMsg("keyword 'while'", "at beginning of while statement")).location;

            SkipNewlines();
            condition = ParseExpression(ErrMsg("expression", "after keyword 'while'"));

            SkipNewlines();
            body = ParseBlockStatement();

            return new PTWhileStmt(beg, condition, body);
        }

        private PTIfStmt ParseIfStatement()
        {
            TokenLocation beg = null, end = null;
            PTExpr condition = null;
            PTStatement ifCase = null;
            PTStatement elseCase = null;

            beg = Consume(TokenType.KwIf, ErrMsg("keyword 'if'", "at beginning of if statement")).location;

            SkipNewlines();
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

            return new PTIfStmt(beg, end, condition, ifCase, elseCase);
        }

        private PTTypeAliasDecl ParseTypeAliasStatement()
        {
            var beg = Consume(TokenType.KwType, ErrMsg("keyword 'type'", "at beginning of type alias")).location;
            SkipNewlines();
            var name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'type'"));
            SkipNewlines();
            Consume(TokenType.Equal, ErrMsg("=", "after type name in type alias"));
            SkipNewlines();
            var type = ParseTypeExpression();
            Consume(TokenType.NewLine, ErrMsg("\\n", "after type alias"));

            return new PTTypeAliasDecl(beg, name, type);
        }

        private PTFunctionDecl ParseFunctionDeclaration()
        {
            TokenLocation beginning = null, end = null;
            List<PTStatement> statements = new List<PTStatement>();
            List<PTFunctionParam> parameters = new List<PTFunctionParam>();
            List<PTDirective> directives = new List<PTDirective>();
            PTTypeExpr returnType = null;


            beginning = NextToken().location;

            SkipNewlines();
            var name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'fn' in function declaration"));

            // parameters
            SkipNewlines();
            Consume(TokenType.OpenParen, ErrMsg("(", "after name in function declaration"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                PTIdentifierExpr pname = null;
                PTTypeExpr ptype = null;

                if (next.type != TokenType.Colon)
                    pname = ParseIdentifierExpr(ErrMsg("identifier"));

                SkipNewlines();
                Consume(TokenType.Colon, ErrMsg(":", "after name in parameter list"));

                SkipNewlines();
                ptype = ParseTypeExpression();

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
                returnType = ParseTypeExpression();
                SkipNewlines();
            }

            while (CheckToken(TokenType.HashTag))
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
                Consume(TokenType.OpenBrace, ErrMsg("{", "after header in function declaration"));

                while (true)
                {
                    SkipNewlines();
                    var token = PeekToken();

                    if (token.type == TokenType.ClosingBrace || token.type == TokenType.EOF)
                        break;

                    var stmt = ParseStatement();
                    if (stmt.stmt != null)
                        statements.Add(stmt.stmt);
                }

                end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of function")).location;
            }

            return new PTFunctionDecl(beginning, end, name, parameters, returnType, statements);
        }

        #region Expression Parsing

        //private PTTypeExpr ParseFunctionTypeExpr()
        //{
        //    var beginning = Expect(TokenType.KwFn, false).location;
        //    Consume(TokenType.OpenParen, true);

        //    List<PTTypeExpr> args = new List<PTTypeExpr>();
        //    if (PeekToken(true).type != TokenType.ClosingParen)
        //    {
        //        while (true)
        //        {
        //            args.Add(ParseTypeExpression());

        //            var next = PeekToken(true);
        //            if (next.type == TokenType.Comma)
        //                NextToken();
        //            else if (next.type == TokenType.ClosingParen)
        //                break;
        //            else
        //            {
        //                ReportError(next.location, $"Failed to parse function type, expected comma or closing paren, got {next.data} ({next.type})");
        //                RecoverExpression();
        //                return null;
        //            }
        //        }
        //    }

        //    var end = Expect(TokenType.ClosingParen, true).location;
        //    PTTypeExpr returnType = null;
        //    if (PeekToken(false).type == TokenType.Colon)
        //    {
        //        NextToken();
        //        returnType = ParseTypeExpression();
        //        end = returnType.End;
        //    }

        //    return new PTFunctionTypeExpr(beginning, end, returnType, args);
        //}

        private PTTypeExpr ParseTypeExpression()
        {
            PTTypeExpr type = null;
            bool cond = true;
            while (cond)
            {
                var next = PeekToken();
                switch (next.type)
                {
                    //case TokenType.KwFn:
                    //    return ParseFunctionTypeExpr();

                    case TokenType.Identifier when type == null:
                        NextToken();
                        type = new PTNamedTypeExpr(next.location, next.location, (string)next.data);
                        break;

                    case TokenType.Asterisk when type != null:
                        NextToken();
                        type = new PTPointerTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.OpenBracket when type != null:
                        NextToken();
                        SkipNewlines();
                        next = Consume(TokenType.ClosingBracket, ErrMsg("]", "after [ in array type expression"));
                        type = new PTArrayTypeExpr(type.Beginning, next.location, type);
                        break;

                    case TokenType.NewLine when type == null:
                        {
                            NextToken();
                            ReportError(next.location, "Expected type expression, found new line");
                            return new PTErrorTypeExpr(next.location);
                        }
                    case TokenType.EOF when type == null:
                        {
                            NextToken();
                            ReportError(next.location, "Expected type expression, found end of file");
                            return new PTErrorTypeExpr(next.location);
                        }

                    case TokenType t when type == null:
                        {
                            NextToken();
                            ReportError(next.location, $"Unexpected token in type expression: {next}");
                            return new PTErrorTypeExpr(next.location);
                        }

                    default:
                        cond = false;
                        break;
                }
            }
            return type;
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
            else if (next.type == TokenType.Asterisk)
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
                                    NextToken();
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
                            var index = ParseExpression(errorMessage);
                            SkipNewlines();
                            var end = Consume(TokenType.ClosingBracket, ErrMsg("]", "at end of [] operator")).location;
                            expr = new PTArrayAccessExpr(expr.Beginning, end, expr, index);
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

                    default:
                        return expr;
                }
            }
        }

        private PTExpr ParseAtomicExpression(ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.KwNew:
                    return ParseStructValue();

                case TokenType.Identifier:
                    NextToken();
                    return new PTIdentifierExpr(token.location, (string)token.data);

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
                        SkipNewlines();
                        var type = ParseTypeExpression();
                        Consume(TokenType.Greater, ErrMsg(">", "after type in cast"));
                        SkipNewlines();
                        var e = ParseUnaryExpression(errorMessage);
                        return new PTCastExpr(token.location, e.End, type, e);
                    }

                case TokenType.OpenParen:
                    NextToken();
                    SkipNewlines();
                    var sub = ParseExpression(errorMessage);
                    SkipNewlines();
                    sub.Beginning = token.location;
                    sub.End = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of () expression")).location;
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
            PTIdentifierExpr name;

            beg = Consume(TokenType.KwNew, ErrMsg("keyword 'new'", "at beginning of struct value expression")).location;

            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'new'"));

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

            return new PTStructValueExpr(name.Beginning, end, name, members);
        }

        private PTIdentifierExpr ParseIdentifierExpr(ErrorMessageResolver customErrorMessage)
        {
            var next = PeekToken();
            if (next.type != TokenType.Identifier)
            {
                ReportError(next.location, customErrorMessage?.Invoke(next) ?? "Expected identifier");
                return new PTIdentifierExpr(next.location, "§");
            }
            NextToken();
            return new PTIdentifierExpr(next.location, (string)next.data);
        }

        private PTExpr ParseEmptyExpression()
        {
            var loc = GetWhitespaceLocation();
            return new PTErrorExpr(loc.beg, loc.end);
        }
        #endregion

    }
}