using BenchmarkDotNet.Attributes;
using CacheTable;

namespace Benchmark
{
    [ClrJob]
    [RankColumn]
    public class Eviction
    {
        private CacheTable<WrappedInt, int> cacheTable;
        private int i;

        [Params(4, 8)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            this.cacheTable = new CacheTable<WrappedInt, int>(10, this.N);

            for (int i = 0; i < this.N; i++)
            {
                this.cacheTable[i] = i;
            }

            this.i = this.N;
        }

        [Benchmark]
        public void CacheTable()
        {
            this.cacheTable[this.i] = this.i++;
        }
    }
}
