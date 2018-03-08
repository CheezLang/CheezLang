using Cheez.Parsing;
using log4net;
using System;
using System.Reflection;

namespace Cheez
{
    public class ErrorHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
