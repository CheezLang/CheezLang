using System.Collections.Generic;
using System.Linq;

namespace Cheez.Ast
{
    public class TokenLocation : ILocation
    {
        public string file { get; set; }
        public int line { get; set; }
        public int index { get; set; }
        public int end { get; set; }
        public int lineStartIndex { get; set; }

        public TokenLocation Beginning => this;
        public TokenLocation End => this;

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
            if (beg == null || end == null) throw new System.Exception("Arguments can't be null");
            this.Beginning = beg;
            this.End = end;
        }

        public Location(IEnumerable<ILocation> locations)
        {
            this.Beginning = locations.First().Beginning;
            this.End = locations.Last().End;
        }

        public static Location FromLocations<T>(IEnumerable<T> expressions)
            where T : ILocation
        {
            return new Location(expressions.First().Beginning, expressions.Last().End);
        }
    }
}
