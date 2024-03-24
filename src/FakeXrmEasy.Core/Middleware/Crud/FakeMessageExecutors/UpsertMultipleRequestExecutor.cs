#if FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// UpsertMultipleRequest Executor
    /// </summary>
    public class UpsertMultipleRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpsertMultipleRequest;
        }

        /// <summary>
        /// Executes the CreateRequestMultiple request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var upsertMultipleRequest = (UpsertMultipleRequest)request;

            ValidateRequest(upsertMultipleRequest, ctx);
            
            var records = upsertMultipleRequest.Targets.Entities;
            List<UpsertResponse> results = new List<UpsertResponse>();

            var service = ctx.GetOrganizationService();
            
            foreach (var record in records)
            {
                var response = service.Execute(new UpsertRequest() { Target = record }) as UpsertResponse;
                results.Add(response);
            }

            return new UpsertMultipleResponse()
            {
                ResponseName = "UpsertMultipleResponse",
                ["Results"] = results.ToArray()
            };
        }

        private void ValidateRequiredParameters(UpsertMultipleRequest request)
        {
            if (request.Targets == null)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    "'Targets' should be one of the parameters for UpsertMultiple.");
            }

            var targets = request.Targets;
            if (string.IsNullOrWhiteSpace(targets.EntityName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.UnExpected,
                    "System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.");
            }
        }
        private void ValidateRecords(UpsertMultipleRequest request)
        {
            var records = request.Targets.Entities;
            if (records.Count == 0)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.UnExpected,
                    $"System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.");
            }

            foreach (var record in records)
            {
                ValidateRecord(request, record);
            }
        }

        private void ValidateRecord(UpsertMultipleRequest request, Entity recordToCreate)
        {
            if (!request.Targets.EntityName.Equals(recordToCreate.LogicalName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    $"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {request.Targets.EntityName} while this entity has Platform Name: {recordToCreate.LogicalName}");
            }
        }

        private void ValidateRequest(UpsertMultipleRequest request, IXrmFakedContext ctx)
        {
            ValidateRequiredParameters(request);
            BulkOperationsCommon.ValidateEntityName(request.Targets.EntityName, ctx);
            ValidateRecords(request);
        }

        /// <summary>
        /// Returns UpsertMultipleRequest
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(UpsertMultipleRequest);
        }
    }
}
#endif