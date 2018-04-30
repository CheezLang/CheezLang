using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;

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

        public PTFile AddFile(Uri uri, string body = null, Workspace workspace = null)
        {
            return AddFile(uri.AbsolutePath, body);
        }

        public PTFile AddFile(string fileName, string body = null, Workspace workspace = null)
        {
            if (workspace == null)
                workspace = mMainWorkspace;

            if (mFiles.ContainsKey(fileName))
            {
                // remove all contributions of old file
                workspace.RemoveFile(mFiles[fileName]);
            }

            var file = ParseFile(fileName, body, ErrorHandler);
            
            mFiles[fileName] = file;
            workspace.AddFile(file);

            return file;
        }

        private PTFile ParseFile(string fileName, string body, IErrorHandler eh)
        {
            var lexer = body != null ? Lexer.FromString(body, eh, fileName) : Lexer.FromFile(fileName, eh);
            var parser = new Parser(lexer, eh);
            var file = new PTFile(fileName, lexer.Text);
            
            while (true)
            {

                var result = parser.ParseStatement();
                var s = result.stmt;

                if (s is PTFunctionDecl || s is PTTypeDecl)
                {
                    s.SourceFile = file;
                    file.Statements.Add(s);
                }
                else if (s != null)
                {
                    eh.ReportError(lexer, s, "Only variable and function declarations are allowed on in global scope");
                }

                if (result.done)
                    break;
            }

            return file;
        }

        public PTFile GetFile(string v)
        {
            if (!mFiles.ContainsKey(v))
                return null;
            return mFiles[v];
        }
    }
}
