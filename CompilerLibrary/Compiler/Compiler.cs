﻿using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Compiler.Parsing;
using Cheez.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Cheez.Compiler
{
    public class PTFile : IText
    {
        public string Name { get; }
        public string Text { get; }

        public Scope ExportScope { get; }
        public Scope PrivateScope { get; }

        public List<AstStatement> Statements { get; } = new List<AstStatement>();

        public List<string> Libraries { get; set; } = new List<string>();
        public List<string> LibrariyIncludeDirectories { get; set; } = new List<string>();

        public PTFile(string name, string raw)
        {
            this.Name = name;
            this.Text = raw;
            ExportScope = new Scope("Export");
            PrivateScope = new Scope("Private", ExportScope);
        }

        public override string ToString()
        {
            return $"PTFile: {Name}";
        }
    }

    public class Compiler : ITextProvider
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Dictionary<string, Lexer> mLoadingFiles = new Dictionary<string, Lexer>();
        private Dictionary<string, Workspace> mWorkspaces = new Dictionary<string, Workspace>();
        public IErrorHandler ErrorHandler { get; }
        public Dictionary<string, string> ModulePaths = new Dictionary<string, string>();

        private Workspace mMainWorkspace;
        public Workspace DefaultWorkspace => mMainWorkspace;

        public Compiler(IErrorHandler errorHandler, string stdlib)
        {
            ErrorHandler = errorHandler;
            mMainWorkspace = new Workspace(this);
            mWorkspaces["main"] = mMainWorkspace;

            ErrorHandler.TextProvider = this;

            string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "libraries");
            if (stdlib != null) exePath = stdlib;
            ModulePaths["std"] = Path.Combine(exePath, "std");
            ModulePaths["opengl"] = Path.Combine(exePath, "opengl");
            ModulePaths["glfw"] = Path.Combine(exePath, "glfw");
            ModulePaths["bmp"] = Path.Combine(exePath, "bmp");
        }

        public PTFile AddFile(string fileNameT, string body = null, Workspace workspace = null, bool reparse = false)
        {
            if (!ValidateFilePath("", fileNameT, false, ErrorHandler, null, out string filePath))
            {
                return null;
            }

            if (workspace == null)
                workspace = mMainWorkspace;

            if (mFiles.ContainsKey(filePath))
            {
                if (!reparse)
                    return mFiles[filePath];

                // remove all contributions of old file
                workspace.RemoveFile(mFiles[filePath]);
            }

            var (file, loadedFiles) = ParseFile(filePath, body, ErrorHandler);
            if (file == null)
                return null;

            mFiles[filePath] = file;
            workspace.AddFile(file);

            foreach (var fname in loadedFiles)
            {
                if (mFiles.ContainsKey(fname))
                    continue;
                var f = AddFile(fname, null, workspace);
                if (f == null)
                    return null;
            }

            return file;
        }

        private bool ValidateFilePath(string dir, string filePath, bool isRel, IErrorHandler eh, (IText file, ILocation loc)? from, out string path)
        {
            path = filePath;
            if (!path.EndsWith(".che"))
            {
                path += ".che";
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

        private bool RequireDirectiveArguments(List<AstExpression> args, params Type[] types)
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

        private void HandleDirective(AstDirectiveStatement directive, IErrorHandler eh, Lexer lexer, List<string> loadedFiles, PTFile file)
        {
            if (directive.Directive.Name.Name == "with_module")
            {
                var d = directive.Directive;
                if (!RequireDirectiveArguments(d.Arguments, typeof(AstStringLiteral), typeof(AstStringLiteral)))
                {
                    eh.ReportError(lexer, d, $"Invalid arguments for with_module directive");
                    return;
                }

                string moduleName = (d.Arguments[0] as AstStringLiteral).StringValue;
                string modulePath = (d.Arguments[1] as AstStringLiteral).StringValue;

                if (modulePath.StartsWith("./"))
                {
                    string sourceFileDir = Path.GetDirectoryName(directive.Beginning.file);
                    modulePath = Path.Combine(sourceFileDir, modulePath);
                }

                ModulePaths[moduleName] = modulePath;
            }
            else if (directive.Directive.Name.Name == "load")
            {
                var d = directive.Directive;
                if (d.Arguments.Count != 1 || !(d.Arguments[0] is AstStringLiteral f))
                {
                    eh.ReportError(lexer, d, "#load takes one string as argument");
                    return;
                }

                string path = f.StringValue;
                int colon = path.IndexOf(':');
                if (colon >= path.Length - 1)
                {
                    eh.ReportError(lexer, d, "Invalid load: path can not be empty");
                    return;
                }

                if (colon >= 0)
                {
                    string module = path.Substring(0, colon);

                    if (module.Length == 0)
                    {
                        eh.ReportError(lexer, d, "empty module name is not allowed");
                        return;
                    }

                    if (!ModulePaths.TryGetValue(module, out string modulePath))
                    {
                        eh.ReportError(lexer, d, $"The module '{module}' is not defined.");
                        return;
                    }

                    string libPath = path.Substring(colon + 1);
                    if (ValidateFilePath(modulePath, libPath, true, eh, (lexer, d), out string pp))
                        loadedFiles.Add(pp);
                }
                else if (ValidateFilePath(Path.GetDirectoryName(file.Name), f.StringValue, true, eh, (lexer, d), out string pp))
                {
                    loadedFiles.Add(pp);
                }
            }
            else if (directive.Directive.Name.Name == "lib")
            {
                var d = directive.Directive;
                if (d.Arguments.Count != 1 || !(d.Arguments[0] is AstStringLiteral f))
                {
                    eh.ReportError(lexer, d, "#lib takes one string as argument");
                    return;
                }

                string libFile = f.StringValue;
                if (libFile.StartsWith("./"))
                {
                    string sourceFileDir = Path.GetDirectoryName(directive.Beginning.file);
                    libFile = Path.Combine(sourceFileDir, f.StringValue);
                }

                file.Libraries.Add(libFile);
            }
            else
            {
                eh.ReportError(lexer, directive, "Invalid directive at this location");
            }
        }

        private (PTFile, List<string>) ParseFile(string fileName, string body, IErrorHandler eh)
        {
            var lexer = body != null ? Lexer.FromString(body, eh, fileName) : Lexer.FromFile(fileName, eh);
            
            mLoadingFiles.Add(fileName, lexer);

            if (lexer == null)
                return (null, null);

            var parser = new Parser(lexer, eh);
            var file = new PTFile(fileName, lexer.Text);

            List<string> loadedFiles = new List<string>();

            while (true)
            {

                var result = parser.ParseStatement();
                var s = result.stmt;

                if (s is AstFunctionDecl || s is AstStructDecl || s is AstImplBlock || s is AstEnumDecl || s is AstVariableDecl || s is AstTypeAliasDecl || s is AstTraitDeclaration)
                {
                    s.SourceFile = file;
                    file.Statements.Add(s);
                }
                else if (s is AstDirectiveStatement directive)
                {
                    HandleDirective(directive, eh, lexer, loadedFiles, file);
                }
                else if (s != null)
                {
                    eh.ReportError(lexer, s, "Only variable and function declarations are allowed on in global scope");
                }

                if (result.done)
                    break;
            }

            mLoadingFiles.Remove(fileName);
            return (file, loadedFiles);
        }

        public PTFile GetFile(string v)
        {
            var normalizedPath = Path.GetFullPath(v).PathNormalize();
            if (!mFiles.ContainsKey(normalizedPath))
                return null;
            return mFiles[normalizedPath];
        }

        public IText GetText(ILocation location)
        {
            var normalizedPath = Path.GetFullPath(location.Beginning.file).PathNormalize();

            if (mFiles.TryGetValue(normalizedPath, out var f))
                return f;
            if (mLoadingFiles.TryGetValue(normalizedPath, out var f2))
                return f2;

            return null;
        }
    }
}