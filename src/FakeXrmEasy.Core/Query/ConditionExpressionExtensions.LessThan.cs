using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToLessThanExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            if (c.Values.Count(v => v != null) != 1)
            {
                throw new FaultException(new FaultReason($"The ConditonOperator.{c.Operator} requires 1 value/s, not {c.Values.Count(v => v != null)}. Parameter Name: {c.AttributeName}"), new FaultCode(""), "");
            }

            if (tc.AttributeType == typeof(string))
            {
                return tc.ToLessThanStringExpression(getAttributeValueExpr, containsAttributeExpr);
            }
            else if (TypeCastExpressionExtensions.GetAppropriateTypeForValue(c.Values[0]) == typeof(string))
            {
                return tc.ToLessThanStringExpression(getAttributeValueExpr, containsAttributeExpr);
            }
            else
            {
                BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
                foreach (object value in c.Values)
                {
                    var leftHandSideExpression = tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, value);
                    var transformedExpression = leftHandSideExpression.TransformValueBasedOnOperator(tc.CondExpression.Operator);

                    expOrValues = Expression.Or(expOrValues,
                            Expression.LessThan(
                                transformedExpression,
                                TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(value, tc.AttributeType).TransformValueBasedOnOperator(tc.CondExpression.Operator)));
                }
                return Expression.AndAlso(
                                containsAttributeExpr,
                                Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                    expOrValues));
            }

        }

        internal static Expression ToLessThanOrEqualExpression(this TypedConditionExpression tc, IXrmFakedContext context, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            return Expression.Or(
                tc.ToLessThanExpression(getAttributeValueExpr, containsAttributeExpr), 
                tc.ToEqualExpression(context, getAttributeValueExpr, containsAttributeExpr));
        }

        internal static Expression ToLessThanStringExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            foreach (object value in c.Values)
            {
                var leftHandSideExpression = tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, value);
                var transformedLeftHandSideExpression = leftHandSideExpression.TransformValueBasedOnOperator(tc.CondExpression.Operator);

                var rightHandSideExpression = TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(value, tc.AttributeType).TransformValueBasedOnOperator(tc.CondExpression.Operator);

                var compareToMethodCall = transformedLeftHandSideExpression.ToCompareToExpression<string>(rightHandSideExpression);

                expOrValues = Expression.Or(expOrValues,
                        Expression.LessThan(compareToMethodCall, Expression.Constant(0)));
            }
            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }
    }
}
