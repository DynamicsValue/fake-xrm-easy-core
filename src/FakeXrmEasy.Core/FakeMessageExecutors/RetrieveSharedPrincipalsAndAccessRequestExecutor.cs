using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Permissions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrieveSharedPrincipalsAndAccessRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrieveSharedPrincipalsAndAccessRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            RetrieveSharedPrincipalsAndAccessRequest req = (RetrieveSharedPrincipalsAndAccessRequest)request;
            return ctx.GetProperty<IAccessRightsRepository>().RetrieveSharedPrincipalsAndAccess(req.Target);
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveSharedPrincipalsAndAccessRequest);
        }
    }
}