using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Core.Exceptions.Query;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class QueryExpressionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="sAlias"></param>
        /// <returns></returns>
        internal static string GetEntityNameFromAlias(this QueryExpression qe, string sAlias)
        {
            if (sAlias == null)
                return qe.EntityName;

            var linkedEntity = qe.LinkEntities
                            .Where(le => le.EntityAlias != null && le.EntityAlias.Equals(sAlias))
                            .FirstOrDefault();

            if (linkedEntity != null)
            {
                return linkedEntity.LinkToEntityName;
            }

            //If the alias wasn't found, it means it  could be any of the EntityNames
            return sAlias;
        }

        /// <summary>
        /// Makes a deep clone of the Query Expression
        /// </summary>
        /// <param name="qe">Query Expression</param>
        /// <returns></returns>
        internal static QueryExpression Clone(this QueryExpression qe)
        {
            return qe.Copy();
        }

        /// <summary>
        /// Converts a QueryExpression into a Queryable
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static IQueryable<Entity> ToQueryable(this QueryExpression qe, IXrmFakedContext context)
        {
            if (qe == null) return null;

            if (qe.TopCount != null && !qe.IsPageInfoEmpty())
            {
                throw CantSetBothPageInfoAndTopCountException.New(qe.TopCount.Value);
            }

            //Start form the root entity and build a LINQ query to execute the query against the In-Memory context:
            context.EnsureEntityNameExistsInMetadata(qe.EntityName);

            IQueryable<Entity> query = null;

            query = context.CreateQuery(qe.EntityName);

            var linkedEntities = new Dictionary<string, int>();

            bool hasAggregates = false;
            
#if  !FAKE_XRM_EASY
            ValidateAliases(qe, context as XrmFakedContext);
#endif
#if FAKE_XRM_EASY_9
            var aggregateExpressions = GetAggregateAttributeExpressions(qe);
            hasAggregates = aggregateExpressions?.Count > 0;
            ValidateAggregates(qe, aggregateExpressions);
#endif

            // Add as many Joins as linked entities
            foreach (var le in qe.LinkEntities)
            {
                if (string.IsNullOrWhiteSpace(le.EntityAlias))
                {
                    le.EntityAlias = EnsureUniqueLinkedEntityAlias(linkedEntities, le.LinkToEntityName);
                }

                query = le.ToQueryable(context, query, qe.ColumnSet, linkedEntities);
            }

            // Compose the expression tree that represents the parameter to the predicate.
            ParameterExpression entity = Expression.Parameter(typeof(Entity));
            var expTreeBody = qe.TranslateQueryExpressionFiltersToExpression(context, entity);
            Expression<Func<Entity, bool>> lambda = Expression.Lambda<Func<Entity, bool>>(expTreeBody, entity);
            query = query.Where(lambda);

            //Sort results
            if (qe.Orders != null)
            {
                if (qe.Orders.Count > 0)
                {
                    IOrderedQueryable<Entity> orderedQuery = null;

                    var order = qe.Orders[0];
                    if (order.OrderType == OrderType.Ascending)
                        orderedQuery = query.OrderBy(e => e.Attributes.ContainsKey(order.AttributeName) ? e[order.AttributeName] : null, new XrmOrderByAttributeComparer());
                    else
                        orderedQuery = query.OrderByDescending(e => e.Attributes.ContainsKey(order.AttributeName) ? e[order.AttributeName] : null, new XrmOrderByAttributeComparer());

                    //Subsequent orders should use ThenBy and ThenByDescending
                    for (var i = 1; i < qe.Orders.Count; i++)
                    {
                        var thenOrder = qe.Orders[i];
                        if (thenOrder.OrderType == OrderType.Ascending)
                            orderedQuery = orderedQuery.ThenBy(e => e.Attributes.ContainsKey(thenOrder.AttributeName) ? e[thenOrder.AttributeName] : null, new XrmOrderByAttributeComparer());
                        else
                            orderedQuery = orderedQuery.ThenByDescending(e => e[thenOrder.AttributeName], new XrmOrderByAttributeComparer());
                    }

                    query = orderedQuery;
                }
            }
            
            #if FAKE_XRM_EASY_9
            if (hasAggregates)
            {
                return TranslateAggregateExpressions(query, qe, qe.ColumnSet.AttributeExpressions.ToList(), context);
            }
            #endif
            
            //Project the attributes in the root column set  (must be applied after the where and order clauses, not before!!)
            query = query.Select(x => x.Clone(x.GetType(), context as XrmFakedContext).ProjectAttributes(qe, context as XrmFakedContext));
            
            //Apply column aliases
            query = query.Select(e => e.ApplyColumnAliases(qe, context));
            
            return query;
        }

        #if FAKE_XRM_EASY_9
        private static List<XrmAttributeExpression> GetAggregateAttributeExpressions(QueryExpression qe)
        {
            return qe.ColumnSet?
                .AttributeExpressions
                .Where(ae => ae.AggregateType != XrmAggregateType.None)
                .ToList();
        }

        private static IQueryable<Entity> TranslateAggregateExpressions(IQueryable<Entity> sequence, QueryExpression qe,
            List<XrmAttributeExpression> attributeExpressions, IXrmFakedContext context)
        {
            var seq = sequence.ToList();
            
            var groupByAttributes = attributeExpressions.Where(att => att.HasGroupBy).ToList();
            if (groupByAttributes.Count > 0)
            {
                var groupedSequence = seq.GroupBy(e => e.ToGroupByKeySelector(groupByAttributes, context))
                    .Select(group =>
                    {
                        var aggregateRecord = TranslateAggregateExpressionsInSubSequence(group.ToList().AsQueryable(), qe, attributeExpressions, context);
                        group.Key.AddGroupKeyAttributes(aggregateRecord);
                        return aggregateRecord;
                    });
                                                    
                return groupedSequence.AsQueryable();
            }
            else
            {

                var aggregatedRecord = TranslateAggregateExpressionsInSubSequence(sequence, qe, attributeExpressions, context);
                return new List<Entity> { aggregatedRecord }.AsQueryable();
            }
        }
        
        private static Entity TranslateAggregateExpressionsInSubSequence(IQueryable<Entity> sequence, QueryExpression qe,
            List<XrmAttributeExpression> attributeExpressions, IXrmFakedContext context)
        {
            var seq = sequence.ToList();
            
            var aggregateRecord = context.NewEntityRecord(qe.EntityName);

            foreach (var attExpression in attributeExpressions)
            {
                attExpression.ToAggregatedAttributeValue(qe, seq, aggregateRecord, context);
            }
            
            return  aggregateRecord;
        }
        
        
        private static void ValidateAggregates(QueryExpression qe, List<XrmAttributeExpression> attributeExpressions)
        {
            if (qe.ColumnSet == null)
            {
                return;
            }
            
            var hasAggregateAttributeExpressions =
                attributeExpressions.Count > 0;

            if (hasAggregateAttributeExpressions && (qe.ColumnSet.AllColumns || qe.ColumnSet.Columns.Count > 0))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidOperation,$"Attribute can not be specified if an aggregate operation is requested.");
            }
        }
