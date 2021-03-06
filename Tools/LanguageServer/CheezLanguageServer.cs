using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Cheez;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Parsing;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Visitors;
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
        public string text;

        public SourceFile(Uri uri, string text)
        {
            this.uri = uri;
            this.text = text;
        }

        internal void UpdateText(string text) {
            this.text = text;
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
                    completionProvider = new CompletionOptions() {
                        triggerCharacters = new string[] {
                            "."
                        }
                    },
                    definitionProvider = true,
                    hoverProvider = true,
                    //signatureHelpProvider = new SignatureHelpOptions
                    //{
                    //    triggerCharacters = new string[]
                    //    {
                    //        "."
                    //    }
                    //},
                    executeCommandProvider = new ExecuteCommandOptions
                    {
                        commands = new string[] { "reload_language_server" }
                    },
                }
            };

            return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(result);
        }

        private void LoadFile(Uri uri, string path, string text)
        {
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (files.ContainsKey(path))
                return;

            var eh = new SilentErrorHandler();
            var lexer = Lexer.FromString(text, eh, path);

            SourceFile file = new SourceFile(uri, text);
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
            try
            {
                var path = GetFilePath(e.Document.uri);

                if (files.TryGetValue(path, out var f))
                    files.Remove(path);

                LoadFile(e.Document.uri, path, e.Document.text);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"_documents_Changed({e.Document.uri}) threw Exception: {ex.Message}\n{ex.StackTrace}");
                files.Remove(GetFilePath(e.Document.uri));
                throw;
            }
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


        private List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)> FindSymbolsInAllFiles(string id) {
            var result = new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
            foreach (var file in files)
                GetMatchingNodes(result, file.Value.uri, file.Value.statements, id, null, TypeKind.None, exactMatch: true);
            return result;
        }

        private void GetSymbolsInScope<T>(List<SymbolInformation> result, Uri uri, List<T> statements, string query, string containerName, TypeKind containerKind, bool exactMatch = false)
            where T : AstStatement
        {
            var symbols = new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
            GetMatchingNodes(symbols, uri, statements, query, containerName, containerKind, exactMatch: exactMatch);
            result.AddRange(symbols.Select(sym => GetSymbolInformationForStatement(sym.stmt, sym.containerKind, sym.container, sym.uri)));
        }

        private string GetNameForStatement(AstStatement statement) {
            switch (statement)
            {
                case AstVariableDecl decl: return decl.Name.Name;
                case AstConstantDeclaration decl: return  decl.Name.Name;
                case AstImplBlock impl: return impl.ToString();
                default: return "";
            }
        }

        enum TypeKind
        {
            None, Struct, Enum, Trait, Impl
        }

        private SymbolKind GetSymbolKindForStatement(AstStatement statement, TypeKind type) {
            switch (statement)
            {
                case AstVariableDecl decl:
                    {
                        switch (type)
                        {
                            case TypeKind.Struct: return SymbolKind.Field;
                            case TypeKind.Enum: return SymbolKind.EnumMember;
                            case TypeKind.Trait: return SymbolKind.Variable;
                            default: return SymbolKind.Variable;
                        } 
                    }

                case AstConstantDeclaration decl:
                    switch (decl.Initializer)
                    {
                        case AstStructTypeExpr _ : return SymbolKind.Struct;
                        case AstEnumTypeExpr   _ : return SymbolKind.Enum;
                        case AstTraitTypeExpr  _ : return SymbolKind.Interface;
                        case AstFuncExpr       _ : return SymbolKind.Function;
                        case AstImportExpr     _ : return SymbolKind.Module;
                        default                  : return SymbolKind.Constant;
                    }

                default: return SymbolKind.Null;
            }
        }

        private SymbolInformation GetSymbolInformationForStatement(AstStatement stmt, TypeKind containerKind, string containerName, Uri uri) {
            string name = GetNameForStatement(stmt);
            var kind = GetSymbolKindForStatement(stmt, containerKind);
            return new SymbolInformation
            {
                name = name,
                kind = kind,
                containerName = containerName,
                location = new LanguageServer.Parameters.Location {
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
            };
        }

        private LanguageServer.Parameters.Location GetLocationForStatement(AstStatement stmt, Uri uri)
        {
            return new LanguageServer.Parameters.Location {
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
            };
        }

        private void GetMatchingNodes<T>(List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)> result, Uri uri, List<T> statements, string query, string containerName, TypeKind containerKind, bool exactMatch = false)
            where T : AstStatement
        {
            foreach (var stmt in statements)
            {
                string name = "";

                switch (stmt)
                {
                    case AstVariableDecl decl:
                        name = decl.Name.Name;
                        break;

                    case AstConstantDeclaration decl:
                        name = decl.Name.Name;

                        switch (decl.Initializer)
                        {
                            case AstEnumTypeExpr type:
                                GetMatchingNodes(result, uri, type.Declarations, query, name, TypeKind.Enum, exactMatch);
                                break;
                            case AstStructTypeExpr type:
                                GetMatchingNodes(result, uri, type.Declarations, query, name, TypeKind.Struct, exactMatch);
                                break;
                            case AstTraitTypeExpr type:
                                GetMatchingNodes(result, uri, type.Declarations, query, name, TypeKind.Trait, exactMatch);
                                break;
                        }
                        break;

                    case AstImplBlock impl:
                        name = impl.ToString();
                        GetMatchingNodes(result, uri, impl.Declarations, query, name, TypeKind.Impl, exactMatch);
                        break;

                    default:
                        continue;
                }

                if (exactMatch && name != query)
                    continue;
                if (!exactMatch && !name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                result.Add((stmt, uri, containerName, containerKind));
            }
        }

        protected override Result<SymbolInformation[], ResponseError> Symbol(WorkspaceSymbolParams @params)
        {
            string query = @params.query;
            var result = new List<SymbolInformation>();
            foreach (var file in files)
            {
                try
                {
                    GetSymbolsInScope(result, file.Value.uri, file.Value.statements, query, null, TypeKind.None);
                }
                catch (Exception e)
                {
                    Logger.Instance.Log($"Symbol({@params.query}) threw Exception: {e.Message}\n{e.StackTrace}");
                }
            }

            return Result<SymbolInformation[], ResponseError>.Success(result.ToArray());
        }

        protected override Result<DocumentSymbolResult, ResponseError> DocumentSymbols(DocumentSymbolParams @params)
        {
            try
            {
                string path = GetFilePath(@params.textDocument.uri);
                if (files.TryGetValue(path, out var file))
                {
                    var result = new List<SymbolInformation>();
                    GetSymbolsInScope(result, file.uri, file.statements, "", null, TypeKind.None);
                    return Result<DocumentSymbolResult, ResponseError>.Success(new DocumentSymbolResult(result.ToArray()));
                }
                return Result<DocumentSymbolResult, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InvalidParams,
                    message = $"File '{@params.textDocument}' not found"
                });
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"DocumentSymbols({@params.textDocument}) threw Exception: {e.Message}\n{e.StackTrace}");
                return Result<DocumentSymbolResult, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InternalError,
                    message = $"{e.Message}"
                });
            }
        }

        private string BuildDocumentationMarkdown((AstStatement stmt, Uri uri, string container, TypeKind containerKind) sym, SymbolInformation symbolInfo, bool saveSpace)
        {
var sb = new StringWriter();
            void Indent(int level)
            {
                for (int i = 0; i < level; i++)
                    sb.Write("    ");
            }

            sb.Write("`");
            sb.Write(Path.GetFileName(GetFilePath(sym.uri)));
            sb.Write("`");

            if (!string.IsNullOrWhiteSpace(sym.container))
            {
                sb.Write(" `");
                sb.Write(sym.container);
                sb.Write("`");
            }

            sb.WriteLine();
            sb.WriteLine();

            // append value
            var printer = new RawAstPrinter(sb);
            switch (sym.stmt)
            {
                case AstConstantDeclaration d:
                    switch (d.Initializer)
                    {
                        case AstFuncExpr func:
                            {
                                Indent(1);

                                if (saveSpace) {
                                    sb.Write("fn(");

                                    sb.Write(string.Join(", ", func.Parameters.Select(param => param.Accept(printer))));

                                    sb.Write(") -> ");
                                    if (func.ReturnTypeExpr != null)
                                        sb.Write(func.ReturnTypeExpr.Accept(printer));
                                    else
                                        sb.Write("void");

                                    if (func.Directives != null)
                                    {
                                        foreach (var directive in func.Directives)
                                        {
                                            sb.Write(" ");
                                            sb.Write(directive.Accept(printer));
                                        }
                                    }
                                } else {
                                    sb.Write("fn");
                                    if (func.Parameters.Count == 0)
                                        sb.Write("()");
                                    sb.Write(" -> ");
                                    if (func.ReturnTypeExpr != null)
                                        sb.Write(func.ReturnTypeExpr.Accept(printer));
                                    else
                                        sb.Write("void");

                                    if (func.Directives != null)
                                    {
                                        foreach (var directive in func.Directives)
                                        {
                                            sb.Write(" ");
                                            sb.Write(directive.Accept(printer));
                                        }
                                    }

                                    if (func.Parameters.Count > 0)
                                    {
                                        sb.WriteLine();
                                        Indent(1);
                                        sb.WriteLine("(");

                                        foreach (var param in func.Parameters)
                                        {
                                            Indent(2);
                                            sb.WriteLine(param.Accept(printer));
                                        }

                                        Indent(1);
                                        sb.Write(")");
                                    }
                                }

                                break;
                            }

                        case AstStructTypeExpr str:
                            {
                                Indent(1);
                                sb.Write("struct");

                                if (str.Parameters != null && str.Parameters.Count > 0)
                                {
                                    sb.Write("(");
                                    int i = 0;
                                    foreach (var param in str.Parameters)
                                    {
                                        if (i > 0)
                                            sb.Write(", ");
                                        sb.Write(param.Accept(printer));
                                        i++;
                                    }
                                    sb.Write(")");
                                }

                                if (str.Directives != null)
                                {
                                    foreach (var directive in str.Directives)
                                    {
                                        sb.Write(" ");
                                        sb.Write(directive.Accept(printer));
                                    }
                                }

                                sb.WriteLine(" {");

                                foreach (var decl in str.Declarations)
                                {
                                    Indent(2);
                                    sb.WriteLine(decl.Accept(printer));
                                }

                                Indent(1);
                                sb.Write("}");
                                break;
                            }

                        case AstEnumTypeExpr str:
                            {
                                Indent(1);
                                sb.Write("enum");

                                if (str.Parameters != null && str.Parameters.Count > 0)
                                {
                                    sb.Write("(");
                                    int i = 0;
                                    foreach (var param in str.Parameters)
                                    {
                                        if (i > 0)
                                            sb.Write(", ");
                                        sb.Write(param.Accept(printer));
                                        i++;
                                    }
                                    sb.Write(")");
                                }

                                if (str.Directives != null)
                                {
                                    foreach (var directive in str.Directives)
                                    {
                                        sb.Write(" ");
                                        sb.Write(directive.Accept(printer));
                                    }
                                }

                                sb.WriteLine(" {");

                                foreach (var decl in str.Declarations)
                                {
                                    Indent(2);
                                    sb.WriteLine(decl.Accept(printer));
                                }

                                Indent(1);
                                sb.Write("}");
                                break;
                            }

                        case AstTraitTypeExpr str:
                            {
                                Indent(1);
                                sb.Write("trait");

                                if (str.Parameters != null && str.Parameters.Count > 0)
                                {
                                    sb.Write("(");
                                    int i = 0;
                                    foreach (var param in str.Parameters)
                                    {
                                        if (i > 0)
                                            sb.Write(", ");
                                        sb.Write(param.Accept(printer));
                                        i++;
                                    }
                                    sb.Write(")");
                                }

                                if (str.Directives != null)
                                {
                                    foreach (var directive in str.Directives)
                                    {
                                        sb.Write(" ");
                                        sb.Write(directive.Accept(printer));
                                    }
                                }

                                sb.WriteLine(" {");

                                foreach (var decl in str.Declarations)
                                {
                                    Indent(2);
                                    sb.WriteLine(decl.Accept(printer));
                                }

                                Indent(1);
                                sb.Write("}");
                                break;
                            }

                        default:
                            Indent(1);
                            printer.PrintExpression(d.Initializer);
                            break;
                    }
                    break;

                default:
                    Indent(1);
                    printer.PrintStatement(sym.stmt);
                    break;
            }

            sb.WriteLine();

            if (sym.stmt is AstDecl decl2 && !string.IsNullOrWhiteSpace(decl2.Documentation)) {
                sb.WriteLine();
                sb.WriteLine(decl2.Documentation);
            }

            return sb.ToString();
        }

        private Documentation BuildDocumentation((AstStatement stmt, Uri uri, string container, TypeKind containerKind) sym, SymbolInformation symbolInfo, bool saveSpace)
        {
            return new Documentation(new MarkupContent
            {
                kind = "markdown",
                value = BuildDocumentationMarkdown(sym, symbolInfo, saveSpace)
            }); 
        }

        protected override Result<CompletionResult, ResponseError> Completion(CompletionParams @params) {
            try
            {
                var result = new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
                foreach (var file in files)
                {

                    try
                    {
                        var symbols = new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
                        GetMatchingNodes(result, file.Value.uri, file.Value.statements, "", null, TypeKind.None, false);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Log($"Completion({@params.textDocument}, {@params.context}, {@params.position}) threw Exception: {e.Message}\n{e.StackTrace}");
                    }
                }

                return Result<CompletionResult, ResponseError>.Success(
                    new CompletionResult(new CompletionList() {
                        isIncomplete = true,
                        items = result.Select(sym =>
                        {
                            var symbolInfo = GetSymbolInformationForStatement(sym.stmt, sym.containerKind, sym.container, sym.uri);

                            Documentation documentation = BuildDocumentation(sym, symbolInfo, false);

                            return new CompletionItem
                            {
                                documentation = documentation,
                                kind = SymbolKindToCompletionItemKind(symbolInfo.kind),
                                label = symbolInfo.name
                            };
                        }).ToArray()
                    })
                );
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"Completion({@params.textDocument}, {@params.context}, {@params.position}) threw Exception: {e.Message}\n{e.StackTrace}");

                return Result<CompletionResult, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InternalError,
                    message = $"{e.Message}"
                });
            }
        }

        protected override Result<LocationSingleOrArray, ResponseError> GotoDefinition(TextDocumentPositionParams @params) {
            try
            {
                string path = GetFilePath(@params.textDocument.uri);
                if (files.TryGetValue(path, out var file)) {
                    string id = GetIdentifierContainingPosition(@params.position, file);
                    if (id == null) {
                        return Result<LocationSingleOrArray, ResponseError>.Success(
                            new LocationSingleOrArray(new LanguageServer.Parameters.Location[0])
                        );
                    }
                    var matchingSymbols = FindSymbolsInAllFiles(id);

                    return Result<LocationSingleOrArray, ResponseError>.Success(
                        new LocationSingleOrArray(matchingSymbols.Select(sym => GetLocationForStatement(sym.stmt, sym.uri)).ToArray())
                    );
                }

                return Result<LocationSingleOrArray, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InvalidParams,
                    message = $"File '{@params.textDocument}' not found"
                });
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"GotoDefinition({@params.textDocument}, {@params.position}) threw Exception: {e.Message}\n{e.StackTrace}");
                return Result<LocationSingleOrArray, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InternalError,
                    message = $"{e.Message}"
                });
            }
        }

        protected override Result<Hover, ResponseError> Hover(TextDocumentPositionParams @params) {
            try
            {
                var matchingSymbols = GetSymbolInformationAtPosition(@params.textDocument.uri, @params.position);
                var contents = matchingSymbols.Select(
                    sym => BuildDocumentationMarkdown(sym, GetSymbolInformationForStatement(sym.stmt, sym.containerKind, sym.container, sym.uri), true));
                return Result<Hover, ResponseError>.Success(new Hover
                {
                    contents = new HoverContents(new MarkupContent
                    {
                        kind = "markdown",
                        value = string.Join("\n---\n", contents)
                    })
                });
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"Hover({@params.textDocument}, {@params.position}) threw Exception: {e.Message}\n{e.StackTrace}");
                return Result<Hover, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InternalError,
                    message = $"{e.Message}"
                });
            }
        }

        protected override Result<SignatureHelp, ResponseError> SignatureHelp(TextDocumentPositionParams @params)
        {
            try
            {
                var matchingSymbols = GetSymbolInformationAtPosition(@params.textDocument.uri, @params.position);
                return Result<SignatureHelp, ResponseError>.Success(new SignatureHelp
                {
                    signatures = matchingSymbols.Select(sym =>
                    {
                        string container = "";
                        if (string.IsNullOrWhiteSpace(sym.container))
                        {
                            container = " --- " + Path.GetFileName(GetFilePath(sym.uri));
                        }
                        else
                        {
                            container = " --- " + sym.container + " --- " + Path.GetFileName(GetFilePath(sym.uri));
                        }
                        return new SignatureInformation
                        {
                            label = sym.stmt.ToString() + container
                        };
                    }).ToArray(),
                });
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"SignatureHelp({@params.textDocument}, {@params.position}) threw Exception: {e.Message}\n{e.StackTrace}");
                return Result<SignatureHelp, ResponseError>.Error(new ResponseError
                {
                    code = ErrorCodes.InternalError,
                    message = $"{e.Message}"
                });
            }
        }

        private List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)> GetSymbolInformationAtPosition(Uri uri, Position position)
        {
            string path = GetFilePath(uri);
            if (files.TryGetValue(path, out var file))
            {
                string id = GetIdentifierContainingPosition(position, file);
                if (id == null)
                {
                    return new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
                }
                var matchingSymbols = FindSymbolsInAllFiles(id);
                return matchingSymbols;
            }
            return new List<(AstStatement stmt, Uri uri, string container, TypeKind containerKind)>();
        }

        private string GetIdentifierContainingPosition(Position pos, SourceFile file) {
            int offset = GetPosition(file.text, (int)pos.line, (int)pos.character);
            if (offset < 0 || offset >= file.text.Length)
                return null;
            char c = file.text[offset];

            bool IsIdChar(char c) => c == '_' || char.IsLetterOrDigit(c);
            if (!IsIdChar(c))
                return null;

            // find start and end
            int start = offset, end = offset;
            while (start > 0 && IsIdChar(file.text[start - 1]))
                start--;
            while (end < file.text.Length - 1 && IsIdChar(file.text[end + 1]))
                end++;
            
            string id = file.text.Substring(start, end - start + 1);
            if (!Regex.IsMatch(id, "[_a-zA-Z]"))
                return null;
            return id;
        }


        /// returns the offset of (line, character) in text
        private int GetPosition(string text, int line, int character)
        {
            var pos = 0;
            for (; 0 < line; line--)
            {
                var lf = text.IndexOf('\n', pos);
                if (lf < 0)
                {
                    return text.Length;
                }
                pos = lf + 1;
            }
            var linefeed = text.IndexOf('\n', pos);
            var max = 0;
            if (linefeed < 0)
            {
                max = text.Length;
            }
            else if (linefeed > 0 && text[linefeed - 1] == '\r')
            {
                max = linefeed - 1;
            }
            else
            {
                max = linefeed;
            }
            pos += character;
            return (pos < max) ? pos : max;
        }

        private CompletionItemKind? SymbolKindToCompletionItemKind(SymbolKind kind) {
            return kind switch {
                SymbolKind.Array            => null,
                SymbolKind.Boolean          => null,
                SymbolKind.Class            => CompletionItemKind.Class,
                SymbolKind.Constant         => CompletionItemKind.Constant,
                SymbolKind.Constructor      => CompletionItemKind.Constructor,
                SymbolKind.Enum             => CompletionItemKind.Enum,
                SymbolKind.EnumMember       => CompletionItemKind.EnumMember,
                SymbolKind.Event            => CompletionItemKind.Event,
                SymbolKind.Field            => CompletionItemKind.Field,
                SymbolKind.File             => CompletionItemKind.File,
                SymbolKind.Function         => CompletionItemKind.Function,
                SymbolKind.Interface        => CompletionItemKind.Interface,
                SymbolKind.Key              => null,
                SymbolKind.Method           => CompletionItemKind.Method,
                SymbolKind.Module           => CompletionItemKind.Module,
                SymbolKind.Namespace        => null,
                SymbolKind.Null             => null,
                SymbolKind.Number           => null,
                SymbolKind.Object           => null,
                SymbolKind.Operator         => CompletionItemKind.Operator,
                SymbolKind.Package          => null,
                SymbolKind.Property         => CompletionItemKind.Property,
                SymbolKind.String           => null,
                SymbolKind.Struct           => CompletionItemKind.Struct,
                SymbolKind.TypeParameter    => CompletionItemKind.TypeParameter,
                SymbolKind.Variable         => CompletionItemKind.Variable,
            };
        }

        protected override VoidResult<ResponseError> Shutdown()
        {
            Console.WriteLine("Shutting down...");

            return VoidResult<ResponseError>.Success();
        }

        #region Document stuff

        protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
        {
            try
            {
                _documents.Add(@params.textDocument);
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"DidOpenTextDocument({@params.textDocument}) threw Exception: {e.Message}\n{e.StackTrace}");
            }
        }

        protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
        {
            try
            {
                _documents.Change(@params.textDocument.uri, @params.textDocument.version, @params.contentChanges);
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"DidChangeTextDocument({@params.textDocument}) threw Exception: {e.Message}\n{e.StackTrace}");
            }
        }

        protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
        {
            try
            {
                _documents.Remove(@params.textDocument.uri);
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"DitCloseTextDocument({@params.textDocument}) threw Exception: {e.Message}\n{e.StackTrace}");
            }
        }

        protected override void DidChangeConfiguration(DidChangeConfigurationParams @params)
        {
            _maxNumberOfProblems = @params?.settings?.cheezls?.maxNumberOfProblems ?? 100;
        }

        #endregion
    }
}
