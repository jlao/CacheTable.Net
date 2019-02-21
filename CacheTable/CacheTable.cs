using System;
using System.Collections;
using System.Collections.Generic;

namespace CacheTable
{
    public class CacheTable<TKey, TValue> : ICacheTable<TKey, TValue>
    {
        private CacheTableInternal<TKey, TValue> table;
        private readonly Random rng = new Random();

        public CacheTable(int rows, int columns)
        {
            this.table = new CacheTableInternal<TKey, TValue>(rows, columns);
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
                this.table.Set(key, value, this.table.FindRow(key), this.rng);
            }
        }

        public void Clear()
        {
            this.table.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return this.table.FindEntry(key, this.table.FindRow(key)) >= 0;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.table.GetEnumerator();

        public bool Remove(TKey key)
        {
            return this.table.Remove(key, this.table.FindRow(key));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.table.TryGetValue(key, this.table.FindRow(key), out value);
        }
    }
}
