using System.IO;
using System.Numerics;
using System.Text;
using System;
using System.Globalization;
using System.Collections.Generic;
using Cheez.Ast;
using Cheez.Extras;
using System.Linq;

namespace Cheez.Parsing
{
    public interface ILexer
    {
        string Text { get; }
        Token PeekToken();
        Token NextToken();
    }

    public enum TokenType
    {
        Unknown,

        NewLine,
        EOF,

        DocComment,

        StringLiteral,
        CharLiteral,
        NumberLiteral,

        Identifier,
        DollarIdentifier,
        HashIdentifier,
        AtSignIdentifier,
        ReplaceIdentifier,

        Semicolon,
        Colon,
        Comma,
        Period,
        PeriodPeriod,
        Equal,
        Ampersand,
        Hat,

        Bang,

        // arithmetic
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

        ReverseArrow,
        Arrow,
        DoubleArrow,
        LessLess,

        OpenParen,
        ClosingParen,

        OpenBrace,
        ClosingBrace,

        OpenBracket,
        ClosingBracket,

        Pipe,

        KwLambda,
        KwReturn,
        Kwfn,
        KwFn,
        KwStruct,
        KwEnum,
        KwImpl,
        KwIf,
        KwElse,
        KwFor,
        KwWhile,
        KwLoop,
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
        KwConst,
        KwDefault,
        KwPub,
        KwThen,
        KwDo,
        KwMut,
        KwImport,
        KwGeneric,
        KwIn,
        KwIs,
    }

    public class Token
    {
        public TokenType type { get; set; }
        public TokenLocation location { get; set; }
        public object data { get; set; }

        public string suffix { get; set; }

        public override string ToString()
        {
            return $"({location.line}:{location.index - location.lineStartIndex}) ({type}) {data}";
        }
    }

    public class Lexer : ILexer
    {
        private string mText;
        private TokenLocation mLocation;

        private char Current => mLocation.index < mText.Length ? mText[mLocation.index] : (char)0;
        private char Next => mLocation.index < mText.Length - 1 ? mText[mLocation.index + 1] : (char)0;
        private char GetChar(int offset) => mLocation.index + offset < mText.Length ? mText[mLocation.index + offset] : (char)0;
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
                mText = File.ReadAllText(fileName, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.InvariantCulture),
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
                mText = str.Replace("\r\n", "\n", StringComparison.InvariantCulture),
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

