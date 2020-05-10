using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Permissions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrievePrincipalAccessRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrievePrincipalAccessRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            RetrievePrincipalAccessRequest req = (RetrievePrincipalAccessRequest)request;
            return ctx.GetProperty<IAccessRightsRepository>().RetrievePrincipalAccess(req.Target, req.Principal);
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrievePrincipalAccessRequest);
        }
    }
}