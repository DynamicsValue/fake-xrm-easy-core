using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Middleware
{
    public class MiddlewareBuilder: IMiddlewareBuilder
    {
        internal MiddlewareFunc First { get; set; }
        internal MiddlewareBuilder() 
        {
            First = null;
        }
        public static IMiddlewareBuilder New() 
        {
            return new MiddlewareBuilder();
        }
        public IMiddlewareBuilder Add(Func<IXrmFakedContext, OrganizationRequest, Func<IXrmFakedContext, OrganizationRequest, OrganizationResponse>, OrganizationResponse> funcOrNext) 
        {
            throw new NotImplementedException();
        }

        public IMiddlewareBuilder AddCrud() 
        {
            throw new NotImplementedException();
        }
        public IMiddlewareBuilder AddFakeMessages() 
        {
            throw new NotImplementedException();
        }
        public IXrmFakedContext Build() 
        {
            throw new NotImplementedException();
        }
    }
}
