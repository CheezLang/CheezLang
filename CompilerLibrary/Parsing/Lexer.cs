using System.IO;
using System.Numerics;
using System.Text;
using System;
using System.Globalization;
using System.Collections.Generic;
using Cheez.Ast;
using Cheez.Extras;

namespace Cheez.Parsing
{
    public enum TokenType
    {
        Unknown,

        NewLine,
        EOF,

        StringLiteral,
        CharLiteral,
        NumberLiteral,

        Identifier,
        DollarIdentifier,
        HashIdentifier,
        AtSignIdentifier,

        Semicolon,
        DoubleColon,
        Colon,
        Comma,
        Period,
        Equal,
        Ampersand,

        Bang,

        Plus,
        Minus,
        Asterisk,
        ForwardSlash,
        Percent,

        AddEq,
        SubEq,
        MulEq,
        DivEq,
        ModEq,

        Less,
        LessEqual,
        Greater,
        GreaterEqual,
        DoubleEqual,
        NotEqual,

        Arrow,
        LessLess,

        OpenParen,
        ClosingParen,

        OpenBrace,
        ClosingBrace,

        OpenBracket,
        ClosingBracket,

        KwReturn,
        KwNew,
        KwRef,
        KwFn,
        KwStruct,
        KwEnum,
        KwImpl,
        KwConstant,
        KwLet,
        KwTypedef,
        KwIf,
        KwElse,
        KwFor,
        KwWhile,
        KwAnd,
        KwOr,
        KwTrue,
        KwFalse,
        KwNull,
        KwUsing,
        KwDefer,
        KwMatch,
        KwBreak,
        KwContinue,
        KwTrait,
        KwCast,
        KwConst
    }

    public class Token
    {
        public TokenType type;
        public TokenLocation location;
        public object data;

        public string suffix;

        public override string ToString()
        {
            return $"({location.line}:{location.index - location.lineStartIndex}) ({type}) {data}";
        }
    }

    

    public interface IText
    {
        string Text { get; }
    }

    public class Lexer : IText
    {
        private string mText;
        private TokenLocation mLocation;

        private char Current => mLocation.index < mText.Length ? mText[mLocation.index] : (char)0;
        private char Next => mLocation.index < mText.Length - 1 ? mText[mLocation.index + 1] : (char)0;
        private char Prev => mLocation.index > 0 ? mText[mLocation.index - 1] : (char)0;
        private Token peek = null;

        private List<TokenLocation> previous = new List<TokenLocation>();

        public string Text => mText;

        private IErrorHandler mErrorHandler;

        public static Lexer FromFile(string fileName, IErrorHandler errorHandler)
        {
            if (!File.Exists(fileName))
            {
                errorHandler.ReportError($"'{fileName}' could not be found.");
                return null;
            }
            return new Lexer
            {
                mErrorHandler = errorHandler,
                mText = File.ReadAllText(fileName, Encoding.UTF8).Replace("\r\n", "\n"),
                mLocation = new TokenLocation
                {
                    file = fileName,
                    line = 1,
                    index = 0,
                    lineStartIndex = 0
                }
            };
        }

        public static Lexer FromString(string str, IErrorHandler errorHandler, string fileName = "string")
        {
            return new Lexer
            {
                mErrorHandler = errorHandler,
                mText = str.Replace("\r\n", "\n"),
                mLocation = new TokenLocation
                {
                    file = fileName,
                    line = 1,
                }
            };
        }

        public void UndoTokens(int count)
        {
            if (previous.Count == 0 || count > previous.Count) throw new NotImplementedException();
            previous.RemoveRange(0, count - 1);
            mLocation = previous[0];
            peek = null;
        }

        public Token PeekToken()
        {
            if (peek == null)
            {
                peek = NextToken();
                //UndoToken();
            }

            return peek;
        }

        public Token NextToken()
        {
            if (peek != null)
            {
                Token t = peek;
                peek = null;
                return t;
            }

            if (SkipWhitespaceAndComments(out TokenLocation loc))
            {
                loc.end = loc.index;
                Token tok = new Token();
                tok.location = loc;
                tok.type = TokenType.NewLine;
                return tok;
            }

            return ReadToken();
        }

