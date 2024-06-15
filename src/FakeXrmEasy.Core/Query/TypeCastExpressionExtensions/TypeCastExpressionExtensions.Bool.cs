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
        internal static Expression GetAppropriateCastExpressionBasedOnBoolean(Expression input)
        {
            return Expression.Condition(
                Expression.TypeIs(input, typeof(BooleanManagedProperty)),
                Expression.Convert(
                    Expression.Call(Expression.TypeAs(input, typeof(BooleanManagedProperty)),
                        typeof(BooleanManagedProperty).GetMethod("get_Value")),
                    typeof(bool)),
                Expression.Condition(Expression.TypeIs(input, typeof(bool)),
                    Expression.Convert(input, typeof(bool)),
                    Expression.Constant(false)));

        }
    }
}
