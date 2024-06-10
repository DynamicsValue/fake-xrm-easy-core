using System;
using System.Globalization;
using System.Linq.Expressions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class TypeCastExpressionExtensions
    {
        internal static Expression GetAppropriateCastExpressionBasedOnDecimal(Expression input)
        {
            return Expression.Condition(
                Expression.TypeIs(input, typeof(Money)),
                Expression.Convert(
                    Expression.Call(Expression.TypeAs(input, typeof(Money)),
                        typeof(Money).GetMethod("get_Value")),
                    typeof(decimal)),
                Expression.Condition(Expression.TypeIs(input, typeof(decimal)),
                    Expression.Convert(input, typeof(decimal)),
                    Expression.Constant(0.0M)));

        }
    }
}
