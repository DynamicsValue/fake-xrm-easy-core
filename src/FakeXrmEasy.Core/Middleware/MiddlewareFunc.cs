
using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Middleware
{
    internal class MiddlewareFunc
    {
        internal Func<IXrmFakedContext, OrganizationRequest, OrganizationResponse> Current { get; set; }
        internal MiddlewareFunc Next { get; set;}

        
    }
}