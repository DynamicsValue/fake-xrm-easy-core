using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static class ExpressionExtensions
    {
        internal static Expression TransformValueBasedOnOperator(this Expression input, ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.Today:
                case ConditionOperator.Yesterday:
                case ConditionOperator.Tomorrow:
                case ConditionOperator.On:
                case ConditionOperator.OnOrAfter:
                case ConditionOperator.OnOrBefore:
                    return input.TransformExpressionGetDateOnlyPart();

                default:
                    return input; //No transformation
            }
        }

        internal static Expression TransformExpressionGetDateOnlyPart(this Expression input)
        {
            return Expression.Call(input, typeof(DateTime).GetMethod("get_Date"));
        }
    }
}
