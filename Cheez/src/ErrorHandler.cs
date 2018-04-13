using Cheez.Parsing;
using System;

namespace Cheez
{
    public class ErrorHandler
    {
        public ErrorHandler()
        {
        }

        private void LogError(string message)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = col;
        }

        public void ReportParsingError(ParsingError error)
        {
            LogError(error.Message);
        }

        public void ReportCompileError(Exception e)
        {
            LogError(e.Message);
        }
    }
}
