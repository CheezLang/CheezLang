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
        public bool HasErrors { get; set; }

        public void ReportError(IText text, ILocation location, string message, List<Error> subErrors, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            ReportError(new Error
            {
                Text = text,
                Location = location,
                Message = message,
                SubErrors = subErrors
            }, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        public void ReportError(Error error, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            HasErrors = true;


            const int linesBefore = 2;
            const int linesAfter = 0;


            TokenLocation beginning = error.Location.Beginning;
            TokenLocation end = error.Location.End;
            int index = beginning.index;
            int lineNumber = beginning.line;
            int lineStart = GetLineStartIndex(error.Text, index);
            int lineEnd = GetLineEndIndex(error.Text, index);
            bool multiLine = beginning.line != end.line;

            var errorLineBackgroundColor = ConsoleColor.Black;
            int lineNumberWidth = (end.line + linesAfter).ToString().Length;

#if DEBUG
            Log($"{Path.GetFileName(callingFunctionFile)}:{callingFunctionName}():{callLineNumber}", ConsoleColor.DarkYellow);
#endif
            // location, message
            Log($"{beginning.file}: {error.Message}", ConsoleColor.Red);

            // lines before current line
            {
                List<string> previousLines = new List<string>();
                int startIndex = lineStart;
                for (int i = 0; i < linesBefore && startIndex > 0; i++)
                {
                    var prevLineEnd = startIndex - 1;
                    var prevLineStart = GetLineStartIndex(error.Text, prevLineEnd);
                    previousLines.Add(error.Text.Text.Substring(prevLineStart, prevLineEnd - prevLineStart));

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
                var firstLine = beginning.line;
                var lastLine = end.line;
                var ls = lineStart; // lineStart
                var le = GetLineEndIndex(error.Text, index); // lineEnd
                var ei = Math.Min(le, end.end); // endIndex
                var i = index;

                for (var line = firstLine; line <= lastLine; ++line)
                {
                    var part1 = error.Text.Text.Substring(ls, i - ls);
                    var part2 = error.Text.Text.Substring(i, ei - i);
                    var part3 = error.Text.Text.Substring(ei, le - ei);

                    LogInline(string.Format($"{{0,{lineNumberWidth}}}> ", line), ConsoleColor.White);

                    LogInline(part1, ConsoleColor.White, errorLineBackgroundColor);
                    LogInline(part2, ConsoleColor.Red, errorLineBackgroundColor);
                    Log(part3, ConsoleColor.White, errorLineBackgroundColor);

                    ls = le + 1;
                    i = ls;
                    le = GetLineEndIndex(error.Text, i);
                    ei = Math.Min(le, end.end);
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
                    lineEnd = GetLineEndIndex(error.Text, lineBegin);
                    if (lineEnd >= error.Text.Text.Length)
                        break;
                    var str = error.Text.Text.Substring(lineBegin, lineEnd - lineBegin);
                    Log(string.Format($"{{0,{lineNumberWidth}}}> {{1}}", line, str), ConsoleColor.White);
                    lineBegin = lineEnd + 1;
                }
            }

            // details
            //if (error.Details?.Count > 0)
            //{
            //    foreach (var d in error.Details)
            //    {
            //        Log("| " + d, ConsoleColor.White);
            //    }
            //}

            Console.Error.WriteLine();

            if (error.SubErrors?.Count > 0)
            {
                Log("Error caused from here:", ConsoleColor.White);

                foreach (var e in error.SubErrors)
                {
                    ReportError(e, callingFunctionFile, callingFunctionName, callLineNumber);
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