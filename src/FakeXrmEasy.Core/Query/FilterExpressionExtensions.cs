using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class FilterExpressionExtensions
    {
        internal static Expression GenerateMultipleExpressionsWithOperator(LogicalOperator op,
            List<Expression> expressionsList)
        {
            if (expressionsList.Count == 0)
            {
                return Expression.Constant(true); //Satisfy filter if there are no conditions nor filters
            }
            if (expressionsList.Count == 1)
            {
                return expressionsList[0];
            }
            if (expressionsList.Count == 2)
            {
                //Satisfy both
                if (op == LogicalOperator.And)
                {
                    return Expression.And(expressionsList[0], expressionsList[1]);
                }
                return Expression.Or(expressionsList[0], expressionsList[1]);
                
            }

            //More than 2 expressions
            //Process recursively
            var firstExpression = expressionsList[0];
            expressionsList.RemoveAt(0);

            var generatedExpression = GenerateMultipleExpressionsWithOperator(op, expressionsList);
            if (op == LogicalOperator.And)
            {
                return Expression.And(firstExpression, generatedExpression);
            }
            return Expression.Or(firstExpression, generatedExpression);
        }
        internal static Expression TranslateFilterExpressionToExpression(this FilterExpression fe, QueryExpression qe, IXrmFakedContext context, string sEntityName, ParameterExpression entity, bool bIsOuter)
        {
            if (fe == null) return Expression.Constant(true);

            var expressionsList = new List<Expression>();
            
            if (fe.Conditions != null && fe.Conditions.Count > 0)
            {
                var conditions = fe.Conditions.ToList();
                var conditionsExpression = conditions.TranslateMultipleConditionExpressions(qe, context, sEntityName, fe.FilterOperator, entity, bIsOuter);
                expressionsList.Add(conditionsExpression);
            }

            //Process nested filters recursively
            if (fe.Filters != null && fe.Filters.Count > 0)
            {
                var filters = fe.Filters.ToList();
                var nestedFiltersExpression = filters.TranslateMultipleFilterExpressions(qe, context, sEntityName, fe.FilterOperator, entity, bIsOuter);
                expressionsList.Add(nestedFiltersExpression);
            }

            //Any / NotAny / All / NotAll operators
            if (fe.AnyAllFilterLinkEntity != null)
            {
                var le = fe.AnyAllFilterLinkEntity;
                var anyAllFilterExpression = le.TranslateAnyAllLinkedEntityToExpression(context, entity);
                expressionsList.Add(anyAllFilterExpression);
            }
            
            return GenerateMultipleExpressionsWithOperator(fe.FilterOperator, expressionsList);
        }

        internal static BinaryExpression TranslateMultipleFilterExpressions(this List<FilterExpression> filters, QueryExpression qe, IXrmFakedContext context, string sEntityName, LogicalOperator op, ParameterExpression entity, bool bIsOuter)
        {
            BinaryExpression binaryExpression = null;
            if (op == LogicalOperator.And)
                binaryExpression = Expression.And(Expression.Constant(true), Expression.Constant(true));
            else
                binaryExpression = Expression.Or(Expression.Constant(false), Expression.Constant(false));

            foreach (var f in filters)
            {
                var thisFilterLambda = f.TranslateFilterExpressionToExpression(qe, context, sEntityName, entity, bIsOuter);

                //Build a binary expression  
                if (op == LogicalOperator.And)
                {
                    binaryExpression = Expression.And(binaryExpression, thisFilterLambda);
                }
                else
                    binaryExpression = Expression.Or(binaryExpression, thisFilterLambda);
            }

            return binaryExpression;
        }
    }
}
