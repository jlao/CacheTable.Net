using CacheTable;
using FluentAssertions;
using System;
using Xunit;

namespace UnitTests
{
    public class CacheTableTests
    {
        [Fact]
        public void SetAndTryGetValue()
        {
            var table = new CacheTable<int, int>(10, 4);
            table[1] = 2;
            table.TryGetValue(1, out int val).Should().BeTrue();
            val.Should().Be(2);
        }

        [Fact]
        public void SetAndIndex()
        {
            var table = new CacheTable<int, int>(10, 4);
            table[1] = 2;
            table[1].Should().Be(2);
        }

        [Fact]
        public void Remove()
        {
            var table = new CacheTable<int, int>(10, 4);
            table[1] = 2;
            table.Remove(1).Should().BeTrue();
            table.TryGetValue(1, out int val).Should().BeFalse();
            val.Should().Be(default(int));
        }

        [Fact]
        public void Clear()
        {
            const int numInserts = 40;

            var table = new CacheTable<int, int>(10, 4);
            for (int i = 0; i < numInserts; i++)
            {
                table[i] = i;
            }

            table.Count.Should().BeGreaterThan(0);
            table.Clear();

            for (int i = 0; i < numInserts; i++)
            {
                table.TryGetValue(i, out _).Should().BeFalse();
            }

            table.Count.Should().Be(0);
        }
    }
}
