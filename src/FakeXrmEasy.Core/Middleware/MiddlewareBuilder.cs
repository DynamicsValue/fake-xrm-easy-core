using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Integrity;
using System.Linq;
using Microsoft.Xrm.Sdk;
using FakeItEasy;
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
    }
}
