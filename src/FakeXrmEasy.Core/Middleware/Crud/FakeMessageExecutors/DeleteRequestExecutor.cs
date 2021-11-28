using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.ServiceModel;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is DeleteRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(DeleteRequest);
        }
    }
}