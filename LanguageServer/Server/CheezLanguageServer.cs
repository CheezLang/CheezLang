using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cheez.Compiler;
using Cheez.Compiler.Parsing;
using LanguageServer;
using LanguageServer.Json;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Window;
using LanguageServer.Parameters.Workspace;
using Newtonsoft.Json;

namespace CheezLanguageServer
{
    class CheezLanguageServer : ServiceConnection
    {
        private Uri _workerSpaceRoot;
        private int _maxNumberOfProblems;
        public static string _logFilePath;
        private TextDocumentManager _documents;

        private bool _firstConfigLoad = true;

        private Compiler _compiler;

        public CheezLanguageServer(Stream input, Stream output) : base(input, output)
        {
            _documents = new TextDocumentManager();
            _documents.Changed += Documents_Changed;
        }

        public static void Log(MessageType type, string message)
        {
            if (_logFilePath != null)
            {
#if DEBUG
                File.AppendAllText(_logFilePath, $"[{type}:Debug] {message}\r\n");
#else
                File.AppendAllText(LogFilePath, $"[{type}:Release] {message}\r\n");
#endif
            }
        }

        private void LogParameters(object param1, [CallerMemberName] string function = "")
        {
            Log(MessageType.Log, $"{function}({JsonConvert.SerializeObject(param1, Formatting.Indented)})");
        }

        private void Documents_Changed(object sender, TextDocumentChangedEventArgs e)
        {
            ValidateTextDocument(e.Document);
        }

        private void ValidateTextDocument(TextDocumentItem document)
        {
            var fileName = Path.GetFileName(document.uri.AbsolutePath);

            Log(MessageType.Info, $"Validating {fileName}, text: \r\n{document.text}");

            var c = new Compiler();
            PTFile file = null;
            try
            {
                file = c.AddFile(document.uri, document.text);
                Log(MessageType.Info, $"Validation result for {fileName}: {file.Errors.Count} errors");
            }
            catch (Exception e)
            {
                Log(MessageType.Info, $"Validation result for {fileName}: {e}");
                return;
            }

            var diagnostics = new List<Diagnostic>();
            var problems = 0;
            
            foreach (var err in file.Errors)
            {
                if (problems >= _maxNumberOfProblems)
                    break;
                problems++;

                var beg = err.Location.Beginning;
                var end = err.Location.End;



                var d = new Diagnostic
                {
                    severity = DiagnosticSeverity.Error,
                    range = new Range
                    {
                        start = new Position { line = beg.line - 1, character = beg.index - beg.lineStartIndex },
                        end = new Position { line = end.line - 1, character = end.end - end.lineStartIndex }
                    },
                    message = err.Message,
                    source = "CheezLang"
                };

                //LogParameters(beg);
                //LogParameters(end);
                //LogParameters(d);
                diagnostics.Add(d);
            }

            Proxy.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
            {
                uri = document.uri,
                diagnostics = diagnostics.ToArray()
            });
        }

        #region ...

        protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
        {
            _workerSpaceRoot = @params.rootUri;
            var result = new InitializeResult
            {
                capabilities = new ServerCapabilities
                {
                    textDocumentSync = TextDocumentSyncKind.Full,
                    completionProvider = new CompletionOptions
                    {
                        resolveProvider = true
                    }
                }
            };
            return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(result);
        }

        protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
        {
            _documents.Add(@params.textDocument);
            Logger.Instance.Log($"{@params.textDocument.uri} opened.");
        }

        protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
        {
            _documents.Change(@params.textDocument.uri, @params.textDocument.version, @params.contentChanges);
            Logger.Instance.Log($"{@params.textDocument.uri} changed.");
        }

        protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
        {
            _documents.Remove(@params.textDocument.uri);
            Logger.Instance.Log($"{@params.textDocument.uri} closed.");
        }

        protected override void DidChangeConfiguration(DidChangeConfigurationParams @params)
        {
            _maxNumberOfProblems = @params?.settings?.cheezls?.maxNumberOfProblems ?? 100;
            _logFilePath = @params?.settings?.cheezls?.logFilePath;

            if (_firstConfigLoad && _logFilePath != null)
            {
                File.WriteAllText(_logFilePath, "");
                Log(MessageType.Info, "Starting Cheez Language Server");
            }
            _firstConfigLoad = false;

            Log(MessageType.Info, "Updated configuration");

            foreach (var document in _documents.All)
            {
                ValidateTextDocument(document);
            }
        }

        protected override void DidChangeWatchedFiles(DidChangeWatchedFilesParams @params)
        {
            Logger.Instance.Log("We received an file change event");
        }

        protected override Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError> Completion(TextDocumentPositionParams @params)
        {
            LogParameters(@params);
            var array = new[]
            {
                new CompletionItem
                {
                    label = "TypeScript",
                    kind = CompletionItemKind.Text,
                    data = 1
                },
                new CompletionItem
                {
                    label = "JavaScript",
                    kind = CompletionItemKind.Text,
                    data = 2
                }
            };
            return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(array);
        }

        protected override Result<CompletionItem, ResponseError> ResolveCompletionItem(CompletionItem @params)
        {
            LogParameters(@params);
            if (@params.data == 1)
            {
                @params.detail = "TypeScript details";
                @params.documentation = "TypeScript documentation";
            }
            else if (@params.data == 2)
            {
                @params.detail = "JavaScript details";
                @params.documentation = "JavaScript documentation";
            }
            return Result<CompletionItem, ResponseError>.Success(@params);
        }

        protected override Result<DocumentHighlight[], ResponseError> DocumentHighlight(TextDocumentPositionParams @params)
        {
            return base.DocumentHighlight(@params);
        }

        protected override Result<SymbolInformation[], ResponseError> DocumentSymbols(DocumentSymbolParams @params)
        {
            var symbols = new List<SymbolInformation>();


            var doc = _documents.GetTextDocumentItem(@params.textDocument);
            

            return Result<SymbolInformation[], ResponseError>.Success(symbols.ToArray());
        }

        #endregion
    }
}
