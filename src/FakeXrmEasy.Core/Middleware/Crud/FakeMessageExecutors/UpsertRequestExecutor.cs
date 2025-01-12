using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// Fake Message executor for Upsert requests
    /// </summary>
    public class UpsertRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// Returns true if this message executor can execute the specified request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpsertRequest;
        }


        /// <summary>
        /// Executes the current request with the given context
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var fakedContext = ctx as XrmFakedContext;
            var upsertRequest = (UpsertRequest)request;
            bool recordCreated;

            var service = ctx.GetOrganizationService();

            var entityLogicalName = upsertRequest.Target.LogicalName;
            var entityId = ctx.GetRecordUniqueId(upsertRequest.Target.ToEntityReferenceWithKeyAttributes(), validate: false);

            if (fakedContext.ContainsEntity(entityLogicalName, entityId))
            {
                recordCreated = false;
                service.Update(upsertRequest.Target);
            }
            else
            {
                recordCreated = true;
                entityId = ctx.CreateEntity(upsertRequest.Target, isUpsert: true);
            }

            var result = new UpsertResponse();
            result.Results.Add("RecordCreated", recordCreated);
            result.Results.Add("Target", new EntityReference(entityLogicalName, entityId));
            return result;
        }

        /// <summary>
        /// Gets request type that will execute this request
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(UpsertRequest);
        }
    }
}
#endif
