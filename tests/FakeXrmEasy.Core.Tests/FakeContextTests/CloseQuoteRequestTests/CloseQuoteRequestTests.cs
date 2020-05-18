using FakeXrmEasy.Abstractions;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.CloseQuoteRequestTests
{
    public class CloseQuoteRequestTests
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public CloseQuoteRequestTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_can_execute_is_called_with_an_invalid_request_result_is_false()
        {
            var executor = new CloseQuoteRequestExecutor();
            var anotherRequest = new RetrieveMultipleRequest();
            Assert.False(executor.CanExecute(anotherRequest));
        }

        [Fact]
        public void Should_Change_Status_When_Closing()
        {

            var quote = new Entity
            {
                LogicalName = "quote",
                Id = Guid.NewGuid(),
                Attributes = new AttributeCollection
                {
                    {"statuscode", new OptionSetValue(0)}
                }
            };

            _context.Initialize(new[]
            {
                quote
            });

            var executor = new CloseQuoteRequestExecutor();

            var req = new CloseQuoteRequest
            {
                QuoteClose = new Entity
                {
                    Attributes = new AttributeCollection
                    {
                        { "quoteid", quote.ToEntityReference() }
                    }
                },
                Status = new OptionSetValue(1)
            };

            executor.Execute(req, _context);

            quote = _service.Retrieve("quote", quote.Id, new ColumnSet(true));

            Assert.Equal(new OptionSetValue(1), quote.GetAttributeValue<OptionSetValue>("statuscode"));
        }
    }
}