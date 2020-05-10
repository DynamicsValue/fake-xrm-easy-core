using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Client;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrieveEntityRequestExecutor : IFakeMessageExecutor
    {
        
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrieveEntityRequest;
        }
        
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var req = request as RetrieveEntityRequest;

            if (string.IsNullOrWhiteSpace(req.LogicalName))
            {
                throw new Exception("A logical name property must be specified in the request");
            }

            // HasFlag -> used to verify flag matches --> to verify EntityFilters.Entity | EntityFilters.Attributes
            if (req.EntityFilters.HasFlag(Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity) ||
                req.EntityFilters.HasFlag(Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes))
            {
                if(ctx.GetEntityMetadataByName(req.LogicalName) == null)
                {
                    throw new Exception($"Entity '{req.LogicalName}' is not found in the metadata cache");
                }

                var entityMetadata = ctx.GetEntityMetadataByName(req.LogicalName);

                var response = new RetrieveEntityResponse()
                {
                    Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadata }
                        }
                };

                return response;
            }

            throw new Exception("At least EntityFilters.Entity or EntityFilters.Attributes must be present on EntityFilters of Request.");
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveEntityRequest);
        }
    }
}