        private StringBuilder tokenDataBuilder = new StringBuilder();
        private Token ReadToken()
        {
            var token = new Token();
            token.location = mLocation.Clone();
            token.location.end = token.location.index;
            token.type = TokenType.EOF;
            if (mLocation.index >= mText.Length)
                return token;

            tokenDataBuilder.Clear();

            switch (Current)
            {
                case '<' when Next == '-': SimpleToken(ref token, TokenType.ReverseArrow, 2); break;
                case '-' when Next == '>': SimpleToken(ref token, TokenType.Arrow, 2); break;
                case '=' when Next == '>': SimpleToken(ref token, TokenType.DoubleArrow, 2); break;
                case '=' when Next == '=': SimpleToken(ref token, TokenType.DoubleEqual, 2); break;
                case '!' when Next == '=': SimpleToken(ref token, TokenType.NotEqual, 2); break;
                case '<' when Next == '=': SimpleToken(ref token, TokenType.LessEqual, 2); break;
                case '<' when Next == '<': SimpleToken(ref token, TokenType.LessLess, 2); break;
                case '>' when Next == '=': SimpleToken(ref token, TokenType.GreaterEqual, 2); break;
                case '+' when Next == '=': SimpleToken(ref token, TokenType.AddEq, 2); break;
                case '-' when Next == '=': SimpleToken(ref token, TokenType.SubEq, 2); break;
                case '*' when Next == '=': SimpleToken(ref token, TokenType.MulEq, 2); break;
                case '/' when Next == '=': SimpleToken(ref token, TokenType.DivEq, 2); break;
                case '%' when Next == '=': SimpleToken(ref token, TokenType.ModEq, 2); break;
                case '.' when Next == '.': SimpleToken(ref token, TokenType.PeriodPeriod, 2); break;
                case '/' when (Next == '/' && GetChar(2) == '/'): {
                    // doc comment

                    if (GetChar(3) == ' ') {
                        token.type = TokenType.DocComment;
                        int index = 0;
                        while (mLocation.index < mText.Length && Current != '\n') {
                            if (index >= 4)
                                tokenDataBuilder.Append(Current);
                            mLocation.index += 1;
                            index += 1;
                        }
                        if (mLocation.index < mText.Length && Current == '\n') {
                            mLocation.index += 1;
                            mLocation.line++;
                            mLocation.lineStartIndex = mLocation.index;
                        }
                        token.data = tokenDataBuilder.ToString();
                    } else if (GetChar(3) == '*') {
                        token.type = TokenType.DocComment;
                        token.data = ParseMultiLineDocComment();
                    } else {
                        throw new Exception("this shouldn't happen");
                    }

                    break;
                }
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
                case '^': SimpleToken(ref token, TokenType.Hat); break;
                case '*': SimpleToken(ref token, TokenType.Asterisk); break;
                case '/': SimpleToken(ref token, TokenType.ForwardSlash); break;
                case '+': SimpleToken(ref token, TokenType.Plus); break;
                case '%': SimpleToken(ref token, TokenType.Percent); break;
                case '-': SimpleToken(ref token, TokenType.Minus); break;
                case '<': SimpleToken(ref token, TokenType.Less); break;
                case '>': SimpleToken(ref token, TokenType.Greater); break;
                case '!': SimpleToken(ref token, TokenType.Bang); break;
                case '|': SimpleToken(ref token, TokenType.Pipe); break;


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
                case '§': ParseIdentifier(ref token, TokenType.ReplaceIdentifier); break;

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
                if (IsIdentBegin(Current))
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
                        mErrorHandler.ReportError(mText, new Location(mLocation), $"Unexpected end of file while parsing string literal");
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
                mErrorHandler.ReportError(mText, new Location(mLocation), $"Unexpected end of string literal");
            }

            token.data = sb.ToString();
        }

        private void SimpleToken(ref Token token, TokenType type, int len = 1)
        {
            token.type = type;
            mLocation.index += len;
        }

