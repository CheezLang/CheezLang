using Cheez.Ast;
using Cheez.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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

        private void Log(string message, ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
        {
            var colf = Console.ForegroundColor;
            var colb = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = colf;
            Console.BackgroundColor = colb;
        }

        private void LogInline(string message, ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
        {
            var colf = Console.ForegroundColor;
            var colb = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Error.Write(message);
            Console.ForegroundColor = colf;
            Console.BackgroundColor = colb;
        }

        public void ReportError(IText text, ILocation location, string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            mErrorCount++;

            const int linesBefore = 1;
            const int linesAfter = 1;

            TokenLocation beginning = location.Beginning;
            TokenLocation end = location.End;
            int index = beginning.index;
            int lineNumber = beginning.line;
            int lineStart = GetLineStartIndex(text, index);
            int lineEnd = GetLineEndIndex(text, index);
            
            var errorLineBackgroundColor = ConsoleColor.Black;
            int lineNumberWidth = (end.line + linesAfter).ToString().Length;

            if (true)
            {
                callingFunctionFile = Path.GetFileName(callingFunctionFile);
                Log($"{callingFunctionFile}:{callingFunctionName}():{callLineNumber}", ConsoleColor.DarkYellow);
            }
            // location, message
            Log($"{beginning}: {message}", ConsoleColor.Red);

            // lines before current line
            {
                List<string> previousLines = new List<string>();
                int startIndex = lineStart;
                for (int i = 0; i < linesBefore && startIndex > 0; i++)
                {
                    var prevLineEnd = startIndex - 1;
                    var prevLineStart = GetLineStartIndex(text, prevLineEnd - 1);
                    previousLines.Add(text.Text.Substring(prevLineStart, prevLineEnd - prevLineStart));

                    startIndex = prevLineStart;
                }

                for (int i = previousLines.Count - 1; i >= 0; i--)
                {
                    int line = lineNumber - previousLines.Count;
                    Log(string.Format($"{{0,{lineNumberWidth}}}> {{1}}", line, previousLines[i]), ConsoleColor.White);
                }
            }

            // line containing error (may be multiple lines)
            {
                var part1 = text.Text.Substring(lineStart, index - lineStart);
                var part2 = text.Text.Substring(index, end.end - index);
                var part3 = text.Text.Substring(end.end, lineEnd - end.end);
                LogInline(string.Format($"{{0,{lineNumberWidth}}}> ", lineNumber), ConsoleColor.White);
                LogInline(part1, ConsoleColor.White, errorLineBackgroundColor);
                LogInline(part2, ConsoleColor.Red, errorLineBackgroundColor);
                Log(part3, ConsoleColor.White, errorLineBackgroundColor);
            }

            // underline
            {
                char firstChar = '^'; // ^ ~
                char underlineChar = '—'; // — ~
                var str = new string(' ', index - lineStart + lineNumberWidth + 2) + firstChar + new string(underlineChar, end.end - index - 1);
                Log(str, ConsoleColor.DarkRed);
            }

            // lines after current line
            {
                var sb = new StringBuilder();
                int lineBegin = lineEnd + 1;
                for (int i = 0; i < linesAfter; i++)
                {
                    int line = end.line + i + 1;
                    lineEnd = GetLineEndIndex(text, lineBegin);
                    if (lineEnd >= text.Text.Length)
                        break;
                    var str = text.Text.Substring(lineBegin, lineEnd - lineBegin);
                    Log(string.Format($"{{0,{lineNumberWidth}}}> {{1}}", line, str), ConsoleColor.White);
                    lineBegin = lineEnd + 1;
                }
            }
        }

        public void ReportCompilerError(string v)
        {
            mErrorCount++;
            Log(v, ConsoleColor.Red);
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
