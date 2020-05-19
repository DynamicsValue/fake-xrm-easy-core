using System;
using System.Collections.Generic;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Integrity;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Middleware.Crud.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Middleware.Crud
{
    public static class MiddlewareBuilderCrudExtensions 
    {
        private class CrudMessageExecutors : Dictionary<Type, IFakeMessageExecutor>
        {

        }
        
        public static IMiddlewareBuilder AddCrud(this IMiddlewareBuilder builder) 
        {
            builder.Add(context => {
                var service = context.GetOrganizationService();

                //Get Crud Message Executors
                var crudMessageExecutors = new CrudMessageExecutors();
                crudMessageExecutors.Add(typeof(CreateRequest), new CreateRequestExecutor());
                crudMessageExecutors.Add(typeof(RetrieveMultipleRequest), new RetrieveMultipleRequestExecutor());
                crudMessageExecutors.Add(typeof(RetrieveRequest), new RetrieveRequestExecutor());
                crudMessageExecutors.Add(typeof(UpdateRequest), new UpdateRequestExecutor());
                crudMessageExecutors.Add(typeof(DeleteRequest), new DeleteRequestExecutor());

                #if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
                crudMessageExecutors.Add(typeof(UpsertRequest), new UpsertRequestExecutor());
                #endif

                context.SetProperty(crudMessageExecutors);
                AddFakeCreate(context, service);
                AddFakeRetrieve(context, service);
                AddFakeRetrieveMultiple(context, service);
                AddFakeUpdate(context,service);
                AddFakeDelete(context,service);
            });

            return builder;
        }

        public static IMiddlewareBuilder AddCrud(this IMiddlewareBuilder builder, IIntegrityOptions integrityOptions) 
        {
            builder.AddCrud();

            //Add now integrity options
            builder.Add(context => {
                context.SetProperty(integrityOptions);
            });

            return builder;
        }

        public static IMiddlewareBuilder UseCrud(this IMiddlewareBuilder builder) 
        {

            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {

                return (IXrmFakedContext context, OrganizationRequest request) => {
                    
                    if(CanHandleRequest(context, request)) 
                    {
                        return ProcessRequest(context, request);
                    }
                    else 
                    {
                        return next.Invoke(context, request);
                    }
                };
            };
            
            builder.Use(middleware);
            return builder;
        }

        private static bool CanHandleRequest(IXrmFakedContext context, OrganizationRequest request) 
        {
            var crudMessageExecutors = context.GetProperty<CrudMessageExecutors>();
            return crudMessageExecutors.ContainsKey(request.GetType());
        }

        private static OrganizationResponse ProcessRequest(IXrmFakedContext context, OrganizationRequest request) 
        {
            var crudMessageExecutors = context.GetProperty<CrudMessageExecutors>();
            return crudMessageExecutors[request.GetType()].Execute(request, context);
        }

        private static void AddFakeCreate(IXrmFakedContext context, IOrganizationService service) 
        {
            A.CallTo(() => service.Create(A<Entity>._))
                .ReturnsLazily((Entity e) =>
                {
                    var request = new CreateRequest();
                    request.Target = e;
                    var response = service.Execute(request) as CreateResponse;
                    return response.id;
                });
        }

        private static void AddFakeUpdate(IXrmFakedContext context, IOrganizationService service) 
        {
            A.CallTo(() => service.Update(A<Entity>._))
                .Invokes((Entity e) =>
                {
                    var request = new UpdateRequest();
                    request.Target = e;
                    service.Execute(request);
                });
        }

        private static void AddFakeRetrieveMultiple(IXrmFakedContext context, IOrganizationService service)
        {
            //refactored from RetrieveMultipleExecutor
            A.CallTo(() => service.RetrieveMultiple(A<QueryBase>._))
                .ReturnsLazily((QueryBase req) => {
                    var request = new RetrieveMultipleRequest { Query = req };

                    var response = service.Execute(request) as RetrieveMultipleResponse;
                    return response.EntityCollection;
                });
        }

        private static void AddFakeRetrieve(IXrmFakedContext context, IOrganizationService service)
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
        }

        private static void AddFakeDelete(IXrmFakedContext context, IOrganizationService service)
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
        }

        
        
    }
}