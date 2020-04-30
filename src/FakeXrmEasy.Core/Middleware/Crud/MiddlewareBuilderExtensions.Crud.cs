using System;
using System.Collections.Generic;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Middleware.Crud.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Middleware.Crud
{
    public static class MiddlewareBuilderCrudExtensions 
    {
        public class CrudMessageExecutors : Dictionary<Type, IFakeMessageExecutor>
        {

        }
        
        public static IMiddlewareBuilder AddCrud(this IMiddlewareBuilder builder) 
        {
            builder.Add(context => {
                var service = context.GetOrganizationService();

                //Get Crud Message Executors
                var crudMessageExecutors = new CrudMessageExecutors();
                crudMessageExecutors.Add(typeof(CreateRequest), new CreateRequestExecutor());
                

                context.SetProperty(crudMessageExecutors);
                AddFakeCreate(context, service);
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

        
    }
}