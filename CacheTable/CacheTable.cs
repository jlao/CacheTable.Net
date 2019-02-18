using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CacheTable
{
    public class CacheTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly KeyValuePair<TKey, TValue>?[] table;
        private readonly int numRows;
        private readonly int numColumns;
        private readonly Random rng = new Random();
        private int count;

        public CacheTable(int rows, int columns)
        {
            this.table = new KeyValuePair<TKey, TValue>?[rows * columns];
            this.numRows = rows;
            this.numColumns = columns;
        }

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
                var kvp = new KeyValuePair<TKey, TValue>(key, value);
                (int start, int end) = this.FindRow(key);
                int empty = -1;

                for (int i = start; i < end; i++)
                {
                    if (this.table[i].HasValue)
                    {
                        if (this.table[i].Value.Key.Equals(key))
                        {
                            this.table[i] = kvp;
                            return;
                        }
                    }
                    else if (empty < 0)
                    {
                        empty = i;
                    }
                }

                if (empty >= 0)
                {
                    Debug.Assert(!this.table[empty].HasValue);

                    this.table[empty] = kvp;
                    this.count++;
                    Debug.Assert(this.count <= (this.numRows * this.numColumns));
                    return;
                }

                // No empty slot, randomly replace.
                int loc = start + this.rng.Next(this.numColumns);
                Debug.Assert(this.table[loc].HasValue);
                Debug.Assert(!this.table[loc].Value.Key.Equals(key));
                this.table[loc] = kvp;
            }
        }

        public int Count => this.count;

        public void Clear()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                this.table[i] = null;
            }

            this.count = 0;
        }

        public bool ContainsKey(TKey key) => this.FindEntry(key) >= 0;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                if (table[i].HasValue)
                {
                    yield return table[i].Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Remove(TKey key)
        {
            int loc = this.FindEntry(key);
            if (loc < 0)
            {
                return false;
            }

            this.table[loc] = null;
            this.count--;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int loc = this.FindEntry(key);
            if (loc >= 0)
            {
                KeyValuePair<TKey, TValue>? item = this.table[loc];
                Debug.Assert(item.HasValue, "item is null");
                value = item.Value.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        private int FindEntry(TKey key)
        {
            (int start, int end) = this.FindRow(key);
            for (int i = start; i < end; i++)
            {
                if (this.table[i].HasValue && this.table[i].Value.Key.Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        private (int start, int end) FindRow(TKey key)
        {
            int hash = key.GetHashCode() & 0x7FFFFFFF;
            int row = hash % this.numRows;
            int start = row * this.numColumns;
            int end = start + this.numColumns;
            return (start, end);
        }
    }
}
