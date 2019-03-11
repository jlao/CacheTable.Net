using CacheTable;
using System;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class GuidKeysReadFullTable
    {
        private readonly CacheTable<WrappedString, int> cacheTable = new CacheTable<WrappedString, int>(10, 4);
        private readonly Dictionary<WrappedString, int> dictionary = new Dictionary<WrappedString, int>();
        private WrappedString[] keys;

        struct WrappedString : IEquatable<WrappedString>
        {
            public string Value;

            public bool Equals(WrappedString other) => this.Value.Equals(other.Value);

            public override int GetHashCode() => this.Value.GetHashCode();

            public override bool Equals(object obj) => this.Equals((WrappedString)obj);

            public static implicit operator WrappedString(string str) => new WrappedString { Value = str };
        }

        [GlobalSetup]
        public void Setup()
        {
            while (cacheTable.Count != 40)
            {
                this.cacheTable[Guid.NewGuid().ToString()] = 42;
            }

            this.keys = new WrappedString[40];
            int i = 0;
            foreach (KeyValuePair<WrappedString, int> kvp in cacheTable)
            {
                this.dictionary[kvp.Key] = kvp.Value;
                this.keys[i++] = kvp.Key;
            }

            Random rng = new Random();
            Shuffle(rng, this.keys);
        }

        public static void Shuffle<T>(Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        [Benchmark]
        public int CacheTable()
        {
            int sum = 0;
            foreach (var k in this.keys)
            {
                sum += this.cacheTable[k];
            }

            return sum;
        }

        [Benchmark(Baseline = true)]
        public int Dictionary()
        {
            int sum = 0;
            foreach (var k in this.keys)
            {
                sum += this.dictionary[k];
            }

            return sum;
        }
    }
}
