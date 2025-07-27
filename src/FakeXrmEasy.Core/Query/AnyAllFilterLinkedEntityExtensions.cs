#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static class AnyAllFilterLinkedEntityExtensions
    {
        internal static Expression ToAnyExpression(this List<Guid> childIds, Expression getAttributeValueExpr,
            Expression containsAttributeExpr)
        {
            BinaryExpression orExpression = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            foreach (var id in childIds)
            {
                var leftHandSideExpression =
                    typeof(Guid).GetAppropriateCastExpressionBasedOnType(getAttributeValueExpr, id);
                var rightHandSideExpression =
                    TypeCastExpressionExtensions.GetAppropriateTypedValueAndType(id, typeof(Guid));
                    
                var matchExpression = Expression.AndAlso(containsAttributeExpr,
                    Expression.Equal(leftHandSideExpression, rightHandSideExpression));

                orExpression = Expression.Or(orExpression, matchExpression);
            }

            return orExpression;
        }
        
        internal static Expression TranslateAnyAllLinkedEntityToExpression(this LinkEntity le, IXrmFakedContext context, ParameterExpression entity)
        {
            var constantExpression = Expression.Constant(false);
                
            //creates and evaluates the inner query expression, and then applies relevant filtering based on JoinOperator
            var childQueryExpression = new QueryExpression()
            {
                EntityName = le.LinkToEntityName,
                Criteria = le.LinkCriteria,
                ColumnSet = new ColumnSet(le.LinkToAttributeName),
            };

            var childQueryElements = childQueryExpression.ToQueryable(context)
                .ToList();
                
                
            var childIds = childQueryElements.Select(e => e.GetAttributePrimaryKeyIdOrEntityReferenceId(le.LinkToAttributeName))
                .ToList();
         
            var getAttributeValueExpr = entity.ToAttributeValueExpression(le.LinkFromAttributeName);
            var containsAttributeExpr = entity.ToContainsAttributeExpression(le.LinkFromAttributeName);
            
            if (le.JoinOperator == JoinOperator.Any)
            {
                return childIds.ToAnyExpression(getAttributeValueExpr, containsAttributeExpr);
            }
            else if (le.JoinOperator == JoinOperator.NotAny)
            {
                return Expression.Not(childIds.ToAnyExpression(getAttributeValueExpr, containsAttributeExpr));
            }

            return Expression.Constant(true);
        }
    }
}
#endif