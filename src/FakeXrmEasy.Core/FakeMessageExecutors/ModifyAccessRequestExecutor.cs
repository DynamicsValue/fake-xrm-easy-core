using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Permissions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class ModifyAccessRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is ModifyAccessRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            ModifyAccessRequest req = (ModifyAccessRequest)request;
            ctx.GetProperty<IAccessRightsRepository>().ModifyAccessOn(req.Target, req.PrincipalAccess);
            return new ModifyAccessResponse();
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(ModifyAccessRequest);
        }
    }
}
