using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// 
    /// </summary>
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

        internal static Expression ToStringExpression<T>(this Expression e)
        {
            return Expression.Call(e, typeof(T).GetMethod("ToString", new Type[] { }));
        }
        internal static Expression ToCaseInsensitiveExpression(this Expression e)
        {
            return Expression.Call(e,
                                typeof(string).GetMethod("ToLowerInvariant", new Type[] { }));
        }

        internal static Expression ToCompareToExpression<T>(this Expression left, Expression right)
        {
            return Expression.Call(left, typeof(T).GetMethod("CompareTo", new Type[] { typeof(string) }), new[] { right });
        }
    }
}
