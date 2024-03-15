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
    /// CreateMultipleRequest Executor
    /// </summary>
    public class CreateMultipleRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is CreateMultipleRequest;
        }

        /// <summary>
        /// Executes the CreateRequestMultiple request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var createMultipleRequest = (CreateMultipleRequest)request;

            ValidateRequest(createMultipleRequest, ctx);
            
            var records = createMultipleRequest.Targets.Entities;
            List<Guid> createdIds = new List<Guid>();
            
            foreach (var record in records)
            {
                var id = ctx.CreateEntity(record);
                createdIds.Add(id);
            }

            return new CreateMultipleResponse()
            {
                ResponseName = "CreateMultipleResponse",
                ["Ids"] = createdIds.ToArray()
            };
        }

        private void ValidateRequiredParameters(CreateMultipleRequest request, IXrmFakedContext ctx)
        {
            if (request.Targets == null)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    "Required field 'Targets' is missing");
            }

            var targets = request.Targets;
            if (string.IsNullOrWhiteSpace(targets.EntityName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    "Required member 'EntityName' missing for field 'Targets'");
            }
        }
        private void ValidateRecords(CreateMultipleRequest request, IXrmFakedContext ctx)
        {
            var records = request.Targets.Entities;
            if (records.Count == 0)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.UnExpected,
                    $"System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.");
            }

            foreach (var record in records)
            {
                ValidateRecord(request, record, ctx);
            }
        }

        private void ValidateRecord(CreateMultipleRequest request, Entity recordToCreate, IXrmFakedContext ctx)
        {
            if (!request.Targets.EntityName.Equals(recordToCreate.LogicalName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    $"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {request.Targets.EntityName} while this entity has Platform Name: {recordToCreate.LogicalName}");
            }
            
            var exists = ctx.ContainsEntity(recordToCreate.LogicalName, recordToCreate.Id);
            if (exists)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.DuplicateRecord,
                    $"Cannot insert duplicate key.");
            }
        }

        private void ValidateRequest(CreateMultipleRequest request, IXrmFakedContext ctx)
        {
            ValidateRequiredParameters(request, ctx);
            BulkOperationsCommon.ValidateEntityName(request.Targets.EntityName, ctx);
            ValidateRecords(request, ctx);
        }

        /// <summary>
        /// Returns CreateMultipleRequest
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(CreateMultipleRequest);
        }
    }
}
#endif