#endif
        #if !FAKE_XRM_EASY 
        private static void ValidateAliases(QueryExpression qe, XrmFakedContext context)
        {
            if (qe.Criteria != null)
                ValidateAliases(qe, context, qe.Criteria);
            if (qe.LinkEntities != null)
                foreach (var link in qe.LinkEntities)
                {
                    ValidateAliases(qe, context, link);
                }
        }

        private static void ValidateAliases(QueryExpression qe, XrmFakedContext context, LinkEntity link)
        {
            if (link.LinkCriteria != null)
                ValidateAliases(qe, context, link.LinkCriteria);
            if (link.LinkEntities != null)
                foreach (var innerLink in link.LinkEntities)
                {
                    ValidateAliases(qe, context, innerLink);
                }
        }

        private static void ValidateAliases(QueryExpression qe, XrmFakedContext context, FilterExpression filter)
        {
            if (filter.Filters != null)
                foreach (var innerFilter in filter.Filters)
                {
                    ValidateAliases(qe, context, innerFilter);
                }

            if (filter.Conditions != null)
                foreach (var condition in filter.Conditions)
                {
                    if (!string.IsNullOrEmpty(condition.EntityName))
                    {
                        ValidateAliases(qe, context, condition);
                    }
                }
        }

        private static void ValidateAliases(QueryExpression qe, XrmFakedContext context, ConditionExpression condition)
        {
            var matches = qe.LinkEntities != null ? MatchByAlias(qe, context, condition, qe.LinkEntities) : 0;
            if (matches > 1)
            {
                throw FakeOrganizationServiceFaultFactory.New($"Table {condition.EntityName} is not unique amongst all top-level table and join aliases");
            }
            else if (matches == 0)
            {
                if (qe.LinkEntities != null) matches = MatchByEntity(qe, context, condition, qe.LinkEntities);
                if (matches > 1)
                {
                    throw FakeOrganizationServiceFaultFactory.New($"There's more than one LinkEntity expressions with name={condition.EntityName}");
                }
                else if (matches == 0)
                {
                    if (condition.EntityName == qe.EntityName) return;
                    throw FakeOrganizationServiceFaultFactory.New($"LinkEntity with name or alias {condition.EntityName} is not found");
                }
                condition.EntityName += "1";
            }
        }

        private static int MatchByEntity(QueryExpression qe, XrmFakedContext context, ConditionExpression condition, DataCollection<LinkEntity> linkEntities)
        {
            var matches = 0;
            foreach (var link in linkEntities)
            {
                if (string.IsNullOrEmpty(link.EntityAlias) && condition.EntityName == link.LinkToEntityName)
                {
                    matches += 1;
                }
                if (link.LinkEntities != null) matches += MatchByEntity(qe, context, condition, link.LinkEntities);
            }
            return matches;
        }

        private static int MatchByAlias(QueryExpression qe, XrmFakedContext context, ConditionExpression condition, DataCollection<LinkEntity> linkEntities)
        {
            var matches = 0;
            foreach (var link in linkEntities)
            {
                if (link.EntityAlias == condition.EntityName)
                {
                    matches += 1;
                }
                if (link.LinkEntities != null) matches += MatchByAlias(qe, context, condition, link.LinkEntities);
            }
            return matches;
        }
