using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToLastXExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            var beforeDateTime = default(DateTime);
            var currentDateTime = DateTime.UtcNow;
            switch (c.Operator)
            {
                case ConditionOperator.LastXHours:
                    beforeDateTime = currentDateTime.AddHours(-(int)c.Values[0]);
                    break;
                case ConditionOperator.LastXDays:
                    beforeDateTime = currentDateTime.AddDays(-(int)c.Values[0]);
                    break;
                case ConditionOperator.Last7Days:
                    beforeDateTime = currentDateTime.AddDays(-7);
                    break;
                case ConditionOperator.LastXWeeks:
                    beforeDateTime = currentDateTime.AddDays(-7 * (int)c.Values[0]);
                    break;
                case ConditionOperator.LastXMonths:
                    beforeDateTime = currentDateTime.AddMonths(-(int)c.Values[0]);
                    break;
                case ConditionOperator.LastXYears:
                    beforeDateTime = currentDateTime.AddYears(-(int)c.Values[0]);
                    break;
            }

            c.Values.Clear();          
            c.Values.Add(beforeDateTime);
            c.Values.Add(currentDateTime);
            
            return tc.ToBetweenExpression(getAttributeValueExpr, containsAttributeExpr);
        }
    }
}
