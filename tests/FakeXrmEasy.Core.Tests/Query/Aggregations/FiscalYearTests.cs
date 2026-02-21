using System;
using FakeXrmEasy.Core.Query.Aggregations;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class FiscalYearTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_2026_for_Jan_1st()
        {
            var date = new DateTime(2026, 1, 1);
            Assert.Equal(2026, FiscalYear.GetFiscalYear(_context, date));
        }
    }
}