using System;
using System.Collections;
using System.Collections.Generic;

namespace CacheTable
{
    public class CacheTable<TKey, TValue> : ICacheTable<TKey, TValue>
    {
        private CacheTableInternal<TKey, TValue> table;
        private int count;
        private readonly Random rng = new Random();

        public CacheTable(int rows, int columns)
        {
            this.table = new CacheTableInternal<TKey, TValue>(rows, columns);
        }

        public int Count => this.count;

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

        public void Clear()
        {
            this.table.Clear();
            this.count = 0;
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
            if (this.table.Remove(key, this.table.FindRow(key)))
            {
                this.count--;
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.table.TryGetValue(key, this.table.FindRow(key), out value);
        }
    }
}
