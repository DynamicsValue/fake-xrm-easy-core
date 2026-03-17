using System;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    /// <summary>
    /// Implements quarter functionality
    /// </summary>
    internal static class Quarter
    {
        /// <summary>
        /// Returns the quarter for the specified date where the first quarter starts on Jan 1st
        /// </summary>
        /// <param name="date">Date to retrieve the quarter from</param>
        /// <returns></returns>
        internal static int GetQuarter(DateTime date)
        {
            var quarterStart = new DateTime(date.Year, 1, 1);
            return GetQuarter(date, quarterStart);
        }

        /// <summary>
        /// Returns the quarter for the specified date where the first quarter starts on a specified start date
        /// </summary>
        /// <param name="date">Date to retrieve the quarter from</param>
        /// <param name="firstQuarterStartDate">The start date of the 1st quarter</param>
        /// <returns></returns>
        internal static int GetQuarter(DateTime date, DateTime firstQuarterStartDate)
        {
            var dateOnly = date.Date;

            var quarterStart = firstQuarterStartDate;
            var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);

            if (dateOnly >= quarterStart && dateOnly <= quarterEnd)
            {
                return 1;
            }

            quarterStart = quarterStart.AddMonths(3);
            quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
            if (dateOnly >= quarterStart && dateOnly <= quarterEnd)
            {
                return 2;
            }
            
            quarterStart = quarterStart.AddMonths(3);
            quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
            if (dateOnly >= quarterStart && dateOnly <= quarterEnd)
            {
                return 3;
            }

            return 4; 
        }
    }
}