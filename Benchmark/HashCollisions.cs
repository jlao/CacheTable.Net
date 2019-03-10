using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class HashCollisions
    {
        private static readonly WrappedInt Zero = 0;
        private static readonly WrappedInt One = 1;
        private static readonly WrappedInt Two = 2;
        private static readonly WrappedInt Three = 3;

        private readonly CacheTable<WrappedInt, int> cacheTable = new CacheTable<WrappedInt, int>(10, 4);
        private readonly ConcurrentCacheTable<WrappedInt, int> concurrentCacheTable = new ConcurrentCacheTable<WrappedInt, int>(10, 4, Environment.ProcessorCount);
        private readonly Dictionary<WrappedInt, int> dictionary = new Dictionary<WrappedInt, int>();
        private readonly ConcurrentDictionary<WrappedInt, int> concurrentDictionary = new ConcurrentDictionary<WrappedInt, int>();

        struct WrappedInt : IEquatable<WrappedInt>
        {
            public int Value;

            public bool Equals(WrappedInt other) => this.Value == other.Value;

            public override int GetHashCode() => 42;

            public override bool Equals(object obj) => this.Equals((WrappedInt)obj);

            public static implicit operator WrappedInt(int i) => new WrappedInt { Value = i };
        }

        [Benchmark]
        public void CacheTable()
        {
            this.cacheTable[Zero] = 0;
            this.cacheTable[One] = 1;
            this.cacheTable[Two] = 2;
            this.cacheTable[Three] = 3;
        }

        [Benchmark]
        public void ConcurrentCacheTable()
        {
            this.concurrentCacheTable[Zero] = 0;
            this.concurrentCacheTable[One] = 1;
            this.concurrentCacheTable[Two] = 2;
            this.concurrentCacheTable[Three] = 3;
        }

        [Benchmark(Baseline = true)]
        public void Dictionary()
        {
            this.dictionary[Zero] = 0;
            this.dictionary[One] = 1;
            this.dictionary[Two] = 2;
            this.dictionary[Three] = 3;
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            this.concurrentDictionary[Zero] = 0;
            this.concurrentDictionary[One] = 1;
            this.concurrentDictionary[Two] = 2;
            this.concurrentDictionary[Three] = 3;
        }
    }
}
