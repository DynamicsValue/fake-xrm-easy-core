using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class FetchXmlToQueryExpressionRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is FetchXmlToQueryExpressionRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var req = request as FetchXmlToQueryExpressionRequest;
            var service = ctx.GetOrganizationService();
            FetchXmlToQueryExpressionResponse response = new FetchXmlToQueryExpressionResponse();
            response["Query"] = req.FetchXml.ToQueryExpression(ctx);
            return response;
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(FetchXmlToQueryExpressionRequest);
        }
    }
}
