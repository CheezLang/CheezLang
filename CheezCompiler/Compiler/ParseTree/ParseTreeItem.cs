using Cheez.Compiler.Parsing;

namespace Cheez.Compiler.ParseTree
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

        public IText Text => throw new System.NotImplementedException();

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
