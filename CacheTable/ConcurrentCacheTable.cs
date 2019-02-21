using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CacheTable
{
    public class ConcurrentCacheTable<TKey, TValue> : ICacheTable<TKey, TValue>
    {
        private CacheTableInternal<TKey, TValue> table;
        private readonly object[] lockObjects;
        private readonly Random[] rngs;

        public ConcurrentCacheTable(int rows, int columns, int concurrency)
        {
            this.table = new CacheTableInternal<TKey, TValue>(rows, columns);

            this.lockObjects = new object[concurrency];
            for (int i = 0; i < concurrency; i++)
            {
                this.lockObjects[i] = new object();
            }

            this.rngs = new Random[concurrency];
            for (int i = 0; i < concurrency; i++)
            {
                this.rngs[i] = new Random();
            }
        }

        public int Count => this.table.count;

        public TValue this[TKey key]
        {
            get
            {
                bool found = this.TryGetValue(key, out TValue val);
                if (!found)
                {
                    throw new KeyNotFoundException("No value found for " + key);
                }

                return val;
            }

            set
            {
                int row = this.table.FindRow(key);
                lock (this.GetLockObjectForRow(row))
                {
                    this.table.Set(key, value, row, this.GetRng(row));
                }
            }
        }

        public void Clear()
        {
            this.AcquireAllLocks();
            try
            {
                this.table.Clear();
            }
            finally
            {
                this.ReleaseAllLocks();
            }
        }

        public bool ContainsKey(TKey key)
        {
            int row = this.table.FindRow(key);
            lock (this.GetLockObjectForRow(row))
            {
                return this.table.FindEntry(key, row) >= 0;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int lockNum = 0; lockNum < this.lockObjects.Length; lockNum++)
            {
                lock (this.lockObjects[lockNum])
                {
                    for (int row = lockNum; row < this.table.numRows; row += this.lockObjects.Length)
                    {
                        (int rowStart, int rowEnd) = this.table.GetRowRange(row);
                        for (int i = rowStart; i < rowEnd; i++)
                        {
                            if (this.table.table[i].HasValue)
                            {
                                yield return this.table.table[i].Value;
                            }
                        }
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Remove(TKey key)
        {
            int row = this.table.FindRow(key);
            lock (this.GetLockObjectForRow(row))
            {
                return this.table.Remove(key, row);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int row = this.table.FindRow(key);
            lock (this.GetLockObjectForRow(row))
            {
                return this.table.TryGetValue(key, row, out value);
            }
        }

        private object GetLockObjectForRow(int row)
        {
            return this.lockObjects[row % this.lockObjects.Length];
        }

        private Random GetRng(int row)
        {
            return this.rngs[row % this.rngs.Length];
        }

        private void AcquireAllLocks()
        {
            foreach (object lockObject in this.lockObjects)
            {
                Monitor.Enter(lockObject);
            }
        }

        private void ReleaseAllLocks()
        {
            foreach (object lockObject in this.lockObjects)
            {
                Monitor.Exit(lockObject);
            }
        }
    }
}