        private Token ReadToken()
        {
            previous.Insert(0, mLocation.Clone());
            if (previous.Count > 2)
            {
                previous.RemoveAt(previous.Count - 1);
            }

            var token = new Token();
            token.location = mLocation.Clone();
            token.location.end = token.location.index;
            token.type = TokenType.EOF;
            if (mLocation.index >= mText.Length)
                return token;

            switch (Current)
            {
                case '=' when Next == '=': SimpleToken(ref token, TokenType.DoubleEqual, 2); break;
                case '!' when Next == '=': SimpleToken(ref token, TokenType.NotEqual, 2); break;
                case '<' when Next == '=': SimpleToken(ref token, TokenType.LessEqual, 2); break;
                case '<' when Next == '<': SimpleToken(ref token, TokenType.LessLess, 2); break;
                case '>' when Next == '=': SimpleToken(ref token, TokenType.GreaterEqual, 2); break;
                case ':' when Next == ':': SimpleToken(ref token, TokenType.DoubleColon, 2); break;
                case '-' when Next == '>': SimpleToken(ref token, TokenType.Arrow, 2); break;
                case '+' when Next == '=': SimpleToken(ref token, TokenType.AddEq, 2); break;
                case '-' when Next == '=': SimpleToken(ref token, TokenType.SubEq, 2); break;
                case '*' when Next == '=': SimpleToken(ref token, TokenType.MulEq, 2); break;
                case '/' when Next == '=': SimpleToken(ref token, TokenType.DivEq, 2); break;
                case '%' when Next == '=': SimpleToken(ref token, TokenType.ModEq, 2); break;
                case ':': SimpleToken(ref token, TokenType.Colon); break;
                case ';': SimpleToken(ref token, TokenType.Semicolon); break;
                case '.': SimpleToken(ref token, TokenType.Period); break;
                case '=': SimpleToken(ref token, TokenType.Equal); break;
                case '(': SimpleToken(ref token, TokenType.OpenParen); break;
                case ')': SimpleToken(ref token, TokenType.ClosingParen); break;
                case '{': SimpleToken(ref token, TokenType.OpenBrace); break;
                case '}': SimpleToken(ref token, TokenType.ClosingBrace); break;
                case '[': SimpleToken(ref token, TokenType.OpenBracket); break;
                case ']': SimpleToken(ref token, TokenType.ClosingBracket); break;
                case ',': SimpleToken(ref token, TokenType.Comma); break;
                case '&': SimpleToken(ref token, TokenType.Ampersand); break;
                case '*': SimpleToken(ref token, TokenType.Asterisk); break;
                case '/': SimpleToken(ref token, TokenType.ForwardSlash); break;
                case '+': SimpleToken(ref token, TokenType.Plus); break;
                case '%': SimpleToken(ref token, TokenType.Percent); break;
                case '-': SimpleToken(ref token, TokenType.Minus); break;
                case '<': SimpleToken(ref token, TokenType.Less); break;
                case '>': SimpleToken(ref token, TokenType.Greater); break;
                case '!': SimpleToken(ref token, TokenType.Bang); break;


                case '"': ParseStringLiteral(ref token, '"'); break;
                case '\'':
                    {
                        ParseStringLiteral(ref token, '\'');
                        token.type = TokenType.CharLiteral;
                        break;
                    }

                case char cc when IsDigit(cc):
                    ParseNumberLiteral(ref token);
                    break;

                case '$': ParseIdentifier(ref token, TokenType.DollarIdentifier); break;
                case '#': ParseIdentifier(ref token, TokenType.HashIdentifier); break;
                case '@': ParseIdentifier(ref token, TokenType.AtSignIdentifier); break;

                case char cc when IsIdentBegin(cc):
                    ParseIdentifier(ref token, TokenType.Identifier);
                    CheckKeywords(ref token);
                    break;

                default:
                    token.type = TokenType.Unknown;
                    mLocation.index += 1;
                    break;
            }

            if (token.type == TokenType.StringLiteral || token.type == TokenType.NumberLiteral || token.type == TokenType.CharLiteral)
            {
                if (IsAlpha(Current))
                {
                    token.suffix = "" + Current;
                    mLocation.index++;

                    while (IsIdent(Current))
                    {
                        token.suffix += Current;
                        mLocation.index++;
                    }
                }
            }

            token.location.end = mLocation.index;
            return token;
        }

