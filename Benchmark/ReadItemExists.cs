using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class ReadItemExists
    {
        private readonly CacheTable<int, int> cacheTable = new CacheTable<int, int>(10, 4);
        private readonly ConcurrentCacheTable<int, int> concurrentCacheTable = new ConcurrentCacheTable<int, int>(10, 4, Environment.ProcessorCount);
        private readonly Dictionary<int, int> dictionary = new Dictionary<int, int>();
        private readonly ConcurrentDictionary<int, int> concurrentDictionary = new ConcurrentDictionary<int, int>();
        private readonly int[] array = new int[10];

        [GlobalSetup]
        public void GlobalSetup()
        {
            this.cacheTable[42] = 42;
            this.concurrentCacheTable[42] = 42;
            this.dictionary[42] = 42;
            this.concurrentDictionary[42] = 42;
            this.array[0] = 42;
        }

        [Benchmark]
        public int CacheTable()
        {
            return this.cacheTable[42];
        }

        [Benchmark]
        public int ConcurrentCacheTable()
        {
            return this.concurrentCacheTable[42];
        }

        [Benchmark(Baseline = true)]
        public int Dictionary()
        {
            return this.dictionary[42];
        }

        [Benchmark]
        public int ConcurrentDictionary()
        {
            return this.concurrentDictionary[42];
        }

        [Benchmark]
        public int Array()
        {
            return this.array[0];
        }
    }
}
