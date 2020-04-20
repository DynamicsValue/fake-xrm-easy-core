using System;
using FakeXrmEasy.Abstractions.Middleware;

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

        }

        public IMiddlewareBuilder AddCrud() 
        {

        }
        public IMiddlewareBuilder AddFakeMessages() 
        {

        }
        public IXrmFakedContext Build() 
        {

        }
    }
}
