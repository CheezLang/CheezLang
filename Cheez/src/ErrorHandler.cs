using Cheez.Ast;
using Cheez.Parsing;
using System;
using System.Text;

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

        private void LogWarning(string message)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = col;
        }

        public void ReportError(IText text, ILocation location, string message)
        {
            var sb = new StringBuilder();

            var beg = location.Beginning;
            var end = location.End;

            var locationString = beg.ToString();
            sb.AppendLine($"{locationString}: {message}");

            int lineEnd = beg.lineStartIndex;
            for (; lineEnd < text.Text.Length; lineEnd++)
            {
                if (text.Text[lineEnd] == '\n')
                    break;
            }

            sb.Append("> ").AppendLine(text.Text.Substring(beg.lineStartIndex, lineEnd - beg.lineStartIndex));
            sb.Append(new string(' ', beg.index - beg.lineStartIndex + 2));
            sb.Append("^").AppendLine(new string('-', end.end - beg.index - 1));

            LogError(sb.ToString());
        }
    }
}
