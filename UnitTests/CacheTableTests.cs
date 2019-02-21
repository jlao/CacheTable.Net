using CacheTable;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class CacheTableTests : CacheTableTestBase<CacheTable<int, int>>
    {
        protected override CacheTable<int, int> CreateTable(int numRows, int numColumns)
        {
            return new CacheTable<int, int>(numRows, numColumns);
        }
    }
}
