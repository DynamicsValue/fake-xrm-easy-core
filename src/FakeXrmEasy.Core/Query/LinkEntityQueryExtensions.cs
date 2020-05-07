using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FakeXrmEasy.Abstractions;
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

                var outerQuery = TranslateQueryExpressionToLinq(context, outerQueryExpression);
                inner = outerQuery;

            }
            else
            {
                //Filters are applied after joins
                inner = context.CreateQuery<Entity>(le.LinkToEntityName);
            }

            //if (!le.Columns.AllColumns && le.Columns.Columns.Count == 0)
            //{
            //    le.Columns.AllColumns = true;   //Add all columns in the joined entity, otherwise we can't filter by related attributes, then the Select will actually choose which ones we need
            //}

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
                    nestedLinkedEntity.EntityAlias = EnsureUniqueLinkedEntityAlias(linkedEntities, nestedLinkedEntity.LinkToEntityName);
                }

                query = TranslateLinkedEntityToLinq(context, nestedLinkedEntity, query, le.Columns, linkedEntities, le.EntityAlias, le.LinkToEntityName);
            }

            return query;
        }
    }
}
