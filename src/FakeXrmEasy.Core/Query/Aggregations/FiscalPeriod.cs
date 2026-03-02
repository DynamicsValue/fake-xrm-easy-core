using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Settings;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal static class FiscalPeriod
    {
        /// <summary>
        /// Returns the fiscal period value based on the current fiscal period settings in the context
        /// </summary>
        /// <param name="context">The In-Memory context that has fiscal period settings</param>
        /// <param name="date">The date from which to retrieve the fiscal period</param>
        /// <returns>Returns the fiscal period (i.e. for quarterly "2025-01")</returns>
        internal static string GetFiscalPeriod(IXrmFakedContext context, DateTime date)
        {
            var hasFiscalYearSettings = context.HasProperty<FiscalYearSettings>();
            DateTime fiscalYearStartDate = new DateTime(date.Year, 1, 1);
            FiscalYearSettings.Template fiscalPeriodTemplate = FiscalYearSettings.Template.Quarterly;
            
            if (hasFiscalYearSettings)
            {
                var fiscalYearSettings = context.GetProperty<FiscalYearSettings>();
                fiscalYearStartDate = fiscalYearSettings.StartDate;
                fiscalPeriodTemplate = fiscalYearSettings.FiscalPeriodTemplate;
            }
            
            switch (fiscalPeriodTemplate)
            {
                case FiscalYearSettings.Template.Quarterly:
                    var fiscalYear = FiscalYear.GetFiscalYear(context, date);
                    var quarter = Quarter.GetQuarter(date, fiscalYearStartDate);
                    return $"{fiscalYear}-{quarter.ToString().PadLeft(2, '0')}";
                    break;
            }

            
            
            return "";
        }
    }
}