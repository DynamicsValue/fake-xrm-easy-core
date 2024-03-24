using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToInExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));

#if FAKE_XRM_EASY_9
            if (tc.AttributeType?.IsOptionSetValueCollection() == true)
            {
                var leftHandSideExpression = tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, null);
                var rightHandSideExpression = Expression.Constant(OptionSetValueCollectionExtensions.ConvertToHashSetOfInt(c.Values, isOptionSetValueCollectionAccepted: false));

                expOrValues = Expression.Equal(
                    Expression.Call(leftHandSideExpression, typeof(HashSet<int>).GetMethod("SetEquals"), rightHandSideExpression),
                    Expression.Constant(true));
            }
            else
#endif
            {
                foreach (object value in c.Values)
                {
                    if (!(value is Array))
                    {
                        expOrValues = Expression.Or(expOrValues, Expression.Equal(
                            tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, value),
                            TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(value, tc.AttributeType)));
                    }
                }
            }

            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }

    }
}
