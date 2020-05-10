using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrieveExchangeRateRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrieveExchangeRateRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var retrieveExchangeRateRequest = (RetrieveExchangeRateRequest)request;

            var currencyId = retrieveExchangeRateRequest.TransactionCurrencyId;

            if (currencyId == Guid.Empty)
            {
                throw FakeOrganizationServiceFaultFactory.New("Can not retrieve Exchange Rate without Transaction Currency Guid");
            }

            var service = ctx.GetOrganizationService();

            var result = service.RetrieveMultiple(new QueryExpression("transactioncurrency")
            {
                ColumnSet = new ColumnSet("exchangerate"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("transactioncurrencyid", ConditionOperator.Equal, currencyId)
                    }
                }
            }).Entities;

            if (!result.Any())
            {
                throw FakeOrganizationServiceFaultFactory.New("Transaction Currency not found");
            }

            var exchangeRate = result.First().GetAttributeValue<decimal>("exchangerate");

            return new RetrieveExchangeRateResponse
            {
                Results = new ParameterCollection
                {
                    {"ExchangeRate", exchangeRate}
                }
            };
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveExchangeRateRequest);
        }
    }
}