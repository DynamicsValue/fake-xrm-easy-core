using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Query;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// 
    /// </summary>
    public class RetrieveRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request.GetType().Equals(GetResponsibleRequestType());
        }

        /// <summary>
        /// Executes this RetrieveRequest executor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var context = (ctx as XrmFakedContext);
            var req = request as RetrieveRequest;

            if (req.Target == null)
            {
                throw new ArgumentNullException("Target", "RetrieveRequest without Target is invalid.");
            }

            var entityName = req.Target.LogicalName;
            var columnSet = req.ColumnSet;
            if (columnSet == null)
            {
                throw FakeOrganizationServiceFaultFactory.New("Required field 'ColumnSet' is missing");
            }

            var id = context.GetRecordUniqueId(req.Target);

            //Entity logical name exists, so , check if the requested entity exists
            if(!context.ContainsEntity(entityName, id))
            {
                // Entity not found in the context => FaultException //unchecked((int)0x80040217)
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, $"{entityName} With Id = {id:D} Does Not Exist");
            }
            //Return the subset of columns requested only
            var reflectedType = context.FindReflectedType(entityName);

                
            //Entity found => return only the subset of columns specified or all of them
            var resultEntity = context.GetEntityById_Internal(entityName, id).Clone(reflectedType, context);
            if (!columnSet.AllColumns)
            {
                resultEntity = resultEntity.ProjectAttributes(columnSet, context);
            }
            resultEntity.ApplyDateBehaviour(context);

            if (req.RelatedEntitiesQuery != null && req.RelatedEntitiesQuery.Count > 0)
            {
                AddRelatedEntities(context, req, resultEntity);
            }

            return new RetrieveResponse
            {
                Results = new ParameterCollection { { "Entity", resultEntity } }
            };
            

        }

        private static void AddRelatedEntities(XrmFakedContext context, RetrieveRequest req, Entity resultEntity)
        {
            foreach (var relatedEntitiesQuery in req.RelatedEntitiesQuery)
            {
                if (relatedEntitiesQuery.Value == null)
                {
                    throw new ArgumentNullException("relateEntitiesQuery.Value",
                        string.Format("RelatedEntitiesQuery for \"{0}\" does not contain a Query Expression.",
                            relatedEntitiesQuery.Key.SchemaName));
                }

                var fakeRelationship = context.GetRelationship(relatedEntitiesQuery.Key.SchemaName);
                if (fakeRelationship == null)
                {
                    throw new Exception(string.Format("Relationship \"{0}\" does not exist in the metadata cache.",
                        relatedEntitiesQuery.Key.SchemaName));
                }

                var relatedEntitiesQueryValue = (QueryExpression)relatedEntitiesQuery.Value;
                QueryExpression retrieveRelatedEntitiesQuery = relatedEntitiesQueryValue.Clone();

                if (fakeRelationship.RelationshipType == XrmFakedRelationship.FakeRelationshipType.OneToMany)
                {
                    AddRelatedEntitiesOneToMany(req, resultEntity, fakeRelationship, relatedEntitiesQueryValue, retrieveRelatedEntitiesQuery);
                }
                else
                {
                    AddRelatedEntitiesManyToMany(resultEntity, fakeRelationship, retrieveRelatedEntitiesQuery);
                }

                var retrieveRelatedEntitiesRequest = new RetrieveMultipleRequest
                {
                    Query = retrieveRelatedEntitiesQuery
                };

                //use of an executor directly; if to use service.RetrieveMultiple then the result will be
                //limited to the number of records per page (somewhere in future release).
                //ALL RECORDS are needed here.
                var executor = new RetrieveMultipleRequestExecutor();
                var retrieveRelatedEntitiesResponse = executor
                    .Execute(retrieveRelatedEntitiesRequest, context) as RetrieveMultipleResponse;

                if (retrieveRelatedEntitiesResponse.EntityCollection.Entities.Count == 0)
                    continue;

                resultEntity.RelatedEntities
                    .Add(relatedEntitiesQuery.Key, retrieveRelatedEntitiesResponse.EntityCollection);
            }
        }

        private static void AddRelatedEntitiesManyToMany(Entity resultEntity, XrmFakedRelationship fakeRelationship, QueryExpression retrieveRelatedEntitiesQuery)
        {
            var isFrom1 = fakeRelationship.Entity1LogicalName == retrieveRelatedEntitiesQuery.EntityName;
            var linkAttributeName = isFrom1 ? fakeRelationship.Entity1Attribute : fakeRelationship.Entity2Attribute;
            var conditionAttributeName = isFrom1 ? fakeRelationship.Entity2Attribute : fakeRelationship.Entity1Attribute;

            var linkEntity = new LinkEntity
            {
                Columns = new ColumnSet(false),
                LinkFromAttributeName = linkAttributeName,
                LinkFromEntityName = retrieveRelatedEntitiesQuery.EntityName,
                LinkToAttributeName = linkAttributeName,
                LinkToEntityName = fakeRelationship.IntersectEntity,
                LinkCriteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression(conditionAttributeName , ConditionOperator.Equal, resultEntity.Id)
                            }
                }
            };
            retrieveRelatedEntitiesQuery.LinkEntities.Add(linkEntity);
        }

        private static void AddRelatedEntitiesOneToMany(RetrieveRequest req, Entity resultEntity, XrmFakedRelationship fakeRelationship, QueryExpression relatedEntitiesQueryValue, QueryExpression retrieveRelatedEntitiesQuery)
        {
            var isFrom1to2 = relatedEntitiesQueryValue.EntityName == fakeRelationship.Entity1LogicalName
                                    || req.Target.LogicalName != fakeRelationship.Entity1LogicalName
                                    || string.IsNullOrWhiteSpace(relatedEntitiesQueryValue.EntityName);

            if (isFrom1to2)
            {
                var fromAttribute = fakeRelationship.Entity1Attribute;
                var toAttribute = fakeRelationship.Entity2Attribute;

                var linkEntity = new LinkEntity
                {
                    Columns = new ColumnSet(false),
                    LinkFromAttributeName = fromAttribute,
                    LinkFromEntityName = retrieveRelatedEntitiesQuery.EntityName,
                    LinkToAttributeName = toAttribute,
                    LinkToEntityName = resultEntity.LogicalName
                };

                if (retrieveRelatedEntitiesQuery.Criteria == null)
                {
                    retrieveRelatedEntitiesQuery.Criteria = new FilterExpression();
                }

                retrieveRelatedEntitiesQuery.Criteria
                    .AddFilter(LogicalOperator.And)
                    .AddCondition(linkEntity.LinkFromAttributeName, ConditionOperator.Equal, resultEntity.Id);
            }
            else
            {
                var link = retrieveRelatedEntitiesQuery.AddLink(fakeRelationship.Entity1LogicalName, fakeRelationship.Entity2Attribute, fakeRelationship.Entity1Attribute);
                link.LinkCriteria.AddCondition(resultEntity.LogicalName + "id", ConditionOperator.Equal, resultEntity.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveRequest);
        }
    }
}