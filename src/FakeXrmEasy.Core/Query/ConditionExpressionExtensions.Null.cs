using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToNullExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            return Expression.Or(Expression.AndAlso(
                                    containsAttributeExpr,
                                    Expression.Equal(
                                    getAttributeValueExpr,
                                    Expression.Constant(null))),   //Attribute is null
                                 Expression.AndAlso(
                                    Expression.Not(containsAttributeExpr),
                                    Expression.Constant(true)));   //Or attribute is not defined (null)
        }
    }
}
