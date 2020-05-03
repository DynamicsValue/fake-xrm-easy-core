using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    public class UpsertRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpsertRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var fakedContext = ctx as XrmFakedContext;
            var upsertRequest = (UpsertRequest)request;
            bool recordCreated;

            var service = ctx.GetOrganizationService();

            var entityLogicalName = upsertRequest.Target.LogicalName;
            var entityId = ctx.GetRecordUniqueId(upsertRequest.Target.ToEntityReferenceWithKeyAttributes(), validate: false);

            if (fakedContext.Data.ContainsKey(entityLogicalName) &&
                fakedContext.Data[entityLogicalName].ContainsKey(entityId))
            {
                recordCreated = false;
                service.Update(upsertRequest.Target);
            }
            else
            {
                recordCreated = true;
                entityId = service.Create(upsertRequest.Target);
            }

            var result = new UpsertResponse();
            result.Results.Add("RecordCreated", recordCreated);
            result.Results.Add("Target", new EntityReference(entityLogicalName, entityId));
            return result;
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(UpsertRequest);
        }
    }
}
#endif
