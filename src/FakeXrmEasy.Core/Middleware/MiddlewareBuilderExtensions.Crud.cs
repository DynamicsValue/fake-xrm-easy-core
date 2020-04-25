using System;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Middleware
{
    public static class MiddlewareBuilderCrudExtensions 
    {
        public static IXrmFakedContext AddCrud(this IXrmFakedContext context) 
        {
            var service = context.GetOrganizationService();
            AddFakeCreate(context, service);
            return context;
        }

        public static IMiddlewareBuilder UseCrud(this IMiddlewareBuilder builder) 
        {

            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {

                return (IXrmFakedContext context, OrganizationRequest request) => {
                    return new OrganizationResponse();
                };
            };
            
            builder.Use(middleware);
            return builder;
        }

        private static bool CanHandleRequest(OrganizationRequest request) 
        {
            return request is CreateRequest 
                || request is UpdateRequest
                || request is DeleteRequest
                || request is RetrieveRequest;
        }
        private static void AddFakeCreate(IXrmFakedContext context, IOrganizationService service) 
        {
            A.CallTo(() => service.Create(A<Entity>._))
                .ReturnsLazily((Entity e) =>
                {
                    return context.CreateEntity(e);
                });
        }

        
    }
}