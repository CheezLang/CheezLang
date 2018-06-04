using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Cheez.Compiler;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;

namespace CheezCLI
{
    class ConsoleErrorHandler : IErrorHandler
    {
        public bool HasErrors { get; private set; }

        public void ReportError(IText text, ILocation location, string message, List<Error> subErrors, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            HasErrors = true;


            const int linesBefore = 2;
            const int linesAfter = 0;


            TokenLocation beginning = location.Beginning;
            TokenLocation end = location.End;
            int index = beginning.index;
            int lineNumber = beginning.line;
            int lineStart = GetLineStartIndex(text, index);
            int lineEnd = GetLineEndIndex(text, index);
            bool multiLine = beginning.line != end.line;

            var errorLineBackgroundColor = ConsoleColor.Black;
            int lineNumberWidth = (end.line + linesAfter).ToString().Length;

#if DEBUG
            Log($"{Path.GetFileName(callingFunctionFile)}:{callingFunctionName}():{callLineNumber}", ConsoleColor.DarkYellow);
#endif
            // location, message
            Log($"{beginning.file}: {message}", ConsoleColor.Red);

            // lines before current line
            {
                List<string> previousLines = new List<string>();
                int startIndex = lineStart;
                for (int i = 0; i < linesBefore && startIndex > 0; i++)
                {
                    var prevLineEnd = startIndex - 1;
                    var prevLineStart = GetLineStartIndex(text, prevLineEnd);
                    previousLines.Add(text.Text.Substring(prevLineStart, prevLineEnd - prevLineStart));

                    startIndex = prevLineStart;
                }

                for (int i = previousLines.Count - 1; i >= 0; i--)
                {
                    int line = lineNumber - 1 - i;
                    Log(string.Format($"{{0,{lineNumberWidth}}}> {{1}}", line, previousLines[i]), ConsoleColor.White);
                }
            }

            // line containing error (may be multiple lines)
            {
                if (!multiLine)
                {
                    var part1 = text.Text.Substring(lineStart, index - lineStart);
                    var part2 = text.Text.Substring(index, end.end - index);
                    var part3 = text.Text.Substring(end.end, lineEnd - end.end);
                    LogInline(string.Format($"{{0,{lineNumberWidth}}}> ", lineNumber), ConsoleColor.White);
                    LogInline(part1, ConsoleColor.White, errorLineBackgroundColor);
                    LogInline(part2, ConsoleColor.Red, errorLineBackgroundColor);
                    Log(part3, ConsoleColor.White, errorLineBackgroundColor);
                }
                else
                {
                    Log(text.Text.Substring(lineStart, GetLineEndIndex(text, end.index) - lineStart), ConsoleColor.White);
                }
            }

            // underline
            if (!multiLine)
            {
                char firstChar = '^'; // ^ ~
                char underlineChar = '—'; // — ~
                var str = new string(' ', index - lineStart + lineNumberWidth + 2) + firstChar;
                if (end.end - index - 1 > 0)
                    str += new string(underlineChar, end.end - index - 1);
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

            if (subErrors?.Count > 0)
            {
                Log("Error caused from here:", ConsoleColor.White);

                foreach (var e in subErrors)
                {
                    ReportError(e.Text, e.Location, e.Message, e.SubErrors, e.File, e.Function, e.LineNumber);
                }
            }
        }

        public void ReportError(string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            HasErrors = true;
#if DEBUG
            Log($"{Path.GetFileName(callingFunctionFile)}:{callingFunctionName}():{callLineNumber}", ConsoleColor.DarkYellow);
#endif

            Log(message, ConsoleColor.Red);
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
            if (currentIndex >= text.Text.Length)
                currentIndex = text.Text.Length - 1;

            if (text.Text[currentIndex] == '\n')
                currentIndex--;

            for (; currentIndex >= 0; currentIndex--)
            {
                if (text.Text[currentIndex] == '\n')
                    return currentIndex + 1;
            }

            return 0;
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
    }
}
