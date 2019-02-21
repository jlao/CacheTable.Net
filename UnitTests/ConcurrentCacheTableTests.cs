using CacheTable;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class ConcurrentCacheTableTests : CacheTableTestBase<ConcurrentCacheTable<int, int>>
    {
        protected override ConcurrentCacheTable<int, int> CreateTable(int numRows, int numColumns)
        {
            return new ConcurrentCacheTable<int, int>(numRows, numColumns, 3);
        }
    }
}
