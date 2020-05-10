using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Metadata;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrieveOptionSetRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrieveOptionSetRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var retrieveOptionSetRequest = (RetrieveOptionSetRequest)request;

            if (retrieveOptionSetRequest.MetadataId != Guid.Empty) //ToDo: Implement retrieving option sets by Id
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, $"Could not find optionset with optionset id: {retrieveOptionSetRequest.MetadataId}");
            }

            var name = retrieveOptionSetRequest.Name;

            if (string.IsNullOrEmpty(name))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument, "Name is required when optionSet id is not specified");
            }

            var optionSetMetadataRepository = ctx.GetProperty<IOptionSetMetadataRepository>();

            var optionSetMetadata = optionSetMetadataRepository.GetByName(name);
            if (optionSetMetadata == null)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, string.Format("An OptionSetMetadata with the name {0} does not exist.", name));
            }

            var response = new RetrieveOptionSetResponse()
            {
                Results = new ParameterCollection
                        {
                            { "OptionSetMetadata", optionSetMetadata }
                        }
            };

            return response;
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveOptionSetRequest);
        }
    }
}