        private void ParseStringLiteral(ref Token token, char end)
        {
            token.type = TokenType.StringLiteral;
            int start = mLocation.index++;
            StringBuilder sb = new StringBuilder();

            bool foundEnd = false;
            while (mLocation.index < mText.Length)
            {
                char c = Current;
                mLocation.index++;
                if (c == end)
                {
                    foundEnd = true;
                    break;
                }
                else if (c == '`')
                {
                    if (mLocation.index >= mText.Length)
                    {
                        mErrorHandler.ReportError(this, new Location(mLocation), $"Unexpected end of file while parsing string literal");
                        token.data = sb.ToString();
                        return;
                    }
                    switch (Current)
                    {
                        case '0': sb.Append('\0'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        default: sb.Append(Current); break;
                    }
                    mLocation.index++;
                    continue;
                }

                if (c == '\n')
                {
                    mLocation.line++;
                    mLocation.lineStartIndex = mLocation.index;
                }

                sb.Append(c);
            }

            if (!foundEnd)
            {
                mErrorHandler.ReportError(this, new Location(mLocation), $"Unexpected end of string literal");
            }

            token.data = sb.ToString();
        }

        private void SimpleToken(ref Token token, TokenType type, int len = 1)
        {
            token.type = type;
            mLocation.index += len;
        }

        private void CheckKeywords(ref Token token)
        {
            switch (token.data as string)
            {
                case "return": token.type = TokenType.KwReturn; break;
                case "new": token.type = TokenType.KwNew; break;
                case "ref": token.type = TokenType.KwRef; break;
                case "fn": token.type = TokenType.KwFn; break;
                case "struct": token.type = TokenType.KwStruct; break;
                case "impl": token.type = TokenType.KwImpl; break;
                case "constant": token.type = TokenType.KwConstant; break;
                case "let": token.type = TokenType.KwLet; break;
                case "typedef": token.type = TokenType.KwTypedef; break;
                case "if": token.type = TokenType.KwIf; break;
                case "else": token.type = TokenType.KwElse; break;
                case "for": token.type = TokenType.KwFor; break;
                case "while": token.type = TokenType.KwWhile; break;
                case "and": token.type = TokenType.KwAnd; break;
                case "or": token.type = TokenType.KwOr; break;
                case "true": token.type = TokenType.KwTrue; break;
                case "false": token.type = TokenType.KwFalse; break;
                case "null": token.type = TokenType.KwNull; break;
                case "use": token.type = TokenType.KwUsing; break;
                case "defer": token.type = TokenType.KwDefer; break;
                case "enum": token.type = TokenType.KwEnum; break;
                case "match": token.type = TokenType.KwMatch; break;
                case "break": token.type = TokenType.KwBreak; break;
                case "continue": token.type = TokenType.KwContinue; break;
                case "trait": token.type = TokenType.KwTrait; break;
                case "cast": token.type = TokenType.KwCast; break;
                case "const": token.type = TokenType.KwConst; break;
            }
        }

        private void ParseIdentifier(ref Token token, TokenType idtype)
        {
            token.type = idtype;

            int start = mLocation.index;

            switch (idtype)
            {
                case TokenType.AtSignIdentifier:
                case TokenType.DollarIdentifier:
                case TokenType.HashIdentifier:
                    mLocation.index++;
                    start++;
                    break;
            }

            while (mLocation.index < mText.Length && IsIdent(Current))
            {
                mLocation.index++;
            }

            token.data = mText.Substring(start, mLocation.index - start);
        }

        private void ParseNumberLiteral(ref Token token)
        {
            token.type = TokenType.NumberLiteral;
            var dataIntBase = 10;
            var dataStringValue = "";
            var dataType = NumberData.NumberType.Int;

            const int StateInit = 0;
            const int State0 = 1;
            const int StateX = 2;
            const int StateB = 3;
            const int StateDecimalDigit = 5;
            const int StateBinaryDigit = 6;
            const int StateHexDigit = 7;
            const int StateDone = 9;
            const int StateFloatPoint = 10;
            const int StateFloatDigit = 11;
            int state = StateInit;
            string error = null;


            while (mLocation.index < mText.Length && state != -1 && state != StateDone)
            {
                char c = Current;

                switch (state)
                {
                    case StateInit:
                        {
                            if (c == '0')
                            {
                                dataStringValue += '0';
                                state = State0;
                            }
                            else if (IsDigit(c))
                            {
                                dataStringValue += c;
                                state = StateDecimalDigit;
                            }
                            else
                            {
                                state = -1;
                                error = "THIS SHOULD NOT HAPPEN!";
                            }
                            break;
                        }



                    case State0:
                        {
                            if (c == 'x')
                            {
                                dataIntBase = 16;
                                dataStringValue = "";
                                state = StateX;
                            }
                            else if (c == 'b')
                            {
                                dataIntBase = 2;
                                dataStringValue = "";
                                state = StateB;
                            }
                            else if (IsDigit(c))
                            {
                                dataStringValue += c;
                                state = StateDecimalDigit;
                            }
                            else if (c == '.')
                            {
                                dataStringValue += c;
                                state = StateFloatPoint;
                                dataType = NumberData.NumberType.Float;
                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StateDecimalDigit:
                        {
                            if (IsDigit(c))
                                dataStringValue += c;
                            else if (c == '.')
                            {
                                dataStringValue += c;
                                state = StateFloatPoint;
                                dataType = NumberData.NumberType.Float;

                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StateFloatPoint:
                        {
                            if (IsDigit(c))
                            {
                                dataStringValue += c;
                                state = StateFloatDigit;
                            }
                            else
                            {
                                error = "Invalid character, expected digit";
                                state = -1;
                            }
                            break;
                        }

                    case StateFloatDigit:
                        {
                            if (IsDigit(c))
                            {
                                dataStringValue += c;
                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StateX:
                        {
                            if (IsHexDigit(c))
                            {
                                dataStringValue += c;
                                state = StateHexDigit;
                            }
                            else
                            {
                                error = "Invalid character, expected hex digit";
                                state = -1;
                            }
                            break;
                        }

                    case StateHexDigit:
                        {
                            if (IsHexDigit(c))
                                dataStringValue += c;
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StateB:
                        {
                            if (IsBinaryDigit(c))
                            {
                                dataStringValue += c;
                                state = StateBinaryDigit;
                            }
                            else if (IsDigit(c))
                            {
                                error = "Invalid character, expected binary digit";
                                state = -1;
                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StateBinaryDigit:
                        {
                            if (IsBinaryDigit(c))
                                dataStringValue += c;
                            else if (IsDigit(c))
                            {
                                error = "Invalid character, expected binary digit";
                                state = -1;
                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }
                }

                if (state != StateDone)
                {
                    mLocation.index++;
                }
            }

            if (state == -1)
            {
                token.type = TokenType.Unknown;
                token.data = error;
                return;
            }



            token.data = new NumberData(dataType, dataStringValue, dataIntBase);
        }

        private bool IsBinaryDigit(char c)
        {
            return c == '0' || c == '1';
        }

        private bool IsHexDigit(char c)
        {
            return IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsIdentBegin(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsIdent(char c)
        {
            return IsIdentBegin(c) || (c >= '0' && c <= '9');
        }

        private bool SkipWhitespaceAndComments(out TokenLocation loc)
        {
            loc = null;

            while (mLocation.index < mText.Length)
            {
                char c = Current;
                if (c == '/' && Next == '*')
                {
                    ParseMultiLineComment();
                }

                else if (c == '/' && Next == '/')
                {
                    ParseSingleLineComment();
                }

                else if (c == ' ' || c == '\t')
                {
                    mLocation.index++;
                }

                else if (c == '\r')
                {
                    mLocation.index++;
                }

                else if (c == '\n')
                {
                    if (loc == null)
                    {
                        loc = mLocation.Clone();
                    }

                    mLocation.line++;
                    mLocation.index++;
                    mLocation.lineStartIndex = mLocation.index;
                }

                else break;
            }

            if (loc != null)
            {
                loc.end = mLocation.index;
                return true;
            }

            return false;
        }

        private void ParseSingleLineComment()
        {
            while (mLocation.index < mText.Length)
            {
                if (Current == '\n')
                    break;
                mLocation.index++;
            }
        }

        private void ParseMultiLineComment()
        {
            int level = 0;
            while (mLocation.index < mText.Length)
            {
                char curr = Current;
                char next = Next;
                mLocation.index++;

                if (curr == '/' && next == '*')
                {
                    mLocation.index++;
                    level++;
                }

                else if (curr == '*' && next == '/')
                {
                    mLocation.index++;
                    level--;

                    if (level == 0)
                        break;
                }

                else if (curr == '\n')
                {
                    mLocation.line++;
                    mLocation.lineStartIndex = mLocation.index;
                }
            }
        }
    }
}
