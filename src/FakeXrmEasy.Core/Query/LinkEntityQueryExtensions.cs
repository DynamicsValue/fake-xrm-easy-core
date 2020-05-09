using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static class LinkEntityQueryExtensions
    {
        internal static IQueryable<Entity> ToQueryable(this LinkEntity le, IXrmFakedContext context, IQueryable<Entity> query, ColumnSet previousColumnSet, Dictionary<string, int> linkedEntities, string linkFromAlias = "", string linkFromEntity = "") 
        {
            if (!string.IsNullOrEmpty(le.EntityAlias))
            {
                if (!Regex.IsMatch(le.EntityAlias, "^[A-Za-z_](\\w|\\.)*$", RegexOptions.ECMAScript))
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderInvalid_Alias, $"Invalid character specified for alias: {le.EntityAlias}. Only characters within the ranges [A-Z], [a-z] or [0-9] or _ are allowed.  The first character may only be in the ranges [A-Z], [a-z] or _.");
                }
            }

            var leAlias = string.IsNullOrWhiteSpace(le.EntityAlias) ? le.LinkToEntityName : le.EntityAlias;
            context.EnsureEntityNameExistsInMetadata(le.LinkFromEntityName != linkFromAlias ? le.LinkFromEntityName : linkFromEntity);
            context.EnsureEntityNameExistsInMetadata(le.LinkToEntityName);

            if (!context.AttributeExistsInMetadata(le.LinkToEntityName, le.LinkToAttributeName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute, string.Format("The attribute {0} does not exist on this entity.", le.LinkToAttributeName));
            }

            IQueryable<Entity> inner = null;
            if (le.JoinOperator == JoinOperator.LeftOuter)
            {
                //inner = context.CreateQuery<Entity>(le.LinkToEntityName);


                //filters are applied in the inner query and then ignored during filter evaluation
                var outerQueryExpression = new QueryExpression()
                {
                    EntityName = le.LinkToEntityName,
                    Criteria = le.LinkCriteria,
                    ColumnSet = new ColumnSet(true)
                };

                var outerQuery = outerQueryExpression.ToQueryable(context);
                inner = outerQuery;

            }
            else
            {
                //Filters are applied after joins
                inner = context.CreateQuery(le.LinkToEntityName);
            }

            if (string.IsNullOrWhiteSpace(linkFromAlias))
            {
                linkFromAlias = le.LinkFromAttributeName;
            }
            else
            {
                linkFromAlias += "." + le.LinkFromAttributeName;
            }

            switch (le.JoinOperator)
            {
                case JoinOperator.Inner:
                case JoinOperator.Natural:
                    query = query.Join(inner,
                                    outerKey => outerKey.KeySelector(linkFromAlias, context),
                                    innerKey => innerKey.KeySelector(le.LinkToAttributeName, context),
                                    (outerEl, innerEl) => outerEl.Clone(outerEl.GetType(), context).JoinAttributes(innerEl, new ColumnSet(true), leAlias, context));

                    break;
                case JoinOperator.LeftOuter:
                    query = query.GroupJoin(inner,
                                    outerKey => outerKey.KeySelector(linkFromAlias, context),
                                    innerKey => innerKey.KeySelector(le.LinkToAttributeName, context),
                                    (outerEl, innerElemsCol) => new { outerEl, innerElemsCol })
                                                .SelectMany(x => x.innerElemsCol.DefaultIfEmpty()
                                                            , (x, y) => x.outerEl
                                                                            .JoinAttributes(y, new ColumnSet(true), leAlias, context));


                    break;
                default: //This shouldn't be reached as there are only 3 types of Join...
                    throw new PullRequestException(string.Format("The join operator {0} is currently not supported. Feel free to implement and send a PR.", le.JoinOperator));

            }

            // Process nested linked entities recursively
            foreach (var nestedLinkedEntity in le.LinkEntities)
            {
                if (string.IsNullOrWhiteSpace(le.EntityAlias))
                {
                    le.EntityAlias = le.LinkToEntityName;
                }

                if (string.IsNullOrWhiteSpace(nestedLinkedEntity.EntityAlias))
                {
                    nestedLinkedEntity.EntityAlias = QueryExpressionExtensions.EnsureUniqueLinkedEntityAlias(linkedEntities, nestedLinkedEntity.LinkToEntityName);
                }

                query = nestedLinkedEntity.ToQueryable(context, query, le.Columns, linkedEntities, le.EntityAlias, le.LinkToEntityName);
            }

            return query;
        }

        internal static List<Expression> TranslateLinkedEntityFilterExpressionToExpression(this LinkEntity le, QueryExpression qe, IXrmFakedContext context, ParameterExpression entity)
        {
            //In CRM 2011, condition expressions are at the LinkEntity level without an entity name
            //From CRM 2013, condition expressions were moved to outside the LinkEntity object at the QueryExpression level,
            //with an EntityName alias attribute

            //If we reach this point, it means we are translating filters at the Link Entity level (2011),
            //Therefore we need to prepend the alias attribute because the code to generate attributes for Joins (JoinAttribute extension) is common across versions
            var linkedEntitiesQueryExpressions = new List<Expression>();

            if (le.LinkCriteria != null)
            {
                var earlyBoundType = context.FindReflectedType(le.LinkToEntityName);

                var fakedContext = context as XrmFakedContext;
                var attributeMetadata = fakedContext.AttributeMetadataNames.ContainsKey(le.LinkToEntityName) ? fakedContext.AttributeMetadataNames[le.LinkToEntityName] : null;

                foreach (var ce in le.LinkCriteria.Conditions)
                {
                    if (earlyBoundType != null)
                    {
                        var attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(ce.AttributeName);
                        if (attributeInfo == null && ce.AttributeName.EndsWith("name"))
                        {
                            // Special case for referencing the name of a EntityReference
                            var sAttributeName = ce.AttributeName.Substring(0, ce.AttributeName.Length - 4);
                            attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(sAttributeName);

                            if (attributeInfo.PropertyType == typeof(EntityReference))
                            {
                                // Don't mess up if other attributes follow this naming pattern
                                ce.AttributeName = sAttributeName;
                            }
                        }
                    }
                    else if (attributeMetadata != null && !attributeMetadata.ContainsKey(ce.AttributeName) && ce.AttributeName.EndsWith("name"))
                    {
                        // Special case for referencing the name of a EntityReference
                        var sAttributeName = ce.AttributeName.Substring(0, ce.AttributeName.Length - 4);
                        if (attributeMetadata.ContainsKey(sAttributeName))
                        {
                            ce.AttributeName = sAttributeName;
                        }
                    }

                    var entityAlias = !string.IsNullOrEmpty(le.EntityAlias) ? le.EntityAlias : le.LinkToEntityName;
                    ce.AttributeName = entityAlias + "." + ce.AttributeName;
                }

                foreach (var fe in le.LinkCriteria.Filters)
                {
                    foreach (var ce in fe.Conditions)
                    {
                        var entityAlias = !string.IsNullOrEmpty(le.EntityAlias) ? le.EntityAlias : le.LinkToEntityName;
                        ce.AttributeName = entityAlias + "." + ce.AttributeName;
                    }
                }
            }

            //Translate this specific Link Criteria
            linkedEntitiesQueryExpressions.Add(le.LinkCriteria.TranslateFilterExpressionToExpression(qe, context, le.LinkToEntityName, entity, le.JoinOperator == JoinOperator.LeftOuter));

            //Processed nested linked entities
            foreach (var nestedLinkedEntity in le.LinkEntities)
            {
                var listOfExpressions = nestedLinkedEntity.TranslateLinkedEntityFilterExpressionToExpression(qe, context, entity);
                linkedEntitiesQueryExpressions.AddRange(listOfExpressions);
            }

            return linkedEntitiesQueryExpressions;
        }

    }
}
