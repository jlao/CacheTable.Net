using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace CacheTable
{
    struct CacheTableInternal<TKey, TValue>
    {
        public readonly KeyValuePair<TKey, TValue>?[] table;
        public readonly int numRows;
        public readonly int numColumns;
        public volatile int count;

        public CacheTableInternal(int numRows, int numColumns)
        {
            this.numRows = numRows;
            this.numColumns = numColumns;
            this.table = new KeyValuePair<TKey, TValue>?[numRows * numColumns];
            this.count = 0;
        }

        public void Clear()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                this.table[i] = null;
            }

            this.count = 0;
        }

        public int FindRow(TKey key)
        {
            int hash = key.GetHashCode() & 0x7FFFFFFF;
            return hash % this.numRows;
        }

        public (int start, int end) GetRowRange(int row)
        {
            int start = row * this.numColumns;
            int end = start + this.numColumns;
            return (start, end);
        }

        public int FindEntry(TKey key, int row)
        {
            (int rowStart, int rowEnd) = this.GetRowRange(row);
            for (int i = rowStart; i < rowEnd; i++)
            {
                if (this.table[i].HasValue && this.table[i].Value.Key.Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool TryGetValue(TKey key, int row, out TValue value)
        {
            int loc = this.FindEntry(key, row);
            if (loc >= 0)
            {
                KeyValuePair<TKey, TValue>? item = table[loc];
                Debug.Assert(item.HasValue, "item is null");
                value = item.Value.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool Remove(TKey key, int row)
        {
            int loc = this.FindEntry(key, row);
            if (loc < 0)
            {
                return false;
            }

            this.table[loc] = null;
            Interlocked.Decrement(ref this.count);
            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                if (this.table[i].HasValue)
                {
                    yield return this.table[i].Value;
                }
            }
        }

        public void Set(TKey key, TValue value, int row, Random rng)
        {
            (int rowStart, int rowEnd) = this.GetRowRange(row);
            var kvp = new KeyValuePair<TKey, TValue>(key, value);
            int empty = -1;

            for (int i = rowStart; i < rowEnd; i++)
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
                Interlocked.Increment(ref this.count);
                Debug.Assert(this.count <= (this.numRows * this.numColumns));
                return;
            }

            // No empty slot, randomly replace.
            int loc = rowStart + rng.Next(this.numColumns);
            Debug.Assert(this.table[loc].HasValue);
            Debug.Assert(!this.table[loc].Value.Key.Equals(key));
            this.table[loc] = kvp;
        }
    }
}
