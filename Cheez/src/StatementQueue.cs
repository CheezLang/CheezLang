using Cheez.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cheez
{
    public class StatementQueue
    {
        private List<(int prio, Statement stmt)> mHeap;
        private readonly object _lock = new object();

        public StatementQueue()
        {
            mHeap = new List<(int, Statement)>();
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return mHeap.Count;
                }
            }
        }

        private Random _random = new Random();
        public void Enqueue(Statement statement)
        {
            switch (statement)
            {
                case FunctionDeclarationAst fd:
                    Insert(1, statement);
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

        public Statement Dequeue(CompilationQueue queue)
        {
            lock (_lock)
            {
                while (mHeap.Count == 0)
                {
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
    }
}
