using System;
using FakeXrmEasy.Abstractions.Settings;
using FakeXrmEasy.Core.Query.Aggregations;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class FiscalPeriodTests: FakeXrmEasyTestsBase
    {
        [Theory]
        [InlineData("2026-01", 2026,1,1)]
        [InlineData("2026-02", 2026,4,1)]
        [InlineData("2026-03", 2026,7,1)]
        [InlineData("2026-04", 2026,10,1)]
        [InlineData("2026-01", 2026,3,31)]
        [InlineData("2026-02", 2026,6,30)]
        [InlineData("2026-03", 2026,9,30)]
        [InlineData("2026-04", 2026,12,31)]
        public void Should_return_expected_fiscal_period_with_default_fiscal_year_settings(string expectedPeriod, int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedPeriod, FiscalPeriod.GetFiscalPeriod(_context, date));
        }
    }
}