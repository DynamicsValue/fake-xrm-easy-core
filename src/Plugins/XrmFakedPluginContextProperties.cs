
using System;
using FakeXrmEasy.Abstractions.Plugins;
using Microsoft.Xrm.Sdk;
using FakeItEasy;

namespace FakeXrmEasy 
{
    public class XrmFakedPluginContextProperties : IXrmFakedPluginContextProperties
    {
        protected readonly IOrganizationService _service;
        protected readonly IXrmFakedTracingService _tracingService;
        protected readonly IEntityDataSourceRetrieverService _entityDataSourceRetrieverService;
        protected readonly IOrganizationServiceFactory _organizationServiceFactory;
        protected readonly IServiceEndpointNotificationService _serviceEndpointNotificationService;
        public XrmFakedPluginContextProperties(IOrganizationService service) 
        {
            _service = service;
            _tracingService = new XrmFakedTracingService();

            _organizationServiceFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => _organizationServiceFactory.CreateOrganizationService(A<Guid?>._)).ReturnsLazily((Guid? g) => _service);

        }


        public IOrganizationService OrganizationService => _service;
        public IXrmFakedTracingService TracingService => _tracingService;
        public IEntityDataSourceRetrieverService EntityDataSourceRetrieverService => _entityDataSourceRetrieverService;
        public IOrganizationServiceFactory OrganizationServiceFactory => _organizationServiceFactory;
        public IServiceEndpointNotificationService ServiceEndpointNotificationService => _serviceEndpointNotificationService;

        public IServiceProvider GetServiceProvider(XrmFakedPluginExecutionContext plugCtx) 
        {

            var fakedServiceProvider = A.Fake<IServiceProvider>();

            
            A.CallTo(() => fakedServiceProvider.GetService(A<Type>._))
               .ReturnsLazily((Type t) =>
               {
                   if (t == typeof(IOrganizationService))
                   {
                       return _service;
                   }

                   if (t == typeof(ITracingService))
                   {
                       return _tracingService;
                   }

                   if (t == typeof(IPluginExecutionContext))
                   {
                       return GetFakedPluginContext(plugCtx);
                   }

                   if (t == typeof(IExecutionContext))
                   {
                       return GetFakedExecutionContext(plugCtx);
                   }

                   if (t == typeof(IOrganizationServiceFactory))
                   {
                       return _organizationServiceFactory;
                   }

#if FAKE_XRM_EASY_9
                   if (t == typeof(IEntityDataSourceRetrieverService))
                   {
                       return _entityDataSourceRetrieverService;
                   }
#endif
                   throw new PullRequestException("The specified service type is not supported");
               });

            return fakedServiceProvider;
        
        }

        protected IPluginExecutionContext GetFakedPluginContext(XrmFakedPluginExecutionContext ctx)
        {
            var context = A.Fake<IPluginExecutionContext>();

            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);

            A.CallTo(() => context.ParentContext).ReturnsLazily(() => ctx.ParentContext);
            A.CallTo(() => context.Stage).ReturnsLazily(() => ctx.Stage);

            return context;
        }

        

        protected IExecutionContext GetFakedExecutionContext(XrmFakedPluginExecutionContext ctx)
        {
            var context = A.Fake<IExecutionContext>();

            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);

            return context;
        }

        protected void PopulateExecutionContextPropertiesFromFakedContext(IExecutionContext context, XrmFakedPluginExecutionContext ctx)
        {
            var newUserId = Guid.NewGuid();

            A.CallTo(() => context.Depth).ReturnsLazily(() => ctx.Depth <= 0 ? 1 : ctx.Depth);
            A.CallTo(() => context.IsExecutingOffline).ReturnsLazily(() => ctx.IsExecutingOffline);
            A.CallTo(() => context.InputParameters).ReturnsLazily(() => ctx.InputParameters);
            A.CallTo(() => context.OutputParameters).ReturnsLazily(() => ctx.OutputParameters);
            A.CallTo(() => context.PreEntityImages).ReturnsLazily(() => ctx.PreEntityImages);
            A.CallTo(() => context.PostEntityImages).ReturnsLazily(() => ctx.PostEntityImages);
            A.CallTo(() => context.MessageName).ReturnsLazily(() => ctx.MessageName);
            A.CallTo(() => context.Mode).ReturnsLazily(() => ctx.Mode);
            A.CallTo(() => context.OrganizationName).ReturnsLazily(() => ctx.OrganizationName);
            A.CallTo(() => context.OrganizationId).ReturnsLazily(() => ctx.OrganizationId);
            A.CallTo(() => context.InitiatingUserId).ReturnsLazily(() => ctx.InitiatingUserId == Guid.Empty ? newUserId : ctx.InitiatingUserId);
            A.CallTo(() => context.UserId).ReturnsLazily(() => ctx.UserId == Guid.Empty ? newUserId : ctx.UserId);
            A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => ctx.PrimaryEntityName);
            A.CallTo(() => context.SecondaryEntityName).ReturnsLazily(() => ctx.SecondaryEntityName);
            A.CallTo(() => context.SharedVariables).ReturnsLazily(() => ctx.SharedVariables);
            A.CallTo(() => context.BusinessUnitId).ReturnsLazily(() => ctx.BusinessUnitId);
            A.CallTo(() => context.CorrelationId).ReturnsLazily(() => ctx.CorrelationId);
            A.CallTo(() => context.OperationCreatedOn).ReturnsLazily(() => ctx.OperationCreatedOn);
            A.CallTo(() => context.IsolationMode).ReturnsLazily(() => ctx.IsolationMode);
            A.CallTo(() => context.IsInTransaction).ReturnsLazily(() => ctx.IsInTransaction);


            // Create message will pass an Entity as the target but this is not always true
            // For instance, a Delete request will receive an EntityReference
            if (ctx.InputParameters != null && ctx.InputParameters.ContainsKey("Target"))
            {
                if (ctx.InputParameters["Target"] is Entity)
                {
                    var target = (Entity)ctx.InputParameters["Target"];
                    A.CallTo(() => context.PrimaryEntityId).ReturnsLazily(() => target.Id);
                    A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => target.LogicalName);
                }
                else if (ctx.InputParameters["Target"] is EntityReference)
                {
                    var target = (EntityReference)ctx.InputParameters["Target"];
                    A.CallTo(() => context.PrimaryEntityId).ReturnsLazily(() => target.Id);
                    A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => target.LogicalName);
                }
            }
        }
    }
}