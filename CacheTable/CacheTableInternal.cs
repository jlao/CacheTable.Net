using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CacheTable
{
    struct Entry<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
        public bool IsSet { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            this.Key = default(K);
            this.Value = default(V);
            this.IsSet = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<K, V> CreateKvp() => new KeyValuePair<K, V>(this.Key, this.Value);
    }

    struct CacheTableInternal<TKey, TValue>
    {
        public readonly Entry<TKey, TValue>[] table;
        public readonly int numRows;
        public readonly int numColumns;

        public CacheTableInternal(int numRows, int numColumns)
        {
            this.numRows = numRows;
            this.numColumns = numColumns;
            this.table = new Entry<TKey, TValue>[numRows * numColumns];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                this.table[i].Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindRow(TKey key)
        {
            int hash = key.GetHashCode() & 0x7FFFFFFF;
            return hash % this.numRows;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int start, int end) GetRowRange(int row)
        {
            int start = row * this.numColumns;
            int end = start + this.numColumns;
            return (start, end);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindEntry(TKey key, int row)
        {
            (int rowStart, int rowEnd) = this.GetRowRange(row);
            for (int i = rowStart; i < rowEnd; i++)
            {
                if (this.table[i].IsSet && this.table[i].Key.Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, int row, out TValue value)
        {
            int loc = this.FindEntry(key, row);
            if (loc >= 0)
            {
                Debug.Assert(table[loc].IsSet, "item is null");
                value = table[loc].Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, int row)
        {
            int loc = this.FindEntry(key, row);
            if (loc < 0)
            {
                return false;
            }

            this.table[loc].Clear();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < this.table.Length; i++)
            {
                if (this.table[i].IsSet)
                {
                    yield return this.table[i].CreateKvp();
                }
            }
        }

        // Returns true if item was inserted into empty slot. False otherwise.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(TKey key, TValue value, int row, XorShiftRandom rng)
        {
            (int rowStart, int rowEnd) = this.GetRowRange(row);
            int empty = -1;

            for (int i = rowStart; i < rowEnd; i++)
            {
                if (this.table[i].IsSet)
                {
                    if (this.table[i].Key.Equals(key))
                    {
                        this.table[i].Value = value;
                        return false;
                    }
                }
                else if (empty < 0)
                {
                    empty = i;
                }
            }

            if (empty >= 0)
            {
                Debug.Assert(!this.table[empty].IsSet);

                this.table[empty].Key = key;
                this.table[empty].Value = value;
                this.table[empty].IsSet = true;
                return true;
            }

            // No empty slot, randomly replace.
            int loc = rowStart + rng.Next(this.numColumns);
            Debug.Assert(this.table[loc].IsSet);
            Debug.Assert(!this.table[loc].Key.Equals(key));
            this.table[loc].Key = key;
            this.table[loc].Value = value;

            return false;
        }
    }
}
