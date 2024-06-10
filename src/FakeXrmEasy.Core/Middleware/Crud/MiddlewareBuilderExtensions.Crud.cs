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
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Extensions.OrganizationRequests;

namespace FakeXrmEasy.Middleware.Crud
{
    /// <summary>
    /// 
    /// </summary>
    public static class MiddlewareBuilderCrudExtensions 
    {
        private class CrudMessageExecutors : Dictionary<Type, IFakeMessageExecutor>
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
                crudMessageExecutors.Add(typeof(AssociateRequest), new AssociateRequestExecutor());
                crudMessageExecutors.Add(typeof(DisassociateRequest), new DisassociateRequestExecutor());

                #if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
                crudMessageExecutors.Add(typeof(UpsertRequest), new UpsertRequestExecutor());
                #endif

                #if FAKE_XRM_EASY_9
                crudMessageExecutors.Add(typeof(CreateMultipleRequest), new CreateMultipleRequestExecutor());
                crudMessageExecutors.Add(typeof(UpdateMultipleRequest), new UpdateMultipleRequestExecutor());
                crudMessageExecutors.Add(typeof(UpsertMultipleRequest), new UpsertMultipleRequestExecutor());
                #endif
                
                context.SetProperty(crudMessageExecutors);
                service.AddFakeCreate()
                    .AddFakeRetrieve()
                    .AddFakeRetrieveMultiple()
                    .AddFakeUpdate()
                    .AddFakeDelete()
                    .AddFakeAssociate()
                    .AddFakeDisassociate();
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="integrityOptions"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddCrud(this IMiddlewareBuilder builder, IIntegrityOptions integrityOptions) 
        {
            builder.AddCrud();

            //Add now integrity options
            builder.Add(context => {
                context.SetProperty(integrityOptions);
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder UseCrud(this IMiddlewareBuilder builder) 
        {
            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {

                return (IXrmFakedContext context, OrganizationRequest request) => {

                    if (request.IsCrudRequest())
                    {
                        request = request.ToStronglyTypedCrudRequest();
                    }

                    if (CanHandleRequest(context, request)) 
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
            var fakeMessageExecutor = crudMessageExecutors[request.GetType()] as IBaseFakeMessageExecutor;
            return fakeMessageExecutor.Execute(request, context);
        }

    }
}