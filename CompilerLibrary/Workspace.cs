using Cheez.Ast;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using Cheez.Util;
using Cheez.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public enum TargetArchitecture
    {
        X86,
        X64
    }

    public partial class Workspace
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private CheezCompiler mCompiler;

        public IEnumerable<PTFile> Files => mFiles.Select(kv => kv.Value);

        private Queue<PTFile> mUnresolvedFiles = new Queue<PTFile>();

        public Scope GlobalScope { get; }

        public AstFuncExpr MainFunction { get; set; }

        public Dictionary<CheezType, List<AstImplBlock>> TypeTraitMap { get; } = new Dictionary<CheezType, List<AstImplBlock>>();

        public int MaxPolyStructResolveStepCount { get; set; } = 10;
        public int MaxPolyFuncResolveStepCount { get; set; } = 10;

        private AstFuncExpr currentFunction = null;

        public TargetArchitecture TargetArch { get; } = TargetArchitecture.X64;

        private Stack<IErrorHandler> m_errorHandlerReplacements = new Stack<IErrorHandler>();
        public IErrorHandler CurrentErrorHandler => m_errorHandlerReplacements.Count > 0 ? m_errorHandlerReplacements.Peek() : mCompiler.ErrorHandler;

        private int mNameGen = 0;

        public Workspace(CheezCompiler comp)
        {
            mCompiler = comp;

            GlobalScope = new Scope("global");
            GlobalScope.DefineBuiltInTypes();
            GlobalScope.DefineBuiltInOperators();
        }

        private string GetUniqueName(string str = "")
        {
            return $"\"{str}-{mNameGen++}\"";
        }

        public void PushErrorHandler(IErrorHandler handler)
        {
            m_errorHandlerReplacements.Push(handler);
        }

        public SilentErrorHandler PushSilentErrorHandler()
        {
            var errHandler = new SilentErrorHandler();
            m_errorHandlerReplacements.Push(errHandler);
            return errHandler;
        }

        public void PopErrorHandler()
        {
            m_errorHandlerReplacements.Pop();
        }

        public void AddFile(PTFile file)
        {
            mUnresolvedFiles.Enqueue(file);
            mFiles[file.Name] = file;
        }

        public PTFile GetFile(string file)
        {
            return mFiles[file];
        }

        public void CompileAll()
        {
            // new
            ResolveImports();
            ResolveDeclarations();

            if (mCompiler.ErrorHandler.HasErrors)
                return;

            var mains = mAllFunctions.Where(f => f.HasDirective("main"));
            if (mains.Count() > 1)
            {
                ReportError("Only one main function is allowed",
                    mains.Select(f => ("Main function defined here:", f.ParameterLocation)).ToList());
            }
            else if (mains.Count() == 0)
            {
                var scopesWithMain = Files.Where(f => f.FileScope.GetSymbol("Main") is AstConstantDeclaration c && c.Initializer is AstFuncExpr);

                if (scopesWithMain.Count() == 0)
                    mCompiler.ErrorHandler.ReportError("No main function was specified");
                else if (scopesWithMain.Count() > 1)
                    mCompiler.ErrorHandler.ReportError("No main function was specified");
                else
                {
                    var mainDecl = scopesWithMain.First().FileScope.GetSymbol("Main") as AstConstantDeclaration;
                    MainFunction = mainDecl?.Initializer as AstFuncExpr;
                }

                if (MainFunction == null)
                {
                    mCompiler.ErrorHandler.ReportError("No main function was specified");
                }
            }
            else
            {
                MainFunction = mains.First();
            }

            //ReportError("error");
        }

        [SkipInStackFrameAttribute]
        public void ReportError(Error error)
        {
            CurrentErrorHandler.ReportError(error);
        }

        [SkipInStackFrameAttribute]
        public void ReportError(string errorMessage)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(errorMessage, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        [SkipInStackFrameAttribute]
        public void ReportError(string errorMessage, List<(string, ILocation)> details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(new Error
            {
                Message = errorMessage,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrameAttribute]
        public void ReportError(string errorMessage, (string, ILocation) details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(new Error
            {
                Message = errorMessage,
                Details = new List<(string, ILocation)> { details },
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrameAttribute]
        public void ReportError(ILocation lc, string message, (string, ILocation)? detail = null)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            var details = detail != null ? new List<(string, ILocation)> { detail.Value } : null;
            CurrentErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrameAttribute]
        public void ReportError(ILocation lc, string message, List<Error> subErrors, params (string, ILocation)[] details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
                SubErrors = subErrors
            });
        }

        [SkipInStackFrameAttribute]
        public void ReportError(ILocation lc, string message, params (string, ILocation)[] details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        [SkipInStackFrameAttribute]
        public void ReportError(ILocation lc, string message, IEnumerable<(string, ILocation)> details)
        {
            var (callingFunctionName, callingFunctionFile, callLineNumber) = Utilities.GetCallingFunction().GetValueOrDefault(("", "", -1));
            CurrentErrorHandler.ReportError(new Error
            {
                Location = lc,
                Message = message,
                Details = details,
                File = callingFunctionFile,
                Function = callingFunctionName,
                LineNumber = callLineNumber,
            });
        }

        private void AddTraitForType(CheezType type, AstImplBlock impl)
        {
            List<AstImplBlock> traits = null;
            if (!TypeTraitMap.TryGetValue(type, out traits))
            {
                traits = new List<AstImplBlock>();
                TypeTraitMap.Add(type, traits);
            }

            traits.Add(impl);
        }

        private int logScope = 0;
        private static void Log(string message, params string[] comments)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (comments is null)
                throw new ArgumentNullException(nameof(comments));
#if false
            var indent = new string(' ', logScope * 4);

            var cc = System.Console.ForegroundColor;

            System.Console.ForegroundColor = System.ConsoleColor.DarkCyan;
            System.Console.WriteLine($"[LOG] {indent}{message}");


            System.Console.ForegroundColor = System.ConsoleColor.Blue;
            foreach (var comment in comments)
            {
                System.Console.WriteLine($"      {indent}{comment}");
            }
            System.Console.ForegroundColor = cc;
#endif
        }

        private void PushLogScope()
        {
            logScope++;
        }

        private void PopLogScope()
        {
            logScope--;
        }

        private static void WellThatsNotSupposedToHappen(string message = null)
        {
            throw new Exception($"[{message}] Well that's not supposed to happen...");
        }
    }
}
