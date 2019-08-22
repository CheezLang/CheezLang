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
        public delegate string ErrorMessageResolver(Token t);
        private delegate AstExpression ExpressionParser(ErrorMessageResolver e);

        private ILexer mLexer;
        private IErrorHandler mErrorHandler;

        private Dictionary<string, AstExpression> Replacements;

        private Token lastNonWhitespace = null;
        private Token mCurrentToken = null;
        private Token CurrentToken => mCurrentToken;

        public Parser(ILexer lex, IErrorHandler errHandler)
        {
            mLexer = lex;
            mErrorHandler = errHandler;
        }

        internal static AstExpression ParseExpression(string v, Dictionary<string, AstExpression> dictionary, IErrorHandler errorHandler, string id)
        {
            var l = Lexer.FromString(v, errorHandler, id);
            var p = new Parser(l, errorHandler);
            p.Replacements = dictionary;
            return p.ParseExpression();
        }

        internal static AstStatement ParseStatement(string v, Dictionary<string, AstExpression> dictionary, IErrorHandler errorHandler, string id)
        {
            var l = Lexer.FromString(v, errorHandler, id);
            var p = new Parser(l, errorHandler);
            p.Replacements = dictionary;
            return p.ParseStatement();
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
        public void SkipNewlines()
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
        public Token NextToken()
        {
            mCurrentToken = mLexer.NextToken();
            if (mCurrentToken.type != TokenType.NewLine)
                lastNonWhitespace = mCurrentToken;
            return mCurrentToken;
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        public void ReportError(TokenLocation location, string message)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer.Text, new Location(location), message, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        public void ReportError(ILocation location, string message)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            mErrorHandler.ReportError(mLexer.Text, location, message, null, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [DebuggerStepThrough]
        public static ErrorMessageResolver ErrMsg(string expect, string where = null)
        {
            return t => $"Expected {expect} {where}";
        }

        [DebuggerStepThrough]
        public ErrorMessageResolver ErrMsgUnexpected(string expect, string where = null)
        {
            return t => $"Unexpected token {t} at {where}. Expected {expect}";
        }


        [SkipInStackFrame]
        [DebuggerStepThrough]
        public bool Expect(TokenType type, ErrorMessageResolver customErrorMessage)
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
        public Token Consume(TokenType type, ErrorMessageResolver customErrorMessage)
        {
            if (!Expect(type, customErrorMessage))
                NextToken();
            return CurrentToken;
        }

        [SkipInStackFrame]
        [DebuggerStepThrough]
        public Token ConsumeUntil(TokenType type, ErrorMessageResolver customErrorMessage)
        {
            var tok = PeekToken();
            while (tok.type != type)
            {
                ReportError(tok.location, customErrorMessage?.Invoke(tok));
                NextToken();
                tok = PeekToken();

                if (tok.type == TokenType.EOF)
                    break;
            }

            if (!Expect(type, customErrorMessage))
                NextToken();
            return CurrentToken;
        }

        [DebuggerStepThrough]
        public bool CheckToken(TokenType type)
        {
            var next = PeekToken();
            return next.type == type;
        }

        [DebuggerStepThrough]
        public bool CheckTokens(params TokenType[] types)
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
        public bool IsTypeExprToken()
        {
            var next = PeekToken();
            switch (next.type)
            {
                case TokenType.OpenParen:
                case TokenType.OpenBracket:
                case TokenType.Ampersand:
                case TokenType.Identifier:
                case TokenType.DollarIdentifier:
                case TokenType.KwFn:
                    return true;

                default:
                    return false;
            }
        }

        public bool IsExprToken(params TokenType[] exclude)
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
        public Token PeekToken()
        {
            return mLexer.PeekToken();
        }

        [SkipInStackFrame]
        public Token ReadToken(bool SkipNewLines = false)
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

        public AstStatement ParseStatement(bool expectNewline = true)
        {
            var stmt = ParseStatementHelper();

            if (stmt == null)
                return null;

            if (CheckToken(TokenType.Semicolon))
            {
                var stmts = new List<AstStatement> { stmt };
                while (CheckToken(TokenType.Semicolon))
                {
                    NextToken();
                    SkipNewlines();
                    var s = ParseStatementHelper();
                    if (s == null)
                        break;

                    stmts.Add(s);
                }

                var location = new Location(stmts.First().Beginning, stmts.Last().End);

                // @temporary, these statements should not create a new scope
                var block = new AstBlockExpr(stmts, location);
                stmt = new AstExprStmt(block, location);
            }

            var next = PeekToken();
            if (expectNewline && next.type != TokenType.NewLine && next.type != TokenType.EOF)
            {
                ReportError(next.location, $"Expected newline after statement");
                RecoverStatement();
            }

            return stmt;
        }

        public AstStatement ParseStatementHelper()
        {
            SkipNewlines();
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.EOF:
                    return null;

                case TokenType.KwBreak:
                    NextToken();
                    return new AstBreakStmt(new Location(token.location));

                case TokenType.KwContinue:
                    NextToken();
                    return new AstContinueStmt(new Location(token.location));

                case TokenType.HashIdentifier:
                        return ParseDirectiveStatement();

                case TokenType.KwDefer:
                    {
                        NextToken();
                        var next = PeekToken();
                        if (next.type == TokenType.NewLine || next.type == TokenType.EOF)
                        {
                            ReportError(token.location, "Expected statement after keyword 'defer'");
                            return new AstEmptyStatement(new Location(token.location));
                        }

                        var s = ParseStatement();
                        if (s != null)
                            return new AstDeferStmt(s, Location: new Location(token.location));
                        else
                        {
                            ReportError(token.location, $"Expected statement after keyword 'defer'");
                        }

                        return new AstEmptyStatement(new Location(token.location));
                    }

                case TokenType.KwReturn:
                    return ParseReturnStatement();
                case TokenType.KwFn:
                    return ParseFunctionDeclaration();
                case TokenType.KwLet:
                    return ParseVariableDeclaration(TokenType.ClosingBrace);
                case TokenType.KwTypedef:
                    return ParseTypedefDeclaration();
                case TokenType.KwWhile:
                    return ParseWhileStatement();
                case TokenType.KwEnum:
                    return ParseEnumDeclaration();
                case TokenType.KwStruct:
                    return ParseStructDeclaration();
                case TokenType.KwImpl:
                    return ParseImplBlock();
                case TokenType.KwTrait:
                    return ParseTraitDeclaration();
                case TokenType.OpenBrace:
                    return ParseBlockStatement();

                case TokenType.KwUsing:
                    return ParseUsingStatement();

                default:
                    {
                        var expr = ParseExpression();
                        if (expr is AstEmptyExpr)
                        {
                            NextToken();
                            return new AstEmptyStatement(expr.Location);
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
                            return new AstAssignment(expr, val, op, new Location(expr.Beginning, val.End));
                        }
                        else
                        {
                            return new AstExprStmt(expr, new Location(expr.Beginning, expr.End));
                        }
                    }
            }
        }

        private AstStatement ParseTraitDeclaration()
        {
            TokenLocation beg = null, end = null;
            AstIdExpr name = null;
            var functions = new List<AstFunctionDecl>();
            var variables = new List<AstStructMember>();
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

                if (next.type == TokenType.KwFn)
                    functions.Add(ParseFunctionDeclaration());
                else if (next.type == TokenType.Identifier)
                {
                    var vname = ParseIdentifierExpr(ErrMsg("identifier"));
                    SkipNewlines();
                    Consume(TokenType.Colon, ErrMsg("':'"));
                    SkipNewlines();
                    var vtype = ParseExpression();
                    variables.Add(new AstStructMember(vname, vtype, null, new Location(vname.Beginning, vtype.End)));
                }
                SkipNewlines();
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of trait")).location;

            return new AstTraitDeclaration(name, parameters, functions, variables, new Location(beg, end));
        }

        private AstExpression ParseMatchExpr()
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
                SkipNewlines();

                AstExpression cond = null;
                if (CheckToken(TokenType.KwIf))
                {
                    NextToken();
                    cond = ParseExpression();
                    SkipNewlines();
                }

                Consume(TokenType.Arrow, ErrMsg("->", "after value in match case"));

                SkipNewlines();
                var body = ParseExpression();
                cases.Add(new AstMatchCase(v, cond, body, new Location(v.Beginning, body.End)));

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

            return new AstMatchExpr(value, cases, Location: new Location(beg, end));
        }

        private AstStatement ParseEnumDeclaration()
        {
            TokenLocation beginning = null, end = null;
            AstIdExpr name;
            var members = new List<AstEnumMember>();
            List<AstParameter> parameters = null;

            beginning = NextToken().location;
            SkipNewlines();
            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'enum'"));



            SkipNewlines();
            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList(out var _, out var _);
                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "after name in enum declaration"));

            while (true)
            {
                SkipNewlines();

                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var memberName = ParseIdentifierExpr(ErrMsg("identifier", "at enum member"));
                next = PeekToken();

                AstExpression associatedType = null;
                AstExpression value = null;
                TokenLocation e = memberName.End;

                next = PeekToken();
                if (next.type == TokenType.Colon)
                {
                    NextToken();
                    SkipNewlines();
                    associatedType = ParseExpression();
                    e = associatedType.End;
                }

                next = PeekToken();
                if (next.type == TokenType.Equal)
                {
                    NextToken();
                    SkipNewlines();
                    value = ParseExpression();
                    e = value.End;
                }

                members.Add(new AstEnumMember(memberName, associatedType, value, new Location(memberName.Location.Beginning, e)));

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
            return new AstEnumDecl(name, members, parameters, Location: new Location(beginning, end));
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

        private List<AstDirective> ParseDirectives()
        {
            var result = new List<AstDirective>();

            while (CheckToken(TokenType.HashIdentifier))
            {
                result.Add(ParseDirective());
            }

            return result;
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

        private AstParameter ParseParameter(bool allowDefaultValue = true)
        {
            AstIdExpr pname = null;
            AstExpression ptype = null;
            AstExpression defaultValue = null;

            TokenLocation beg = null, end = null;

            var e = ParseExpression();
            beg = e.Beginning;
            SkipNewlines();

            // if next token is : then e is the name of the parameter
            if (CheckToken(TokenType.Colon))
            {
                if (e is AstIdExpr i)
                {
                    pname = i;
                }
                else
                {
                    ReportError(e, $"Name of parameter must be an identifier");
                }

                Consume(TokenType.Colon, ErrMsg(":", "after name in parameter"));
                SkipNewlines();

                ptype = ParseExpression();
            }
            else
            {
                ptype = e;
            }

            end = ptype.End;

            if (allowDefaultValue)
            {
                // optional default value
                SkipNewlines();
                if (CheckToken(TokenType.Equal))
                {
                    NextToken();
                    SkipNewlines();
                    defaultValue = ParseExpression();
                    end = defaultValue.End;
                }
            }

            return new AstParameter(pname, ptype, defaultValue, new Location(beg, end));
        }

        private List<AstParameter> ParseParameterList(out TokenLocation beg, out TokenLocation end, bool allowDefaultValue = true)
        {
            var parameters = new List<AstParameter>();

            beg = Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of parameter list")).location;
            SkipNewlines();

            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                var a = ParseParameter(allowDefaultValue);
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
                    ReportError(next.location, $"Expected ',' or ')', got '{next}'");
                }
            }

            end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of parameter list")).location;

            return parameters;
        }

        private AstStructDecl ParseStructDeclaration()
        {
            TokenLocation beg = null, end = null;
            var members = new List<AstStructMember>();
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

                TokenLocation memberBeg = null;

                bool pub = false;
                bool get = false;
                if (next.type == TokenType.KwPub)
                {
                    memberBeg = next.location;
                    NextToken();
                    pub = true;
                    if (CheckToken(TokenType.KwConst))
                    {
                        get = true;
                        NextToken();
                    }
                }

                var mName = ParseIdentifierExpr(ErrMsg("identifier", "in struct member"));
                if (memberBeg == null)
                    memberBeg = mName.Location.Beginning;

                SkipNewlines();
                Consume(TokenType.Colon, ErrMsg(":", "after struct member name"));
                SkipNewlines();

                var mType = ParseExpression();
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

                var mem = new AstStructMember(mName, mType, init, new Location(memberBeg, memberEnd));
                mem.IsPublic = pub;
                mem.IsReadOnly = get;
                members.Add(mem);
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of struct declaration")).location;

            return new AstStructDecl(name, parameters, members, directives, new Location(beg, end));
        }

        private AstImplBlock ParseImplBlock()
        {
            TokenLocation beg = null, end = null;
            var functions = new List<AstFunctionDecl>();
            AstExpression target = null;
            AstExpression trait = null;
            List<AstParameter> parameters = null;
            List<ImplCondition> conditions = null;

            beg = Consume(TokenType.KwImpl, ErrMsg("keyword 'impl'", "at beginning of impl statement")).location;
            SkipNewlines();

            if (CheckToken(TokenType.OpenParen))
            {
                parameters = ParseParameterList(out var pbeg, out var pend, false);
                if (parameters.Count == 0)
                {
                    ReportError(new Location(pbeg, pend), $"impl parameter list can't be empty");
                    parameters = null;
                }
                SkipNewlines();
            }

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

            if (CheckToken(TokenType.KwIf))
            {
                NextToken();
                SkipNewlines();

                conditions = new List<ImplCondition>();

                if (!CheckToken(TokenType.OpenBrace))
                {
                    while (true)
                    {
                        var next = PeekToken();
                        if (next.type == TokenType.HashIdentifier && next.data as string == "notyet")
                        {
                            NextToken();
                            conditions.Add(new ImplConditionNotYet(new Location(next.location)));
                        }
                        else
                        {
                            var typ = ParseExpression();
                            SkipNewlines();
                            ConsumeUntil(TokenType.Colon, ErrMsg("':'", "after type in impl condition"));
                            SkipNewlines();
                            var trt = ParseExpression();
                            SkipNewlines();

                            conditions.Add(new ImplConditionImplTrait(typ, trt, new Location(typ.Beginning, trt.End)));
                        }

                        if (CheckToken(TokenType.Comma))
                        {
                            NextToken();
                            SkipNewlines();
                        }
                        else if (CheckToken(TokenType.OpenBrace))
                        {
                            break;
                        }
                        else
                        {
                            ReportError(PeekToken().location, $"Unexpected token {PeekToken()}, expected ',' or '{{'");
                        }
                    }
                }

                SkipNewlines();
            }

            Consume(TokenType.OpenBrace, ErrMsg("{", "after type"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;
                
                var f = ParseFunctionDeclaration();
                functions.Add(f);

                SkipNewlines();
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of impl statement")).location;

            return new AstImplBlock(parameters, target, trait, conditions, functions, new Location(beg, end));
        }

        public AstExprStmt ParseBlockStatement()
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
                if (s != null)
                {
                    statements.Add(s);

                    next = PeekToken();

                    if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                        break;

                    switch (s)
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
            AstExpression value = null;

            beg = Consume(TokenType.KwTypedef, ErrMsg("keyword 'typedef'", "at beginning of type alias declaration")).location;
            SkipNewlines();

            name = ParseIdentifierExpr(ErrMsg("identifier", "after keyword 'typedef'"));
            SkipNewlines();

            Consume(TokenType.Equal, ErrMsg("type expression", "after '=' in type alias declaration"));
            SkipNewlines();

            value = ParseExpression();
            end = value.End;

            return new AstTypeAliasDecl(name, value, Location: new Location(beg, end));
        }

        private AstVariableDecl ParseVariableDeclaration(params TokenType[] delimiters)
        {
            TokenLocation beg = null, end = null;
            var directives = new List<AstDirective>();
            AstExpression pattern = null;
            AstExpression type = null;
            AstExpression init = null;
            bool isConst = false;

            beg = Consume(TokenType.KwLet, ErrMsg("keyword 'let'", "at beginning of variable declaration")).location;

            SkipNewlines();
            while (CheckToken(TokenType.HashIdentifier))
            {
                directives.Add(ParseDirective());
                SkipNewlines();
            }

            if (CheckToken(TokenType.KwConst))
            {
                isConst = true;
                NextToken();
                SkipNewlines();
            }

            pattern = ParseExpression();
            end = pattern.End;

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

            return new AstVariableDecl(pattern, type, init, isConst, directives, new Location(beg, end));
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
                post = ParseStatement(false);
                SkipNewlines();
            }

            body = ParseBlockExpr();

            return new AstWhileStmt(condition, body, init, post, new Location(beg, body.End));
        }

        private AstIfExpr ParseIfExpr()
        {
            TokenLocation beg = null, end = null;
            AstExpression condition = null;
            AstNestedExpression ifCase = null;
            AstNestedExpression elseCase = null;
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

            if (CheckToken(TokenType.KwThen))
            {
                NextToken();
                SkipNewlines();
                var stmt = ParseStatement(false);
                ifCase = new AstBlockExpr(new List<AstStatement> { stmt }, stmt.Location);
            }
            else
            {
                ifCase = ParseBlockExpr();
            }
            end = ifCase.End;


            SkipNewlines();
            if (CheckToken(TokenType.KwElse))
            {
                NextToken();
                SkipNewlines();


                if (CheckToken(TokenType.KwIf))
                    elseCase = ParseIfExpr();
                else if (CheckToken(TokenType.OpenBrace))
                    elseCase = ParseBlockExpr();
                else
                {
                    var stmt = ParseStatement(false);
                    elseCase = new AstBlockExpr(new List<AstStatement> { stmt }, stmt.Location);
                }
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

            beginning = ConsumeUntil(TokenType.KwFn, ErrMsgUnexpected("keyword 'fn'", "beginning of function declaration")).location;
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

        private AstExpression ParseFunctionTypeExpr()
        {
            var args = new List<AstExpression>();
            AstExpression returnType = null;

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

            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                returnType = ParseExpression();
                end = returnType.End;
            }

            var dirs = ParseDirectives();

            return new AstFunctionTypeExpr(args, returnType, dirs, new Location(beginning, end));
        }

        public AstExpression ParseExpression(ErrorMessageResolver errorMessage = null)
        {
            errorMessage = errorMessage ?? (t => $"Unexpected token '{t}' in expression");

            var expr = ParseOrExpression(errorMessage);

            if (CheckToken(TokenType.KwImpl))
            {
                NextToken();
                SkipNewlines();
                var trait = ParseExpression();
                return new AstImplTraitTypeExpr(expr, trait, new Location(expr.Beginning, trait.End));
            }

            return expr;
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

            var e = ParseExpression();
            beg = e.Beginning;
            SkipNewlines();

            // if next token is : then e is the name of the parameter
            if (CheckToken(TokenType.Equal))
            {
                if (e is AstIdExpr i && !i.IsPolymorphic)
                {
                    name = i;
                }
                else
                {
                    ReportError(e, $"Name of argument must be an identifier");
                }

                Consume(TokenType.Equal, ErrMsg("=", "after name in argument"));
                SkipNewlines();

                expr = ParseExpression();
            }
            else
            {
                expr = e;
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
                SkipNewlines();
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

        private AstExpression ParseStructValue()
        {
            TokenLocation beg = null, end = null;
            List<AstStructMemberInitialization> members = new List<AstStructMemberInitialization>();
            AstExpression type = null;

            beg = Consume(TokenType.KwNew, ErrMsg("keyword 'new'", "at beginning of struct value expression")).location;

            SkipNewlines();
            var maybeOpenBrace = PeekToken();
            if (maybeOpenBrace.type != TokenType.OpenBrace)
            {
                type = ParseExpression();
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

        public AstExpression ParseEmptyExpression()
        {
            var loc = GetWhitespaceLocation();
            return new AstEmptyExpr(new Location(loc.beg, loc.end));
        }

        private AstExpression ParseTupleExpression()
        {
            var list = ParseParameterList(out var beg, out var end, allowDefaultValue: false);

            bool isType = false;
            foreach (var v in list)
            {
                if (v.Name != null)
                    isType = true;
            }

            if (!isType)
            {
                if (list.Count == 0)
                    ReportError(new Location(beg, end), $"Invalid expression");
                else if (list.Count == 1)
                {
                    var expr = list[0].TypeExpr;
                    expr.Location = new Location(beg, end);
                    return expr;
                }
            }

            return new AstTupleExpr(list, new Location(beg, end));
        }

        private AstExpression ParseArrayOrSliceExpression()
        {
            var token = NextToken();
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

            if (IsTypeExprToken())
            {
                var target = ParseExpression();
                if (values.Count == 0)
                    return new AstSliceTypeExpr(target, new Location(token.location, target.End));
                else
                {
                    if (values.Count > 1)
                        ReportError(new Location(values), $"Too many expressions in array type expression");
                    return new AstArrayTypeExpr(target, values[0], new Location(token.location, target.End));
                }
            }

            return new AstArrayExpr(values, new Location(token.location, end));
        }

        private AstExpression ParseLambdaExpr()
        {
            var beg = ConsumeUntil(TokenType.Pipe, ErrMsg("|", "at beginning of lambda")).location;
            var parameters = new List<AstParameter>();
            AstExpression retType = null;

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();

                if (next.type == TokenType.Identifier)
                {
                    NextToken();
                    var name = new AstIdExpr(next.data as string, false, new Location(next.location));
                    AstExpression type = null;
                    var end = name.End;

                    SkipNewlines();
                    if (CheckToken(TokenType.Colon))
                    {
                        NextToken();
                        SkipNewlines();
                        type = ParseExpression();
                        end = type.End;
                        SkipNewlines();
                    }

                    parameters.Add(new AstParameter(name, type, null, new Location(name.Beginning, end)));

                    next = PeekToken();
                    if (next.type == TokenType.Comma)
                    {
                        NextToken();
                        SkipNewlines();
                    }
                }
                else if (next.type == TokenType.Pipe || next.type == TokenType.EOF)
                {
                    break;
                }
                else
                {
                    NextToken();
                    ReportError(next.location, $"Unexpected token {next.type} in parameter list of lamda");
                }
            }

            ConsumeUntil(TokenType.Pipe, ErrMsg("|", "in lambda"));

            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                SkipNewlines();
                retType = ParseExpression();
            }

            var body = ParseExpression();

            return new AstLambdaExpr(parameters, body, retType, new Location(beg, body.End));
        }

        private AstExpression ParseMacro(Token name)
        {
            if (name == null)
            {
                name = ConsumeUntil(TokenType.Identifier, ErrMsg("name", "at beginning of macro"));
            }

            var beg = name.location;
            TokenLocation end;

            end = ConsumeUntil(TokenType.Bang, ErrMsg("!", "after name of macro")).location;

            var tokens = new List<Token>();
            var stack = new Stack<TokenType>();

            bool startsWithParen = false;

            while (true) {
                var next = PeekToken();
                switch (next.type)
                {
                    case TokenType.OpenBrace:
                    case TokenType.OpenBracket:
                    case TokenType.OpenParen:
                        stack.Push(next.type);

                        if (tokens.Count == 0)
                            startsWithParen = true;
                        break;

                    case TokenType.ClosingBrace:
                    case TokenType.ClosingBracket:
                    case TokenType.ClosingParen:
                        if (stack.Count == 0)
                        {
                            goto end;
                        }
                        else if ((stack.Peek() == TokenType.OpenBrace && next.type == TokenType.ClosingBrace) ||
                            (stack.Peek() == TokenType.OpenBracket && next.type == TokenType.ClosingBracket) ||
                            (stack.Peek() == TokenType.OpenParen && next.type == TokenType.ClosingParen))
                        {
                            stack.Pop();

                            if (startsWithParen && stack.Count == 0)
                            {
                                tokens.Add(NextToken());
                                goto end;
                            }
                        }
                        else
                        {
                            ReportError(next.location, $"Unexpected token {next.type} in macro. Parenthesis have to be balanced!");
                        }
                        break;

                    case TokenType.NewLine:
                        if (stack.Count == 0)
                        {
                            goto end;
                        }
                        break;
                }

                tokens.Add(NextToken());
            }

            end:

            end = tokens.LastOrDefault()?.location ?? end;

            if (startsWithParen)
            {
                tokens.RemoveAt(tokens.Count - 1);
                tokens.RemoveAt(0);
            }

            return new AstMacroExpr(new AstIdExpr(name.data as string, false, new Location(name.location)), tokens, new Location(beg, end));
        }

        private AstExpression ParseAtomicExpression(ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.KwDefault:
                    NextToken();
                    return new AstDefaultExpr(new Location(token.location));

                case TokenType.Pipe:
                    return ParseLambdaExpr();

                case TokenType.KwNew:
                    return ParseStructValue();

                case TokenType.KwNull:
                    NextToken();
                    return new AstNullExpr(new Location(token.location));

                case TokenType.ReplaceIdentifier:
                    NextToken();
                    if (Replacements == null)
                    {
                        ReportError(token.location, $"No replacements defined");
                        return ParseEmptyExpression();
                    }
                    else if (Replacements.ContainsKey(token.data as string))
                    {
                        return Replacements[token.data as string];
                    }
                    else
                    {
                        ReportError(token.location, $"No replacement '{token.data as string}' defined");
                        return ParseEmptyExpression();
                    }

                case TokenType.AtSignIdentifier:
                    {
                        NextToken();
                        var args = ParseArgumentList(out var end);
                        var name = new AstIdExpr((string)token.data, false, new Location(token.location));
                        return new AstCompCallExpr(name, args, new Location(token.location, end));
                    }

                case TokenType.OpenBracket:
                    return ParseArrayOrSliceExpression();

                case TokenType.DollarIdentifier:
                    NextToken();
                    return new AstIdExpr((string)token.data, true, new Location(token.location));

                case TokenType.Identifier:
                    NextToken();
                    if (CheckToken(TokenType.Bang))
                        return ParseMacro(token);
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

                case TokenType.KwMatch:
                    return ParseMatchExpr();

                case TokenType.OpenParen:
                    return ParseTupleExpression();

                case TokenType.KwRef:
                    NextToken();
                    SkipNewlines();
                    var target = ParseExpression();
                    return new AstReferenceTypeExpr(target, new Location(token.location, target.End));

                case TokenType.KwFn:
                    return ParseFunctionTypeExpr();


                case TokenType.KwCast:
                    {
                        var beg = token.location;
                        AstExpression type = null;

                        NextToken();
                        SkipNewlines();

                        var next = PeekToken();
                        if (next.type == TokenType.OpenParen)
                        {
                            NextToken();
                            SkipNewlines();
                            type = ParseExpression();
                            SkipNewlines();
                            Consume(TokenType.ClosingParen, ErrMsg("')'", "after type in cast expression"));
                            SkipNewlines();
                        }

                        var sub = ParseExpression();
                        return new AstCastExpr(type, sub, new Location(beg, sub.End));
                    }

                default:
                    //NextToken();
                    ReportError(token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
                    return ParseEmptyExpression();
            }
        }
        #endregion
    }
}
