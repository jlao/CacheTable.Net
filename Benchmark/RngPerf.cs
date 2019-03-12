using BenchmarkDotNet.Attributes;
using CacheTable;
using System;

namespace Benchmark
{
    [CoreJob]
    [RankColumn]
    public class RngPerf
    {
        private readonly Random random = new Random();
        private readonly XorShiftRandom xorshift = new XorShiftRandom();
        
        [Benchmark(Baseline = true)]
        public int Random()
        {
            return this.random.Next(12);
        }

        [Benchmark]
        public int XortShiftRandom()
        {
            return this.xorshift.Next(12);
        }
    }
}
