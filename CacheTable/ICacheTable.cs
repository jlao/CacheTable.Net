using System.Collections.Generic;

namespace CacheTable
{
    public interface ICacheTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }

        TValue this[TKey key] { get; set; }

        void Clear();

        bool ContainsKey(TKey key);

        bool Remove(TKey key);

        bool TryGetValue(TKey key, out TValue value);
    }
}
