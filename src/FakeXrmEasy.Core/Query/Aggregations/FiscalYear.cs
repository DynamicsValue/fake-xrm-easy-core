using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Settings;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    /// <summary>
    /// Implements quarter functionality
    /// </summary>
    internal static class FiscalYear
    {
        /// <summary>
        /// Returns the fiscal year for the specified date, taking into account the fiscal year settings in the context
        /// </summary>
        /// <param name="context">The current IXrmFakedContext In-Memory context</param>
        /// <param name="date">The date to retrieve the fiscal year from</param>
        /// <returns></returns>
        internal static int GetFiscalYear(IXrmFakedContext context, DateTime date)
        {
            DateTime fiscalYearDateInCurrentYear = new DateTime(date.Year, 1, 1);
            var hasFiscalYearSettings = context.HasProperty<FiscalYearSettings>();
            if (hasFiscalYearSettings)
            {
                var fiscalYearDate = context.GetProperty<FiscalYearSettings>()?.StartDate ?? new DateTime(date.Year, 1, 1);
                fiscalYearDateInCurrentYear = new DateTime(date.Year, fiscalYearDate.Month, fiscalYearDate.Day);
            }

            var year = date.Year;
            if (date < fiscalYearDateInCurrentYear)
                year -= 1;

            return year;
        }
    }
}