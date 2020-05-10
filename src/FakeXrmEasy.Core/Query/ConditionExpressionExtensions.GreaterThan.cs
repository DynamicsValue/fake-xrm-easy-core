using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        internal static Expression ToGreaterThanExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            if (c.Values.Count(v => v != null) != 1)
            {
                throw new FaultException(new FaultReason($"The ConditonOperator.{c.Operator} requires 1 value/s, not {c.Values.Count(v => v != null)}. Parameter Name: {c.AttributeName}"), new FaultCode(""), "");
            }

            if (tc.AttributeType == typeof(string))
            {
                return tc.ToGreaterThanStringExpression(getAttributeValueExpr, containsAttributeExpr);
            }
            else if (TypeCastExpressions.GetAppropiateTypeForValue(c.Values[0]) == typeof(string))
            {
                return tc.ToGreaterThanStringExpression(getAttributeValueExpr, containsAttributeExpr);
            }
            else
            {
                BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
                foreach (object value in c.Values)
                {
                    var leftHandSideExpression = tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, value);
                    var transformedExpression = leftHandSideExpression.TransformValueBasedOnOperator(tc.CondExpression.Operator);

                    expOrValues = Expression.Or(expOrValues,
                            Expression.GreaterThan(
                                transformedExpression,
                                TypeCastExpressions.GetAppropiateTypedValueAndType(value, tc.AttributeType).TransformValueBasedOnOperator(tc.CondExpression.Operator)));
                }
                return Expression.AndAlso(
                                containsAttributeExpr,
                                Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                    expOrValues));
            }

        }

        internal static Expression ToGreaterThanOrEqualExpression(this TypedConditionExpression tc, IXrmFakedContext context, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            return Expression.Or(
                tc.ToGreaterThanExpression(getAttributeValueExpr, containsAttributeExpr), 
                tc.ToEqualExpression(context, getAttributeValueExpr, containsAttributeExpr));
        }


        internal static Expression ToGreaterThanStringExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            foreach (object value in c.Values)
            {
                var leftHandSideExpression = tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, value);
                var transformedExpression = leftHandSideExpression.TransformValueBasedOnOperator(tc.CondExpression.Operator);

                var left = transformedExpression;
                var right = TypeCastExpressions.GetAppropiateTypedValueAndType(value, tc.AttributeType).TransformValueBasedOnOperator(tc.CondExpression.Operator);

                var methodCallExpr = left.ToCompareToExpression<string>(right);

                expOrValues = Expression.Or(expOrValues,
                        Expression.GreaterThan(
                            methodCallExpr,
                            Expression.Constant(0)));
            }

            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }
    }
}
