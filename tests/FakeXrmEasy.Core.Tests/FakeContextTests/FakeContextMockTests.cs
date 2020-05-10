using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests
{
    public class FakeContextMockTests
    {
        [Fact]
        public void Should_Execute_Mock_For_OrganizationRequests()
        {
            var context = new XrmFakedContext();
            var service = context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            context.Initialize(new[] { e });
            context.AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock);

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)service.Execute(request);

            Assert.Equal("Successful", response.ResponseName);
            var ex = Record.Exception(() => context.RemoveExecutionMock<RetrieveEntityRequest>());
            Assert.Null(ex);
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
            var context = new XrmFakedContext();
            var service = context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            context.Initialize(new[] { e });
            context.AddExecutionMock<RetrieveEntityRequest>(RetrieveEntityMock);
            context.AddExecutionMock<RetrieveEntityRequest>(AnotherRetrieveEntityMock);

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)service.Execute(request);

            Assert.Equal("Another", response.ResponseName);
            var ex = Record.Exception(() => context.RemoveExecutionMock<RetrieveEntityRequest>());
            Assert.Null(ex);
        }

        [Fact]
        public void Should_Override_FakeMessageExecutor()
        {
            var context = new XrmFakedContext();
            var service = context.GetOrganizationService();

            var e = new Entity("Contact") { Id = Guid.NewGuid() };
            context.Initialize(new[] { e });
            context.AddFakeMessageExecutor<RetrieveEntityRequest>(new FakeRetrieveEntityRequestExecutor());

            var request = new RetrieveEntityRequest
            {
                LogicalName = "Contact",
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = false
            };
            var response = (RetrieveEntityResponse)service.Execute(request);

            Assert.Equal("Successful", response.ResponseName);
            var ex = Record.Exception(() => context.RemoveFakeMessageExecutor<RetrieveEntityRequest>());
            Assert.Null(ex);
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