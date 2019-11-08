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
        private delegate AstExpression ExpressionParser(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e);

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
            return p.ParseExpression(true);
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
        private static ErrorMessageResolver ErrMsgUnexpected(string expect, string where = null)
        {
            return t => $"Unexpected token {t} at {where}. Expected {expect}";
        }


        //[SkipInStackFrame]
        //[DebuggerStepThrough]
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
                case TokenType.Kwfn:
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

        private void RecoverUntil(params TokenType[] types)
        {
            while (true)
            {
                var next = PeekToken();
                if (types.Contains(next.type))
                    return;
                NextToken();
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
                block.SetFlag(ExprFlags.Anonymous, true);
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
                case TokenType.KwWhile:
                    return ParseWhileStatement();
                case TokenType.KwFor:
                    return ParseForStatement();
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
                        var expr = ParseExpression(true);
                        if (expr is AstEmptyExpr)
                        {
                            NextToken();
                            return new AstEmptyStatement(expr.Location);
                        }
                        if (CheckToken(TokenType.Colon))
                        {
                            return ParseDeclaration(expr, true);
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
                            var val = ParseExpression(true);
                            return new AstAssignment(expr, val, op, new Location(expr.Beginning, val.End));
                        }
                        else
                        {
                            return new AstExprStmt(expr, new Location(expr.Beginning, expr.End));
                        }
                    }
            }
        }

        private AstDecl ParseDeclaration(AstExpression expr, bool allowCommaTuple)
        {
            if (expr == null)
                expr = ParseExpression(allowCommaTuple);

            Consume(TokenType.Colon, ErrMsg(":", "after pattern in declaration"));

            AstExpression typeExpr = null;

            // constant declaration
            if (!CheckTokens(TokenType.Colon, TokenType.Equal))
            {
                typeExpr = ParseExpression(allowCommaTuple);
            }

            // constant declaration
            if (CheckToken(TokenType.Colon))
            {
                NextToken();
                var init = ParseExpression(allowCommaTuple, allowFunctionExpression: true);
                return new AstConstantDeclaration(expr, typeExpr, init, Location: new Location(expr.Beginning, init.End));
            }

            // variable declaration
            if (CheckToken(TokenType.Equal))
            {
                NextToken();
                var init = ParseExpression(allowCommaTuple);
                return new AstVariableDecl(expr, typeExpr, init, Location: new Location(expr.Beginning, init.End));
            }

            // variable declaration without initializer
            if (CheckToken(TokenType.NewLine))
            {
                return new AstVariableDecl(expr, typeExpr, null, Location: new Location(expr.Beginning, typeExpr.End));
            }

            //
            ReportError(PeekToken().location, $"Unexpected token. Expected ':' or '=' or '\\n'");
            return new AstVariableDecl(expr, typeExpr, null, Location: expr);
        }

        private AstVariableDecl ParseVariableDeclaration(AstExpression expr)
        {
            Consume(TokenType.Colon, ErrMsg(":", "after pattern in declaration"));

            AstExpression typeExpr = null;

            // constant declaration
            if (!CheckTokens(TokenType.Equal))
            {
                typeExpr = ParseExpression(false);
            }

            // variable declaration
            ConsumeUntil(TokenType.Equal, ErrMsg("=", "in variable declaration"));
            var init = ParseExpression(false);
            return new AstVariableDecl(expr, typeExpr, init, Location: new Location(expr.Beginning, init.End));
        }

        private AstStatement ParseTraitDeclaration()
        {
            TokenLocation beg = null, end = null;
            AstIdExpr name = null;
            var functions = new List<AstFuncExpr>();
            var variables = new List<AstVariableDecl>();
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

                var vname = ParseIdentifierExpr(ErrMsg("identifier"));
                var decl = ParseDeclaration(vname, false);

                if (decl is AstVariableDecl v)
                    variables.Add(v);
                else if (decl is AstConstantDeclaration con)
                {
                    if (con.Initializer is AstFuncExpr func)
                    {
                        func.Name = vname.Name;
                        functions.Add(func);
                    }
                    else
                    {
                        ReportError(con.Initializer, "Expected constant function expression");
                    }
                }
                else
                {
                    ReportError(decl, "Expected constant function expression");
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

            value = ParseExpression(true);
            SkipNewlines();

            Consume(TokenType.OpenBrace, ErrMsg("{", "after value in match statement"));

            while (true)
            {
                SkipNewlines();
                var next = PeekToken();

                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var v = ParseExpression(true);
                SkipNewlines();

                AstExpression cond = null;
                if (CheckToken(TokenType.KwIf))
                {
                    NextToken();
                    cond = ParseExpression(true);
                    SkipNewlines();
                }

                Consume(TokenType.Arrow, ErrMsg("->", "after value in match case"));

                SkipNewlines();
                var body = ParseExpression(true);
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

        private AstStatement ParseUsingStatement()
        {
            var beg = Consume(TokenType.KwUsing, ErrMsg("keyword 'using'", "at beginning of using statement")).location;
            SkipNewlines();
            var expr = ParseExpression(true, errorMessage: ErrMsg("expression", "after keyword 'using'"));
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
                returnValue = ParseExpression(true);
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

                    var expr = ParseExpression(false);
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

        private AstParameter ParseParameter(bool allowCommaForTuple, bool allowDefaultValue = true)
        {
            AstIdExpr pname = null;
            AstExpression ptype = null;
            AstExpression defaultValue = null;

            TokenLocation beg = null, end = null;

            var e = ParseExpression(allowCommaForTuple);
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

                ptype = ParseExpression(allowCommaForTuple);
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
                    defaultValue = ParseExpression(allowCommaForTuple);
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

                var a = ParseParameter(false, allowDefaultValue);
                parameters.Add(a);

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
                    SkipNewlines();
                    ReportError(next.location, $"Expected ',' or ')', got '{next}'");
                }
            }

            end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of parameter list")).location;

            return parameters;
        }

        private AstImplBlock ParseImplBlock()
        {
            TokenLocation beg = null, end = null;
            var functions = new List<AstFuncExpr>();
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

            target = ParseExpression(true);
            SkipNewlines();

            if (CheckToken(TokenType.KwFor))
            {
                NextToken();
                SkipNewlines();
                trait = target;
                target = ParseExpression(true);
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
                        else if (next.type == TokenType.AtSignIdentifier)
                        {
                            var expr = ParseExpression(false);
                            conditions.Add(new ImplConditionAny(expr, new Location(next.location)));
                        }
                        else
                        {
                            var typ = ParseExpression(false);
                            SkipNewlines();
                            ConsumeUntil(TokenType.Colon, ErrMsg("':'", "after type in impl condition"));
                            SkipNewlines();
                            var trt = ParseExpression(false);
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

                var name = ParseIdentifierExpr();
                Consume(TokenType.Colon, null);
                Consume(TokenType.Colon, null);
                var f = ParseFuncExpr(null, null);
                f.Name = name.Name;
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
            var expr = ParseExpression(true);
            return new AstExprStmt(expr, new Location(expr.Beginning, expr.End));
        }

        private AstForStmt ParseForStatement()
        {
            AstIdExpr varName = null;
            AstIdExpr indexName = null;
            List<AstArgument> args = null;
            AstExpression collection;
            AstExpression body;
            AstIdExpr label = null;

            var beg = Consume(TokenType.KwFor, ErrMsg("keyword 'for'", "at beginning of for loop"));
            SkipNewlines();

            // parse arguments
            if (CheckToken(TokenType.OpenParen))
            {
                args = ParseArgumentList(out var _);
                if (args.Count == 0)
                    args = null;
                SkipNewlines();
            }

            // names
            if (CheckToken(TokenType.Identifier))
            {
                varName = ParseIdentifierExpr(ErrMsg("identifier"));
                SkipNewlines();
                if (CheckToken(TokenType.Comma))
                {
                    NextToken();
                    SkipNewlines();
                    indexName = ParseIdentifierExpr(ErrMsg("identifier"));
                    SkipNewlines();
                }
            }

            // :
            Consume(TokenType.Colon, ErrMsg("':'"));
            SkipNewlines();

            // collection
            collection = ParseExpression(true);
            SkipNewlines();

            if (CheckToken(TokenType.HashIdentifier))
            {
                var dir = ParseIdentifierExpr(identType: TokenType.HashIdentifier);
                if (dir.Name == "label")
                {
                    label = ParseIdentifierExpr();
                    SkipNewlines();
                }
                else
                {
                    ReportError(dir.Location, $"Unknown directive '{dir.Name}'");
                    RecoverUntil(TokenType.OpenBrace);
                }
            }

            body = ParseExpression(true);

            return new AstForStmt(varName, indexName, collection, body, args, label, new Location(beg.location, collection.End));
        }

        private AstWhileStmt ParseWhileStatement()
        {
            TokenLocation beg = null;
            AstExpression condition = null;
            AstBlockExpr body = null;
            AstVariableDecl init = null;
            AstStatement post = null;
            AstIdExpr label = null;

            beg = Consume(TokenType.KwWhile, ErrMsg("keyword 'while'", "at beginning of while statement")).location;
            SkipNewlines();

            condition = ParseExpression(false, errorMessage: ErrMsg("expression", "after keyword 'while'"));

            // new syntax for variable declaration
            if (CheckToken(TokenType.Colon))
            {
                init = ParseVariableDeclaration(condition);
                SkipNewlines();
                Consume(TokenType.Comma, ErrMsg(",", "after variable declaration in while statement"));
                SkipNewlines();
                condition = ParseExpression(false, errorMessage: ErrMsg("expression", "after keyword 'while'"));
            }

            SkipNewlines();

            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                SkipNewlines();
                post = ParseStatement(false);
                SkipNewlines();
            }

            if (CheckToken(TokenType.HashIdentifier))
            {
                var dir = ParseIdentifierExpr(identType: TokenType.HashIdentifier);
                if (dir.Name == "label")
                {
                    label = ParseIdentifierExpr();
                    SkipNewlines();
                }
                else
                {
                    ReportError(dir.Location, $"Unknown directive '{dir.Name}'");
                    RecoverUntil(TokenType.OpenBrace);
                }
            }

            body = ParseBlockExpr();

            return new AstWhileStmt(condition, body, init, post, label, new Location(beg, body.End));
        }

        private AstIfExpr ParseIfExpr(bool allowCommaForTuple)
        {
            TokenLocation beg = null, end = null;
            AstExpression condition = null;
            AstExpression ifCase = null;
            AstExpression elseCase = null;
            AstVariableDecl pre = null;
            bool isConstIf = false;

            beg = Consume(TokenType.KwIf, ErrMsg("keyword 'if'", "at beginning of if statement")).location;
            SkipNewlines();

            if (CheckToken(TokenType.KwConst)) {
                NextToken();
                SkipNewlines();
                isConstIf = true;
            }

            condition = ParseExpression(false, errorMessage: ErrMsg("expression", "after keyword 'if'"));

            // new syntax for variable declaration
            if (CheckToken(TokenType.Colon))
            {
                pre = ParseVariableDeclaration(condition);
                SkipNewlines();
                Consume(TokenType.Comma, ErrMsg(",", "after variable declaration in if expr"));
                SkipNewlines();
                condition = ParseExpression(false, errorMessage: ErrMsg("expression", "after keyword 'if'"));
            }

            SkipNewlines();

            bool allowNewline = false;
            if (CheckToken(TokenType.OpenBrace))
                ifCase = ParseExpression(allowCommaForTuple);
            else
            {
                Consume(TokenType.KwThen, ErrMsg("keyword 'then'", "after condition in if expression"));
                SkipNewlines();
                ifCase = ParseExpression(allowCommaForTuple);
                allowNewline = true;
            }
            end = ifCase.End;

            if (allowNewline)
                SkipNewlines();
            if (CheckToken(TokenType.KwElse))
            {
                NextToken();
                SkipNewlines();
                elseCase = ParseExpression(allowCommaForTuple);
                end = elseCase.End;
            }

            return new AstIfExpr(condition, ifCase, elseCase, pre, isConstIf, new Location(beg, end));
        }

        #region Expression Parsing

        private AstExpression ParseFunctionTypeExpr(bool allowCommaForTuple)
        {
            var args = new List<AstExpression>();
            AstExpression returnType = null;
            bool isFatFunction = false;

            var fn = NextToken();
            TokenLocation beginning = fn.location;
            if (fn.type == TokenType.Kwfn)
            {
                isFatFunction = false;
            }
            else if(fn.type == TokenType.KwFn)
            {
                isFatFunction = true;
            }
            else
            {
                ReportError(fn.location, $"Expected keyword 'fn' or 'Fn' at beginning of function type expression");
            }

            SkipNewlines();

            Consume(TokenType.OpenParen, ErrMsg("(", "after keyword 'fn'"));
            SkipNewlines();

            while (true)
            {
                SkipNewlines();
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;

                args.Add(ParseExpression(false));
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
                returnType = ParseExpression(allowCommaForTuple);
                end = returnType.End;
            }

            var dirs = ParseDirectives();

            return new AstFunctionTypeExpr(args, returnType, isFatFunction, dirs, new Location(beginning, end));
        }

        public AstExpression ParseExpression(bool allowCommaForTuple, bool allowFunctionExpression = false, ErrorMessageResolver errorMessage = null)
        {
            errorMessage = errorMessage ?? (t => $"Unexpected token '{t}' in expression");

            var expr = ParseOrExpression(false, allowFunctionExpression, errorMessage);

            if (allowCommaForTuple)
            {
                List<AstParameter> list = null;
                while (CheckToken(TokenType.Comma))
                {
                    if (list == null)
                    {
                        list = new List<AstParameter>();
                        list.Add(new AstParameter(null, expr, null, expr));
                    }

                    NextToken();

                    expr = ParseOrExpression(false, allowFunctionExpression, errorMessage);
                    list.Add(new AstParameter(null, expr, null, expr));
                }

                if (list != null)
                    expr = new AstTupleExpr(list, new Location(list.First().Beginning, list.Last().End));
            }

            return expr;
        }

        [DebuggerStepThrough]
        private AstExpression ParseOrExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseAndExpression, allowCommaForTuple, allowFunctionExpression, e,
                (TokenType.KwOr, "or"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseAndExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseComparisonExpression, allowCommaForTuple, allowFunctionExpression, e,
                (TokenType.KwAnd, "and"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseComparisonExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseRangeExpression, allowCommaForTuple, allowFunctionExpression, e,
                (TokenType.Less, "<"),
                (TokenType.LessEqual, "<="),
                (TokenType.Greater, ">"),
                (TokenType.GreaterEqual, ">="),
                (TokenType.DoubleEqual, "=="),
                (TokenType.NotEqual, "!="));
        }

        //[DebuggerStepThrough]
        private AstExpression ParseRangeExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            var lhs = ParseAddSubExpression(allowCommaForTuple, allowFunctionExpression, e);

            if (CheckToken(TokenType.PeriodPeriod))
            {
                NextToken();
                SkipNewlines();
                var rhs = ParseAddSubExpression(allowCommaForTuple, allowFunctionExpression, e);
                return new AstRangeExpr(lhs, rhs, new Location(lhs.Beginning, rhs.End));
            }

            return lhs;
        }

        [DebuggerStepThrough]
        private AstExpression ParseAddSubExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseMulDivExpression, allowCommaForTuple, allowFunctionExpression, e,
                (TokenType.Plus, "+"),
                (TokenType.Minus, "-"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseMulDivExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver e)
        {
            return ParseBinaryLeftAssociativeExpression(ParseUnaryExpression, allowCommaForTuple, allowFunctionExpression, e,
                (TokenType.Asterisk, "*"),
                (TokenType.ForwardSlash, "/"),
                (TokenType.Percent, "%"));
        }

        [DebuggerStepThrough]
        private AstExpression ParseBinaryLeftAssociativeExpression(ExpressionParser sub, bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver errorMessage, params (TokenType, string)[] types)
        {
            return ParseLeftAssociativeExpression(sub, allowCommaForTuple, allowFunctionExpression, errorMessage, type =>
            {
                foreach (var (t, o) in types)
                {
                    if (t == type)
                        return o;
                }

                return null;
            });
        }

        private AstExpression ParseLeftAssociativeExpression(
            ExpressionParser sub,
            bool allowCommaForTuple,
            bool allowFunctionExpression,
            ErrorMessageResolver errorMessage,
            Func<TokenType, string> tokenMapping)
        {
            var lhs = sub(allowCommaForTuple, allowFunctionExpression, errorMessage);
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
                rhs = sub(allowCommaForTuple, allowFunctionExpression, errorMessage);
                lhs = new AstBinaryExpr(op, lhs, rhs, new Location(lhs.Beginning, rhs.End));
            }
        }

        private AstExpression ParseUnaryExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver errorMessage = null)
        {
            var next = PeekToken();
            if (next.type == TokenType.Ampersand)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);
                return new AstAddressOfExpr(sub, new Location(next.location, sub.End));
            }
            else if (next.type == TokenType.LessLess)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);
                return new AstDereferenceExpr(sub, new Location(next.location, sub.End));
            }
            else if (next.type == TokenType.Minus || next.type == TokenType.Plus)
            {
                NextToken();
                SkipNewlines();
                var sub = ParseUnaryExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);
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
                var sub = ParseUnaryExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);
                return new AstUnaryExpr("!", sub, new Location(next.location, sub.End));
            }

            return ParsePostUnaryExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);
        }

        private AstArgument ParseArgumentExpression()
        {
            TokenLocation beg;
            AstExpression expr;
            AstIdExpr name = null;

            var e = ParseExpression(false);
            beg = e.Beginning;

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

                expr = ParseExpression(false);
            }
            else
            {
                expr = e;
            }

            return new AstArgument(expr, name, new Location(beg, expr.End));
        }

        private AstExpression ParsePostUnaryExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver errorMessage)
        {
            var expr = ParseAtomicExpression(allowCommaForTuple, allowFunctionExpression, errorMessage);

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

                                next = PeekToken();
                                if (next.type == TokenType.NewLine)
                                {
                                    NextToken();
                                }
                                else if (next.type == TokenType.Comma)
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

                            var args = new List<AstExpression>();
                            while (true)
                            {
                                var next = PeekToken();
                                if (next.type == TokenType.ClosingBracket || next.type == TokenType.EOF)
                                    break;
                                args.Add(ParseExpression(false));
                                SkipNewlines();

                                next = PeekToken();
                                if (next.type == TokenType.Comma)
                                {
                                    NextToken();
                                    SkipNewlines();
                                }
                                else if (next.type == TokenType.ClosingBracket)
                                    break;
                                else
                                {
                                    NextToken();
                                    ReportError(next.location, $"Failed to parse operator [], expected ',' or ']'");
                                    //RecoverExpression();
                                }
                            }
                            var end = Consume(TokenType.ClosingBracket, ErrMsg("]", "at end of [] operator")).location;
                            if (args.Count == 0)
                            {
                                ReportError(end, "At least one argument required");
                                args.Add(ParseEmptyExpression());
                            }
                            expr = new AstArrayAccessExpr(expr, args, new Location(expr.Beginning, end));
                        }
                        break;

                    case TokenType.Period:
                        {
                            NextToken();
                            SkipNewlines();
                            var right = ParseIdentifierExpr(ErrMsg("identifier", "after ."));

                            expr = new AstDotExpr(expr, right, new Location(expr.Beginning, right.End));
                            break;
                        }

                    default:
                        return expr;
                }
            }
        }

        private List<AstArgument> ParseArgumentList(out TokenLocation end)
        {
            Consume(TokenType.OpenParen, ErrMsg("(", "at beginning of argument list"));

            SkipNewlines();
            var args = new List<AstArgument>();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingParen || next.type == TokenType.EOF)
                    break;
                args.Add(ParseArgumentExpression());

                next = PeekToken();
                if (next.type == TokenType.NewLine)
                {
                    NextToken();
                }
                else if (next.type == TokenType.Comma)
                {
                    NextToken();
                    SkipNewlines();
                }
                else if (next.type == TokenType.ClosingParen)
                    break;
                else
                {
                    NextToken();
                    ReportError(next.location, $"Failed to parse argument list, expected ',' or ')'");
                }
            }
            end = Consume(TokenType.ClosingParen, ErrMsg(")", "at end of argument list")).location;

            return args;
        }

        private AstIdExpr ParseIdentifierExpr(ErrorMessageResolver customErrorMessage = null, TokenType identType = TokenType.Identifier)
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

        private AstFuncExpr ParseFuncExpr(List<AstParameter> parameters, Location paramsLocation)
        {
            if (parameters == null)
            {
                parameters = ParseParameterList(out var beg, out var end, true);
                paramsLocation = new Location(beg, end);
            }

            AstBlockExpr body = null;
            AstParameter returnType = null;

            // function decl with return type
            if (CheckToken(TokenType.Arrow))
            {
                NextToken();
                returnType = ParseParameter(true);
            }

            var directives = ParseDirectives();

            if (CheckToken(TokenType.Semicolon))
                NextToken(); // do nothing
            else
                body = ParseBlockExpr();
            return new AstFuncExpr(parameters, returnType, body, directives, Location: new Location(paramsLocation.Beginning, body?.End ?? paramsLocation.End), ParameterLocation: paramsLocation);
        }

        private AstExpression ParseTupleExpression(bool allowFunctionExpression)
        {
            var list = ParseParameterList(out var beg, out var end, allowDefaultValue: true);

            // function expression
            // hash identifier for directives
            if (allowFunctionExpression && CheckTokens(TokenType.Arrow, TokenType.OpenBrace, TokenType.HashIdentifier, TokenType.Semicolon))
            {
                return ParseFuncExpr(list, new Location(beg, end));
            }

            bool isType = false;
            foreach (var v in list)
            {
                if (v.Name != null)
                    isType = true;
            }

            if (!isType)
            {
                if (list.Count == 1)
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

                values.Add(ParseExpression(false));

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
                var target = ParseExpression(false);
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

        private AstExpression ParseLambdaExpr(bool allowCommaForTuple)
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
                        type = ParseExpression(false);
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
                retType = ParseExpression(false);
            }

            var body = ParseExpression(allowCommaForTuple);

            return new AstLambdaExpr(parameters, body, retType, new Location(beg, body.End));
        }

        private AstExpression ParseContinueExpr()
        {
            var token = NextToken();
            AstIdExpr name = null;
            if (CheckToken(TokenType.Identifier))
                name = ParseIdentifierExpr();
            return new AstContinueExpr(name, new Location(token.location, name?.Location?.End ?? token.location));
        }

        private AstExpression ParseBreakExpr()
        {
            var token = NextToken();
            AstIdExpr name = null;
            if (CheckToken(TokenType.Identifier))
                name = ParseIdentifierExpr();
            return new AstBreakExpr(name, new Location(token.location, name?.Location?.End ?? token.location));
        }

        private AstExpression ParseStructTypeExpression()
        {
            TokenLocation beg = null, end = null;
            var declarations = new List<AstDecl>();
            var directives = new List<AstDirective>();
            List<AstParameter> parameters = null;

            beg = Consume(TokenType.KwStruct, ErrMsg("keyword 'struct'", "at beginning of struct type")).location;

            if (CheckToken(TokenType.OpenParen))
                parameters = ParseParameterList(out var _, out var _);

            while (CheckToken(TokenType.HashIdentifier))
            {
                var dir = ParseDirective();
                if (dir != null)
                    directives.Add(dir);
            }

            ConsumeUntil(TokenType.OpenBrace, ErrMsg("{", "at beginning of struct body"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                declarations.Add(ParseDeclaration(null, true));

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
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of struct declaration")).location;

            return new AstStructTypeExpr(parameters, declarations, directives, new Location(beg, end));
        }

        private AstExpression ParseEnumTypeExpression()
        {
            TokenLocation beg = null, end = null;
            var declarations = new List<AstDecl>();
            var directives = new List<AstDirective>();
            List<AstParameter> parameters = null;

            beg = Consume(TokenType.KwEnum, ErrMsg("keyword 'enum'", "at beginning of enum type")).location;

            if (CheckToken(TokenType.OpenParen))
                parameters = ParseParameterList(out var _, out var _);

            while (CheckToken(TokenType.HashIdentifier))
            {
                var dir = ParseDirective();
                if (dir != null)
                    directives.Add(dir);
            }

            ConsumeUntil(TokenType.OpenBrace, ErrMsg("{", "at beginning of enum body"));

            SkipNewlines();
            while (true)
            {
                var next = PeekToken();
                if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                    break;

                var name = ParseIdentifierExpr();
                AstDecl declaration = null;
                if (CheckToken(TokenType.Colon))
                {
                    declaration = ParseDeclaration(name, true);
                }
                else if (CheckToken(TokenType.Equal))
                {
                    NextToken();
                    var value = ParseExpression(false);
                    declaration = new AstVariableDecl(name, null, value, Location: new Location(name.Beginning, value.End));
                }
                else
                {
                    declaration = new AstVariableDecl(name, null, null, Location: name.Location);
                }

                declarations.Add(declaration);

                next = PeekToken();
                if (next.type == TokenType.NewLine)
                {
                    SkipNewlines();
                }
                else if (next.type == TokenType.Comma)
                {
                    NextToken();
                    SkipNewlines();
                }
                else if (next.type == TokenType.ClosingBrace || next.type == TokenType.EOF)
                {
                    break;
                }
                else
                {
                    NextToken();
                    ReportError(next.location, $"Unexpected token {next} after enum member");
                }
            }

            end = Consume(TokenType.ClosingBrace, ErrMsg("}", "at end of enum declaration")).location;

            return new AstEnumTypeExpr(parameters, declarations, directives, new Location(beg, end));
        }

        private AstExpression ParseAtomicExpression(bool allowCommaForTuple, bool allowFunctionExpression, ErrorMessageResolver errorMessage)
        {
            var token = PeekToken();
            switch (token.type)
            {
                case TokenType.KwBreak:
                    return ParseBreakExpr();
                case TokenType.KwContinue:
                    return ParseContinueExpr();

                case TokenType.KwDefault:
                    NextToken();
                    return new AstDefaultExpr(new Location(token.location));

                case TokenType.Pipe:
                    return ParseLambdaExpr(allowCommaForTuple);

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
                        return Replacements[token.data as string].Clone();
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
                    return ParseIfExpr(allowCommaForTuple);

                case TokenType.KwMatch:
                    return ParseMatchExpr();

                case TokenType.OpenParen:
                    return ParseTupleExpression(allowFunctionExpression);
                    // {
                    //     var start = NextToken().location;
                    //     SkipNewlines();
                    //     if (CheckToken(TokenType.ClosingParen))
                    //     {
                    //         var end = NextToken().location;
                    //         return new AstTupleExpr(new List<AstParameter>(), new Location(start, end));
                    //     }
                    //     else
                    //     {
                    //         var expr = ParseExpression(true);
                    //         var end = ConsumeUntil(TokenType.ClosingParen, ErrMsg(")", "at end of tuple")).location;
                    //         return expr;
                    //     }
                    // }

                case TokenType.KwRef:
                    NextToken();
                    SkipNewlines();
                    var target = ParseExpression(allowCommaForTuple);
                    return new AstReferenceTypeExpr(target, new Location(token.location, target.End));

                case TokenType.Kwfn:
                case TokenType.KwFn:
                    return ParseFunctionTypeExpr(allowCommaForTuple);


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
                            type = ParseExpression(true);
                            SkipNewlines();
                            Consume(TokenType.ClosingParen, ErrMsg("')'", "after type in cast expression"));
                            SkipNewlines();
                        }

                        var sub = ParseExpression(allowCommaForTuple);
                        return new AstCastExpr(type, sub, new Location(beg, sub.End));
                    }

                case TokenType.KwStruct:
                    return ParseStructTypeExpression();

                case TokenType.KwEnum:
                    return ParseEnumTypeExpression();

                default:
                    //NextToken();
                    ReportError(token.location, errorMessage?.Invoke(token) ?? $"Failed to parse expression, unpexpected token ({token.type}) {token.data}");
                    return ParseEmptyExpression();
            }
        }
        #endregion
    }
}
