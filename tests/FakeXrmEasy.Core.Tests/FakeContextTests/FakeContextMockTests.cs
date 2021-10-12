using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests
{
    public class FakeContextMockTests
    {
        private IXrmFakedContext _context;
        private IOrganizationService _service;
        public FakeContextMockTests()
        {
        }

        [Fact]
        public void Should_Execute_Mock_For_OrganizationRequests()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .UseMessages()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _service = _context.GetOrganizationService();  

             var e = new Entity("Contact") { Id = Guid.NewGuid() };          
            _context.Initialize(new[] { e });
            
            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)_service.Execute(request);

            Assert.Equal("Successful", response.ResponseName);
        }

        public OrganizationResponse RetrieveEntityMock(OrganizationRequest req)
        {
            return new RetrieveEntityResponse { ResponseName = "Successful" };
        }

        public OrganizationResponse AnotherRetrieveEntityMock(OrganizationRequest req)
        {
            return new RetrieveEntityResponse { ResponseName = "Another" };
        }

        [Fact]
        public void Should_Override_Execution_Mock()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .AddExecutionMock<RetrieveEntityRequest>(AnotherRetrieveEntityMock)
                        .UseMessages()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _service = _context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            _context.Initialize(new[] { e });

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)_service.Execute(request);

            Assert.Equal("Another", response.ResponseName);
        }

        [Fact]
        public void Should_Override_FakeMessageExecutor()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddFakeMessageExecutors()
                        .AddFakeMessageExecutor(new FakeRetrieveEntityRequestExecutor())
                        .UseMessages()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _service = _context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            _context.Initialize(new[] { e });

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)_service.Execute(request);

            Assert.Equal("Successful", response.ResponseName);
        }

        protected class FakeRetrieveEntityRequestExecutor : IFakeMessageExecutor
        {
            public bool CanExecute(OrganizationRequest request)
            {
                return request is RetrieveEntityRequest;
            }

            public Type GetResponsibleRequestType()
            {
                return typeof(RetrieveEntityRequest);
            }

            public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
            {
                return new RetrieveEntityResponse { ResponseName = "Successful" };
            }
        }
    }
}