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
    public static class QueryExpressionExtensions
    {
        public static IQueryable<Entity> ToQueryable(this QueryExpression qe, IXrmFakedContext context)
        {
            if (qe == null) return null;

            //Start form the root entity and build a LINQ query to execute the query against the In-Memory context:
            context.EnsureEntityNameExistsInMetadata(qe.EntityName);


            IQueryable<Entity> query = null;

            query = context.CreateQuery(qe.EntityName);

            var linkedEntities = new Dictionary<string, int>();

#if  !FAKE_XRM_EASY
            ValidateAliases(qe, context as XrmFakedContext);
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
            var expTreeBody = TranslateQueryExpressionFiltersToExpression(context, qe, entity);
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

            //Project the attributes in the root column set  (must be applied after the where and order clauses, not before!!)
            query = query.Select(x => x.Clone(x.GetType(), context as XrmFakedContext).ProjectAttributes(qe, context as XrmFakedContext));

            return query;
        }

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


        private static string EnsureUniqueLinkedEntityAlias(IDictionary<string, int> linkedEntities, string entityName)
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
    }
}
