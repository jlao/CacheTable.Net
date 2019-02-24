using CacheTable;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public class ConcurrentCacheTableTests : CacheTableTestBase<ConcurrentCacheTable<int, int>>
    {
        public ConcurrentCacheTableTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override ConcurrentCacheTable<int, int> CreateTable(int numRows, int numColumns)
        {
            return new ConcurrentCacheTable<int, int>(numRows, numColumns, 3);
        }

        [Fact]
        public async Task ConcurrentWrites()
        {
            var table = CreateTable(5, 4);

            const int numWrites = 1000;

            Task thread1 = Task.Factory.StartNew(() =>
            {
                for (int j = 0; j < numWrites; j++)
                {
                    //this.output.WriteLine($"t1 writes {j}");
                    table[0] = j;
                }
            });

            Task thread2 = Task.Factory.StartNew(() =>
            {
                for (int j = numWrites; j < 2 * numWrites; j++)
                {
                    //this.output.WriteLine($"t2 writes {j}");
                    table[0] = j;
                }
            });

            await thread1;
            await thread2;
            
            table[0].Should().BeOneOf(numWrites - 1, (2 * numWrites) - 1);
        }
    }
}
