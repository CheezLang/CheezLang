using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cheez;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using LanguageServer;
using LanguageServer.Json;
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

        private SilentErrorHandler _errorHandler;
        private CheezCompiler _compiler;

        public CheezLanguageServer(Stream input, Stream output) : base(input, output)
        {
            ResetLanguageServer();
        }

        private Result<dynamic, ResponseError> ResetLanguageServer()
        {
            _documents = new TextDocumentManager();
            _documents.Changed += Documents_Changed;

            _errorHandler = new SilentErrorHandler();
            _compiler = new CheezCompiler(_errorHandler, "");
            return Result<dynamic, ResponseError>.Success(true);
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
                }
            }
            catch (Exception)
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
                    message = CreateErrorMessage(err),
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

        private string CreateErrorMessage(Error e)
        {
            string msg = e.Message;
            if (e.SubErrors?.Count > 0)
            {
                foreach (var s in e.SubErrors)
                {
                    msg += "\n" + CreateErrorMessage(s);
                }
            }

            return msg;
        }

        #region ...

        protected override Result<dynamic, ResponseError> ExecuteCommand(ExecuteCommandParams @params)
        {
            switch (@params.command)
            {
                case "reload_language_server":
                    Proxy.Window.ShowMessage(new LanguageServer.Parameters.Window.ShowMessageParams
                    {
                        type = LanguageServer.Parameters.Window.MessageType.Info,
                        message = "CheezLanguageServer was reset"
                    });
                    return ResetLanguageServer();
            }

            return Result<dynamic, ResponseError>.Error(new ResponseError
            {
                code = ErrorCodes.InvalidRequest,
                message = $"Unknown command {@params.command}"
            });
        }

        protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
        {
            _workerSpaceRoot = @params.rootUri;
            var result = new InitializeResult
            {
                capabilities = new ServerCapabilities
                {
                    textDocumentSync = TextDocumentSyncKind.Full,
                    documentSymbolProvider = true,
                    executeCommandProvider = new ExecuteCommandOptions
                    {
                        commands = new string[] { "reload_language_server" }
                    },
                }
            };

            Proxy.Window.ShowMessage(new LanguageServer.Parameters.Window.ShowMessageParams
            {
                type = LanguageServer.Parameters.Window.MessageType.Info,
                message = "CheezLanguageServer initialized"
            });

            return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(result);
        }

        #region Document stuff

        protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
        {
            _documents.Add(@params.textDocument);
        }

        protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
        {
            _documents.Change(@params.textDocument.uri, @params.textDocument.version, @params.contentChanges);
        }

        protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
        {
            _documents.Remove(@params.textDocument.uri);
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
        }

        #endregion

        protected override Result<SymbolInformation[], ResponseError> DocumentSymbols(DocumentSymbolParams @params)
        {
            try
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
            catch (Exception)
            {
                return Result<SymbolInformation[], ResponseError>.Success(new SymbolInformation[0]);
            }
        }

        protected override VoidResult<ResponseError> Shutdown()
        {
            Console.WriteLine("Shutting down...");

            return VoidResult<ResponseError>.Success();
        }

        private CompletionItemKind GetCompletionItemKind(ISymbol sym)
        {
            switch (sym)
            {
                case AstParameter _:
                case AstVariableDecl _:
                    return CompletionItemKind.Variable;

                case AstFunctionDecl _:
                    return CompletionItemKind.Function;

                default: return CompletionItemKind.Color;
            }
        }

        private string GetInsertText(ISymbol sym)
        {
            switch (sym)
            {
                case AstFunctionDecl _:
                    return $"{sym.Name}";

                default: return sym.Name.Name;
            }
        }

        private CompletionItem SymbolToCompletionItem(ISymbol sym)
        {
            return new CompletionItem
            {
                label = sym.Name.Name,
                kind = GetCompletionItemKind(sym),
                detail = $"{(sym as ITypedSymbol)?.Type}",
                insertText = GetInsertText(sym)
            };
        }

        protected override Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError> Completion(TextDocumentPositionParams @params)
        {
            try
            {
                var file = _compiler.GetFile(GetFilePath(@params.textDocument.uri));
                if (file == null)
                    return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new ArrayOrObject<CompletionItem, CompletionList>());

                var nodeFinder = new NodeFinder();
                var node = nodeFinder.FindNode(_compiler.DefaultWorkspace, file, (int)@params.position.line, (int)@params.position.character, true);

                if (node == null)
                    return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new ArrayOrObject<CompletionItem, CompletionList>());

                var symbols = node.GetSymbols();
                var completionList = symbols.Select(kv => SymbolToCompletionItem(kv.Value)).ToArray();

                return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new ArrayOrObject<CompletionItem, CompletionList>(completionList));
            }
            catch (Exception)
            {
                return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(new ArrayOrObject<CompletionItem, CompletionList>());
            }
        }

        protected override Result<Hover, ResponseError> Hover(TextDocumentPositionParams @params)
        {
            try
            {
                var file = _compiler.GetFile(GetFilePath(@params.textDocument.uri));
                if (file == null)
                    return Result<Hover, ResponseError>.Error(new ResponseError
                    {
                        code = ErrorCodes.InvalidRequest,
                        message = $"No file called {@params.textDocument.uri} was found!"
                    });

                var nodeFinder = new NodeFinder();
                var node = nodeFinder.FindNode(_compiler.DefaultWorkspace, file, (int)@params.position.line, (int)@params.position.character, true);

                if (node == null || (node.Expr == null && node.Stmt == null && node.Type == null))
                {
                    return Result<Hover, ResponseError>.Success(new Hover());
                }

                string content = GetHoverTextFromNode(node);

                var hover = new Hover
                {
                    contents = new ArrayOrObject<StringOrObject<MarkedString>, StringOrObject<MarkedString>>(new StringOrObject<MarkedString>(content))
                };

                return Result<Hover, ResponseError>.Success(hover);
            }
            catch (Exception e)
            {
                var hover = new Hover
                {
                    contents = new ArrayOrObject<StringOrObject<MarkedString>, StringOrObject<MarkedString>>(new StringOrObject<MarkedString>(e.Message))
                };
                return Result<Hover, ResponseError>.Success(new Hover());
            }
        }

        private string GetHoverTextFromNode(NodeFinderResult node)
        {
            if (node == null)
                return "";

            if (node.Expr != null)
            {
                switch (node.Expr)
                {
                    case AstDotExpr dot:
                        {
                            var t = dot.Left.Type;
                            while (t is PointerType p)
                                t = p.TargetType;
                            if (dot.Left.Type is StructType s)
                                return $"{dot.Left.Type}.{dot.Right}: {s.Declaration.Members.FirstOrDefault(m => m.Name.Name == dot.Right.Name)?.Type}";
                            else
                                return "";
                        }

                    default:
                        return node.Expr.Type.ToString();
                }
            }
            else if (node.Stmt != null)
            {
                var s = node.Stmt;
                switch (s)
                {
                    case AstVariableDecl v:
                        return $"{v.Pattern} : {v.Type}";

                    case AstFunctionDecl f:
                        return $"fn {f.Name}({string.Join(", ", f.Parameters.Select(p => p.Type))}): {f.ReturnType}";
                }
            }
            else if (node.Type != null)
            {
                return node.Type.ToString();
            }

            return "";
        }

        private Range CastLocation(ILocation loc)
        {
            var beg = loc.Beginning;
            var end = loc.End;
            return new Range
            {
                start = new Position { line = beg.line - 1, character = beg.index - beg.lineStartIndex },
                end = new Position { line = end.line - 1, character = end.end - end.lineStartIndex }
            };
        }

        #endregion
    }
}
