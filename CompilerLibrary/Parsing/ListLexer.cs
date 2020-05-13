using Cheez.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheez.Parsing
{
    public class ListLexer : ILexer
    {
        public string Text { get; private set; }
        private int index = -1;
        private List<Token> tokens;
        private Token eof;

        public ListLexer(string text, List<Token> tokens)
        {
            this.Text = text;
            this.tokens = tokens;

            eof = new Token
            {
                location = tokens.Last().location,
                type = TokenType.EOF
            };
        }

        public Token NextToken()
        {
            index++;

            if (index >= tokens.Count)
                return eof;

            return tokens[index];
        }

        public Token PeekToken()
        {
            if (index + 1 >= tokens.Count)
                return eof;

            return tokens[index + 1];
        }
    }
}
