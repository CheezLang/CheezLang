using Cheez.Ast;
using Cheez.Visitor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cheez
{
    public class Semanticer
    {
        private PriorityQueue<Statement> mQueue = new PriorityQueue<Statement>();
        private List<Statement> mWaitingQueue = new List<Statement>();
        private List<Statement> mCompiledStatements = new List<Statement>();
        private readonly object _queueLock = new object();
        private readonly object _waitingQueueLock = new object();

        private volatile bool mClosed = false;

        private Task mTask;

        public void Start()
        {
            if (mTask != null)
                return;
            
            mTask = Task.Factory.StartNew(Process);
        }

        public void Complete()
        {
            mClosed = true;
            lock (_queueLock)
            {
                Monitor.PulseAll(_queueLock);
            }

            mTask.Wait();
        }

        public Statement[] GetCompiledStatements()
        {
            return mCompiledStatements.ToArray();
        }

        public void CompileStatements(IEnumerable<Statement> statements)
        {
            if (mClosed)
                throw new Exception("Cannot add statements to closed semanticer.");

            lock (_queueLock)
            {
                foreach (var statement in statements)
                {
                    mQueue.Enqueue(GetPriority(statement), statement);
                }
                Monitor.Pulse(_queueLock);
            }
        }

        public void CompileStatement(Statement statement)
        {
            if (mClosed)
                throw new Exception("Cannot add statements to closed semanticer.");

            lock (_queueLock)
            {
                mQueue.Enqueue(GetPriority(statement), statement);
                Monitor.Pulse(_queueLock);
            }
        }

        private void ProcessStatement(Statement statement)
        {
            Console.WriteLine(statement.Accept(new AstPrinter()));

            lock (_waitingQueueLock)
            {
                mCompiledStatements.Add(statement);
            }
        }

        private void Process()
        {
            Statement GetStatement()
            {
                lock (_queueLock)
                {
                    while (mQueue.IsEmpty)
                    {
                        if (mClosed)
                            return null;

                        Monitor.Wait(_queueLock);
                    }

                    return mQueue.Dequeue();
                }
            }

            while (true)
            {
                Statement statement = GetStatement();
                if (statement == null)
                {
                    if (!UpdateWaitingQueue())
                        return;
                }

                ProcessStatement(statement);
            }
        }

        private bool UpdateWaitingQueue()
        {
            List<Statement> ready = new List<Statement>();
            lock (_waitingQueueLock)
            {
                for (int i = mWaitingQueue.Count - 1; i >= 0; i--)
                {
                    // if (some condition) // @Todo
                    {
                        var stmt = mWaitingQueue[i];
                        mWaitingQueue.RemoveAt(i);
                        ready.Add(stmt);
                    }
                }
            }

            if (ready.Count == 0)
                return false;

            lock (_queueLock)
            {
                foreach (var stmt in ready)
                {
                    mQueue.Enqueue(GetPriority(stmt), stmt);
                }
                Monitor.PulseAll(_queueLock);
                return true;
            }
        }

        private int GetPriority(Statement statement)
        {
            if (statement is FunctionDeclaration)
                return 1;

            return 0;
        }
    }
}