        private static void CheckKeywords(ref Token token)
        {
            switch (token.data as string)
            {
                case "lambda":   token.type = TokenType.KwLambda; break;
                case "return":   token.type = TokenType.KwReturn; break;
                case "fn":       token.type = TokenType.Kwfn; break;
                case "Fn":       token.type = TokenType.KwFn; break;
                case "struct":   token.type = TokenType.KwStruct; break;
                case "impl":     token.type = TokenType.KwImpl; break;
                case "if":       token.type = TokenType.KwIf; break;
                case "else":     token.type = TokenType.KwElse; break;
                case "for":      token.type = TokenType.KwFor; break;
                case "while":    token.type = TokenType.KwWhile; break;
                case "loop":     token.type = TokenType.KwLoop; break;
                case "and":      token.type = TokenType.KwAnd; break;
                case "or":       token.type = TokenType.KwOr; break;
                case "true":     token.type = TokenType.KwTrue; break;
                case "false":    token.type = TokenType.KwFalse; break;
                case "null":     token.type = TokenType.KwNull; break;
                case "use":      token.type = TokenType.KwUsing; break;
                case "defer":    token.type = TokenType.KwDefer; break;
                case "enum":     token.type = TokenType.KwEnum; break;
                case "match":    token.type = TokenType.KwMatch; break;
                case "break":    token.type = TokenType.KwBreak; break;
                case "continue": token.type = TokenType.KwContinue; break;
                case "trait":    token.type = TokenType.KwTrait; break;
                case "cast":     token.type = TokenType.KwCast; break;
                case "const":    token.type = TokenType.KwConst; break;
                case "default":  token.type = TokenType.KwDefault; break;
                case "pub":      token.type = TokenType.KwPub; break;
                case "then":     token.type = TokenType.KwThen; break;
                case "do":       token.type = TokenType.KwDo; break;
                case "mut":      token.type = TokenType.KwMut; break;
                case "import":   token.type = TokenType.KwImport; break;
                case "generic":  token.type = TokenType.KwGeneric; break;
                case "in":       token.type = TokenType.KwIn; break;
                case "is":       token.type = TokenType.KwIs; break;
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
                case TokenType.ReplaceIdentifier:
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

            const int StateError = -1;
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
            const int StateDecimal_ = 12;
            const int StateHex_ = 13;
            const int StateBinary_ = 14;
            const int StateFloat_ = 15;
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
                                state = StateError;
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
                            else if (c == '.' && Next != '.')
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
                            else if (c == '.' && Next != '.')
                            {
                                dataStringValue += c;
                                state = StateFloatPoint;
                                dataType = NumberData.NumberType.Float;

                            }
                            else if (c == '_')
                            {
                                state = StateDecimal_;
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
                            else if (c == '_')
                            {
                                state = StateFloat_;
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
                            else if (c == '_')
                            {
                                state = StateHex_;
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
                            else if (c == '_')
                            {
                                state = StateBinary_;
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

                    case StateDecimal_:
                        if (IsDigit(c))
                        {
                            dataStringValue += c;
                            state = StateDecimalDigit;
                        }
                        else
                        {
                            error = $"Unexpected character '{c}'. Expected digit";
                            state = StateError;
                        }
                        break;

                    case StateHex_:
                        if (IsHexDigit(c))
                        {
                            dataStringValue += c;
                            state = StateHexDigit;
                        }
                        else
                        {
                            error = $"Unexpected character '{c}'. Expected hex digit";
                            state = StateError;
                        }
                        break;

                    case StateBinary_:
                        if (IsDigit(c))
                        {
                            dataStringValue += c;
                            state = StateBinaryDigit;
                        }
                        else
                        {
                            error = $"Unexpected character '{c}'. Expected binary digit";
                            state = StateError;
                        }
                        break;

                    case StateFloat_:
                        if (IsDigit(c))
                        {
                            dataStringValue += c;
                            state = StateFloatDigit;
                        }
                        else
                        {
                            error = $"Unexpected character '{c}'. Expected digit";
                            state = StateError;
                        }
                        break;
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

        private static bool IsBinaryDigit(char c)
        {
            return c == '0' || c == '1';
        }

        private static bool IsHexDigit(char c)
        {
            return IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsIdentBegin(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private static bool IsIdent(char c)
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
                    if (GetChar(2) == '/') {
                        // potentially doc comment

                        if (GetChar(3) == ' ') {
                            // single line doc comment
                            break;
                        } else if (GetChar(3) == '*') {
                            // multi line doc comment
                            break;
                        }
                    }
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

        private string ParseMultiLineDocComment()
        {
            int startIndex = mLocation.index + 4;
            int initialIndentation = mLocation.Column;

            int endIndex = startIndex;

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
                else if (curr == '/' && next == '/' && GetChar(1) == '*' && GetChar(2) == '/')
                {
                    mLocation.index += 3;
                    level--;

                    if (level == 0) {
                        break;
                    }
                }
                else if (curr == '*' && next == '/')
                {
                    mLocation.index++;
                    level--;

                    if (level == 0) {
                        break;
                    }
                }
                else if (curr == '\n')
                {
                    endIndex = mLocation.index - 1;
                    mLocation.line++;
                    mLocation.lineStartIndex = mLocation.index;
                }
            }

            if (startIndex >= mText.Length)
                startIndex = mText.Length - 1;

            if (endIndex >= mText.Length)
                endIndex = mText.Length - 1;

            return string.Join("\n", mText.Substring(startIndex, endIndex - startIndex)
                .Split("\n")
                .Select(part => {
                    int i = 0;
                    for (; i < initialIndentation - 1 && i < part.Length && part[i] == ' '; i++);
                    return part.Substring(i);
                }));
        }
    }
}
