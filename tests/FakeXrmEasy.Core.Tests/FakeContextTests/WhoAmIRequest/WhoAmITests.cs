using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.WhoAmIRequestTests
{
    public class WhoAmITests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_a_who_am_i_request_is_invoked_the_caller_id_is_returned()
        {
            _context.CallerProperties.CallerId = new EntityReference() { Id = Guid.NewGuid(), Name = "Super Faked User" };

            WhoAmIRequest req = new WhoAmIRequest();

            var response = _service.Execute(req) as WhoAmIResponse;
            Assert.Equal(response.UserId, _context.CallerProperties.CallerId.Id);
        }
    }
}