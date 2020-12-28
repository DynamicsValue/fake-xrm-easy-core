using System;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Settings;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        /// <summary>
        /// Takes a condition expression which needs translating into a 'between two dates' expression and works out the relevant dates
        /// </summary>        
        internal static Expression ToBetweenDatesExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, IXrmFakedContext context)
        {
            var c = tc.CondExpression;

            DateTime? fromDate = null;
            DateTime? toDate = null;

            var today = DateTime.Today;
            var thisYear = today.Year;
            var thisMonth = today.Month;


            switch (c.Operator)
            {
                case ConditionOperator.ThisYear: // From first day of this year to last day of this year
                    fromDate = new DateTime(thisYear, 1, 1);
                    toDate = new DateTime(thisYear, 12, 31);
                    break;
                case ConditionOperator.LastYear: // From first day of last year to last day of last year
                    fromDate = new DateTime(thisYear - 1, 1, 1);
                    toDate = new DateTime(thisYear - 1, 12, 31);
                    break;
                case ConditionOperator.NextYear: // From first day of next year to last day of next year
                    fromDate = new DateTime(thisYear + 1, 1, 1);
                    toDate = new DateTime(thisYear + 1, 12, 31);
                    break;
                case ConditionOperator.ThisMonth: // From first day of this month to last day of this month                    
                    fromDate = new DateTime(thisYear, thisMonth, 1);
                    // Last day of this month: Add one month to the first of this month, and then remove one day
                    toDate = new DateTime(thisYear, thisMonth, 1).AddMonths(1).AddDays(-1);
                    break;
                case ConditionOperator.LastMonth: // From first day of last month to last day of last month                    
                    fromDate = new DateTime(thisYear, thisMonth, 1).AddMonths(-1);
                    // Last day of last month: One day before the first of this month
                    toDate = new DateTime(thisYear, thisMonth, 1).AddDays(-1);
                    break;
                case ConditionOperator.NextMonth: // From first day of next month to last day of next month
                    fromDate = new DateTime(thisYear, thisMonth, 1).AddMonths(1);
                    // LAst day of Next Month: Add two months to the first of this month, and then go back one day
                    toDate = new DateTime(thisYear, thisMonth, 1).AddMonths(2).AddDays(-1);
                    break;
                case ConditionOperator.ThisWeek:
                    fromDate = today.ToFirstDayOfWeek();
                    toDate = fromDate?.AddDays(6);
                    break;
                case ConditionOperator.LastWeek:
                    fromDate = today.AddDays(-7).ToFirstDayOfWeek();
                    toDate = fromDate?.AddDays(6);
                    break;
                case ConditionOperator.NextWeek:
                    fromDate = today.AddDays(7).ToFirstDayOfWeek();
                    toDate = fromDate?.AddDays(6);
                    break;
                case ConditionOperator.InFiscalYear:
                    var fiscalYear = (int)c.Values[0];
                    c.Values.Clear();
                    var fiscalYearDate = context.GetProperty<FiscalYearSettings>()?.StartDate ?? new DateTime(fiscalYear, 1, 1);
                    fromDate = fiscalYearDate;
                    toDate = fiscalYearDate.AddYears(1).AddDays(-1);
                    break;
            }

            c.Values.Add(fromDate);
            c.Values.Add(toDate);

            return tc.ToBetweenExpression(getAttributeValueExpr, containsAttributeExpr);
        }
    }
}
