using Cheez.Compiler;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CheezLanguageServer
{
    class CompilationError
    {
        public string Message { get; }
        public ILocation Location { get; }

        public CompilationError(string message, ILocation loc = null)
        {
            this.Location = loc;
            this.Message = message;
        }
    }

    class ErrorHandler : IErrorHandler
    {
        public List<CompilationError> Errors { get; } = new List<CompilationError>();

        public bool HasErrors => Errors.Count != 0;

        public void ReportError(IText text, ILocation location, string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new CompilationError(message, location));
        }

        public void ClearErrors()
        {
            Errors.Clear();
        }

        public void ReportError(string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new CompilationError(message));
        }
    }
}
