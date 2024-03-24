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
        internal static Expression GetAppropriateCastExpressionBasedOnDateTime(Expression input, object value)
        {
            // Convert to DateTime if string
            DateTime _;
            if (value is DateTime || value is string && DateTime.TryParse(value.ToString(), out _))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            return input; // return directly
        }
    }
}
