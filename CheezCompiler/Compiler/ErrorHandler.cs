using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public interface IErrorHandler
    {
        void ReportError(string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0);
        void ReportError(IText text, ILocation location, string message, List<Error> subErrors = null, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0);

        bool HasErrors { get; }
    }
    
    public class Error
    {
        public IText Text { get; set; }
        public ILocation Location { get; set; }
        public string Message { get; set; }
        public string File { get; set; }
        public string Function { get; set; }
        public int LineNumber { get; set; }

        public List<Error> SubErrors { get; set; } = new List<Error>();
    }

    public class SilentErrorHandler : IErrorHandler
    {
        public bool HasErrors => Errors.Count > 0;

        public List<Error> Errors { get; } = new List<Error>();

        public void ReportError(string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new Error
            {
                Message = message,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber
            });
        }

        public void ReportError(IText text, ILocation location, string message, List<Error> subErrors, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new Error
            {
                Text = text,
                Location = location,
                Message = message,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
                SubErrors = subErrors
            });
        }

        public void ClearErrors()
        {
            Errors.Clear();
        }
    }
}
