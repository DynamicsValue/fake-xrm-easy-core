using System;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    /// <summary>
    /// Provides T-SQL WEEK function implementation for C#.
    /// Mimics SQL Server's DATEPART(week, date) behavior.
    ///
    /// References:
    /// - DATEPART: https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql
    /// - SET DATEFIRST: https://learn.microsoft.com/en-us/sql/t-sql/statements/set-datefirst-transact-sql
    /// - ISO_WEEK: https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql#iso_week-datepart
    /// </summary>
    internal static class Week
    {
        /// <summary>
        /// Default first day of week (Sunday = 7, matching SQL Server's default DATEFIRST).
        /// Per SQL Server docs: "The default value for @@DATEFIRST is 7 (Sunday) for U.S. English."
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/statements/set-datefirst-transact-sql
        /// </summary>
        private const DayOfWeek DefaultFirstDayOfWeek = DayOfWeek.Sunday;

        /// <summary>
        /// Returns the week number of the year for the specified date.
        /// Mimics T-SQL: DATEPART(week, date)
        ///
        /// Per SQL Server docs: "week (wk, ww) - Returns the number of the week of the year
        /// that contains the specified date, according to the SET DATEFIRST argument."
        /// "The first week (week 1) of a year is the week that contains January 1."
        ///
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql
        /// </summary>
        /// <param name="date">The date to get the week number for.</param>
        /// <returns>Week number (1-54).</returns>
        public static int GetWeek(DateTime date)
        {
            return GetWeek(date, DefaultFirstDayOfWeek);
        }

        /// <summary>
        /// Returns the week number of the year for the specified date with a custom first day of week.
        /// Mimics T-SQL: SET DATEFIRST + DATEPART(week, date)
        ///
        /// Per SQL Server docs: "SET DATEFIRST sets the first day of the week to a number from 1 through 7."
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/statements/set-datefirst-transact-sql
        /// </summary>
        /// <param name="date">The date to get the week number for.</param>
        /// <param name="firstDayOfWeek">The first day of the week (mimics SET DATEFIRST).</param>
        /// <returns>Week number (1-54).</returns>
        public static int GetWeek(DateTime date, DayOfWeek firstDayOfWeek)
        {
            // Get January 1 of the same year
            var jan1 = new DateTime(date.Year, 1, 1);

            // Calculate days since January 1 (0-based)
            int dayOfYear = date.DayOfYear;

            // Find which day of the week January 1 falls on, relative to firstDayOfWeek
            int jan1DayOffset = ((int)jan1.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

            // Week 1 starts on the firstDayOfWeek on or before January 1
            // Calculate the week number
            int weekNumber = (dayOfYear + jan1DayOffset - 1) / 7 + 1;

            return weekNumber;
        }
        
        /// <summary>
        /// Returns the week number using a SQL Server DATEFIRST value.
        ///
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/statements/set-datefirst-transact-sql
        /// </summary>
        /// <param name="date">The date to get the week number for.</param>
        /// <param name="dateFirst">SQL Server DATEFIRST value (1-7, where 7=Sunday is default).</param>
        /// <returns>Week number (1-54).</returns>
        public static int GetWeek(DateTime date, int dateFirst)
        {
            return GetWeek(date, DateFirstToDayOfWeek(dateFirst));
        }

        /// <summary>
        /// Returns the ISO 8601 week number of the year.
        /// Mimics T-SQL: DATEPART(iso_week, date)
        ///
        /// Per SQL Server docs: "iso_week - Returns the number of the ISO week of the year
        /// that contains the specified date. The ISO 8601 standard uses the Monday-starting
        /// week. The first week of the year is the first week that includes a Thursday,
        /// which is equivalent to the first week including January 4th."
        ///
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql#iso_week-datepart
        /// </summary>
        /// <param name="date">The date to get the ISO week number for.</param>
        /// <returns>ISO week number (1-53).</returns>
        public static int GetIsoWeek(DateTime date)
        {
            // ISO 8601 week date: week starts on Monday
            // Week 1 is the week containing the first Thursday of the year
            // (or equivalently, the week containing January 4)

            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;

            // Get the day of week (Monday = 0, Sunday = 6 for ISO)
            DayOfWeek day = date.DayOfWeek;

            // ISO weeks start on Monday, so adjust Sunday to be day 7
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Converts SQL Server DATEFIRST value (1-7) to DayOfWeek.
        ///
        /// Per SQL Server docs: "SET DATEFIRST { number | @number_var }"
        /// "1 = Monday, 2 = Tuesday, 3 = Wednesday, 4 = Thursday,
        ///  5 = Friday, 6 = Saturday, 7 = Sunday (default for U.S. English)"
        ///
        /// Reference: https://learn.microsoft.com/en-us/sql/t-sql/statements/set-datefirst-transact-sql
        /// </summary>
        /// <param name="dateFirst">SQL Server DATEFIRST value (1-7).</param>
        /// <returns>Corresponding DayOfWeek.</returns>
        public static DayOfWeek DateFirstToDayOfWeek(int dateFirst)
        {
            if (dateFirst < 1 || dateFirst > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(dateFirst), "DATEFIRST must be between 1 and 7.");
            }

            // SQL Server: 1=Monday, 7=Sunday
            // .NET DayOfWeek: 0=Sunday, 1=Monday, ..., 6=Saturday
            return dateFirst == 7 ? DayOfWeek.Sunday : (DayOfWeek)dateFirst;
        }
    }
}