using System;
using FakeXrmEasy.Abstractions.Settings;
using FakeXrmEasy.Core.Query.Aggregations;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class FiscalYearTests: FakeXrmEasyTestsBase
    {
        [Theory]
        [InlineData(2026, 2026,1,1)]
        [InlineData(2026, 2026,4,1)]
        [InlineData(2026, 2026,2,28)]
        [InlineData(2026, 2026,12,31)]
        [InlineData(2024, 2024,2,29)]
        public void Should_return_expected_fiscal_year_with_default_fiscal_year_settings(int expectedYear, int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedYear, FiscalYear.GetFiscalYear(_context, date));
        }
        
        [Theory]
        [InlineData(2025, 2026,1,1)]
        [InlineData(2025, 2026,4,1)]
        [InlineData(2025, 2026,6,30)] //last day of fiscal year
        [InlineData(2026, 2026,7,1)] //first day of fiscal year
        [InlineData(2026, 2026,12,31)]
        [InlineData(2026, 2027,6,30)] //last day of fiscal year
        [InlineData(2023, 2024,2,29)]
        public void Should_return_expected_fiscal_year_non_default_fiscal_year_settings(int expectedYear, int year, int month, int day)
        {
            _context.SetProperty(new FiscalYearSettings()
            {
                StartDate = new DateTime(2026, 7, 1) //July 1st
            });
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedYear, FiscalYear.GetFiscalYear(_context, date));
        }
    }
}