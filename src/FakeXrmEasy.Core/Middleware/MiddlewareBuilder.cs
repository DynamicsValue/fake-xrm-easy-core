using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Integrity;
using System.Linq;
using Microsoft.Xrm.Sdk;
using FakeItEasy;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Threading;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.Exceptions;
using FakeXrmEasy.Core.FileStorage;

namespace FakeXrmEasy.Middleware
{
    /// <summary>
    /// Middleware Builder
    /// </summary>
    public class MiddlewareBuilder: IMiddlewareBuilder
    {
        private readonly IList<Func<OrganizationRequestDelegate, OrganizationRequestDelegate>> _components = new List<Func<OrganizationRequestDelegate, OrganizationRequestDelegate>>();

        internal IXrmFakedContext _context;
        internal MiddlewareBuilder() 
        {
            _context = new XrmFakedContext(this);
        }
        internal MiddlewareBuilder(XrmFakedContext existingContext) 
        {
            _context = existingContext;
        }

        /// <summary>
        /// New
        /// </summary>
        /// <returns></returns>
        public static IMiddlewareBuilder New() 
        {
            var builder = new MiddlewareBuilder();
            builder.AddDefaults();
            return builder;
        }

        internal static IMiddlewareBuilder New(XrmFakedContext context) 
        {
            var builder = new MiddlewareBuilder(context);
            builder.AddDefaults();
            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addToContextAction"></param>
        /// <returns></returns>
        public IMiddlewareBuilder Add(Action<IXrmFakedContext> addToContextAction)
        {
            addToContextAction.Invoke(_context);
            return this;
        }

        private void AddDefaults()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions() {  ValidateEntityReferences = false });
            _context.SetProperty<IFileStorageSettings>(new FileStorageSettings());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public IMiddlewareBuilder Use(Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware) 
        {
            _components.Add(middleware);
            return this;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LicenseException"></exception>
        public IXrmFakedContext Build() 
        {
            if(_context.LicenseContext == null)
            {
                throw new LicenseException("Please, you need to choose a FakeXrmEasy license. More info at https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/licensing-exception/");
            }

            if (_context.LicenseContext == FakeXrmEasyLicense.Commercial)
            {
                var subscriptionManager = SubscriptionManager.Instance;
                if (subscriptionManager.SubscriptionInfo != null)
                {
                    var subscriptionValidator = new SubscriptionValidator(
                        new EnvironmentReader(),
                        subscriptionManager.SubscriptionInfo,
                        subscriptionManager.SubscriptionUsage,
                        subscriptionManager.RenewalRequested);

                    subscriptionValidator.IsValid();
                }
            }
            
            OrganizationRequestDelegate app = (context, request) => {
                
                //return default PullRequestException at the end of the pipeline
                throw UnsupportedExceptionFactory.NotImplementedOrganizationRequest(_context.LicenseContext.Value, request.GetType());
            };

            foreach(var component in _components.Reverse())
            {
                app = component(app);
            }

            var service = _context.GetOrganizationService();

            A.CallTo(() => service.Execute(A<OrganizationRequest>._))
                .ReturnsLazily((OrganizationRequest request) => app.Invoke(_context, request));

            var serviceAsync = _context.GetAsyncOrganizationService();
            AddOrganizationServiceAsyncFake(serviceAsync);

            var serviceAsync2 = _context.GetAsyncOrganizationService2();
            AddOrganizationServiceAsyncFake(serviceAsync2);
            AddOrganizationServiceAsyncFake2(serviceAsync2);
            
            return _context;
        }

        /// <summary>
        /// FakeXrmEasy can be used under 3 different licences, this method defines the license. More info at: https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/license/
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public IMiddlewareBuilder SetLicense(FakeXrmEasyLicense license)
        {
            _context.LicenseContext = license;
            return this;
        }
        
        /// <summary>
        /// Use this method to provide an implementation for a subscription storage provider when you are using a commercial license and have a license key 
        /// </summary>
        /// <param name="storageProvider">An implementation of a ISubscriptionStorageProvider that is capable of reading and writing subscription usage data as well as your license key</param>
        /// <param name="upgradeRequested">Set to true if you exceeded the number of users that your current subscription allows and you have already requested an upgrade to DynamicsValue via your organisation's established process</param>
        /// <param name="renewalRequested">Set to true if your subscription expired and you have already requested an renewal to DynamicsValue via your organisation's established process</param>
        /// <returns></returns>
        public IMiddlewareBuilder SetSubscriptionStorageProvider(ISubscriptionStorageProvider storageProvider, bool upgradeRequested = false, bool renewalRequested = false)
        {
            var userReader = new UserReader();
            Console.WriteLine($"Setting Subscription Storage Provider...");
            Console.WriteLine($"  -> Running as '{userReader.GetCurrentUserName()}' ...");

            var subscriptionManagerInstance = SubscriptionManager.Instance;
            subscriptionManagerInstance.SetSubscriptionStorageProvider(storageProvider, userReader, upgradeRequested, renewalRequested);
            
            Console.WriteLine($"Setting Subscription Storage Provider ok.");
            return this;
        }
        
        private void AddOrganizationServiceAsyncFake(IOrganizationServiceAsync serviceAsync)
        {
            var service = _context.GetOrganizationService();

            A.CallTo(() => serviceAsync.Create(A<Entity>._))
                .ReturnsLazily((Entity entity) => service.Create(entity));

            A.CallTo(() => serviceAsync.CreateAsync(A<Entity>._))
                .ReturnsLazily((Entity entity) => service.Create(entity));

            A.CallTo(() => serviceAsync.Update(A<Entity>._))
                .Invokes((Entity entity) => service.Update(entity));

            A.CallTo(() => serviceAsync.UpdateAsync(A<Entity>._))
                .Invokes((Entity entity) => service.Update(entity));

            A.CallTo(() => serviceAsync.Delete(A<string>._, A<Guid>._))
                .Invokes((string entityLogicalName, Guid id) => service.Delete(entityLogicalName, id));

            A.CallTo(() => serviceAsync.DeleteAsync(A<string>._, A<Guid>._))
                .Invokes((string entityLogicalName, Guid id) => service.Delete(entityLogicalName, id));

            A.CallTo(() => serviceAsync.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._))
                .ReturnsLazily((string entityLogicalName, Guid id, ColumnSet columnSet) => service.Retrieve(entityLogicalName, id, columnSet));

            A.CallTo(() => serviceAsync.RetrieveAsync(A<string>._, A<Guid>._, A<ColumnSet>._))
                .ReturnsLazily((string entityLogicalName, Guid id, ColumnSet columnSet) => service.Retrieve(entityLogicalName, id, columnSet));

            A.CallTo(() => serviceAsync.RetrieveMultiple(A<QueryBase>._))
                .ReturnsLazily((QueryBase query) => service.RetrieveMultiple(query));

            A.CallTo(() => serviceAsync.RetrieveMultipleAsync(A<QueryBase>._))
                .ReturnsLazily((QueryBase query) => service.RetrieveMultiple(query));

            A.CallTo(() => serviceAsync.ExecuteAsync(A<OrganizationRequest>._))
                .ReturnsLazily((OrganizationRequest request) => service.Execute(request));

            A.CallTo(() => serviceAsync.Execute(A<OrganizationRequest>._))
                .ReturnsLazily((OrganizationRequest request) => service.Execute(request));

            A.CallTo(() => serviceAsync.AssociateAsync(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection) 
                    => service.Associate(entityLogicalName, id, relationship, entityRefCollection ));

            A.CallTo(() => serviceAsync.Associate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection) 
                    => service.Associate(entityLogicalName, id, relationship, entityRefCollection ));

