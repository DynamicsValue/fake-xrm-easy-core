using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Xunit;
using System;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;

namespace FakeXrmEasy.Tests.Middleware
{
    public partial class MiddlewareBuilderTests 
    {
        [Fact]
        public void Should_Execute_Mock_For_OrganizationRequests()
        {
            var context = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .UseMessages()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            var service = context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            context.Initialize(new[] { e });

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse) service.Execute(request);

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
        public void Should_add_execution_mock()
        {
            var ctx = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            Assert.True(ctx.HasProperty<ExecutionMocks>());

            var mocks = ctx.GetProperty<ExecutionMocks>();
            Assert.True(mocks.ContainsKey(typeof(RetrieveEntityRequest)));
            Assert.Equal(RetrieveEntityMock, mocks[typeof(RetrieveEntityRequest)]);
        }

        [Fact]
        public void Should_override_execution_mock()
        {
            var ctx = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .AddExecutionMock<RetrieveEntityRequest>(AnotherRetrieveEntityMock)
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            Assert.True(ctx.HasProperty<ExecutionMocks>());

            var mocks = ctx.GetProperty<ExecutionMocks>();
            Assert.True(mocks.ContainsKey(typeof(RetrieveEntityRequest)));
            Assert.Equal(AnotherRetrieveEntityMock, mocks[typeof(RetrieveEntityRequest)]);
        }

        [Fact]
        public void Should_remove_execution_mock()
        {
            var ctx = MiddlewareBuilder
                        .New()
                        .AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock)
                        .RemoveExecutionMock<RetrieveEntityRequest>()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            Assert.True(ctx.HasProperty<ExecutionMocks>());

            var mocks = ctx.GetProperty<ExecutionMocks>();
            Assert.False(mocks.ContainsKey(typeof(RetrieveEntityRequest)));
        }
    }


    
    
}
