using System;
using Xunit;
using FakeXrmEasy.Core.Query.Aggregations;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class WeekTests
    {
        #region GetWeek with default DATEFIRST (Sunday)

        [Fact]
        public void GetWeek_January1_ReturnsWeek1()
        {
            var date = new DateTime(2024, 1, 1);
            Assert.Equal(1, Week.GetWeek(date));
        }

        [Fact]
        public void GetWeek_December31_ReturnsLastWeek()
        {
            // 2024 is a leap year, Dec 31 is a Tuesday
            var date = new DateTime(2024, 12, 31);
            int week = Week.GetWeek(date);
            Assert.Equal(53, week);
        }

        [Fact]
        public void GetWeek_FirstSundayOfYear_StartsNewWeek()
        {
            // 2024: Jan 1 is Monday, so Jan 7 is Sunday (start of week 2 with DATEFIRST=7)
            var date = new DateTime(2024, 1, 7);
            Assert.Equal(2, Week.GetWeek(date));
        }

        [Theory]
        [InlineData(2023, 1, 1, 1)]   // Sunday
        [InlineData(2023, 1, 7, 1)]   // Saturday (still week 1)
        [InlineData(2023, 1, 8, 2)]   // Sunday (week 2 starts)
        [InlineData(2023, 6, 15, 24)] // Mid-year (Thursday)
        [InlineData(2023, 12, 31, 53)] // Last day of 2023 (Sunday, starts week 53)
        public void GetWeek_VariousDates2023_ReturnsExpectedWeek(int year, int month, int day, int expectedWeek)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedWeek, Week.GetWeek(date));
        }

        [Theory]
        [InlineData(2024, 1, 1, 1)]   // Monday
        [InlineData(2024, 1, 6, 1)]   // Saturday
        [InlineData(2024, 1, 7, 2)]   // Sunday (week 2)
        [InlineData(2024, 2, 29, 9)]  // Leap day
        [InlineData(2024, 12, 31, 53)] // Last day of 2024
        public void GetWeek_VariousDates2024_ReturnsExpectedWeek(int year, int month, int day, int expectedWeek)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedWeek, Week.GetWeek(date));
        }

        #endregion

        #region GetWeek with custom DayOfWeek (DATEFIRST)

        [Theory]
        [InlineData(DayOfWeek.Monday)]
        [InlineData(DayOfWeek.Tuesday)]
        [InlineData(DayOfWeek.Wednesday)]
        [InlineData(DayOfWeek.Thursday)]
        [InlineData(DayOfWeek.Friday)]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public void GetWeek_January1_AlwaysReturnsWeek1(DayOfWeek firstDay)
        {
            var date = new DateTime(2024, 1, 1);
            Assert.Equal(1, Week.GetWeek(date, firstDay));
        }

        [Fact]
        public void GetWeek_MondayFirst_WeekStartsOnMonday()
        {
            // 2024: Jan 1 is Monday
            var jan1 = new DateTime(2024, 1, 1);
            var jan7 = new DateTime(2024, 1, 7); // Sunday
            var jan8 = new DateTime(2024, 1, 8); // Monday

            Assert.Equal(1, Week.GetWeek(jan1, DayOfWeek.Monday));
            Assert.Equal(1, Week.GetWeek(jan7, DayOfWeek.Monday));
            Assert.Equal(2, Week.GetWeek(jan8, DayOfWeek.Monday));
        }

        [Fact]
        public void GetWeek_DifferentFirstDays_ProduceDifferentResults()
        {
            // 2024: Jan 3 is Wednesday
            var date = new DateTime(2024, 1, 3);

            int weekSundayFirst = Week.GetWeek(date, DayOfWeek.Sunday);
            int weekMondayFirst = Week.GetWeek(date, DayOfWeek.Monday);
            int weekWednesdayFirst = Week.GetWeek(date, DayOfWeek.Wednesday);

            // With Sunday first: Jan 1 (Mon) is in week 1, Jan 7 (Sun) starts week 2
            // With Monday first: Jan 1 (Mon) starts week 1, Jan 8 (Mon) starts week 2
            // With Wednesday first: Jan 3 (Wed) starts week 2
            Assert.Equal(1, weekSundayFirst);
            Assert.Equal(1, weekMondayFirst);
            Assert.Equal(2, weekWednesdayFirst);
        }

        #endregion

        #region GetWeek with SQL Server DATEFIRST integer

        [Theory]
        [InlineData(1)] // Monday
        [InlineData(2)] // Tuesday
        [InlineData(3)] // Wednesday
        [InlineData(4)] // Thursday
        [InlineData(5)] // Friday
        [InlineData(6)] // Saturday
        [InlineData(7)] // Sunday
        public void GetWeek_ValidDateFirst_DoesNotThrow(int dateFirst)
        {
            var date = new DateTime(2024, 6, 15);
            var exception = Record.Exception(() => Week.GetWeek(date, dateFirst));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(8)]
        [InlineData(100)]
        public void GetWeek_InvalidDateFirst_ThrowsArgumentOutOfRangeException(int dateFirst)
        {
            var date = new DateTime(2024, 6, 15);
            Assert.Throws<ArgumentOutOfRangeException>(() => Week.GetWeek(date, dateFirst));
        }

        [Fact]
        public void GetWeek_DateFirst7_MatchesDefaultBehavior()
        {
            var date = new DateTime(2024, 6, 15);
            Assert.Equal(
                Week.GetWeek(date),
                Week.GetWeek(date, 7)
            );
        }

        #endregion

        #region GetIsoWeek

        [Theory]
        [InlineData(2024, 1, 1, 1)]   // Monday - ISO week 1
        [InlineData(2024, 1, 7, 1)]   // Sunday - still ISO week 1
        [InlineData(2024, 1, 8, 2)]   // Monday - ISO week 2
        [InlineData(2024, 12, 30, 1)] // Monday - ISO week 1 of 2025
        [InlineData(2024, 12, 31, 1)] // Tuesday - ISO week 1 of 2025
        public void GetIsoWeek_VariousDates2024_ReturnsExpectedWeek(int year, int month, int day, int expectedWeek)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedWeek, Week.GetIsoWeek(date));
        }

        [Theory]
        [InlineData(2023, 1, 1, 52)]  // Sunday - ISO week 52 of 2022
        [InlineData(2023, 1, 2, 1)]   // Monday - ISO week 1
        [InlineData(2023, 12, 31, 52)] // Sunday
        public void GetIsoWeek_VariousDates2023_ReturnsExpectedWeek(int year, int month, int day, int expectedWeek)
        {
            var date = new DateTime(year, month, day);
            Assert.Equal(expectedWeek, Week.GetIsoWeek(date));
        }

        [Fact]
        public void GetIsoWeek_WeekAlwaysStartsOnMonday()
        {
            // Find a Monday and verify the whole week has the same ISO week number
            var monday = new DateTime(2024, 3, 4); // Monday
            int expectedWeek = Week.GetIsoWeek(monday);

            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);
                Assert.Equal(expectedWeek, Week.GetIsoWeek(date));
            }

            // Next Monday should be different week
            Assert.Equal(expectedWeek + 1, Week.GetIsoWeek(monday.AddDays(7)));
        }

        [Fact]
        public void GetIsoWeek_ReturnsValueBetween1And53()
        {
            // Test entire year
            var startDate = new DateTime(2024, 1, 1);
            for (int i = 0; i < 366; i++)
            {
                var date = startDate.AddDays(i);
                int week = Week.GetIsoWeek(date);
                Assert.InRange(week, 1, 53);
            }
        }

        #endregion

        #region DateFirstToDayOfWeek

        [Theory]
        [InlineData(1, DayOfWeek.Monday)]
        [InlineData(2, DayOfWeek.Tuesday)]
        [InlineData(3, DayOfWeek.Wednesday)]
        [InlineData(4, DayOfWeek.Thursday)]
        [InlineData(5, DayOfWeek.Friday)]
        [InlineData(6, DayOfWeek.Saturday)]
        [InlineData(7, DayOfWeek.Sunday)]
        public void DateFirstToDayOfWeek_ValidValues_ReturnsCorrectDayOfWeek(int dateFirst, DayOfWeek expected)
        {
            Assert.Equal(expected, Week.DateFirstToDayOfWeek(dateFirst));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(8)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void DateFirstToDayOfWeek_InvalidValues_ThrowsArgumentOutOfRangeException(int dateFirst)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Week.DateFirstToDayOfWeek(dateFirst));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void GetWeek_LeapYearFeb29_ReturnsCorrectWeek()
        {
            var leapDay = new DateTime(2024, 2, 29);
            int week = Week.GetWeek(leapDay);
            Assert.Equal(9, week);
        }

        [Fact]
        public void GetWeek_YearBoundary_HandlesCorrectly()
        {
            var dec31 = new DateTime(2023, 12, 31);
            var jan1 = new DateTime(2024, 1, 1);

            int lastWeek2023 = Week.GetWeek(dec31);
            int firstWeek2024 = Week.GetWeek(jan1);

            Assert.Equal(53, lastWeek2023);
            Assert.Equal(1, firstWeek2024);
        }

        [Fact]
        public void GetWeek_MinDateTime_DoesNotThrow()
        {
            var exception = Record.Exception(() => Week.GetWeek(DateTime.MinValue));
            Assert.Null(exception);
        }

        [Fact]
        public void GetWeek_MaxDateTime_DoesNotThrow()
        {
            var exception = Record.Exception(() => Week.GetWeek(DateTime.MaxValue));
            Assert.Null(exception);
        }

        [Fact]
        public void GetIsoWeek_YearWithWeek53_ReturnsWeek53()
        {
            // 2020 has 53 ISO weeks (Dec 31, 2020 is Thursday in week 53)
            var date = new DateTime(2020, 12, 31);
            Assert.Equal(53, Week.GetIsoWeek(date));
        }

        #endregion
    }
}