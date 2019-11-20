using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Parsing;
using Cheez.Types;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Cheez
{
    public class PTFile
    {
        public string Name { get; }
        public string Text { get; }

        public Scope FileScope { get; }
        public Scope ExportScope { get; }

        public List<AstStatement> Statements { get; } = new List<AstStatement>();

        public List<string> Libraries { get; set; } = new List<string>();

        public PTFile(string name, string raw, Scope scope)
        {
            this.Name = name;
            this.Text = raw;
            FileScope = scope;
            ExportScope = new Scope("export");
        }

        public override string ToString()
        {
            return $"PTFile: {Name}";
        }
    }

    public class CheezCompiler : ITextProvider
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Dictionary<string, Lexer> mLoadingFiles = new Dictionary<string, Lexer>();
        private Dictionary<string, Workspace> mWorkspaces = new Dictionary<string, Workspace>();
        public IErrorHandler ErrorHandler { get; }
        public Dictionary<string, string> ModulePaths { get; } = new Dictionary<string, string>();

        private Workspace mMainWorkspace;
        public Workspace DefaultWorkspace => mMainWorkspace;

        public List<AstDirective> TestOutputs { get; } = new List<AstDirective>();

        //
        private Dictionary<string, string> strings = new Dictionary<string, string>();
        private int stringId = 0;

        private Scope mGlobalConstIfScope = null;

        public CheezCompiler(IErrorHandler errorHandler, string stdlib, string preload)
        {
            ErrorHandler = errorHandler;
            mMainWorkspace = new Workspace(this);
            mWorkspaces["main"] = mMainWorkspace;

            ErrorHandler.TextProvider = this;

            string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "libraries");
            if (stdlib != null) exePath = stdlib;
            ModulePaths["std"]    = exePath;
            ModulePaths["opencv"] = exePath + "/libraries";
            ModulePaths["glfw"]   = exePath + "/libraries";
            ModulePaths["imgui"]  = exePath + "/libraries";
            ModulePaths["opengl"] = exePath + "/libraries";
            ModulePaths["olc_pge"]= exePath + "/libraries";
            ModulePaths["bmp"]    = exePath + "/libraries";
            ModulePaths["lua"]    = exePath + "/libraries";

            mGlobalConstIfScope = new Scope("global_const_if");
            mGlobalConstIfScope.DefineBuiltInTypes();
            mGlobalConstIfScope.DefineBuiltInOperators();

            // add preload file
            AddFile(Path.Combine(exePath, preload ?? "std/preload.che"), globalScope: true);
        }

        public PTFile AddFile(string fileNameT, string body = null, Workspace workspace = null, bool globalScope = false)
        {
            if (!ValidateFilePath("", fileNameT, false, ErrorHandler, null, out string filePath))
            {
                return null;
            }

            if (workspace == null)
                workspace = mMainWorkspace;

            if (mFiles.ContainsKey(filePath))
            {
                return mFiles[filePath];
            }

            var file = ParseFile(filePath, body, ErrorHandler, globalScope);
            if (file == null)
                return null;

            mFiles[filePath] = file;
            workspace.AddFile(file);

            return file;
        }

        private static bool ValidateFilePath(string dir, string filePath, bool isRel, IErrorHandler eh, (string file, ILocation loc)? from, out string path)
        {
            path = filePath;

            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                path += ".che";
            }
            else if (extension != ".che")
            {
                eh.ReportError($"Invalid extension '{extension}'. Cheez source files must have the extension .che");
                return false;
            }

            if (isRel)
            {
                path = Path.Combine(dir, path);
            }

            path = Path.GetFullPath(path);
            path = path.PathNormalize();

            if (!File.Exists(path))
            {
                if (from != null)
                {
                    eh.ReportError(from.Value.file, from.Value.loc, $"File '{path}' does not exist");
                }
                else
                {
                    eh.ReportError($"File '{path}' does not exist");
                }

                return false;
            }

            return true;
        }

        private static bool RequireDirectiveArguments(List<AstExpression> args, params Type[] types)
        {
            if (args.Count != types.Length)
            {
                return false;
            }

            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].GetType() != types[i])
                    return false;
            }

            return true;
        }

        private void HandleDirective(AstDirectiveStatement directive, IErrorHandler eh, Lexer lexer, PTFile file)
        {
            if (directive.Directive.Name.Name == "lib")
            {
                var d = directive.Directive;
                if (d.Arguments.Count != 1 || !(d.Arguments[0] is AstStringLiteral f))
                {
                    eh.ReportError(lexer.Text, d, "#lib takes one string as argument");
                    return;
                }

                string libFile = f.StringValue;
                if (libFile.StartsWith("./", StringComparison.InvariantCulture))
                {
                    string sourceFileDir = Path.GetDirectoryName(directive.Beginning.file);
                    libFile = Path.Combine(sourceFileDir, f.StringValue);
                }

                file.Libraries.Add(libFile);
            }
            else if (directive.Directive.Name.Name == "test_expect_output")
            {
                var d = directive.Directive;
                foreach (var a in d.Arguments)
                {
                    if (!(a is AstStringLiteral))
                    {
                        eh.ReportError(lexer.Text, a, "Arguments to #test_expect_output must be string literals.");
                    }
                }

                TestOutputs.Add(directive.Directive);
            }
            else
            {
                eh.ReportError(lexer.Text, directive, "Invalid directive at this location");
            }
        }

        private PTFile ParseFile(string fileName, string body, IErrorHandler eh, bool globalScope = false)
        {
            var lexer = body != null ? Lexer.FromString(body, eh, fileName) : Lexer.FromFile(fileName, eh);
            
            mLoadingFiles.Add(fileName, lexer);

            if (lexer == null)
                return null;

            var parser = new Parser(lexer, eh);

            var fileScope = globalScope ?
                mMainWorkspace.GlobalScope :
                new Scope($"{Path.GetFileNameWithoutExtension(fileName)}.che", mMainWorkspace.GlobalScope);
            var file = new PTFile(fileName, lexer.Text, fileScope);

            bool isPublic = false;

            void HandleStatement(AstStatement s)
            {
                s.Scope = file.FileScope;
                if (s is AstImplBlock ||
                    s is AstVariableDecl ||
                    s is AstConstantDeclaration ||
                    (s is AstExprStmt es && es.Expr is AstImportExpr) ||
                    s is AstUsingStmt)
                {
                    s.SourceFile = file;
                    s.SetFlag(StmtFlags.GlobalScope);
                    s.SetFlag(StmtFlags.ExportScope, isPublic);
                    file.Statements.Add(s);
                }
                else if (s is AstDirectiveStatement dir && dir.Directive.Name.Name == "file_scope")
                {
                    isPublic = false;
                }
                else if (s is AstDirectiveStatement dir2 && dir2.Directive.Name.Name == "export_scope")
                {
                    isPublic = true;
                }
                else if (s is AstDirectiveStatement directive)
                {
                    HandleDirective(directive, eh, lexer, file);
                }
                else if (s is AstExprStmt exprStmt && exprStmt.Expr is AstIfExpr @if && @if.IsConstIf)
                {
                    @if.Condition.Scope = mGlobalConstIfScope;
                    var cond = mMainWorkspace.InferType(@if.Condition, CheezType.Bool);
                    if (!cond.Type.IsErrorType)
                    {
                        if (!cond.IsCompTimeValue)
                        {
                            eh.ReportError(lexer.Text, exprStmt, "Must be a constant value");
                        }
                        else
                        {
                            AstExpression expr = null;
                            if ((bool)cond.Value)
                                expr = @if.IfCase;
                            else
                                expr = @if.ElseCase;

                            if (expr != null)
                            {
                                if (expr is AstBlockExpr block)
                                {
                                    foreach (var stmt in block.Statements)
                                    {
                                        HandleStatement(stmt);
                                    }
                                }
                                else
                                    eh.ReportError(lexer.Text, expr, "Must be a block");
                            }
                        }
                    }
                }
                else if (s != null)
                {
                    eh.ReportError(lexer.Text, s, "This type of statement is not allowed in global scope");
                }
            }

            while (true)
            {

                var s = parser.ParseStatement();
                if (s == null)
                    break;

                HandleStatement(s);
            }

            mLoadingFiles.Remove(fileName);
            return file;
        }

        public PTFile GetFile(string v)
        {
            var normalizedPath = Path.GetFullPath(v).PathNormalize();
            if (!mFiles.ContainsKey(normalizedPath))
                return null;
            return mFiles[normalizedPath];
        }

        public string GetText(ILocation location)
        {
            if (location is null)
                throw new ArgumentNullException(nameof(location));

            var normalizedPath = Path.GetFullPath(location.Beginning.file).PathNormalize();

            // files
            if (mFiles.TryGetValue(normalizedPath, out var f))
                return f.Text;
            if (mLoadingFiles.TryGetValue(normalizedPath, out var f2))
                return f2.Text;

            // strings
            if (strings.TryGetValue(location.Beginning.file, out var f3))
                return f3;

            return null;
        }

        public AstStatement ParseStatement(string str, Dictionary<string, AstExpression> dictionary = null)
        {
            var id = $"string{stringId++}";
            strings[id] = str;
            return Parser.ParseStatement(str, dictionary, ErrorHandler, id);
        }

        public AstExpression ParseExpression(string str, Dictionary<string, AstExpression> dictionary = null)
        {
            var id = $"string{stringId++}";
            strings[id] = str;
            return Parser.ParseExpression(str, dictionary, ErrorHandler, id);
        }
    }
}
