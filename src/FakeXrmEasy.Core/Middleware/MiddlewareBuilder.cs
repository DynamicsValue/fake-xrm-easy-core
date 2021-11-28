using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Integrity;
using System.Linq;
using Microsoft.Xrm.Sdk;
using FakeItEasy;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Exceptions;

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

            OrganizationRequestDelegate app = (context, request) => {
                
                //return default PullRequestException at the end of the pipeline
                throw PullRequestException.NotImplementedOrganizationRequest(request.GetType());
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
        /// 
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public IMiddlewareBuilder SetLicense(FakeXrmEasyLicense license)
        {
            _context.LicenseContext = license;
            return this;
        }
    }
}
