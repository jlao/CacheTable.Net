using System;
using System.Collections;
using System.Collections.Generic;

namespace CacheTable
{
    public class CacheTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
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
                (_, int rowStart, int rowEnd) = this.table.FindRow(key);
                this.table.Set(key, value, rowStart, rowEnd, this.rng);
            }
        }

        public void Clear()
        {
            this.table.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            (_, int rowStart, int rowEnd) = this.table.FindRow(key);
            return this.table.FindEntry(key, rowStart, rowEnd) >= 0;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.table.GetEnumerator();

        public bool Remove(TKey key)
        {
            (_, int rowStart, int rowEnd) = this.table.FindRow(key);
            int loc = this.table.FindEntry(key, rowStart, rowEnd);
            return this.table.Remove(loc);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            (_, int rowStart, int rowEnd) = this.table.FindRow(key);
            int loc = this.table.FindEntry(key, rowStart, rowEnd);
            return this.table.TryGetValue(loc, out value);
        }
    }
}
