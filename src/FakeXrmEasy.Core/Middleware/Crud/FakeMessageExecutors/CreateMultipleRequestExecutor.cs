#if FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
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
            
            var response = new CreateMultipleResponse();
            
            return new CreateResponse()
            {
                ResponseName = "CreateMultipleResponse"
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

        private void ValidateEntityName(CreateMultipleRequest request, IXrmFakedContext ctx)
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
            ValidateEntityName(request, ctx);
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