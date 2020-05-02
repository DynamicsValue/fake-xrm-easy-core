using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.ServiceModel;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    public class DeleteRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is DeleteRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var deleteRequest = (DeleteRequest)request;

            var target = deleteRequest.Target;

            if (target == null)
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not delete without target");
            }

            var targetId = ctx.GetRecordUniqueId(target);
            target.Id = targetId;

            ctx.DeleteEntity(target);

            return new DeleteResponse();
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(DeleteRequest);
        }
    }
}