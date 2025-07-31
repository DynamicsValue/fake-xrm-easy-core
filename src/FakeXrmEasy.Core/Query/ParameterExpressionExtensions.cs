using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    internal static class ParameterExpressionExtensions
    {
        internal static Expression ToAttributeValueExpression(this ParameterExpression entityParameterExpression, string attributeName)
        {
            Expression attributesProperty = Expression.Property(
                entityParameterExpression,
                "Attributes"
            );
            
            Expression getAttributeValueExpr = Expression.Property(
                attributesProperty, "Item",
                Expression.Constant(attributeName, typeof(string))
            );

            return getAttributeValueExpr;
        }

        internal static Expression ToContainsAttributeExpression(this ParameterExpression entityParameterExpression,
            string attributeName)
        {
            Expression attributesProperty = Expression.Property(
                entityParameterExpression,
                "Attributes"
            );
            
            return Expression.Call(
                attributesProperty,
                typeof(AttributeCollection).GetMethod("ContainsKey", new Type[] { typeof(string) }),
                Expression.Constant(attributeName)
            );
        }
    }
}