#endif


        internal static string EnsureUniqueLinkedEntityAlias(IDictionary<string, int> linkedEntities, string entityName)
        {
            if (linkedEntities.ContainsKey(entityName))
            {
                linkedEntities[entityName]++;
            }
            else
            {
                linkedEntities[entityName] = 1;
            }

            return $"{entityName}{linkedEntities[entityName]}";
        }

        internal static Expression TranslateQueryExpressionFiltersToExpression(this QueryExpression qe, IXrmFakedContext context, ParameterExpression entity)
        {
            var linkedEntitiesQueryExpressions = new List<Expression>();
            foreach (var le in qe.LinkEntities)
            {
                var listOfExpressions = le.TranslateLinkedEntityFilterExpressionToExpression(qe, context, entity);
                linkedEntitiesQueryExpressions.AddRange(listOfExpressions);
            }

            Expression combinedExpressions = Expression.Constant(true);
            foreach (var e in linkedEntitiesQueryExpressions)
            {
                combinedExpressions = Expression.And(e, combinedExpressions);
            }

            if (qe.Criteria != null)
            {
                var criteriaExpression = qe.Criteria.TranslateFilterExpressionToExpression(qe, context, qe.EntityName, entity, false);
                combinedExpressions = Expression.And(criteriaExpression, combinedExpressions);
            }
            
            return combinedExpressions;
        }


        internal static bool IsPageInfoEmpty(this QueryExpression qe)
        {
            if (qe.PageInfo == null)
                return true;

            return qe.PageInfo.PageNumber == 0 &&
                    qe.PageInfo.Count == 0 &&
                    !qe.PageInfo.ReturnTotalRecordCount &&
                    string.IsNullOrEmpty(qe.PageInfo.PagingCookie);
        }
        
    }
}
