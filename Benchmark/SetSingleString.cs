using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class SetSingleString
    {
        private static readonly WrappedString Zero = "0";

        private readonly CacheTable<WrappedString, int> cacheTable = new CacheTable<WrappedString, int>(10, 4);
        private readonly ConcurrentCacheTable<WrappedString, int> concurrentCacheTable = new ConcurrentCacheTable<WrappedString, int>(10, 4, Environment.ProcessorCount);
        private readonly Dictionary<WrappedString, int> dictionary = new Dictionary<WrappedString, int>();
        private readonly ConcurrentDictionary<WrappedString, int> concurrentDictionary = new ConcurrentDictionary<WrappedString, int>();

        // Wrap the string because Dictionary has special code to optimize
        // string hashing. This levels the playing field.
        struct WrappedString : IEquatable<WrappedString>
        {
            public string Value;

            public bool Equals(WrappedString other) => this.Value.Equals(other.Value);

            public override int GetHashCode() => this.Value.GetHashCode();

            public override bool Equals(object obj) => this.Equals((WrappedString)obj);

            public static implicit operator WrappedString(string str) => new WrappedString { Value = str };
        }

        [Benchmark]
        public void CacheTable()
        {
            this.cacheTable[Zero] = 0;
        }

        [Benchmark]
        public void ConcurrentCacheTable()
        {
            this.concurrentCacheTable[Zero] = 0;
        }

        [Benchmark]
        public void Dictionary()
        {
            this.dictionary[Zero] = 0;
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            this.concurrentDictionary[Zero] = 0;
        }

        [Benchmark(Baseline = true)]
        public int Hash()
        {
            return Zero.GetHashCode();
        }
    }
}
