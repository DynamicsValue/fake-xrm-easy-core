using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static class FilterExpressionExtensions
    {
        internal static Expression TranslateFilterExpressionToExpression(this FilterExpression fe, QueryExpression qe, IXrmFakedContext context, string sEntityName, ParameterExpression entity, bool bIsOuter)
        {
            if (fe == null) return Expression.Constant(true);

            BinaryExpression conditionsLambda = null;
            BinaryExpression filtersLambda = null;
            if (fe.Conditions != null && fe.Conditions.Count > 0)
            {
                var conditions = fe.Conditions.ToList();
                conditionsLambda = conditions.TranslateMultipleConditionExpressions(qe, context, sEntityName, fe.FilterOperator, entity, bIsOuter);
            }

            //Process nested filters recursively
            if (fe.Filters != null && fe.Filters.Count > 0)
            {
                var filters = fe.Filters.ToList();
                filtersLambda = filters.TranslateMultipleFilterExpressions(qe, context, sEntityName, fe.FilterOperator, entity, bIsOuter);
            }

            if (conditionsLambda != null && filtersLambda != null)
            {
                //Satisfy both
                if (fe.FilterOperator == LogicalOperator.And)
                {
                    return Expression.And(conditionsLambda, filtersLambda);
                }
                else
                {
                    return Expression.Or(conditionsLambda, filtersLambda);
                }
            }
            else if (conditionsLambda != null)
                return conditionsLambda;
            else if (filtersLambda != null)
                return filtersLambda;

            return Expression.Constant(true); //Satisfy filter if there are no conditions nor filters
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
