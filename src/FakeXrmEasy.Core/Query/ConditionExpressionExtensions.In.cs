using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        internal static Expression ToInExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));

#if FAKE_XRM_EASY_9
            if (tc.AttributeType == typeof(OptionSetValueCollection))
            {
                var leftHandSideExpression = tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, null);
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
                    if (value is Array)
                    {
                        //foreach (var a in ((Array)value))
                        //{
                        //    expOrValues = Expression.Or(expOrValues, Expression.Equal(
                        //        tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, a),
                        //        TypeCastExpressions.GetAppropiateTypedValueAndType(a, tc.AttributeType)));
                        //}
                    }
                    else
                    {
                        expOrValues = Expression.Or(expOrValues, Expression.Equal(
                                    tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, value),
                                    TypeCastExpressions.GetAppropiateTypedValueAndType(value, tc.AttributeType)));
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
