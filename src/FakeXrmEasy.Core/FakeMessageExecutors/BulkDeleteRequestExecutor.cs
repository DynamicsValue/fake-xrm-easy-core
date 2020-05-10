using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class BulkDeleteRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is BulkDeleteRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var bulkDeleteRequest = (BulkDeleteRequest)request;
           
            if (string.IsNullOrEmpty(bulkDeleteRequest.JobName))
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not Bulk delete without JobName");
            }
            if (bulkDeleteRequest.QuerySet == null)
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not Bulk delete without QuerySet");
            }
            if (bulkDeleteRequest.CCRecipients == null)
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not Bulk delete without CCRecipients");
            }
            if (bulkDeleteRequest.ToRecipients == null)
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not Bulk delete without ToRecipients");
            }

            var service = ctx.GetOrganizationService();

            // generate JobId
            var jobId = Guid.NewGuid();

            // create related asyncOperation
            Entity asyncOpertation = new Entity("asyncoperation")
            {
                Id = jobId
            };

            service.Create(asyncOpertation);

            // delete all records from all queries
            foreach (QueryExpression queryExpression in bulkDeleteRequest.QuerySet)
            {
                EntityCollection recordsToDelete = service.RetrieveMultiple(queryExpression);
                foreach (Entity record in recordsToDelete.Entities)
                {
                    service.Delete(record.LogicalName, record.Id);
                }
            }

            // set ayncoperation to completed
            asyncOpertation["statecode"] = new OptionSetValue(3);
            service.Update(asyncOpertation);

            // return result
            return new BulkDeleteResponse { ResponseName = "BulkDeleteResponse", ["JobId"] = jobId};
        }
         
        public Type GetResponsibleRequestType()
        {
            return typeof(BulkDeleteRequest);
        }
    }
}
