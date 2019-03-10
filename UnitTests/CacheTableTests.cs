using CacheTable;
using Xunit.Abstractions;

namespace UnitTests
{
    public class CacheTableTests : CacheTableTestBase
    {
        public CacheTableTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override ICacheTable<TKey, TValue> CreateTable<TKey, TValue>(int numRows, int numColumns)
        {
            return new CacheTable<TKey, TValue>(numRows, numColumns);
        }
    }
}
