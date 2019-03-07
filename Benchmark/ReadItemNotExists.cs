using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class ReadItemNotExists
    {
        private readonly CacheTable<int, int> cacheTable = new CacheTable<int, int>(10, 4);
        private readonly ConcurrentCacheTable<int, int> concurrentCacheTable = new ConcurrentCacheTable<int, int>(10, 4, Environment.ProcessorCount);
        private readonly Dictionary<int, int> dictionary = new Dictionary<int, int>();
        private readonly ConcurrentDictionary<int, int> concurrentDictionary = new ConcurrentDictionary<int, int>();
        private readonly int[] array = new int[10];

        [Benchmark]
        public int CacheTable()
        {
            this.cacheTable.TryGetValue(42, out int val);
            return val;
        }

        [Benchmark]
        public int ConcurrentCacheTable()
        {
            this.concurrentCacheTable.TryGetValue(42, out int val);
            return val;
        }

        [Benchmark(Baseline = true)]
        public int Dictionary()
        {
            this.dictionary.TryGetValue(42, out int val);
            return val;
        }

        [Benchmark]
        public int ConcurrentDictionary()
        {
            this.concurrentDictionary.TryGetValue(42, out int val);
            return val;
        }

        [Benchmark]
        public int Array()
        {
            return this.array[0];
        }
    }
}
