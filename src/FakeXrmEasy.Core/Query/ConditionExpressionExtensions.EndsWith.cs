using System;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Models;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        internal static Expression ToEndsWithExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            //Append a ´%´at the end of each condition value
            var computedCondition = new ConditionExpression(c.AttributeName, c.Operator, c.Values.Select(x => "%" + x.ToString()).ToList());
            var typedComputedCondition = new TypedConditionExpression(computedCondition);
            typedComputedCondition.AttributeType = tc.AttributeType;

            return typedComputedCondition.ToLikeExpression(getAttributeValueExpr, containsAttributeExpr);
        }
    }
}
