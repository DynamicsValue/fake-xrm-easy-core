using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Permissions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RevokeAccessRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RevokeAccessRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            RevokeAccessRequest req = (RevokeAccessRequest)request;
            ctx.GetProperty<IAccessRightsRepository>().RevokeAccessTo(req.Target, req.Revokee);
            return new RevokeAccessResponse();
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RevokeAccessRequest);
        }
    }
}