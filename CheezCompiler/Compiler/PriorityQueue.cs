using System;
using System.Collections;
using System.Collections.Generic;

namespace Cheez.Compiler
{
    public class PriorityQueue<T> : ICollection, IEnumerable
    {
        private List<(int priority, T data)> mList = new List<(int priority, T data)>();

        public int Count => mList.Count;
        public bool IsEmpty => mList.Count == 0;
        
        public object SyncRoot => null;
        public bool IsSynchronized => false;

        public void Enqueue(int priority, T data)
        {
            int index = mList.Count;
            mList.Add((priority, data));

            // bubble up
            while (index != 0)
            {
                var parentIndex = (index - 1) / 2;
                var parent = mList[parentIndex];
                if (parent.priority >= priority)
                    break;

                mList[parentIndex] = mList[index];
                mList[index] = parent;
                index = parentIndex;
            }
        }

        public T Dequeue()
        {
            if (IsEmpty)
                throw new System.Exception("Trying to dequeue from empty heap");

            Swap(0, mList.Count - 1);

            var entry = mList[mList.Count - 1];
            mList.RemoveAt(mList.Count - 1);

            // sift down
            int index = 0;
            while (true)
            {
                int i1 = index * 2 + 1;
                int i2 = i1 + 1;
                int maxIndex = -1;
                if (i1 < mList.Count && mList[i1].priority > mList[index].priority)
                    maxIndex = i1;
                if (i2 < mList.Count && mList[i2].priority > mList[i1].priority && mList[i2].priority > mList[index].priority)
                    maxIndex = i2;

                if (maxIndex != -1)
                {
                    Swap(index, maxIndex);
                    index = maxIndex;
                }
                else break;
            }

            return entry.data;
        }

        private void Swap(int i1, int i2)
        {
            var v = mList[i1];
            mList[i1] = mList[i2];
            mList[i2] = v;
        }

        public void CopyTo(Array array, int index)
        {
            int i = 0;
            foreach (var entry in mList)
            {
                array.SetValue(entry, index + i);
                i++;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return mList.GetEnumerator();
        }
    }
}
