# CacheTable.Net

CacheTable.Net implements a simple data structure suitable for use as a cache.

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/CacheTable.Net.svg)](https://www.nuget.org/packages/CacheTable.Net/)
[![Build Status](https://jlao.visualstudio.com/CacheTable.Net/_apis/build/status/jlao.CacheTable.Net?branchName=master)](https://jlao.visualstudio.com/CacheTable.Net/_build/latest?definitionId=1&branchName=master)

## Cache tables

A cache table is a fixed size set associative dictionary. When you create a `CacheTable<TKey, TValue>`, or its thread-safe variant `ConcurrentCacheTable<TKey, TValue>`, you specify the size of the cache by specifying the number of rows and columns for the cache.

    // Create a cache with 1000 rows and 4 columns.
    var cache = new CacheTable<int, int>(1000, 4);

When a new key is inserted, the cache maps it to a row and attempts to insert it into an empty column. If all columns in the row are occupied, a random key in the row is evicted to make space.

Both cache table implementations implement the `ICacheTable<TKey, TValue>` interface which has handful of methods which do what you would expect.

- `int Count { get; }`
- `TValue Item[TKey key] { get; set; }`
- `bool ContainsKey(TKey key)`
- `bool Remove(TKey key)`
- `bool TryGetValue(TKey key, out TValue value)`
- `void Clear()`

`ICacheTable<TKey, TValue>` inherits from `IEnumerable<KeyValuePair<TKey, TValue>>` so it can be enumerated as well.

## ConcurrentCacheTable

Thread safety in `ConcurrentCacheTable<TKey, TValue>` is implemented via a simple striped locking scheme. The constructor takes an additional argument that allows you to specify the number of locks to use which roughly translates to the overall desired concurrency of the cache.

    // Create a cache with 1000 rows, 4 columns, and 5 locks.
    var cache = new CacheConcurrentTable<int, int>(1000, 4, 5);

Each row is synchronized by one of the locks. If there are `L` locks, then row `i` is synchronized by lock `i % L`.

The following methods only require taking a single row lock:

- `TValue Item[TKey key] { get; set; }`
- `bool ContainsKey(TKey key)`
- `bool Remove(TKey key)`
- `bool TryGetValue(TKey key, out TValue value)`

The following methods require taking all locks and should be used with care since they will prevent all other operations until the method returns. These should be used with care in performance sensitive scenarios.

- `int Count { get; }`
- `void Clear()`

The `GetEnumerator()` will take all locks but will only hold one lock at a time as it enumerates the rows. As a consequence, the enumeration does not represent a moment-in-time snapshot of the cache. Modifications made to the cache after calling `GetEnumerator()` may appear in the enumeration.

## References

- https://fgiesen.wordpress.com/2019/02/11/cache-tables/