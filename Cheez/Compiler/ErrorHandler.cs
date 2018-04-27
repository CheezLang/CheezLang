using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public interface IErrorHandler
    {
        void ReportError(IText text, ILocation location, string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0);
        //void ReportError(ILocation location, string message, object ptn, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0);

        bool HasErrors { get; }
    }
}
