using System;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    /// <summary>
    /// Implements quarter functionality
    /// </summary>
    internal static class Quarter
    {
        /// <summary>
        /// Returns the quarter for the specified date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetQuarter(DateTime date)
        {
            var dateOnly = date.Date;
            var quarterStart = new DateTime(date.Year, 1, 1);
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