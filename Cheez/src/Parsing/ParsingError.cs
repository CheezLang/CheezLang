using System;

namespace Cheez.Parsing
{
    public class ParsingError : Exception
    {
        public LocationInfo Location { get; private set; }

        public ParsingError(LocationInfo loc, string message)
            : base($"{loc}: {message}")
        {
            Location = loc;
        }
    }
}
