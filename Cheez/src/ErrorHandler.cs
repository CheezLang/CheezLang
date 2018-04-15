using Cheez.Ast;
using Cheez.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cheez
{
    public class ErrorHandler
    {
        private int mErrorCount = 0;
        public bool HasErrors => mErrorCount > 0;

        public ErrorHandler()
        {
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }
        
        private void LogErrorLine(string message, ConsoleColor foreground = ConsoleColor.Red, ConsoleColor background = ConsoleColor.Black)
        {
            var colf = Console.ForegroundColor;
            var colb = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = colf;
            Console.BackgroundColor = colb;
        }

        private void LogError(string message, ConsoleColor foreground = ConsoleColor.Red, ConsoleColor background = ConsoleColor.Black)
        {
            var colf = Console.ForegroundColor;
            var colb = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Error.Write(message);
            Console.ForegroundColor = colf;
            Console.BackgroundColor = colb;
        }

        private void LogWarning(string message)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = col;
        }

        public void ReportError(IText text, ILocation location, string message, int LinesBefore = 1, int LinesAfter = 1)
        {
            mErrorCount++;

            var sb = new StringBuilder();

            var beg = location.Beginning;
            var end = location.End;

            var locationString = beg.ToString();
            LogErrorLine($"{locationString}: {message}");

            // line before current line
            {
                List<string> previousLines = new List<string>();
                int lineStart = beg.lineStartIndex;
                for (int i = 0; i < LinesBefore && lineStart > 0; i++)
                {
                    var prevLineEnd = lineStart - 1;
                    var prevLineStart = GetLineStartIndex(text, prevLineEnd - 1);
                    previousLines.Add(text.Text.Substring(prevLineStart, prevLineEnd - prevLineStart));

                    lineStart = prevLineStart;
                }

                for (int i = previousLines.Count - 1; i >= 0; i--)
                {
                    int line = beg.line - previousLines.Count;
                    sb.Append($"{line,4}> ").AppendLine(previousLines[i]);
                }
            }

            LogError(sb.ToString(), ConsoleColor.White);
            sb.Clear();

            int lineEnd = GetLineEndIndex(text, beg.lineStartIndex);

            {
                var part1 = text.Text.Substring(beg.lineStartIndex, beg.index - beg.lineStartIndex);
                var part2 = text.Text.Substring(beg.index, end.end - beg.index);
                var part3 = text.Text.Substring(end.end, lineEnd - end.end);
                LogError($"{beg.line,4}> ", ConsoleColor.White);
                LogError(part1, ConsoleColor.White);
                LogError(part2, ConsoleColor.DarkRed);
                LogErrorLine(part3, ConsoleColor.White);
                sb.Clear();
            }
            //sb.Append("").Append(text.Text.Substring(beg.lineStartIndex, lineEnd - beg.lineStartIndex));
            //LogErrorLine(sb.ToString(), ConsoleColor.White);
            //sb.Clear();

            // underline
            if (false)
            {
                sb.Append(new string(' ', beg.index - beg.lineStartIndex + 6));
                //sb.Append('^').Append(new string('—', end.end - beg.index - 1));
                sb.Append('~').Append(new string('~', end.end - beg.index - 1));
                LogErrorLine(sb.ToString(), ConsoleColor.Red);
                sb.Clear();
            }

            // lines after current line
            {
                int lineBegin = lineEnd + 1;
                for (int i = 0; i < LinesAfter; i++)
                {
                    int line = end.line + i + 1;
                    lineEnd = GetLineEndIndex(text, lineBegin);
                    if (lineEnd >= text.Text.Length)
                        break;
                    sb.Append($"{line,4}> ");
                    sb.AppendLine(text.Text.Substring(lineBegin, lineEnd - lineBegin));
                    lineBegin = lineEnd + 1;
                }
            }
            LogErrorLine(sb.ToString(), ConsoleColor.White);
        }

        public void ReportCompilerError(string v)
        {
            mErrorCount++;
            LogErrorLine(v);
        }

        private int GetLineEndIndex(IText text, int currentIndex)
        {
            for (; currentIndex < text.Text.Length; currentIndex++)
            {
                if (text.Text[currentIndex] == '\n')
                    break;
            }

            return currentIndex;
        }

        private int GetLineStartIndex(IText text, int currentIndex)
        {
            for (; currentIndex >= 0; currentIndex--)
            {
                if (text.Text[currentIndex] == '\n')
                    return currentIndex + 1;
            }

            return 0;
        }
    }
}
