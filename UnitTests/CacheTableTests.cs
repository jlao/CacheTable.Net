using CacheTable;
using Xunit.Abstractions;

namespace UnitTests
{
    public class CacheTableTests : CacheTableTestBase<CacheTable<int, int>>
    {
        public CacheTableTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CacheTable<int, int> CreateTable(int numRows, int numColumns)
        {
            return new CacheTable<int, int>(numRows, numColumns);
        }
    }
}
