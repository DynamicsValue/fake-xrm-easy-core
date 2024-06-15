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
        internal static Expression GetAppropriateCastExpressionBasedOnInt(Expression input)
        {
            return Expression.Condition(
                Expression.TypeIs(input, typeof(OptionSetValue)),
                Expression.Convert(
                    Expression.Call(Expression.TypeAs(input, typeof(OptionSetValue)),
                        typeof(OptionSetValue).GetMethod("get_Value")),
                    typeof(int)),
                Expression.Convert(input, typeof(int)));
        }
    }
}
