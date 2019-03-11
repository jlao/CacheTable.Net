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

        [Params(32, 64)]
        public int N;

        [Benchmark]
        public void CacheTable()
        {
            for (int i = 0; i < this.N; i++) this.cacheTable[i] = i;
        }

        [Benchmark(Baseline = true)]
        public void Dictionary()
        {
            for (int i = 0; i < this.N; i++) this.dictionary[i] = i;
        }

        [Benchmark]
        public void ConcurrentCacheTable()
        {
            for (int i = 0; i < this.N; i++) this.concurrentCacheTable[i] = i;
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            for (int i = 0; i < this.N; i++) this.concurrentDictionary[i] = i;
        }
    }
}
