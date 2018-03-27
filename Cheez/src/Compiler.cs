using Cheez.Ast;
using Cheez.Parsing;
using Cheez.SemanticAnalysis;
using System.Collections.Generic;

namespace Cheez
{
    public class CheezFile
    {
        public Scope ExportScope { get; } = new Scope();
        private Scope mPrivateScope = new Scope();
        public ScopeRef PrivateScope { get; }

        public List<Statement> Statements { get; }

        public CheezFile(List<Statement> statements)
        {
            PrivateScope = new ScopeRef(mPrivateScope, ExportScope);
            Statements = statements;
        }
    }

    public struct CompilationUnit
    {
        public CheezFile file;
        public Statement statement;
    }

    public class Compiler
    {
        private Dictionary<string, CheezFile> mFiles = new Dictionary<string, CheezFile>();
        private Workspace mMainWorkspace = new Workspace();
        private Dictionary<string, Workspace> mWorkspaces = new Dictionary<string, Workspace>();

        private PriorityQueue<CompilationUnit> mCompilationQueue = new PriorityQueue<CompilationUnit>();
        private List<(CompilationUnit unit, object condition)> mWaitingQueue = new List<(CompilationUnit, object)>();

        public Compiler()
        {
            mWorkspaces["main"] = mMainWorkspace;
        }

        public CheezFile AddFile(string fileName, Workspace workspace = null)
        {
            if (mFiles.ContainsKey(fileName))
                return mFiles[fileName];

            if (workspace == null)
                workspace = mMainWorkspace;

            // parse file
            List<Statement> statements = new List<Statement>();
            {
                var lexer = Lexer.FromFile(fileName);
                var parser = new Parser(lexer);

                while (true)
                {
                    var s = parser.ParseStatement();
                    if (s == null)
                        break;
                    statements.Add(s);
                }
            }

            var file = new CheezFile(statements);
            mFiles[fileName] = file;
            workspace.AddFile(file);

            // queue statements for compilation
            foreach (var s in statements)
            {
                EnqueueUnit(new CompilationUnit { file = file, statement = s });
            }

            return file;
        }

        private void CheckWaitingQueue()
        {
            for (int i = mWaitingQueue.Count - 1; i >= 0; i--)
            {
                var v = mWaitingQueue[i];
                var unit = v.unit;
                var condition = v.condition;

                switch (condition)
                {
                    case WaitForType t:
                        if (unit.file.PrivateScope.Types.GetCType(t.TypeName) != null)
                        {
                            EnqueueUnit(unit);
                            mWaitingQueue.RemoveAt(i);
                        }
                        break;
                }
            }
        }

        public void CompileAll()
        {
            while (true)
            {
                if (mCompilationQueue.IsEmpty)
                {
                    if (mWaitingQueue.Count == 0)
                        return;

                    CheckWaitingQueue();
                    if (mCompilationQueue.IsEmpty && mWaitingQueue.Count > 0)
                    {
                        // compilation error: unresolved references
                        throw new System.Exception("Compilation Error");
                    }
                }

                var next = mCompilationQueue.Dequeue();

                try
                {
                    Compile(next);
                }
                catch (WaitForType w)
                {
                    mWaitingQueue.Add((next, w));
                }
            }
        }

        private void EnqueueUnit(CompilationUnit unit)
        {
            mCompilationQueue.Enqueue(0, unit);
        }

        private void Compile(CompilationUnit unit)
        {
            //var typeChecker = new TypeChecker(unit);
            //typeChecker.CheckTypes();
        }
    }
}
