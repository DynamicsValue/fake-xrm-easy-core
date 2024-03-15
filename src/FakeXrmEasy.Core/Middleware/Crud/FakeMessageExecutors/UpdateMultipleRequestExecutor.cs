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
    public class UpdateMultipleRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpdateMultipleRequest;
        }

        /// <summary>
        /// Executes the CreateRequestMultiple request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var updateMultipleRequest = (UpdateMultipleRequest)request;

            ValidateRequest(updateMultipleRequest, ctx);
            
            var records = updateMultipleRequest.Targets.Entities;
            
            foreach (var record in records)
            {
                ctx.UpdateEntity(record);
            }

            return new UpdateMultipleResponse()
            {
                ResponseName = "UpdateMultipleResponse"
            };
        }

        private void ValidateRequiredParameters(UpdateMultipleRequest request, IXrmFakedContext ctx)
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

        private void ValidateEntityName(UpdateMultipleRequest request, IXrmFakedContext ctx)
        {
            var targets = request.Targets;
            if (ctx.ProxyTypesAssemblies.Any())
            {
                var earlyBoundType = ctx.FindReflectedType(targets.EntityName);
                if (earlyBoundType == null)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoEntity,
                        $"The entity with a name = '{targets.EntityName}' with namemapping = 'Logical' was not found in the MetadataCache.");
                }
            }

            if (ctx.CreateMetadataQuery().Any())
            {
                var entityMetadata = ctx.CreateMetadataQuery().FirstOrDefault(m => m.LogicalName == targets.EntityName);
                if (entityMetadata == null)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoEntity,
                        $"The entity with a name = '{targets.EntityName}' with namemapping = 'Logical' was not found in the MetadataCache.");
                }
            }
        }

        private void ValidateRecords(UpdateMultipleRequest request, IXrmFakedContext ctx)
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

        private void ValidateRecord(UpdateMultipleRequest request, Entity recordToUpdate, IXrmFakedContext ctx)
        {
            if (!request.Targets.EntityName.Equals(recordToUpdate.LogicalName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute,
                    $"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {request.Targets.EntityName} while this entity has Platform Name: {recordToUpdate.LogicalName}");
            }

            if (recordToUpdate.Id == Guid.Empty)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist,
                    $"Entity Id must be specified for Operation");
            }
            
            var exists = ctx.ContainsEntity(recordToUpdate.LogicalName, recordToUpdate.Id);
            if (!exists)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist,
                    $"{recordToUpdate.LogicalName} With Ids = {recordToUpdate.Id} Do Not Exist");
            }
        }

        private void ValidateRequest(UpdateMultipleRequest request, IXrmFakedContext ctx)
        {
            ValidateRequiredParameters(request, ctx);
            ValidateEntityName(request, ctx);
            ValidateRecords(request, ctx);
        }

        /// <summary>
        /// Returns CreateMultipleRequest
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(UpdateMultipleRequest);
        }
    }
}
#endif