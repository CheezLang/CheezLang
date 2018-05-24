using Cheez.Compiler.ParseTree;
using System;
using System.IO;
using System.Text;

namespace Cheez.Compiler.Parsing
{
    public enum TokenType
    {
        Unknown,

        NewLine,
        EOF,

        StringLiteral,
        NumberLiteral,

        Identifier,

        Semicolon,
        DoubleColon,
        Colon,
        Comma,
        Period,
        Equal,
        Ampersand,
        HashTag,

        Plus,
        Minus,
        Asterisk,
        ForwardSlash,
        Percent,

        Less,
        LessEqual,
        Greater,
        GreaterEqual,
        DoubleEqual,
        NotEqual,

        OpenParen,
        ClosingParen,

        OpenBrace,
        ClosingBrace,
        
        OpenBracket,
        ClosingBracket,

        KwReturn,
        KwCast,
        KwRef,
        KwFn,
        KwStruct,
        KwEnum,
        KwImpl,
        KwConstant,
        KwVar,
        KwIf,
        KwElse,
        KwFor,
        KwWhile,
        KwAnd,
        KwOr,
        KwTrue,
        KwFalse,
        KwUsing,
        KwPrint, // @Temporary
        KwPrintln, // @Temporary
    }

    public class TokenLocation
    {
        public string file;
        public int line;
        public int index;
        public int end;
        public int lineStartIndex;

        public TokenLocation()
        {

        }

        public TokenLocation Clone()
        {
            return new TokenLocation
            {
                file = file,
                line = line,
                index = index,
                end = end,
                lineStartIndex = lineStartIndex
            };
        }

        public override string ToString()
        {
            return $"{file} ({line}:{index-lineStartIndex+1})";
        }
    }

    public class Token
    {
        public TokenType type;
        public TokenLocation location;
        public object data;

        public override string ToString()
        {
            return $"({location.line}:{location.index - location.lineStartIndex}) ({type}) {data}";
        }
    }

    public struct NumberData
    {
        public enum NumberType
        {
            Float,
            Int
        }

        public int IntBase;
        public string Suffix;
        public string StringValue;
        public object Value;
        public NumberType Type;

        public override string ToString()
        {
            return StringValue;
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

        private char Current => mText[mLocation.index];
        private char Next => mLocation.index < mText.Length - 1 ? mText[mLocation.index + 1] : (char)0;
        private char Prev => mLocation.index > 0 ? mText[mLocation.index - 1] : (char)0;
        private Token peek = null;

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

        public Token PeekToken()
        {
            if (peek == null)
            {
                peek = NextToken();
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
                case '>' when Next == '=': SimpleToken(ref token, TokenType.GreaterEqual, 2); break;
                case ':' when Next == ':': SimpleToken(ref token, TokenType.DoubleColon, 2); break;
                case ':': SimpleToken(ref token, TokenType.Colon); break;
                case ';': SimpleToken(ref token, TokenType.Semicolon); break;
                case '.': SimpleToken(ref token, TokenType.Period); break;
                case '#': SimpleToken(ref token, TokenType.HashTag); break;
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

                case '"': ParseStringLiteral(ref token); break;

                case char cc when IsDigit(cc):
                    ParseNumberLiteral(ref token);
                    break;

                case char cc when IsIdentBegin(cc):
                    ParseIdentifier(ref token);
                    CheckKeywords(ref token);
                    break;
            }

            token.location.end = mLocation.index;
            return token;
        }

        private void ParseStringLiteral(ref Token token)
        {
            token.type = TokenType.StringLiteral;
            int start = mLocation.index++;
            StringBuilder sb = new StringBuilder();

            bool foundEnd = false;
            while (mLocation.index < mText.Length)
            {
                char c = Current;
                mLocation.index++;
                if (c == '"')
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
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case '0': sb.Append('\0'); break;
                        default: sb.Append(Current); break;
                    }
                    mLocation.index++;
                    continue;
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
                case "cast": token.type = TokenType.KwCast; break;
                case "ref": token.type = TokenType.KwRef; break;
                case "fn": token.type = TokenType.KwFn; break;
                case "struct": token.type = TokenType.KwStruct; break;
                case "impl": token.type = TokenType.KwImpl; break;
                case "constant": token.type = TokenType.KwConstant; break;
                case "let": token.type = TokenType.KwVar; break;
                case "if": token.type = TokenType.KwIf; break;
                case "else": token.type = TokenType.KwElse; break;
                case "for": token.type = TokenType.KwFor; break;
                case "while": token.type = TokenType.KwWhile; break;
                case "and": token.type = TokenType.KwAnd; break;
                case "or": token.type = TokenType.KwOr; break;
                case "true": token.type = TokenType.KwTrue; break;
                case "false": token.type = TokenType.KwFalse; break;
                case "using": token.type = TokenType.KwUsing; break;
                case "enum": token.type = TokenType.KwEnum; break;
                case "print": token.type = TokenType.KwPrint; break; // @Temporary
                case "println": token.type = TokenType.KwPrintln; break; // @Temporary
            }
        }

        private void ParseIdentifier(ref Token token)
        {
            token.type = TokenType.Identifier;

            int start = mLocation.index;
            while (mLocation.index < mText.Length && IsIdent(Current))
            {
                mLocation.index++;
            }

            token.data = mText.Substring(start, mLocation.index - start);
        }

        private void ParseNumberLiteral(ref Token token)
        {
            token.type = TokenType.NumberLiteral;
            NumberData data = new NumberData();
            data.IntBase = 10;
            data.Suffix = "";
            data.StringValue = "";
            data.Value = null;
            data.Type = NumberData.NumberType.Int;

            const int StateInit = 0;
            const int State0 = 1;
            const int StateX = 2;
            const int StateB = 3;
            const int StateDecimalDigit = 5;
            const int StateBinaryDigit = 6;
            const int StateHexDigit = 7;
            const int StatePostfix = 8;
            const int StateDone = 9;
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
                                data.StringValue += '0';
                                state = State0;
                            }
                            else if (IsDigit(c))
                            {
                                data.StringValue += c;
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
                                data.IntBase = 16;
                                data.StringValue = "";
                                state = StateX;
                            }
                            else if (c == 'b')
                            {
                                data.IntBase = 2;
                                data.StringValue = "";
                                state = StateB;
                            }
                            else if (IsDigit(c))
                            {
                                data.StringValue += c;
                                state = StateDecimalDigit;
                            }
                            else if (IsIdentBegin(c))
                            {
                                data.Suffix += c;
                                state = StatePostfix;
                            }
                            else
                            {
                                state = StateDone;
                            }
                            break;
                        }

                    case StatePostfix:
                        {
                            if (IsIdent(c))
                            {
                                data.Suffix += c;
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
                                data.StringValue += c;
                            else if (IsIdentBegin(c))
                            {
                                data.Suffix += c;
                                state = StatePostfix;
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
                                data.StringValue += c;
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
                                data.StringValue += c;
                            else if (IsIdentBegin(c))
                            {
                                data.Suffix += c;
                                state = StatePostfix;
                            }
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
                                data.StringValue += c;
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
                                data.StringValue += c;
                            else if (IsDigit(c))
                            {
                                error = "Invalid character, expected binary digit";
                                state = -1;
                            }
                            else if (IsIdentBegin(c))
                            {
                                data.Suffix += c;
                                state = StatePostfix;
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



            token.data = data;
        }

        private bool IsBinaryDigit(char c)
        {
            return c == '0' || c == '1';
        }

        private bool IsHexDigit(char c)
        {
            return IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c >= 'F');
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
