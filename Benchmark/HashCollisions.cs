using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class HashCollisions
    {
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
            for (int i = 0; i < 4; i++) this.cacheTable[i] = i;
        }

        [Benchmark]
        public void ConcurrentCacheTable()
        {
            for (int i = 0; i < 4; i++) this.concurrentCacheTable[i] = i;
        }

        [Benchmark(Baseline = true)]
        public void Dictionary()
        {
            for (int i = 0; i < 4; i++) this.dictionary[i] = i;
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            for (int i = 0; i < 4; i++) this.concurrentDictionary[i] = i;
        }
    }
}
