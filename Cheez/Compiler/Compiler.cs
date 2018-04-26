using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cheez.Compiler
{
    public class ParseError
    {
        public ILocation Location { get; }
        public string Message { get; }

        public object ParseTreeNode { get; }
        public PTExpr ParseTreeNodeExpr => ParseTreeNode as PTExpr;
        public PTStatement ParseTreeNodeStmt => ParseTreeNode as PTStatement;

        public ParseError(ILocation loc, string message, object ptn = null)
        {
            this.Location = loc;
            this.Message = message;
            this.ParseTreeNode = ptn;
        }
    }

    public class PTFile : IText, IErrorHandler
    {
        public string Name { get; }
        public string Text { get; }

        public Scope ExportScope { get; }
        public Scope PrivateScope { get; }

        public List<PTStatement> Statements { get; } = new List<PTStatement>();

        public List<ParseError> Errors { get; } = new List<ParseError>();

        public bool HasErrors => Errors.Count != 0;

        public PTFile(string name, string raw)
        {
            this.Name = name;
            this.Text = raw;
            ExportScope = new Scope("Export");
            PrivateScope = new Scope("Private", ExportScope);
        }

        public void ReportError(IText text, ILocation location, string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new ParseError(location, message));
        }

        public void ReportError(ILocation location, string message, object ptn = null, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            Errors.Add(new ParseError(location, message, ptn));
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

        public PTFile AddFile(Uri uri, string body = null, Workspace workspace = null)
        {
            return AddFile(uri.AbsolutePath, body);
        }

        public PTFile AddFile(string fileName, string body = null, Workspace workspace = null)
        {
            if (mFiles.ContainsKey(fileName))
                return mFiles[fileName];

            if (workspace == null)
                workspace = mMainWorkspace;

            // parse file
            List<PTStatement> statements = new List<PTStatement>();
            var lexer = body != null ? Lexer.FromString(body) : Lexer.FromFile(fileName);
            var file = new PTFile(fileName, lexer.Text);
            var parser = new Parser(lexer, file);

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
                        file.ReportError(s, "Only variable and function declarations are allowed on in global scope", s);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (parser.HasErrors)
                mErrorHandler.ReportCompilerError($"Failed to parse file '{fileName}'");

            mFiles[fileName] = file;
            workspace.AddFile(file);

            return file;
        }
    }
}
