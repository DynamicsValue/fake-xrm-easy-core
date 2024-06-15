using System;
using FakeItEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Middleware.Crud
{
    internal static class IOrganizationServiceMiddlewareExtensions
    {
        public static IOrganizationService AddFakeCreate(this IOrganizationService service) 
        {
            A.CallTo(() => service.Create(A<Entity>._))
                .ReturnsLazily((Entity e) =>
                {
                    var request = new CreateRequest();
                    request.Target = e;
                    var response = service.Execute(request) as CreateResponse;
                    return response.id;
                });

            return service;
        }
        
        public static IOrganizationService AddFakeUpdate(this IOrganizationService service) 
        {
            A.CallTo(() => service.Update(A<Entity>._))
                .Invokes((Entity e) =>
                {
                    var request = new UpdateRequest();
                    request.Target = e;
                    service.Execute(request);
                });
            
            return service;
        }
        
        public static IOrganizationService AddFakeRetrieveMultiple(this IOrganizationService service)
        {
            //refactored from RetrieveMultipleExecutor
            A.CallTo(() => service.RetrieveMultiple(A<QueryBase>._))
                .ReturnsLazily((QueryBase req) => {
                    var request = new RetrieveMultipleRequest { Query = req };

                    var response = service.Execute(request) as RetrieveMultipleResponse;
                    return response.EntityCollection;
                });

            return service;
        }
        
        public static IOrganizationService AddFakeRetrieve(this IOrganizationService service)
        {
            A.CallTo(() => service.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet) =>
                {
                    var request = new RetrieveRequest()
                    {
                        Target = new EntityReference() { LogicalName = entityName, Id = id },
                        ColumnSet = columnSet
                    };

                    var response = service.Execute(request) as RetrieveResponse;
                    return response.Entity;
                });
            
            return service;
        }
        
        public static IOrganizationService AddFakeDelete(this IOrganizationService service)
        {
            A.CallTo(() => service.Delete(A<string>._, A<Guid>._))
                .Invokes((string entityName, Guid id) =>
                {
                    if (string.IsNullOrWhiteSpace(entityName))
                    {
                        throw new InvalidOperationException("The entity logical name must not be null or empty.");
                    }

                    if (id == Guid.Empty)
                    {
                        throw new InvalidOperationException("The id must not be empty.");
                    }

                    var entityReference = new EntityReference(entityName, id);

                    var request = new DeleteRequest() { Target = entityReference };
                    service.Execute(request);
                });
            
            return service;
        }
        
        public static IOrganizationService AddFakeAssociate(this IOrganizationService service)
        {
            A.CallTo(() => service.Associate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                {
                    var request = new AssociateRequest()
                    {
                        Target = new EntityReference() { Id = entityId, LogicalName = entityName },
                        Relationship = relationship,
                        RelatedEntities = entityCollection
                    };
                    service.Execute(request);
                });

            return service;
        }
        
        public static IOrganizationService AddFakeDisassociate(this IOrganizationService service)
        {
            A.CallTo(() => service.Disassociate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                {
                    var request = new DisassociateRequest()
                    {
                        Target = new EntityReference() { Id = entityId, LogicalName = entityName },
                        Relationship = relationship,
                        RelatedEntities = entityCollection
                    };
                    service.Execute(request);
                });

            return service;
        }
    }
}