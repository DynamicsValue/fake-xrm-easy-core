using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToNextXExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            var nextDateTime = default(DateTime);
            var currentDateTime = DateTime.UtcNow;
            switch (c.Operator)
            {
                case ConditionOperator.NextXHours:
                    nextDateTime = currentDateTime.AddHours((int)c.Values[0]);
                    break;
                case ConditionOperator.NextXDays:
                    nextDateTime = currentDateTime.AddDays((int)c.Values[0]);
                    break;
                case ConditionOperator.Next7Days:
                    nextDateTime = currentDateTime.AddDays(7);
                    break;
                case ConditionOperator.NextXWeeks:                  
                    nextDateTime = currentDateTime.AddDays(7 * (int)c.Values[0]);
                    break;              
                case ConditionOperator.NextXMonths:
                    nextDateTime = currentDateTime.AddMonths((int)c.Values[0]);
                    break;
                case ConditionOperator.NextXYears:
                    nextDateTime = currentDateTime.AddYears((int)c.Values[0]);
                    break;
            }

            c.Values.Clear();
            c.Values.Add(currentDateTime);
            c.Values.Add(nextDateTime);


            return tc.ToBetweenExpression(getAttributeValueExpr, containsAttributeExpr);
        }
    }
}
