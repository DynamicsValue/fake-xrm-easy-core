using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// ConditionExpression Extensions
    /// </summary>
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToBetweenExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            object value1, value2;
            value1 = c.Values[0];
            value2 = c.Values[1];

            //Between the range... 
            var exp = Expression.And(
                Expression.GreaterThanOrEqual(
                            tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, value1),
                            TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(value1, tc.AttributeType)),

                Expression.LessThanOrEqual(
                            tc.AttributeType.GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, value2),
                            TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(value2, tc.AttributeType)));


            //and... attribute exists too
            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                exp));
        }
    }
}
