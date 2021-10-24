using System;
using Xunit;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;

namespace FakeXrmEasy.Tests.FakeContextTests
{
    public class TestGenericMessageExecutors
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;
        public TestGenericMessageExecutors() 
        {
            _context = MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud()   
                        .AddFakeMessageExecutors()
                        .AddGenericFakeMessageExecutor("new_TestAction", new FakeMessageExecutor())

                        // Use* -> Defines pipeline sequence
                        .UseCrud() 
                        .UseMessages()

                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
                        
            _service = _context.GetOrganizationService();
        }
        [Fact]
        public void Should_execute_generic_message()
        {
            OrganizationRequest request = new OrganizationRequest("new_TestAction");
            request["input"] = "testinput";
            OrganizationResponse response = _service.Execute(request);
            Assert.Equal("testinput", response["output"]);
        }

        [Fact]
        public void TestGenericMessageRemoval()
        {
            OrganizationRequest request = new OrganizationRequest("new_TestAction");
            request["input"] = "testinput";
            OrganizationResponse response = _service.Execute(request);
            Assert.Equal("testinput", response["output"]);

            (_context as XrmFakedContext).RemoveGenericFakeMessageExecutor("new_TestAction");
            Assert.Throws(typeof(FakeXrmEasy.PullRequestException), () => _service.Execute(request));
        }
    }

    class FakeMessageExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request.RequestName == "new_TestAction";
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            OrganizationResponse response = new OrganizationResponse();
            response["output"] = request["input"];
            return response;
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(OrganizationRequest);
        }
    }
}
