using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Middleware.Messages
{
    public static class MiddlewareBuilderMessagesExtensions 
    {
        private class MessageExecutors : Dictionary<Type, IFakeMessageExecutor>
        {
            public MessageExecutors(Dictionary<Type, IFakeMessageExecutor> other): base(other)
            {
                
            }
        }
        
        public static IMiddlewareBuilder AddMessages(this IMiddlewareBuilder builder) 
        {
            builder.Add(context => {
                var service = context.GetOrganizationService();
               
                var fakeMessageExecutorsDictionary = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IFakeMessageExecutor)))
                    .Select(t => Activator.CreateInstance(t) as IFakeMessageExecutor)
                    .ToDictionary(t => t.GetResponsibleRequestType(), t => t);
                    
                var messageExecutors = new MessageExecutors(fakeMessageExecutorsDictionary);

                context.SetProperty(messageExecutors);
            });

            return builder;
        }

        public static IMiddlewareBuilder UseMessages(this IMiddlewareBuilder builder) 
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
            var messageExecutors = context.GetProperty<MessageExecutors>();
            return messageExecutors.ContainsKey(request.GetType());
        }

        private static OrganizationResponse ProcessRequest(IXrmFakedContext context, OrganizationRequest request) 
        {
            var messageExecutors = context.GetProperty<MessageExecutors>();
            return messageExecutors[request.GetType()].Execute(request, context);
        }
        
    }
}