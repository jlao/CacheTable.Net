using System.Collections.Generic;

namespace CacheTable
{
    /// <summary>
    /// Represents a fixed size cache that maps keys to values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of values in the cache.</typeparam>
    public interface ICacheTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Gets the count of items in the cache.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets or sets the value associated with the specified key. This may
        /// evict a different key from the cache if there is not sufficient
        /// capacity.
        /// </summary>
        /// <param name="key">The key of the item to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is
        /// not found, a get operation throws a <see
        /// cref="KeyNotFoundException"/>, and a set operation creates a new
        /// element with the specified key.
        /// </returns>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>True if the cache contains the specified key; otherwise, false.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>True if the item was removed. False if no item with the specified key is in the cache.</returns>
        bool Remove(TKey key);

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
        bool TryGetValue(TKey key, out TValue value);
    }
}
