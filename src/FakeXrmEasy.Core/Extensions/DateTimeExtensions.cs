using System;
using System.Globalization;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// DateTime Extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="week"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static DateTime ToDayOfWeek(this DateTime dateTime, Int32 week, DayOfWeek dayOfWeek)
        {
            DateTime startOfYear = dateTime.AddDays(1 - dateTime.DayOfYear);
            return startOfYear.AddDays(7 * (week - 2) + ((dayOfWeek - startOfYear.DayOfWeek + 7) % 7));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="deltaWeek"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static DateTime ToDayOfDeltaWeek(this DateTime dateTime, Int32 deltaWeek, DayOfWeek dayOfWeek)
            => dateTime.ToDayOfWeek(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime
                , CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule
                , CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) + deltaWeek, dayOfWeek);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="deltaWeek"></param>
        /// <returns></returns>
        public static DateTime ToLastDayOfDeltaWeek(this DateTime dateTime, Int32 deltaWeek = 0)
            => dateTime.ToDayOfDeltaWeek(deltaWeek, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(6);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="deltaWeek"></param>
        /// <returns></returns>
        public static DateTime ToFirstDayOfDeltaWeek(this DateTime dateTime, Int32 deltaWeek = 0)
            => dateTime.ToDayOfDeltaWeek(deltaWeek, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToFirstDayOfWeek(this DateTime dateTime)
        { 
            var dayOfWeekDelta = (int)dateTime.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var delta = (7 - dayOfWeekDelta) % 7;
            return dateTime.AddDays(- delta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static DateTime ToFirstDayOfMonth(this DateTime dateTime, Int32 month)
            => dateTime.AddDays(1 - dateTime.Day).AddMonths(month - dateTime.Month);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToFirstDayOfMonth(this DateTime dateTime)
            => dateTime.ToFirstDayOfMonth(dateTime.Month);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static DateTime ToLastDayOfMonth(this DateTime dateTime, Int32 month)
        {
            Int32 addYears = month > 12 ? month % 12 : 0;
            month = month - 12 * addYears;
            return dateTime
                .AddDays(CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTime.Year + addYears, month) - dateTime.Day)
                .AddMonths(month - dateTime.Month).AddYears(addYears);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToLastDayOfMonth(this DateTime dateTime)
            => dateTime.ToLastDayOfMonth(dateTime.Month);
    }
}
