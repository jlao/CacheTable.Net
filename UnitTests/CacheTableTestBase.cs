using CacheTable;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public abstract class CacheTableTestBase<TCacheTable> where TCacheTable : ICacheTable<int, int>
    {
        protected abstract TCacheTable CreateTable(int numRows, int numColumns);

        [Fact]
        public void SetAndTryGetValue()
        {
            var table = this.CreateTable(10, 4);
            table[1] = 2;
            table.TryGetValue(1, out int val).Should().BeTrue();
            val.Should().Be(2);
        }

        [Fact]
        public void TryGetNonExistentValue()
        {
            var table = this.CreateTable(10, 4);
            table.TryGetValue(1, out int val).Should().BeFalse();
            val.Should().Be(default(int));
        }

        [Fact]
        public void SetAndIndex()
        {
            var table = this.CreateTable(10, 4);
            table[1] = 2;
            table[1].Should().Be(2);
        }

        [Fact]
        public void Update()
        {
            var table = this.CreateTable(10, 4);
            
            for (int i = 0; i < 4; i++)
            {
                table[i] = i;
            }

            for (int i = 0; i < 4; i++)
            {
                table[i] = i + 1;
            }

            for (int i = 0; i < 4; i++)
            {
                table[i].Should().Be(i + 1);
            }
        }

        [Fact]
        public void IndexNonExistentKeyThrows()
        {
            var table = this.CreateTable(10, 4);
            Action act = () => Console.WriteLine(table[0]);
            act.Should().Throw<KeyNotFoundException>()
                .WithMessage("No value found for 0");
        }

        [Fact]
        public void ContainsKey()
        {
            var table = this.CreateTable(10, 4);
            table.ContainsKey(0).Should().BeFalse();
            table[0] = 1;
            table.ContainsKey(0).Should().BeTrue();
        }

        [Fact]
        public void Remove()
        {
            var table = this.CreateTable(10, 4);
            table[1] = 2;
            table.Remove(1).Should().BeTrue();
            table.TryGetValue(1, out int val).Should().BeFalse();
            val.Should().Be(default(int));
            table.Remove(1).Should().BeFalse();
        }

        [Fact]
        public void Clear()
        {
            const int numInserts = 40;

            var table = this.CreateTable(10, 4);
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

        [Fact]
        public void Count()
        {
            var table = this.CreateTable(10, 4);
            table.Count.Should().Be(0);

            for (int i = 0; i < 4; i++)
            {
                table[i] = i;
                table.Count.Should().Be(i + 1);
            }

            for (int i = 0; i < 4; i++)
            {
                table[i] = i + 1;
                table.Count.Should().Be(4);
            }
        }

        [Fact]
        public void Enumerate()
        {
            var table = this.CreateTable(10, 4);
            var rng = new Random();
            List<KeyValuePair<int, int>> expected = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < 4; i++)
            {
                var kvp = new KeyValuePair<int, int>(rng.Next(), rng.Next());
                expected.Add(kvp);
                table[kvp.Key] = kvp.Value;
            }

            List<KeyValuePair<int, int>> actual = table.ToList();
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void InsertOnFullTableOverwrites()
        {
            var table = this.CreateTable(5, 4);

            int i = 0;
            do
            {
                table[i] = i;
                i++;
            } while (table.Count < 20);

            table.Count.Should().Be(20);

            HashSet<int> before = table.Select(kvp => kvp.Value).ToHashSet();
            table[i] = i;

            // The last insertion should overwrite a value so count should still be 20.
            table.Count.Should().Be(20);

            // There should be one new element.
            HashSet<int> after = table.Select(kvp => kvp.Value).ToHashSet();
            after.ExceptWith(before);
            after.Count.Should().Be(1);
            after.First().Should().Be(i);
        }
    }
}
