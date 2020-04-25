using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using System.Linq;
using Microsoft.Xrm.Sdk;
using FakeItEasy;

namespace FakeXrmEasy.Middleware
{
    public class MiddlewareBuilder: IMiddlewareBuilder
    {
        private readonly IList<Func<OrganizationRequestDelegate, OrganizationRequestDelegate>> _components = new List<Func<OrganizationRequestDelegate, OrganizationRequestDelegate>>();

        internal IXrmFakedContext _context;
        internal MiddlewareBuilder() 
        {
            _context = new XrmFakedContext();
        }

        public static IMiddlewareBuilder New() 
        {
            return new MiddlewareBuilder();
        }

        public IMiddlewareBuilder Add(Action<IXrmFakedContext> addToContextAction)
        {
            addToContextAction.Invoke(_context);
            return this;
        }

        public IMiddlewareBuilder Use(Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware) 
        {
            _components.Add(middleware);
            return this;
        }

        public IXrmFakedContext Build() 
        {
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

        
    }
}
