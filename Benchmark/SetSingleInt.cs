using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class SetSingleInt
    {
        private readonly CacheTable<int, int> cacheTable = new CacheTable<int, int>(10, 4);
        private readonly ConcurrentCacheTable<int, int> concurrentCacheTable = new ConcurrentCacheTable<int, int>(10, 4, Environment.ProcessorCount);
        private readonly Dictionary<int, int> dictionary = new Dictionary<int, int>();
        private readonly ConcurrentDictionary<int, int> concurrentDictionary = new ConcurrentDictionary<int, int>();
        private readonly int[] array = new int[10];

        [Benchmark]
        public void CacheTable()
        {
            this.cacheTable[42] = 42;
        }

        [Benchmark]
        public void ConcurrentCacheTable()
        {
            this.concurrentCacheTable[42] = 42;
        }

        [Benchmark(Baseline = true)]
        public void Dictionary()
        {
            this.dictionary[42] = 42;
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            this.concurrentDictionary[42] = 42;
        }

        [Benchmark]
        public void Array()
        {
            this.array[4] = 42;
        }
    }
}
