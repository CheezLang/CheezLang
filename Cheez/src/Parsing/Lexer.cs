using System;
using System.IO;
using System.Text;

namespace Cheez.Parsing
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

        OpenParen,
        ClosingParen,

        OpenBrace,
        ClosingBrace,

        KwFn,
        KwStruct,
        KwImpl,
        KwConstant,
        KwVar,
        KwIf,
        KwElse,
        KwFor,
        KwWhile,
        KwPrint, // @Temporary
    }

    public class LocationInfo
    {
        public string file;
        public int line;
        public int column;

        public LocationInfo Clone()
        {
            return new LocationInfo
            {
                file = file,
                line = line,
                column = column
            };
        }

        public override string ToString()
        {
            return $"{file} ({line}:{column})";
        }
    }

    public class Token
    {
        public TokenType type;
        public LocationInfo location;
        public object data;
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
    }

    public class Lexer
    {
        private string mFile;
        private LocationInfo mLocation;
        private int index = 0;

        private char Current => mFile[index];
        private char Next => index < mFile.Length - 1 ? mFile[index + 1] : (char)0;
        private char Prev => index > 0 ? mFile[index - 1] : (char)0;
        private Token peek = null;

        public static Lexer FromFile(string fileName)
        {
            return new Lexer
            {
                mFile = File.ReadAllText(fileName, Encoding.UTF8),
                mLocation = new LocationInfo
                {
                    file = fileName,
                    line = 1,
                    column = 1
                }
            };
        }

        public static Lexer FromString(string str)
        {
            return new Lexer
            {
                mFile = str,
                mLocation = new LocationInfo
                {
                    file = "string",
                    line = 1,
                    column = 1
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

            if (SkipWhitespaceAndComments(out LocationInfo loc))
            {
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
            token.type = TokenType.EOF;
            if (index >= mFile.Length)
                return token;

            switch (Current)
            {
                case ':' when Next == ':': SimpleToken(ref token, TokenType.DoubleColon, 2); break;
                case ':': SimpleToken(ref token, TokenType.Colon); break;
                case ';': SimpleToken(ref token, TokenType.Semicolon); break;
                case '.': SimpleToken(ref token, TokenType.Period); break;
                case '=': SimpleToken(ref token, TokenType.Equal); break;
                case '(': SimpleToken(ref token, TokenType.OpenParen); break;
                case ')': SimpleToken(ref token, TokenType.ClosingParen); break;
                case '{': SimpleToken(ref token, TokenType.OpenBrace); break;
                case '}': SimpleToken(ref token, TokenType.ClosingBrace); break;
                case ',': SimpleToken(ref token, TokenType.Comma); break;

                case '"': ParseStringLiteral(ref token); break;

                case char cc when IsDigit(cc):
                    ParseNumberLiteral(ref token);
                    break;

                case char cc when IsIdentBegin(cc):
                    ParseIdentifier(ref token);
                    CheckKeywords(ref token);
                    break;
            }


            return token;
        }

        private void ParseStringLiteral(ref Token token)
        {
            token.type = TokenType.StringLiteral;
            int start = index++;
            mLocation.column++;
            StringBuilder sb = new StringBuilder();

            bool foundEnd = false;
            while (index < mFile.Length)
            {
                char c = Current;
                index++;
                mLocation.column++;
                if (c == '"')
                {
                    foundEnd = true;
                    break;
                }
                else if (c == '`')
                {
                    if (index >= mFile.Length)
                        throw new ParsingError(mLocation, $"Unexpected end of file while parsing string literal");
                    switch (Current)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case '0': sb.Append('\0'); break;
                        default: sb.Append(Current); break;
                    }
                    index++;
                    mLocation.column++;
                    continue;
                }

                sb.Append(c);
            }

            if (!foundEnd)
            {
                throw new ParsingError(mLocation, $"Unexpected end of string literal");
            }

            token.data = sb.ToString();
        }

        private void SimpleToken(ref Token token, TokenType type, int len = 1)
        {
            token.type = type;
            index += len;
            mLocation.column += len;
        }

        private void CheckKeywords(ref Token token)
        {
            switch (token.data as string)
            {
                case "fn": token.type = TokenType.KwFn; break;
                case "struct": token.type = TokenType.KwStruct; break;
                case "impl": token.type = TokenType.KwImpl; break;
                case "constant": token.type = TokenType.KwConstant; break;
                case "var": token.type = TokenType.KwVar; break;
                case "if": token.type = TokenType.KwIf; break;
                case "else": token.type = TokenType.KwElse; break;
                case "for": token.type = TokenType.KwFor; break;
                case "while": token.type = TokenType.KwWhile; break;
                case "print": token.type = TokenType.KwPrint; break; // @Temporary
            }
        }

        private void ParseIdentifier(ref Token token)
        {
            token.type = TokenType.Identifier;

            int start = index;
            while (IsIdent(Current))
            {
                index++;
                mLocation.column++;
            }

            token.data = mFile.Substring(start, index - start);
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


            while (index < mFile.Length && state != -1 && state != StateDone)
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
                    index++;
                    mLocation.column++;
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

        private bool SkipWhitespaceAndComments(out LocationInfo loc)
        {
            loc = null;

            while (index < mFile.Length)
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
                    index++;
                    mLocation.column++;
                }

                else if (c == '\r')
                {
                    index++;
                }

                else if (c == '\n')
                {
                    if (loc == null)
                    {
                        loc = new LocationInfo
                        {
                            file = mLocation.file,
                            line = mLocation.line,
                            column = mLocation.column,
                        };
                    }

                    mLocation.line++;
                    mLocation.column = 1;
                    index++;
                }

                else break;
            }

            return loc != null;
        }

        private void ParseSingleLineComment()
        {
            while (index < mFile.Length)
            {
                if (Current == '\n')
                    break;
                index++;
            }
        }

        private void ParseMultiLineComment()
        {
            int level = 0;
            while (index < mFile.Length)
            {
                char curr = Current;
                char next = Next;
                index++;
                mLocation.column++;

                if (curr == '/' && next == '*')
                {
                    index++;
                    mLocation.column++;
                    level++;
                }

                else if (curr == '*' && next == '/')
                {
                    index++;
                    mLocation.column++;
                    level--;

                    if (level == 0)
                        break;
                }

                else if (curr == '\n')
                {
                    mLocation.line++;
                    mLocation.column = 1;
                }
            }
        }
    }
}
