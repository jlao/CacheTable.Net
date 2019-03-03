using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CacheTable
{
    /// <summary>
    /// A thread-safe, fixed-size, set associative cache with random
    /// replacement. Thread safety is implemented with striped locks.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of values in the cache.</typeparam>
    public class ConcurrentCacheTable<TKey, TValue> : ICacheTable<TKey, TValue>
    {
        private readonly CacheTableInternal<TKey, TValue> table;
        private readonly object[] lockObjects;
        private readonly Random[] rngs;
        private readonly int[] counts;

        /// <summary>
        /// Creates a set associative cache with specified number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns per row.</param>
        /// <param name="concurrency">The number of locks to stripe rows across.</param>
        public ConcurrentCacheTable(int rows, int columns, int concurrency)
        {
            this.table = new CacheTableInternal<TKey, TValue>(rows, columns);
            this.counts = new int[rows];

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

        /// <summary>
        /// Gets the count of items in the cache.
        /// </summary>
        /// <remarks>
        /// This will acquire all locks in the cache. Reads and writes will
        /// block while the count is computed. Thus, it should be avoided in
        /// performance sensitive scenarios.
        /// </remarks>
        public int Count
        {
            get
            {
                this.AcquireAllLocks();
                try
                {
                    int count = 0;
                    for (int i = 0; i < this.counts.Length; i++)
                    {
                        count += this.counts[i];
                    }

                    return count;
                }
                finally
                {
                    this.ReleaseAllLocks();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key. When setting
        /// a new value, if the row the key maps to is full, a random key from that
        /// row is evicted to make room for the new key.
        /// </summary>
        /// <param name="key">The key of the item to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is
        /// not found, a get operation throws a <see
        /// cref="KeyNotFoundException"/>, and a set operation creates a new
        /// element with the specified key.
        /// </returns>
        /// <remarks>
        /// This will acquire a single lock for the row the key is mapped to.
        /// </remarks>
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
                    if (this.table.Set(key, value, row, this.GetRng(row)))
                    {
                        this.counts[row]++;
                    }
                }
            }
        }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        /// <remarks>
        /// This will acquire all locks in the cache. All reads and writes to
        /// the cache are blocked while it is cleared.
        /// </remarks>
        public void Clear()
        {
            this.AcquireAllLocks();
            try
            {
                this.table.Clear();
                for (int i = 0; i < this.counts.Length; i++)
                {
                    this.counts[i] = 0;
                }
            }
            finally
            {
                this.ReleaseAllLocks();
            }
        }

        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>True if the cache contains the specified key; otherwise, false.</returns>
        /// <remarks>
        /// This will acquire a single lock for the row the key is mapped to.
        /// </remarks>
        public bool ContainsKey(TKey key)
        {
            int row = this.table.FindRow(key);
            lock (this.GetLockObjectForRow(row))
            {
                return this.table.FindEntry(key, row) >= 0;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An enumerator for the cache.</returns>
        /// <remarks>
        /// The enumerator is thread-safe but does not represent a
        /// moment-in-time snapshot of the cache. Only one lock is held at a
        /// time during the enumeration. Thus, the cache can be modified while
        /// being enumerated over.
        /// </remarks>
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

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An enumerator for the cache.</returns>
        /// <remarks>
        /// The enumerator is thread-safe but does not represent a
        /// moment-in-time snapshot of the cache. Only one lock is held at a
        /// time during the enumeration. Thus, the cache can be modified while
        /// being enumerated over.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>True if the item was removed. False if no item with the specified key is in the cache.</returns>
        /// <remarks>
        /// This will acquire a single lock for the row the key is mapped to.
        /// </remarks>
        public bool Remove(TKey key)
        {
            int row = this.table.FindRow(key);
            lock (this.GetLockObjectForRow(row))
            {
                if (this.table.Remove(key, row))
                {
                    this.counts[row]--;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the
        /// specified key, if the key is found; otherwise, the default value for
        /// the type of the value parameter. This parameter is passed
        /// uninitialized.
        /// </param>
        /// <returns>True if the cache contains a value with the specified key; otherwise, false.</returns>
        /// <remarks>
        /// This will acquire a single lock for the row the key is mapped to.
        /// </remarks>
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
