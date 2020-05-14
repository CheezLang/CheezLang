using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Cheez;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Parsing;
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
    class SourceFile
    {
        public List<AstStatement> statements = new List<AstStatement>();
        public Uri uri;

        public SourceFile(Uri uri)
        {
            this.uri = uri;
        }

        internal void Clear()
        {
            statements.Clear();
        }
    }

    class CheezLanguageServer : ServiceConnection
    {
        private Uri _workerSpaceRoot;
        private int _maxNumberOfProblems;
        private TextDocumentManager _documents;

        private Dictionary<string, SourceFile> files;
        private List<string> modulePaths = new List<string>();

        //private SilentErrorHandler _errorHandler;
        //private CheezCompiler _compiler;

        public CheezLanguageServer(Stream input, Stream output) : base(input, output)
        {
            ResetLanguageServer();
        }

        private Result<dynamic, ResponseError> ResetLanguageServer()
        {
            _documents = new TextDocumentManager();
            _documents.Changed += _documents_Changed;

            files = new Dictionary<string, SourceFile>();

            //_errorHandler = new SilentErrorHandler();
            //_compiler = new CheezCompiler(_errorHandler, null, null);
            return Result<dynamic, ResponseError>.Success(true);
        }

        protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
        {
            // load additional module paths from modules.txt if existent
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string modulesFile = Path.Combine(exePath, "modules.txt");
                if (File.Exists(modulesFile))
                {
                    foreach (var modulePath in File.ReadAllLines(modulesFile))
                    {
                        if (!string.IsNullOrWhiteSpace(modulePath))
                            modulePaths.Add(modulePath);
                    }
                }
            }

            _workerSpaceRoot = @params.rootUri;
            var result = new InitializeResult
            {
                capabilities = new ServerCapabilities
                {
                    textDocumentSync = TextDocumentSyncKind.Full,
                    documentSymbolProvider = true,
                    workspaceSymbolProvider = true,
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

        private void LoadFile(Uri uri, string path, string text)
        {
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (files.ContainsKey(path))
                return;

            var eh = new SilentErrorHandler();
            var lexer = Lexer.FromString(text, eh, path);

            SourceFile file = new SourceFile(uri);
            files[path] = file;

            var parser = new Parser(lexer, eh);

            void HandleImport(string baseFilePath, AstImportExpr import)
            {
                string SearchForModuleInPath(string basePath, AstIdExpr[] module)
                {
                    var path = basePath;

                    for (int i = 0; i < module.Length - 1; i++)
                    {
                        var combined = Path.Combine(path, module[i].Name);
                        if (Directory.Exists(combined))
                        {
                            path = combined;
                        }
                        else
                            return null;
                    }

                    path = Path.Combine(path, module.Last().Name);
                    path += ".che";

                    if (File.Exists(path))
                        return path;
                    return null;
                }

                IEnumerable<string> ModulePaths(string baseFilePath, AstIdExpr[] path)
                {
                    yield return Path.GetDirectoryName(baseFilePath);
                    foreach (var modulePath in modulePaths)
                        yield return modulePath;
                }

                string FindModule()
                {
                    foreach (var modPath in ModulePaths(baseFilePath, import.Path))
                    {
                        var p = SearchForModuleInPath(modPath, import.Path);
                        if (p != null)
                            return p;
                    }
                    return null;
                }

                string path = FindModule();
                if (path == null || files.ContainsKey(path))
                    return;
                var uriPath = path.Replace("\\", "/").Replace("D:", "file:///d%3A");
                var uri = new Uri(uriPath);
                var relative = _workerSpaceRoot.MakeRelativeUri(uri);
                LoadFile(new Uri(_workerSpaceRoot, relative), path, File.ReadAllText(path));
            }

            while (true)
            {
                var s = parser.ParseStatement();
                if (s == null)
                    break;

                file.statements.Add(s);

                switch (s)
                {
                    case AstConstantDeclaration d when d.Initializer is AstImportExpr import:
                        HandleImport(path, import);
                        break;
                    case AstUsingStmt d when d.Value is AstImportExpr import:
                        HandleImport(path, import);
                        break;
                }
            }
        }

        private void _documents_Changed(object sender, TextDocumentChangedEventArgs e)
        {
            var path = GetFilePath(e.Document.uri);

            if (files.TryGetValue(path, out var f))
                files.Remove(path);

            LoadFile(e.Document.uri, path, e.Document.text);
        }

        private static string GetFilePath(Uri uri)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var path = uri.LocalPath;
                path = path.TrimStart('/');
                path = path.Substring(0, 1).ToUpperInvariant() + path.Substring(1);

                return path.Replace("/", "\\"); ;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return uri.LocalPath;
            } else {
                return uri.LocalPath;
            }
        }

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

        private void GetSymbolsInScope<T>(List<SymbolInformation> result, Uri uri, List<T> statements, string query, string containerName)
            where T : AstStatement
        {
            foreach (var stmt in statements)
            {
                string name = "";
                SymbolKind kind = SymbolKind.Null;

                switch (stmt)
                {
                    case AstVariableDecl decl:
                        name = decl.Name.Name;
                        kind = SymbolKind.Variable;
                        break;

                    case AstConstantDeclaration decl:
                        name = decl.Name.Name;
                        switch (decl.Initializer)
                        {
                            case AstStructTypeExpr _:
                                kind = SymbolKind.Struct;
                                break;
                            case AstEnumTypeExpr _:
                                kind = SymbolKind.Enum;
                                break;
                            case AstTraitTypeExpr _:
                                kind = SymbolKind.Interface;
                                break;
                            case AstFuncExpr _:
                                kind = SymbolKind.Function;
                                break;
                            case AstImportExpr _:
                                kind = SymbolKind.Module;
                                break;
                            default:
                                kind = SymbolKind.Constant;
                                break;
                        }
                        break;

                    case AstImplBlock impl:
                        name = impl.ToString();
                        GetSymbolsInScope(result, uri, impl.Declarations, query, name);
                        break;

                    default:
                        continue;
                }

                if (!name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                result.Add(new SymbolInformation
                {
                    name = name,
                    kind = kind,
                    containerName = containerName,
                    location = new LanguageServer.Parameters.Location
                    {
                        uri = uri,
                        range = new LanguageServer.Parameters.Range
                        {
                            start = new Position
                            {
                                line = stmt.Beginning.line - 1,
                                character = stmt.Beginning.Column - 1
                            },
                            end = new Position
                            {
                                line = stmt.End.line - 1,
                                character = stmt.End.Column - 1
                            }
                        }
                    }
                });
            }
        }

        protected override Result<SymbolInformation[], ResponseError> Symbol(WorkspaceSymbolParams @params)
        {
            string query = @params.query;
            var result = new List<SymbolInformation>();
            foreach (var file in files)
            {
                GetSymbolsInScope(result, file.Value.uri, file.Value.statements, query, null);
            }

            return Result<SymbolInformation[], ResponseError>.Success(result.ToArray());
        }

        protected override Result<DocumentSymbolResult, ResponseError> DocumentSymbols(DocumentSymbolParams @params)
        {
            string path = GetFilePath(@params.textDocument.uri);
            if (files.TryGetValue(path, out var file))
            {
                var result = new List<SymbolInformation>();
                GetSymbolsInScope(result, file.uri, file.statements, "", null);
                return Result<DocumentSymbolResult, ResponseError>.Success(new DocumentSymbolResult(result.ToArray()));
            }
            else
            {
                return Result<DocumentSymbolResult, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InvalidParams,
                    message = $"File '{path}' not found"
                });
            }
        }

        protected override VoidResult<ResponseError> Shutdown()
        {
            Console.WriteLine("Shutting down...");

            return VoidResult<ResponseError>.Success();
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
        }

        #endregion
    }
}
