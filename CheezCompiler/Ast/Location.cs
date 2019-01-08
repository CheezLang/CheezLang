using System.Collections.Generic;
using System.Linq;

namespace Cheez.Ast
{
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
            return $"{file}:{line}:{index - lineStartIndex + 1}";
        }
    }
    public interface ILocation
    {
        TokenLocation Beginning { get; }

        TokenLocation End { get; }
    }

    public class Location : ILocation
    {
        public TokenLocation Beginning { get; }

        public TokenLocation End { get; }

        public Location(TokenLocation beg)
        {
            this.Beginning = beg;
            this.End = beg;
        }

        public Location(TokenLocation beg, TokenLocation end)
        {
            this.Beginning = beg;
            this.End = end;
        }

        public Location(IEnumerable<ILocation> locations)
        {
            this.Beginning = locations.First().Beginning;
            this.End = locations.Last().End;
        }
    }
}
