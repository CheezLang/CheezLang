using Cheez.Parsing;

namespace Cheez.Ast
{
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
    }
}
