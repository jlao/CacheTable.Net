using System;
using System.Collections;
using System.Collections.Generic;

namespace CacheTable
{
    /// <summary>
    /// A non-thread-safe, fixed size, set associative cache with random replacement. See
    /// <see cref="ConcurrentCacheTable{TKey, TValue}"/> for a thread-safe version.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of values in the cache.</typeparam>
    public class CacheTable<TKey, TValue> : ICacheTable<TKey, TValue>
    {
        private CacheTableInternal<TKey, TValue> table;
        private int count;
        private readonly XorShiftRandom rng = new XorShiftRandom();

        /// <summary>
        /// Creates a set associative cache with specified number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns per row.</param>
        public CacheTable(int rows, int columns)
        {
            this.table = new CacheTableInternal<TKey, TValue>(rows, columns);
        }

        /// <summary>
        /// Gets the count of items in the cache.
        /// </summary>
        public int Count => this.count;

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
                if (this.table.Set(key, value, this.table.FindRow(key), this.rng))
                {
                    this.count++;
                }
            }
        }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        public void Clear()
        {
            this.table.Clear();
            this.count = 0;
        }

        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>True if the cache contains the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return this.table.FindEntry(key, this.table.FindRow(key)) >= 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An enumerator for the cache.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An enumerator for the cache.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.table.GetEnumerator();

        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>True if the item was removed. False if no item with the specified key is in the cache.</returns>
        public bool Remove(TKey key)
        {
            if (this.table.Remove(key, this.table.FindRow(key)))
            {
                this.count--;
                return true;
            }

            return false;
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
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.table.TryGetValue(key, this.table.FindRow(key), out value);
        }
    }
}
