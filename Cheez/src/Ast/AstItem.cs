using Cheez.Parsing;

namespace Cheez.Ast
{
    public interface ILocation
    {
        LocationInfo Beginning { get; }
        LocationInfo End { get; }
    }

    public class Location : ILocation
    {
        public LocationInfo Beginning { get; }
        public LocationInfo End { get; }

        public Location(LocationInfo beg, LocationInfo end)
        {
            this.Beginning = beg;
            this.End = end;
        }
    }
}
