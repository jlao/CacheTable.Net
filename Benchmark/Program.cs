using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Threading.Tasks;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class CacheTableBenchmark
    {
        private readonly CacheTable<int, int> cacheTable = new CacheTable<int, int>(10, 4);
        private readonly ConcurrentCacheTable<int, int> concurrentCacheTable = new ConcurrentCacheTable<int, int>(10, 4, Environment.ProcessorCount);
        private int i = 0;

        [Params(1000, 10000)]
        public int N;
        
        [Benchmark]
        public void CacheTableSetItems()
        {
            this.cacheTable[i] = i;
            i++;
        }

        [Benchmark]
        public void ConcurrentCacheSetItems()
        {
            this.concurrentCacheTable[i] = i;
            i++;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CacheTableBenchmark>();
        }
    }
}
