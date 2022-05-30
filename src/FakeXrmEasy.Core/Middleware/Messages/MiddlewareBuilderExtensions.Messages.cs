using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Middleware.Messages
{
    /// <summary>
    /// Extension methods to configure fake messages execution in the middleware
    /// </summary>
    public static class MiddlewareBuilderMessagesExtensions 
    {
        /// <summary>
        /// This methods discovers all IFakeMessageExecutor implementations in the current assembly and adds them to the context builder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddFakeMessageExecutors(this IMiddlewareBuilder builder, Assembly assembly = null) 
        {
            builder.Add(context => {
                if (assembly == null) assembly = Assembly.GetExecutingAssembly();
                var service = context.GetOrganizationService();
               
                var fakeMessageExecutorsDictionary = 
                    assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IFakeMessageExecutor)))
                    .Select(t => Activator.CreateInstance(t) as IFakeMessageExecutor)
                    .Where(t => t.GetResponsibleRequestType() != typeof(OrganizationRequest)) //Exclude generic messages
                    .ToDictionary(t => t.GetResponsibleRequestType(), t => t);
                    
                var messageExecutors = new MessageExecutors(fakeMessageExecutorsDictionary);

                if (!context.HasProperty<MessageExecutors>())
                    context.SetProperty(messageExecutors);
                else
                {
                    foreach(var messageExecutorKey in messageExecutors.Keys)
                    {
                        builder.AddFakeMessageExecutor(messageExecutors[messageExecutorKey]);
                    }
                }               
            });

            return builder;
        }

        /// <summary>
        /// Discovers all generic fake message executors in the current executing assembly and adds them to the context
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assembly">The Assembly to search generic fake message executors in, or the executing assembly by default</param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddGenericFakeMessageExecutors(this IMiddlewareBuilder builder, Assembly assembly = null) 
        {
            builder.Add(context => {
                if (assembly == null) assembly = Assembly.GetExecutingAssembly();
                var fakeMessageExecutorsDictionary = 
                    assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IGenericFakeMessageExecutor)))
                    .Select(t => Activator.CreateInstance(t) as IGenericFakeMessageExecutor)
                    .ToDictionary(t => t.GetRequestName(), t => t as IFakeMessageExecutor);
                    
                
                var genericMessageExecutors = new GenericMessageExecutors(fakeMessageExecutorsDictionary);

                if (!context.HasProperty<GenericMessageExecutors>())
                    context.SetProperty(genericMessageExecutors);
                else
                {
                    foreach (var genericMessageExecutorMessage in genericMessageExecutors.Keys)
                    {
                        builder.AddGenericFakeMessageExecutor(genericMessageExecutorMessage, genericMessageExecutors[genericMessageExecutorMessage]);
                    }
                }

            });

            return builder;
        }

        /// <summary>
        /// Adds a particular fake message executor to the available fake message executors. If there was one executor for the same OrganizationRequest,
        /// it'll be replaced with this new instance
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddFakeMessageExecutor(this IMiddlewareBuilder builder, IFakeMessageExecutor executor) 
        {
            builder.Add(context => {
                if (!context.HasProperty<MessageExecutors>())
                    context.SetProperty(new MessageExecutors());
                    
                var messageExecutors = context.GetProperty<MessageExecutors>();
                if (!messageExecutors.ContainsKey(executor.GetResponsibleRequestType()))
                    messageExecutors.Add(executor.GetResponsibleRequestType(), executor);
                else
                    messageExecutors[executor.GetResponsibleRequestType()] = executor;
            });

            return builder;
        }

        /// <summary>
        /// Adds a particular fake message executor to the available fake message executors that will be executed when to the given OrganizationRequest is requested. 
        /// If there was one executor for the same OrganizationRequest, it'll be replaced with this new instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddFakeMessageExecutor<T>(this IMiddlewareBuilder builder, IFakeMessageExecutor executor) where T: OrganizationRequest
        {
            builder.Add(context => {
                if (!context.HasProperty<MessageExecutors>())
                    context.SetProperty(new MessageExecutors());

                var messageExecutors = context.GetProperty<MessageExecutors>();
                if (!messageExecutors.ContainsKey(typeof(T)))
                    messageExecutors.Add(typeof(T), executor);
                else
                    messageExecutors[typeof(T)] = executor;
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder RemoveFakeMessageExecutor<T>(this IMiddlewareBuilder builder) where T: OrganizationRequest
        {
            builder.Add(context => {
                var messageExecutors = context.GetProperty<MessageExecutors>();
                messageExecutors.Remove(typeof(T));
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="mock"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="message"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddGenericFakeMessageExecutor(this IMiddlewareBuilder builder, string message, IFakeMessageExecutor executor)
        {
            builder.Add(context => {
                if(!context.HasProperty<GenericMessageExecutors>())
                    context.SetProperty<GenericMessageExecutors>(new GenericMessageExecutors(new Dictionary<string, IFakeMessageExecutor>()));

                var genericMessageExecutors = context.GetProperty<GenericMessageExecutors>();
                if (!genericMessageExecutors.ContainsKey(message))
                    genericMessageExecutors.Add(message, executor);
                else
                    genericMessageExecutors[message] = executor;
            });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="message"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder RemoveGenericFakeMessageExecutor(this IMiddlewareBuilder builder, string message, IFakeMessageExecutor executor)
        {
            builder.Add(context => {
                var genericMessageExecutors = context.GetProperty<GenericMessageExecutors>();
                if (!genericMessageExecutors.ContainsKey(message))
                    genericMessageExecutors.Remove(message);
            });

            return builder;
        }

        /// <summary>
        /// Implements the handling of fake message executors, generic fake message executors, and execution mocks in the pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
            
            if(context.HasProperty<GenericMessageExecutors>())
            {
                var genericMessageExecutors = context.GetProperty<GenericMessageExecutors>();
                if(genericMessageExecutors.ContainsKey(request.RequestName)) 
                {
                    return true;
                }
            }

            if(context.HasProperty<MessageExecutors>())
            {
                var messageExecutors = context.GetProperty<MessageExecutors>();
                if(messageExecutors.ContainsKey(request.GetType())) 
                {
                    return true;
                }
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

            if(context.HasProperty<GenericMessageExecutors>()) 
            {
                var genericMessageExecutors = context.GetProperty<GenericMessageExecutors>();
                if(genericMessageExecutors.ContainsKey(request.RequestName))
                {
                    return (genericMessageExecutors[request.RequestName] as IBaseFakeMessageExecutor).Execute(request, context); 
                }
            }

            if(context.HasProperty<MessageExecutors>()) 
            {
                var messageExecutors = context.GetProperty<MessageExecutors>();
                if(messageExecutors.ContainsKey(request.GetType()))
                {
                    return (messageExecutors[request.GetType()] as IBaseFakeMessageExecutor).Execute(request, context); 
                }
            }

            throw PullRequestException.NotImplementedOrganizationRequest(request.GetType());
                       
        }
        

        
    }
}