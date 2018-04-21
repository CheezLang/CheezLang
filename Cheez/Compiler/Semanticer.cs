using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cheez.Compiler
{
    public class Semanticer
    {
        private PriorityQueue<AstStatement> mQueue = new PriorityQueue<AstStatement>();
        private List<AstStatement> mWaitingQueue = new List<AstStatement>();
        private List<AstStatement> mCompiledStatements = new List<AstStatement>();
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

        public AstStatement[] GetCompiledStatements()
        {
            return mCompiledStatements.ToArray();
        }

        public void CompileStatements(IEnumerable<AstStatement> statements)
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

        public void CompileStatement(AstStatement statement)
        {
            if (mClosed)
                throw new Exception("Cannot add statements to closed semanticer.");

            lock (_queueLock)
            {
                mQueue.Enqueue(GetPriority(statement), statement);
                Monitor.Pulse(_queueLock);
            }
        }

        private void ProcessStatement(AstStatement statement)
        {
            Console.WriteLine(statement.Accept(new AstPrinter()));

            lock (_waitingQueueLock)
            {
                mCompiledStatements.Add(statement);
            }
        }

        private void Process()
        {
            AstStatement GetStatement()
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
                AstStatement statement = GetStatement();
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
            List<AstStatement> ready = new List<AstStatement>();
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

        private int GetPriority(AstStatement statement)
        {
            if (statement is AstFunctionDecl)
                return 1;

            return 0;
        }
    }
}
