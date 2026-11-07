#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions    
    {
        internal static Expression ToNotUnderExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, IXrmFakedContext context)
        {
            return Expression.Not(tc.ToUnderExpression(getAttributeValueExpr, containsAttributeExpr, context));
        }
    }
}
#endif