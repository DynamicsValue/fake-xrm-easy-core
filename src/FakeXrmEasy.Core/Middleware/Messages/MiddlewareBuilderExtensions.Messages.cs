using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Middleware.Messages
{
    public static class MiddlewareBuilderMessagesExtensions 
    {
        public static IMiddlewareBuilder AddFakeMessageExecutors(this IMiddlewareBuilder builder) 
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

                AddFakeAssociate(context, service);
                AddFakeDisassociate(context, service);
            });

            return builder;
        }

        public static IMiddlewareBuilder AddFakeMessageExecutor(this IMiddlewareBuilder builder, IFakeMessageExecutor executor) 
        {
            builder.Add(context => {
                
                var messageExecutors = context.GetProperty<MessageExecutors>();
                if (!messageExecutors.ContainsKey(executor.GetResponsibleRequestType()))
                    messageExecutors.Add(executor.GetResponsibleRequestType(), executor);
                else
                    messageExecutors[executor.GetResponsibleRequestType()] = executor;
            });

            return builder;
        }

        public static IMiddlewareBuilder AddFakeMessageExecutor<T>(this IMiddlewareBuilder builder, IFakeMessageExecutor executor) where T: OrganizationRequest
        {
            builder.Add(context => {
                
                var messageExecutors = context.GetProperty<MessageExecutors>();
                if (!messageExecutors.ContainsKey(typeof(T)))
                    messageExecutors.Add(typeof(T), executor);
                else
                    messageExecutors[typeof(T)] = executor;
            });

            return builder;
        }

        [Obsolete("Please use AddFakeMessageExecutor method that doesn't use generics and instead decides the OrganizationRequest based on the GetResponsibleRequestType")]
        public static IMiddlewareBuilder RemoveFakeMessageExecutor<T>(this IMiddlewareBuilder builder) where T: OrganizationRequest
        {
            builder.Add(context => {
                var messageExecutors = context.GetProperty<MessageExecutors>();
                messageExecutors.Remove(typeof(T));
            });

            return builder;
        }

        public static IMiddlewareBuilder AddExecutionMock<T>(this IMiddlewareBuilder builder, OrganizationRequestExecution mock) where T : OrganizationRequest
        {
            builder.Add(context => {
                if(!context.HasProperty<ExecutionMocks>())
                    context.SetProperty<ExecutionMocks>(new ExecutionMocks());

                var executionMocks = context.GetProperty<ExecutionMocks>();

                if (!executionMocks.ContainsKey(typeof(T)))
                    executionMocks.Add(typeof(T), mock);
                else
                    executionMocks[typeof(T)] = mock;
            });
           
           return builder;
        }

        public static IMiddlewareBuilder RemoveExecutionMock<T>(this IMiddlewareBuilder builder) where T : OrganizationRequest
        {
            builder.Add(context => {
                var executionMocks = context.GetProperty<ExecutionMocks>();
                if (executionMocks.ContainsKey(typeof(T))) 
                {
                    executionMocks.Remove(typeof(T));
                }
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
            if(context.HasProperty<ExecutionMocks>()) 
            {
                var executionMocks = context.GetProperty<ExecutionMocks>();
                if(executionMocks.ContainsKey(request.GetType()))
                {
                    return true;
                }
            }
            
            if(context.HasProperty<MessageExecutors>())
            {
                var messageExecutors = context.GetProperty<MessageExecutors>();
                return messageExecutors.ContainsKey(request.GetType());
            }
            
            return false;
        }

        private static OrganizationResponse ProcessRequest(IXrmFakedContext context, OrganizationRequest request) 
        {
            if(context.HasProperty<ExecutionMocks>()) 
            {
                var executionMocks = context.GetProperty<ExecutionMocks>();
                if(executionMocks.ContainsKey(request.GetType()))
                {
                    return executionMocks[request.GetType()].Invoke(request);
                }
            }

            var messageExecutors = context.GetProperty<MessageExecutors>();
            return messageExecutors[request.GetType()].Execute(request, context);            
        }
        

        private static void AddFakeAssociate(IXrmFakedContext context, IOrganizationService service)
        {
            A.CallTo(() => service.Associate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                {
                    var messageExecutors = context.GetProperty<MessageExecutors>();

                    if (messageExecutors.ContainsKey(typeof(AssociateRequest)))
                    {
                        var request = new AssociateRequest()
                        {
                            Target = new EntityReference() { Id = entityId, LogicalName = entityName },
                            Relationship = relationship,
                            RelatedEntities = entityCollection
                        };
                        service.Execute(request);
                    }
                    else
                        throw PullRequestException.NotImplementedOrganizationRequest(typeof(AssociateRequest));
                });
        }

        private static void AddFakeDisassociate(IXrmFakedContext context, IOrganizationService service)
        {
            A.CallTo(() => service.Disassociate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                {
                    var messageExecutors = context.GetProperty<MessageExecutors>();

                    if (messageExecutors.ContainsKey(typeof(DisassociateRequest)))
                    {
                        var request = new DisassociateRequest()
                        {
                            Target = new EntityReference() { Id = entityId, LogicalName = entityName },
                            Relationship = relationship,
                            RelatedEntities = entityCollection
                        };
                        service.Execute(request);
                    }
                    else
                        throw PullRequestException.NotImplementedOrganizationRequest(typeof(DisassociateRequest));
                });
        }
    }
}