            A.CallTo(() => serviceAsync.DisassociateAsync(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection) 
                    => service.Disassociate(entityLogicalName, id, relationship, entityRefCollection ));

            A.CallTo(() => serviceAsync.Disassociate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection) 
                    => service.Disassociate(entityLogicalName, id, relationship, entityRefCollection ));

        }

        private void AddOrganizationServiceAsyncFake2(IOrganizationServiceAsync2 serviceAsync)
        {
            var service = _context.GetOrganizationService();

            A.CallTo(() => serviceAsync.CreateAsync(A<Entity>._, A<CancellationToken>._))
                .ReturnsLazily((Entity entity, CancellationToken token) => service.Create(entity));

            A.CallTo(() => serviceAsync.UpdateAsync(A<Entity>._, A<CancellationToken>._))
                .Invokes((Entity entity, CancellationToken token) => service.Update(entity));

            A.CallTo(() => serviceAsync.DeleteAsync(A<string>._, A<Guid>._, A<CancellationToken>._))
                .Invokes((string entityLogicalName, Guid id, CancellationToken token) => service.Delete(entityLogicalName, id));

            A.CallTo(() => serviceAsync.RetrieveAsync(A<string>._, A<Guid>._, A<ColumnSet>._, A<CancellationToken>._))
                .ReturnsLazily((string entityLogicalName, Guid id, ColumnSet columnSet, CancellationToken token) => service.Retrieve(entityLogicalName, id, columnSet));

            A.CallTo(() => serviceAsync.RetrieveMultipleAsync(A<QueryBase>._, A<CancellationToken>._))
                .ReturnsLazily((QueryBase query, CancellationToken token) => service.RetrieveMultiple(query));

            A.CallTo(() => serviceAsync.ExecuteAsync(A<OrganizationRequest>._, A<CancellationToken>._))
                .ReturnsLazily((OrganizationRequest request, CancellationToken token) => service.Execute(request));

            A.CallTo(() => serviceAsync.AssociateAsync(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._, A<CancellationToken>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection, CancellationToken token) 
                    => service.Associate(entityLogicalName, id, relationship, entityRefCollection ));

            A.CallTo(() => serviceAsync.DisassociateAsync(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._, A<CancellationToken>._))
                .Invokes((string entityLogicalName, Guid id, Relationship relationship, EntityReferenceCollection entityRefCollection, CancellationToken token) 
                    => service.Disassociate(entityLogicalName, id, relationship, entityRefCollection ));

        }
    }
}
