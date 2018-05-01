using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cheez.Compiler
{
    public class PTFile : IText
    {
        public string Name { get; }
        public string Text { get; }

        public Scope ExportScope { get; }
        public Scope PrivateScope { get; }

        public List<PTStatement> Statements { get; } = new List<PTStatement>();

        public PTFile(string name, string raw)
        {
            this.Name = name;
            this.Text = raw;
            ExportScope = new Scope("Export");
            PrivateScope = new Scope("Private", ExportScope);
        }
    }

    public class Compiler
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Workspace mMainWorkspace;
        private Dictionary<string, Workspace> mWorkspaces = new Dictionary<string, Workspace>();
        public IErrorHandler ErrorHandler { get; }

        public Workspace DefaultWorkspace => mMainWorkspace;

        public Compiler(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            mMainWorkspace = new Workspace(this);
            mWorkspaces["main"] = mMainWorkspace;
        }

        public PTFile AddFile(string fileName, string body = null, Workspace workspace = null, bool reparse = false)
        {
            var normalizedPath = Path.GetFullPath(fileName).PathNormalize();
            var directory = Path.GetDirectoryName(normalizedPath);

            if (workspace == null)
                workspace = mMainWorkspace;

            if (mFiles.ContainsKey(normalizedPath))
            {
                if (!reparse)
                    return mFiles[normalizedPath];

                // remove all contributions of old file
                workspace.RemoveFile(mFiles[normalizedPath]);
            }

            var (file, loadedFiles) = ParseFile(fileName, body, ErrorHandler);
            if (file == null)
                return null;

            mFiles[normalizedPath] = file;
            workspace.AddFile(file);

            foreach (var fname in loadedFiles)
            {
                var path = Path.Combine(directory, fname);

                if (mFiles.ContainsKey(path))
                    continue;
                var f = AddFile(path, null, workspace);
                if (f == null)
                    return null;
            }

            return file;
        }

        private (PTFile, List<string>) ParseFile(string fileName, string body, IErrorHandler eh)
        {
            var lexer = body != null ? Lexer.FromString(body, eh, fileName) : Lexer.FromFile(fileName, eh);

            if (lexer == null)
                return (null, null);

            var parser = new Parser(lexer, eh);
            var file = new PTFile(fileName, lexer.Text);

            List<string> loadedFiles = new List<string>();

            while (true)
            {

                var result = parser.ParseStatement();
                var s = result.stmt;

                if (s is PTFunctionDecl || s is PTTypeDecl)
                {
                    s.SourceFile = file;
                    file.Statements.Add(s);
                }
                else if (s is PTDirectiveStatement p && p.Directive.Name.Name == "load")
                {
                    var d = p.Directive;
                    if (d.Arguments.Count != 1 || !(d.Arguments[0] is PTStringLiteral f))
                    {
                        eh.ReportError(lexer, d, "#load takes one string as argument");
                    }
                    else
                    {
                        if (f.Value.EndsWith(".che"))
                        {
                            loadedFiles.Add(f.Value);
                        }
                        else
                        {
                            loadedFiles.Add(f.Value + ".che");
                        }
                    }
                }
                else if (s != null)
                {
                    eh.ReportError(lexer, s, "Only variable and function declarations are allowed on in global scope");
                }

                if (result.done)
                    break;
            }

            return (file, loadedFiles);
        }

        public PTFile GetFile(string v)
        {
            var normalizedPath = Path.GetFullPath(v).PathNormalize();
            if (!mFiles.ContainsKey(normalizedPath))
                return null;
            return mFiles[normalizedPath];
        }
    }
}
