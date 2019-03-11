using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez.Parsing
{
    public class Parser
    {
        private delegate string ErrorMessageResolver(Token t);
        private delegate AstExpression ExpressionParser(ErrorMessageResolver e);

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

#region Helpers

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
        private void UndoTokens(int count)
        {
            mLexer.UndoTokens(count);
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private void ReportError(TokenLocation location, string message)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer, new Location(location), message, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        private void ReportError(ILocation location, string message)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
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
        private bool IsExprToken(params TokenType[] exclude)
        {
            var next = PeekToken();
            if (exclude.Contains(next.type))
                return false;
            switch (next.type)
            {
                case TokenType.KwNew:
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.LessLess:
                case TokenType.OpenParen:
                case TokenType.OpenBracket:
                case TokenType.OpenBrace:
                case TokenType.StringLiteral:
                case TokenType.CharLiteral:
                case TokenType.NumberLiteral:
                case TokenType.KwNull:
                case TokenType.KwTrue:
                case TokenType.KwFalse:
                case TokenType.KwType:
                case TokenType.KwCast:
                case TokenType.Ampersand:
                case TokenType.Asterisk:
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

#endregion

        public (bool done, AstStatement stmt) ParseStatement(bool expectNewline = true)
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
                    return (false, new AstBreakStmt(new Location(token.location)));

                case TokenType.KwContinue:
                    NextToken();
                    return (false, new AstContinueStmt(new Location(token.location)));

                case TokenType.HashIdentifier:
                    {
                        var dir = ParseDirectiveStatement();
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
                            return (false, new AstDeferStmt(s.stmt, Location: new Location(token.location)));

                        return (false, null);
                    }

                case TokenType.KwReturn:
                    return (false, ParseReturnStatement());
                case TokenType.KwFn:
                    return (false, ParseFunctionDeclaration());
                case TokenType.KwLet:
                    return (false, ParseVariableDeclaration(TokenType.ClosingBrace));
                case TokenType.KwTypedef:
                    return (false, ParseTypedefDeclaration());
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
                        if (expr is AstEmptyExpr)
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
                            return (false, new AstAssignment(expr, val, op, new Location(expr.Beginning, val.End)));
                        }
                        else
                        {
                            if (expectNewline && !Expect(TokenType.NewLine, ErrMsg("\\n", "after expression statement")))
                                RecoverStatement();
                            return (false, new AstExprStmt(expr, new Location(expr.Beginning, expr.End)));
                        }
                    }
            }
        }

        private AstStatement ParseTraitDeclaration()
        {
            TokenLocation beg = null, end = null;
            AstIdExpr name = null;
            var functions = new List<AstFunctionDecl>();
            var parameters = new List<AstParameter>();

            beg = Consume(TokenType.KwTrait, ErrMsg("keyword 'trait'", "at beginning of trait declaration")).location;
            SkipNewlines();

            name = ParseIdentifierExpr(ErrMsg("name", "after keyword 'trait'"));
            SkipNewlines();

            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList(out var _, out var _);
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

            return new AstTraitDeclaration(name, parameters, functions, new Location(beg, end));
        }

        private AstStatement ParseMatchStatement()
        {
            TokenLocation beg = null, end = null;
            AstExpression value = null;
            var cases = new List<AstMatchCase>();

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
                    cases.Add(new AstMatchCase(v, s.stmt, new Location(v.Beginning, s.stmt.End)));
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

            return new AstMatchStmt(value, cases, Location: new Location(beg, end));
        }

        private AstStatement ParseEnumDeclaration()
        {
            TokenLocation beginning = null, end = null;
            AstIdExpr name;
            var members = new List<AstEnumMember>();

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

                members.Add(new AstEnumMember(memberName, null, memberName.Location));

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
            return new AstEnumDecl(name, members, Location: new Location(beginning, end));
        }

        private AstStatement ParseUsingStatement()
        {
            var beg = Consume(TokenType.KwUsing, ErrMsg("keyword 'using'", "at beginning of using statement")).location;
            SkipNewlines();
            var expr = ParseExpression(ErrMsg("expression", "after keyword 'using'"));
            //if (!Expect(TokenType.NewLine, ErrMsg("\\n", "after using statement")))
            //    RecoverStatement();

            return new AstUsingStmt(expr, Location: new Location(beg));
        }

        private AstReturnStmt ParseReturnStatement()
        {
            var beg = Consume(TokenType.KwReturn, ErrMsg("keyword 'return'", "at beginning of return statement")).location;
            AstExpression returnValue = null;

            var next = PeekToken();
            if (next.type != TokenType.NewLine && next.type != TokenType.EOF)
            {
                returnValue = ParseExpression();
            }

            return new AstReturnStmt(returnValue, new Location(beg));
        }

        private AstDirectiveStatement ParseDirectiveStatement()
        {
            var dir = ParseDirective();
            return new AstDirectiveStatement(dir, dir.Location);
        }

        private AstDirective ParseDirective()
        {
            TokenLocation end = null;
            var args = new List<AstExpression>();

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

            return new AstDirective(name, args, new Location(name.Beginning, end));
        }

        private AstParameter ParseParameter()
        {
            AstIdExpr pname = null;
            AstTypeExpr ptype = null;
            AstExpression defaultValue = null;

            TokenLocation beg = null, end = null;

            var next = PeekToken();
            if (next.type == TokenType.Identifier || next.type == TokenType.DollarIdentifier)
            {
                NextToken();
                SkipNewlines();

                var maybeColon = PeekToken();
                if (maybeColon.type == TokenType.Colon)
                {
                    pname = new AstIdExpr((string)next.data, next.type == TokenType.DollarIdentifier, new Location(next.location));
                    beg = pname.Beginning;

                    Consume(TokenType.Colon, ErrMsg(":", "after name in parameter"));
                    SkipNewlines();

                    ptype = ParseTypeExpr();
                }
                else
                {
                    UndoTokens(2);
                    ptype = ParseTypeExpr();
                    beg = ptype.Beginning;
                }
            }
            else
            {
                ptype = ParseTypeExpr();
                beg = ptype.Beginning;
            }

            end = ptype.End;

            // optional default value
            SkipNewlines();
            if (CheckToken(TokenType.Equal))
            {
                NextToken();
                SkipNewlines();
                defaultValue = ParseExpression();
                end = defaultValue.End;
            }

            return new AstParameter(pname, ptype, defaultValue, new Location(beg, end));
        }

        private List<AstParameter> ParseParameterList(out TokenLocation beg, out TokenLocation end)
        {
            var parameters = new List<AstParameter>();

            beg = Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of parameter list")).location;
            SkipNewlines();

            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                var a = ParseParameter();
                parameters.Add(a);

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

            end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of parameter list")).location;

            return parameters;
        }

        private AstStructDecl ParseStructDeclaration()
        {
            TokenLocation beg = null, end = null;
            var members = new List<AstMemberDecl>();
            var directives = new List<AstDirective>();
            AstIdExpr name = null;
            List<AstParameter> parameters = null;

            beg = Consume(TokenType.KwStruct, ErrMsg("keyword 'struct'", "at beginning of struct declaration")).location;
            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'struct'"));
            SkipNewlines();

            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList(out var _, out var _);
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

                var mType = ParseTypeExpr();
                AstExpression init = null;

                next = PeekToken();
                if (next.type == TokenType.Equal)
                {
                    NextToken();
                    init = ParseExpression();
                    next = PeekToken();
                }

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

                var memberEnd = init?.End ?? mType.End;

                members.Add(new AstMemberDecl(mName, mType, init, new Location(mName.Beginning, memberEnd)));
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of struct declaration")).location;

            return new AstStructDecl(name, parameters, members, directives, new Location(beg, end));
        }

        private AstImplBlock ParseImplBlock()
        {
            TokenLocation beg = null, end = null;
            var functions = new List<AstFunctionDecl>();
            AstTypeExpr target = null;
            AstTypeExpr trait = null;

            beg = Consume(TokenType.KwImpl, ErrMsg("keyword 'impl'", "at beginning of impl statement")).location;
            SkipNewlines();

            target = ParseTypeExpr();
            SkipNewlines();

            if (CheckToken(TokenType.KwFor))
            {
                NextToken();
                SkipNewlines();
                trait = target;
                target = ParseTypeExpr();
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

            return new AstImplBlock(target, trait, functions, new Location(beg, end));
        }

        private AstExprStmt ParseBlockStatement()
        {
            var expr = ParseBlockExpr();
            return new AstExprStmt(expr, expr.Location);
        }

        private AstBlockExpr ParseBlockExpr()
        {
            var statements = new List<AstStatement>();
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
                        case AstExprStmt es when es.Expr is AstBlockExpr || es.Expr is AstIfExpr:
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

            return new AstBlockExpr(statements, new Location(beg, end));
        }

        private AstExprStmt ParseExpressionStatement()
        {
            var expr = ParseExpression();
            return new AstExprStmt(expr, new Location(expr.Beginning, expr.End));
        }

        private AstTypeAliasDecl ParseTypedefDeclaration()
        {
            TokenLocation beg = null, end = null;
            AstIdExpr name = null;
            AstTypeExpr value = null;

            beg = Consume(TokenType.KwTypedef, ErrMsg("keyword 'typedef'", "at beginning of type alias declaration")).location;
            SkipNewlines();

            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'typedef'"));
            SkipNewlines();

            Consume(TokenType.Equal, ErrMsg("type expression", "after '=' in type alias declaration"));
            SkipNewlines();

            value = ParseTypeExpr();
            end = value.End;

            return new AstTypeAliasDecl(name, value, Location: new Location(beg, end));
        }

        private AstVariableDecl ParseVariableDeclaration(params TokenType[] delimiters)
        {
            TokenLocation beg = null, end = null;
            var directives = new List<AstDirective>();
            AstExpression pattern = null;
            AstTypeExpr type = null;
            AstExpression init = null;

            beg = Consume(TokenType.KwLet, ErrMsg("keyword 'let'", "at beginning of variable declaration")).location;

            SkipNewlines();
            while (CheckToken(TokenType.HashIdentifier))
            {
                directives.Add(ParseDirective());
                SkipNewlines();
            }

            pattern = ParseExpression();
            end = pattern.End;

            if (CheckToken(TokenType.Colon))
            {
                NextToken();
                SkipNewlines();
                type = ParseTypeExpr();
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

            return new AstVariableDecl(pattern, type, init, directives, new Location(beg, end));
        }

        private AstWhileStmt ParseWhileStatement()
        {
            TokenLocation beg = null;
            AstExpression condition = null;
            AstBlockExpr body = null;
            AstVariableDecl init = null;
            AstStatement post = null;

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

            body = ParseBlockExpr();

            return new AstWhileStmt(condition, body, init, post, new Location(beg, body.End));
        }

        private AstIfExpr ParseIfExpr()
        {
            TokenLocation beg = null, end = null;
            AstExpression condition = null;
            AstExpression ifCase = null;
            AstExpression elseCase = null;
            AstVariableDecl pre = null;

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
            ifCase = ParseBlockExpr();
            end = ifCase.End;

            SkipNewlines();
            if (CheckToken(TokenType.KwElse))
            {
                NextToken();
                SkipNewlines();

                if (CheckToken(TokenType.KwIf))
                    elseCase = ParseIfExpr();
                else
                    elseCase = ParseBlockExpr();
                end = elseCase.End;
            }

            return new AstIfExpr(condition, ifCase, elseCase, pre, new Location(beg, end));
        }

        private AstFunctionDecl ParseFunctionDeclaration()
        {
            TokenLocation beginning = null,
                end = null,
                pbeg = null,
                pend = null;
            AstBlockExpr body = null;
            var parameters = new List<AstParameter>();
            AstParameter returnValue = null;
            var directives = new List<AstDirective>();
            var generics = new List<AstIdExpr>();

            beginning = NextToken().location;
            SkipNewlines();

            var name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'fn' in function declaration"));
            SkipNewlines();

            // parameters
            SkipNewlines();
            parameters = ParseParameterList(out pbeg, out pend);

            SkipNewlines();

            // return type
            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                SkipNewlines();
                returnValue = ParseParameter();
                SkipNewlines();
            }

            while (CheckToken(TokenType.HashIdentifier))
            {
                directives.Add(ParseDirective());
                SkipNewlines();
            }

            if (CheckToken(TokenType.Semicolon))
                end = NextToken().location;
            else
            {
                body = ParseBlockExpr();
                end = body.End;
            }

            return new AstFunctionDecl(name, generics, parameters, returnValue, body, directives, Location: new Location(beginning, end), ParameterLocation: new Location(pbeg, pend));
        }

        #region Expression Parsing

        private AstTypeExpr ParseFunctionTypeExpr()
        {
            var args = new List<AstTypeExpr>();
            AstTypeExpr returnType = null;

            var beginning = Consume(TokenType.KwFn, ErrMsg("keyword 'fn'", "at beginning of function type")).location;
            SkipNewlines();

            Consume(TokenType.OpenParen, ErrMsg("(", "after keyword 'fn'"));
            SkipNewlines();

            while (true)
            {
                SkipNewlines();
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                args.Add(ParseTypeExpr());
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

            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                returnType = ParseTypeExpr();
                end = returnType.End;
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "at end of function type return type list"));
            SkipNewlines();
            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of function type return type list")).location;

            return new AstFunctionTypeExpr(args, returnType, new Location(beginning, end));
        }

        private AstExpression ParseExpression(ErrorMessageResolver errorMessage = null)
        {
            errorMessage = errorMessage ?? (t => $"Unexpected token '{t}' in expression");

            return ParseOrExpression(errorMessage);
        }

        private AstTypeExpr ParseTypeExpr()
        {
            var token = PeekToken();

            if (token.type == TokenType.KwType)
            {
                NextToken();
                return new AstIdTypeExpr("type", false, new Location(token.location));
            }

            if (token.type == TokenType.AtSignIdentifier)
            {
                var sub = ParseExpression();
                return new AstExprTypeExpr(sub, sub.Location);
            }

            if (token.type == TokenType.OpenParen)
            {
                var p = ParseParameterList(out var _, out var end);
                return new AstTupleTypeExpr(p, new Location(token.location, end));
            }

            if (token.type == TokenType.OpenBracket)
            {
                NextToken();
                SkipNewlines();

                var next = PeekToken();
                if (next.type == TokenType.ClosingBracket)
                {
                    NextToken();
                    SkipNewlines();

                    var target = ParseTypeExpr();
                    return new AstSliceTypeExpr(target, new Location(token.location, target.End));
                }

                var size = ParseExpression();
                SkipNewlines();

                Consume(TokenType.ClosingBracket, ErrMsg("']'", "at end of array type expression"));
                SkipNewlines();

                {
                    var target = ParseTypeExpr();
                    return new AstArrayTypeExpr(target, size, new Location(token.location, target.End));
                }
            }

            if (token.type == TokenType.Identifier || token.type == TokenType.DollarIdentifier)
            {
                NextToken();
                AstTypeExpr result = new AstIdTypeExpr(token.data as string, token.type == TokenType.DollarIdentifier, new Location(token.location));

                var next = PeekToken();
                if (next.type == TokenType.OpenParen)
                {
                    NextToken();
                    SkipNewlines();

                    var args = new List<AstTypeExpr>();

                    next = PeekToken();
                    while (next.type != TokenType.ClosingParen)
                    {
                        var a = ParseTypeExpr();
                        if (a != null) args.Add(a);
                        SkipNewlines();

                        next = PeekToken();
                        if (next.type == TokenType.Comma)
                        {
                            NextToken();
                            SkipNewlines();
                        }
                        else if (next.type != TokenType.ClosingParen)
                        {
                            NextToken();
                            ReportError(next.location, $"Unexpected token '{next}' in poly struct type expression. Expected ',' or ')'");
                        }
                    }

                    var end = Consume(TokenType.ClosingParen, ErrMsg("')'", "at end of poly struct type expressino")).location;
                    result = new AstPolyStructTypeExpr(result, args, new Location(result.Beginning, end));
                }

                return result;
            }

            if (token.type == TokenType.Asterisk)
            {
                NextToken();
                SkipNewlines();
                var target = ParseTypeExpr();
                return new AstPointerTypeExpr(target, new Location(token.location, target.End));
            }

            if (token.type == TokenType.KwFn)
            {
                return ParseFunctionTypeExpr();
            }

            NextToken();
            ReportError(token.location, $"Unexpected token in type expression: '{token}'");
            return new AstErrorTypeExpr(new Location(token.location));
        }

        [DebuggerStepThrough]
        private AstExpression ParseOrExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAndExpression, e,
                (TokenType.KwOr, "or"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseAndExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseComparisonExpression, e,
                (TokenType.KwAnd, "and"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseComparisonExpression(ErrorMessageResolver e)
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
        private AstExpression ParseAddSubExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseMulDivExpression, e,
                (TokenType.Plus, "+"),
                (TokenType.Minus, "-"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseMulDivExpression(ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseUnaryExpression, e,
                (TokenType.Asterisk, "*"),
                (TokenType.ForwardSlash, "/"),
                (TokenType.Percent, "%"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseBinaryLeftAssociativeExpression(ExpressionParser sub, ErrorMessageResolver errorMessage, params (TokenType, string)[] types)
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

        private AstExpression ParseLeftAssociativeExpression(ExpressionParser sub, ErrorMessageResolver errorMessage, Func<TokenType, string> tokenMapping)
        {
            var lhs = sub(errorMessage);
            AstExpression rhs = null;

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
                lhs = new AstBinaryExpr(op, lhs, rhs, new Location(lhs.Beginning, rhs.End));
            }
        }

        private AstExpression ParseUnaryExpression(ErrorMessageResolver errorMessage = null)
        {
            var next = PeekToken();
            if (next.type == TokenType.Ampersand)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new AstAddressOfExpr(sub, new Location(next.location, sub.End));
            }
            else if (next.type == TokenType.KwCast)
            {
                var beg = next.location;
                AstTypeExpr type = null;

                NextToken();
                SkipNewlines();

                next = PeekToken();
                if (next.type == TokenType.OpenParen)
                {
                    NextToken();
                    SkipNewlines();
                    type = ParseTypeExpr();
                    SkipNewlines();
                    Consume(TokenType.ClosingParen, ErrMsg("')'", "after type in cast expression"));
                    SkipNewlines();
                }

                var sub = ParseUnaryExpression(errorMessage);
                return new AstCastExpr(type, sub, new Location(beg, sub.End));
            }
            else if (next.type == TokenType.LessLess)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new AstDereferenceExpr(sub, new Location(next.location, sub.End));
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
                return new AstUnaryExpr(op, sub, new Location(next.location, sub.End));
            }
            else if (next.type == TokenType.Bang)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(errorMessage);
                return new AstUnaryExpr("!", sub, new Location(next.location, sub.End));
            }

            return ParsePostUnaryExpression(errorMessage);
        }

        private AstArgument ParseArgumentExpression()
        {
            TokenLocation beg;
            AstExpression expr;
            AstIdExpr name = null;

            var next = PeekToken();
            if (next.type == TokenType.Identifier)
            {
                NextToken();
                SkipNewlines();

                var maybeColon = PeekToken();
                if (maybeColon.type == TokenType.Equal)
                {
                    name = new AstIdExpr((string)next.data, false, new Location(next.location));
                    beg = name.Beginning;

                    Consume(TokenType.Equal, ErrMsg("=", "after name in argument"));
                    SkipNewlines();

                    expr = ParseExpression();
                }
                else
                {
                    UndoTokens(2);
                    expr = ParseExpression();
                    beg = expr.Beginning;
                }
            }
            else
            {
                expr = ParseExpression();
                beg = expr.Beginning;
            }

            return new AstArgument(expr, name, new Location(beg, expr.End));
        }

        private AstExpression ParsePostUnaryExpression(ErrorMessageResolver errorMessage)
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
                            var args = new List<AstArgument>();
                            while (true)
                            {
                                var next = PeekToken();
                                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                                    break;
                                args.Add(ParseArgumentExpression());
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

                            expr = new AstCallExpr(expr, args, new Location(expr.Beginning, end));
                        }
                        break;

                    case TokenType.OpenBracket:
                        {
                            NextToken();
                            SkipNewlines();

                            var index = ParseExpression(errorMessage);
                            SkipNewlines();
                            var end = Consume(TokenType.ClosingBracket, ErrMsg("]", "at end of [] operator")).location;
                            expr = new AstArrayAccessExpr(expr, index, new Location(expr.Beginning, end));
                        }
                        break;

                    case TokenType.Period:
                        {
                            NextToken();
                            SkipNewlines();
                            var right = ParseIdentifierExpr(ErrMsg("identifier", "after ."));

                            expr = new AstDotExpr(expr, right, false, new Location(expr.Beginning, right.End));
                            break;
                        }

                    case TokenType.DoubleColon:
                        {
                            NextToken();
                            SkipNewlines();
                            var right = ParseIdentifierExpr(ErrMsg("identifier", "after ."));

                            expr = new AstDotExpr(expr, right, true, new Location(expr.Beginning, right.End));
                            break;
                        }

                    default:
                        return expr;
                }
            }
        }

        private List<AstExpression> ParseArgumentList(out TokenLocation end)
        {
            Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of argument list"));

            List<AstExpression> args = new List<AstExpression>();
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

        private AstExpression ParseAtomicExpression(ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.KwNew:
                    return ParseStructValue();

                case TokenType.KwNull:
                    NextToken();
                    return new AstNullExpr(new Location(token.location));

                case TokenType.AtSignIdentifier:
                    {
                        NextToken();
                        var args = ParseArgumentList(out var end);
                        var name = new AstIdExpr((string)token.data, false, new Location(token.location));
                        return new AstCompCallExpr(name, args, new Location(token.location, end));
                    }

                case TokenType.OpenBracket:
                    {
                        NextToken();
                        var values = new List<AstExpression>();

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
                        return new AstArrayExpr(values, new Location(token.location, end));
                    }

                case TokenType.DollarIdentifier:
                    NextToken();
                    return new AstIdExpr((string)token.data, true, new Location(token.location));

                case TokenType.Identifier:
                    NextToken();
                    return new AstIdExpr((string)token.data, false, new Location(token.location));

                case TokenType.StringLiteral:
                    NextToken();
                    return new AstStringLiteral((string)token.data, token.suffix, new Location(token.location));

                case TokenType.CharLiteral:
                    NextToken();
                    return new AstCharLiteral((string)token.data, new Location(token.location));

                case TokenType.NumberLiteral:
                    NextToken();
                    return new AstNumberExpr((NumberData)token.data, token.suffix, new Location(token.location));

                case TokenType.KwTrue:
                    NextToken();
                    return new AstBoolExpr(true, new Location(token.location));

                case TokenType.KwFalse:
                    NextToken();
                    return new AstBoolExpr(false, new Location(token.location));

                case TokenType.OpenBrace:
                    return ParseBlockExpr();

                case TokenType.KwIf:
                    return ParseIfExpr();

                case TokenType.KwType:
                    {
                        NextToken();
                        if (IsExprToken())
                        {
                            var sub = ParseTypeExpr();
                            return sub;
                        }
                        else
                        {
                            return new AstIdExpr("type", false, new Location(token.location));
                        }
                    }

                case TokenType.OpenParen:
                    {
                        NextToken();
                        SkipNewlines();
                        var sub = ParseExpression(errorMessage);
                        SkipNewlines();

                        if (CheckToken(TokenType.ClosingParen))
                        {
                            Consume(TokenType.ClosingParen, ErrMsg(")", "at end of () expression"));
                            return sub;
                        }

                        var tupleValues = new List<AstExpression>() { sub };
                        while (true)
                        {
                            var next = PeekToken();
                            if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                                break;
                            else if (next.type == TokenType.Comma)
                            {
                                NextToken();
                                SkipNewlines();
                            }
                            else
                            {
                                ReportError(next.location, $"Unexpected token ({next})");
                                NextToken();
                            }
                            var expr = ParseExpression();
                            if (expr != null)
                                tupleValues.Add(expr);
                            SkipNewlines();
                        }

                        var end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of () expression")).location;
                        return new AstTupleExpr(tupleValues, new Location(token.location, end));
                    }

                default:
                    //NextToken();
                    ReportError(token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
                    return ParseEmptyExpression();
            }
        }

        private AstExpression ParseStructValue()
        {
            TokenLocation beg = null, end = null;
            List<AstStructMemberInitialization> members = new List<AstStructMemberInitialization>();
            AstTypeExpr type = null;

            beg = Consume(TokenType.KwNew, ErrMsg("keyword 'new'", "at beginning of struct value expression")).location;

            SkipNewlines();
            var maybeOpenBrace = PeekToken();
            if (maybeOpenBrace.type != TokenType.OpenBrace)
            {
                type = ParseTypeExpr();
                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "after name in struct value"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var value = ParseExpression();
                AstIdExpr memberName = null;
                if (value is AstIdExpr n && CheckToken(TokenType.Equal))
                {
                    memberName = n;
                    NextToken();
                    SkipNewlines();
                    value = ParseExpression();
                }

                members.Add(new AstStructMemberInitialization(memberName, value, new Location(memberName?.Beginning ?? value.Beginning, value.End)));

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

            return new AstStructValueExpr(type, members, new Location(beg, end));
        }

        private AstIdExpr ParseIdentifierExpr(ErrorMessageResolver customErrorMessage, TokenType identType = TokenType.Identifier)
        {
            var next = PeekToken();
            if (next.type != identType)
            {
                ReportError(next.location, customErrorMessage?.Invoke(next) ?? "Expected identifier");
                return new AstIdExpr("§", false, new Location(next.location));
            }
            NextToken();
            return new AstIdExpr((string)next.data, false, new Location(next.location));
        }

        private AstExpression ParseEmptyExpression()
        {
            var loc = GetWhitespaceLocation();
            return new AstEmptyExpr(new Location(loc.beg, loc.end));
        }
        #endregion

    }
}
