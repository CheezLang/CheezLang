using Cheez.Compiler.Ast;
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

        public List<PTStatement> Statements { get; }

        public PTFile(string name, string raw, List<PTStatement> statements)
        {
            this.Name = name;
            this.Text = raw;
            ExportScope = new Scope("Export");
            PrivateScope = new Scope("Private", ExportScope);
            Statements = statements;
        }
    }

    public struct CompilationUnit
    {
        public PTFile file;
        public AstStatement statement;
        public Workspace workspace;
    }

    public class Compiler
    {
        private Dictionary<string, PTFile> mFiles = new Dictionary<string, PTFile>();
        private Workspace mMainWorkspace;
        private Dictionary<string, Workspace> mWorkspaces = new Dictionary<string, Workspace>();
        private ErrorHandler mErrorHandler = new ErrorHandler();

        public Workspace DefaultWorkspace => mMainWorkspace;

        public bool HasErrors {
            get
            {
                if (mErrorHandler.HasErrors)
                    return true;
                foreach (var w in mWorkspaces.Values)
                    if (w.HasErrors)
                        return true;
                return false;
            }
        }

        public Compiler()
        {
            mMainWorkspace = new Workspace(this);
            mWorkspaces["main"] = mMainWorkspace;
        }

        public PTFile AddFile(string fileName, Workspace workspace = null)
        {
            if (mFiles.ContainsKey(fileName))
                return mFiles[fileName];

            if (workspace == null)
                workspace = mMainWorkspace;

            // parse file
            List<PTStatement> statements = new List<PTStatement>();
            var lexer = Lexer.FromFile(fileName);
            var parser = new Parser(lexer);

            try
            {
                while (true)
                {

                    var s = parser.ParseStatement();

                    if (s == null)
                    {
                        break;
                    }
                    if (s is PTFunctionDecl || s is PTTypeDecl)
                    {
                        statements.Add(s);
                    }
                    else
                    {
                        mErrorHandler.ReportError(lexer, s, "Only variable and function declarations are allowed on in global scope");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (parser.HasErrors)
                mErrorHandler.ReportCompilerError($"Failed to parse file '{fileName}'");

            var file = new PTFile(fileName, lexer.Text, statements);
            mFiles[fileName] = file;
            workspace.AddFile(file);

            return file;
        }
    }
}
