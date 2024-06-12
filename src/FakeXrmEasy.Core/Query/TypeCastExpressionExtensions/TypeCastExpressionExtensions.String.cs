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
        internal static Expression GetAppropriateCastExpressionBasedOnStringAndType(Expression input, object value, Type attributeType)
        {
            var defaultStringExpression = GetAppropriateCastExpressionDefault(input, value).ToCaseInsensitiveExpression();

            int iValue;
            if (attributeType.IsOptionSet() && int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetAppropriateCastExpressionBasedOnInt(input).ToStringExpression<Int32>(),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }
        internal static Expression GetAppropriateCastExpressionBasedOnString(Expression input, object value)
        {
            var defaultStringExpression = GetAppropriateCastExpressionDefault(input, value).ToCaseInsensitiveExpression();

            DateTime dtDateTimeConversion;
            if (DateTime.TryParse(value.ToString(), out dtDateTimeConversion))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            int iValue;
            if (int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetAppropriateCastExpressionBasedOnInt(input).ToStringExpression<Int32>(),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }
    }
}
