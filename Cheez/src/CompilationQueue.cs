using Cheez.Ast;
using Cheez.Parsing;
using Cheez.Visitor;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Cheez
{
    public class CompilationQueue
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ErrorHandler mErrorHandler;

        // threading
        private List<Task> mTasks = new List<Task>();
        private volatile int mActiveThreads = 0;
        private readonly object _lock = new object();
        
        private List<(int prio, Statement stmt)> mHeap;
        private volatile int mStatementCount = 0;

        private volatile bool mClosed = false;
        private volatile bool mTerminationSignalSent = false;

        //
        private List<Statement> mCompiledStatements;
        private readonly object _compiledStatementsLock = new object();

        private int Count
        {
            get
            {
                lock (_lock)
                {
                    return mHeap.Count;
                }
            }
        }

        // constructor
        public CompilationQueue(int threadCount)
        {
            mHeap = new List<(int, Statement)>();
            mErrorHandler = new ErrorHandler();
            mCompiledStatements = new List<Statement>();

            for (int i = 0; i < threadCount; i++)
            {
                int id = i;
                var task = Task.Factory.StartNew(() => Consumer(id));
                mTasks.Add(task);
            }
        }

        public Statement[] GetCompiledStatements()
        {
            return mCompiledStatements.ToArray();
        }

        public void Complete()
        {
            lock (_lock)
            {
                mClosed = true;
                Monitor.PulseAll(_lock);
            }

            Console.WriteLine($"Waiting for threads to finish...");
            foreach (var task in mTasks)
            {
                task.Wait();
            }
            Console.WriteLine("All threads stopped.");
        }

        public void CompileFile(string name)
        {
            try
            {
                var lexer = Lexer.FromFile(name);
                CompileFromLexer(lexer);
            }
            catch (Exception e)
            {
                mErrorHandler.ReportCompileError(e);
            }
        }
        
        public void CompileString(string str)
        {
            var lexer = Lexer.FromString(str);
            CompileFromLexer(lexer);
        }

        private void CompileFromLexer(Lexer lexer)
        {
            var parser = new Parser(lexer);

            while (true)
            {
                try
                {
                    var statement = parser.ParseStatement();
                    if (statement == null)
                        break;

                    mStatementCount++;
                    Enqueue(statement);
                }
                catch (ParsingError err)
                {
                    mErrorHandler.ReportParsingError(err);
                    break;
                }
            }
        }

        private void Consumer(int id)
        {
            //log.Debug($"Starting consumer {id}");
            while (true)
            {
                var statement = Dequeue(id);
                if (statement == null)
                    break;
                
                mActiveThreads++;
                ProcessStatement(id, statement);
                mActiveThreads--;
            }

            //log.Debug($"Thread {id} terminated.");
        }

        private void ProcessStatement(int id, Statement _statement)
        {
            // @Debug
            AstPrinter printer = new AstPrinter();
            var str = _statement.Visit(printer);

            switch (_statement)
            {
                case FunctionDeclaration fd:
                    {
                        log.Info($"{fd.Beginning} Function:\n{str}");
                        break;
                    }

                case PrintStatement print:
                    {
                        log.Info($"{print.Beginning} Print:\n{str}");
                        break;
                    }
            }
            
            lock (_compiledStatementsLock)
            {
                mCompiledStatements.Add(_statement);
            }
        }

        #region Queue
        public void Enqueue(Statement statement)
        {
            switch (statement)
            {
                case FunctionDeclaration _:
                    Insert(1, statement);
                    break;
                case PrintStatement _:
                    Insert(0, statement);
                    break;
            }
        }

        private void Insert(int priority, Statement statement)
        {
            lock (_lock)
            {
                int index = mHeap.Count;
                mHeap.Add((priority, statement));

                // bubble up
                while (index != 0)
                {
                    var parentIndex = (index - 1) / 2;
                    var parent = mHeap[parentIndex];
                    if (parent.prio >= priority)
                        break;

                    mHeap[parentIndex] = mHeap[index];
                    mHeap[index] = parent;
                    index = parentIndex;
                }

                Monitor.Pulse(_lock);
            }
        }

        public Statement Dequeue(int id)
        {
            lock (_lock)
            {
                //log.Debug($"[Thread {id}] dequeing from {mHeap.Count} elements");
                while (mHeap.Count == 0)
                {
                    if (mClosed && mActiveThreads == 0)
                    {
                        //log.Debug($"[Thread {id}] Closed, empty queue, no active threads, terminating...");

                        if (!mTerminationSignalSent)
                        {
                            //log.Debug($"[Thread {id}] sending termination signal");
                            mTerminationSignalSent = true;
                            Monitor.PulseAll(_lock);
                        }
                        return null;
                    }

                    //log.Debug($"[Thread {id}] waiting for statements...");
                    Monitor.Wait(_lock);
                }

                Swap(0, mHeap.Count - 1);

                var data = mHeap.Last();
                mHeap.RemoveAt(mHeap.Count - 1);

                // sift down
                int index = 0;
                while (true)
                {
                    int i1 = index * 2 + 1;
                    int i2 = i1 + 1;
                    int maxIndex = -1;
                    if (i1 < mHeap.Count && mHeap[i1].prio > mHeap[index].prio)
                        maxIndex = i1;
                    if (i2 < mHeap.Count && mHeap[i2].prio > mHeap[i1].prio && mHeap[i2].prio > mHeap[index].prio)
                        maxIndex = i2;

                    if (maxIndex != -1)
                    {
                        Swap(index, maxIndex);
                        index = maxIndex;
                    }
                    else break;
                }

                return data.stmt;
            }
        }

        private void Swap(int i1, int i2)
        {
            var v = mHeap[i1];
            mHeap[i1] = mHeap[i2];
            mHeap[i2] = v;
        }
        #endregion
    }
}
