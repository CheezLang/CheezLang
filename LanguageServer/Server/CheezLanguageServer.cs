using System;
using System.Collections.Generic;
using System.IO;
using Cheez.Compiler;
using Cheez.Compiler.CodeGeneration;
using LanguageServer;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Workspace;

namespace CheezLanguageServer
{
    class CheezLanguageServer : ServiceConnection
    {
        private Uri _workerSpaceRoot;
        private int _maxNumberOfProblems;
        private TextDocumentManager _documents;

        private ErrorHandler _errorHandler;
        private Compiler _compiler;

        public CheezLanguageServer(Stream input, Stream output) : base(input, output)
        {
            _documents = new TextDocumentManager();
            _documents.Changed += Documents_Changed;

            _errorHandler = new ErrorHandler();
            _compiler = new Compiler(_errorHandler);
        }

        private static string GetFilePath(Uri uri)
        {
            var path = uri.LocalPath;
            path = path.TrimStart('/');
            path = path.Substring(0, 1).ToUpperInvariant() + path.Substring(1);
            return path;
        }

        private void Documents_Changed(object sender, TextDocumentChangedEventArgs e)
        {
            ValidateTextDocument(e.Document);
        }

        private void ValidateTextDocument(TextDocumentItem document)
        {
            _errorHandler.ClearErrors();
                        
            PTFile file = null;
            try
            {
                var filePath = GetFilePath(document.uri);
                file = _compiler.AddFile(filePath, document.text, reparse: true);

                if (!_errorHandler.HasErrors)
                {
                    _compiler.DefaultWorkspace.CompileAll();

                    if (!_errorHandler.HasErrors)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath) + ".cpp";
                        var dir = Path.GetDirectoryName(filePath);
                        var gen = new CppCodeGenerator();
                        var code = gen.GenerateCode(_compiler.DefaultWorkspace);
                        File.WriteAllText(Path.Combine(dir, fileName), code);
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }

            var diagnostics = new List<Diagnostic>();
            var problems = 0;

            foreach (var err in _errorHandler.Errors)
            {
                if (problems >= _maxNumberOfProblems)
                    break;
                problems++;

                var d = new Diagnostic
                {
                    severity = DiagnosticSeverity.Error,
                    message = err.Message,
                    source = "CheezLang"
                };
                if (err.Location != null)
                {
                    var beg = err.Location.Beginning;
                    var end = err.Location.End;
                    d.range = new Range
                    {
                        start = new Position { line = beg.line - 1, character = beg.index - beg.lineStartIndex },
                        end = new Position { line = end.line - 1, character = end.end - end.lineStartIndex }
                    };
                }
                
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
                    documentSymbolProvider = true
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

            foreach (var document in _documents.All)
            {
                ValidateTextDocument(document);
            }
        }

        protected override void DidChangeWatchedFiles(DidChangeWatchedFilesParams @params)
        {
            Logger.Instance.Log("We received an file change event");
        }

        protected override Result<SymbolInformation[], ResponseError> DocumentSymbols(DocumentSymbolParams @params)
        {
            var file = _compiler.GetFile(GetFilePath(@params.textDocument.uri));
            if (file == null)
                return Result<SymbolInformation[], ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InvalidRequest,
                    message = $"No file called {@params.textDocument.uri} was found!"
                });


            var symbolFinder = new SymbolFinder();
            var symbols = symbolFinder.FindSymbols(_compiler.DefaultWorkspace, file);
            
            return Result<SymbolInformation[], ResponseError>.Success(symbols.ToArray());
        }

        protected override Result<SymbolInformation[], ResponseError> Symbol(WorkspaceSymbolParams @params)
        {
            return base.Symbol(@params);
        }
        
        protected override VoidResult<ResponseError> Shutdown()
        {
            Console.WriteLine("Shutting down...");
            
            return VoidResult<ResponseError>.Success();
        }

        #endregion
    }
}
