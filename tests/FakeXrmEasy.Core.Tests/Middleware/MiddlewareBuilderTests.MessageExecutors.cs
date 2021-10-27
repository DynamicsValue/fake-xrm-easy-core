using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Xunit;
using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware.Messages;
using Microsoft.Xrm.Sdk.Messages;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Tests.Middleware
{
    public partial class MiddlewareBuilderTests 
    {
        
        [Fact]
        public void Should_Override_FakeMessageExecutor()
        {
            var context = MiddlewareBuilder
                        .New()
                        .AddFakeMessageExecutors()
                        .AddFakeMessageExecutor(new FakeRetrieveEntityRequestExecutor())
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

        [Fact]
        public void Should_add_fake_message_executor_with_request()
        {
            var ctx = MiddlewareBuilder
                            .New()
                            .AddFakeMessageExecutor<AssociateRequest>(new MyDummyFakeMessageExecutor())
                            .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                            .Build();

            Assert.True(ctx.HasProperty<MessageExecutors>());

            var executors = ctx.GetProperty<MessageExecutors>();
            Assert.True(executors.ContainsKey(typeof(AssociateRequest)));
            Assert.IsType<MyDummyFakeMessageExecutor>(executors[typeof(AssociateRequest)]);
        }

        [Fact]
        public void Should_add_and_override_fake_message_executor_with_request()
        {
            var ctx = MiddlewareBuilder
                            .New()
                            .AddFakeMessageExecutor<AssociateRequest>(new MyDummyFakeMessageExecutor())
                            .AddFakeMessageExecutor<AssociateRequest>(new AnotherDummyFakeMessageExecutor())
                            .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                            .Build();

            Assert.True(ctx.HasProperty<MessageExecutors>());

            var executors = ctx.GetProperty<MessageExecutors>();
            Assert.True(executors.ContainsKey(typeof(AssociateRequest)));
            Assert.IsType<AnotherDummyFakeMessageExecutor>(executors[typeof(AssociateRequest)]);
        }

        [Fact]
        public void Should_remove_fake_message_executor_with_request()
        {
            var ctx = MiddlewareBuilder
                            .New()
                            .AddFakeMessageExecutor<AssociateRequest>(new MyDummyFakeMessageExecutor())
                            .RemoveFakeMessageExecutor<AssociateRequest>()
                            .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                            .Build();

            Assert.True(ctx.HasProperty<MessageExecutors>());

            var executors = ctx.GetProperty<MessageExecutors>();
            Assert.False(executors.ContainsKey(typeof(AssociateRequest)));
        }

        private class MyDummyFakeMessageExecutor : IFakeMessageExecutor
        {
            public bool CanExecute(OrganizationRequest request)
            {
                return request is AssociateRequest;
            }

            public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
            {
                return new OrganizationResponse();
            }

            public Type GetResponsibleRequestType()
            {
                return typeof(AssociateRequest);
            }
        }

        private class AnotherDummyFakeMessageExecutor : IFakeMessageExecutor
        {
            public bool CanExecute(OrganizationRequest request)
            {
                return request is AssociateRequest;
            }

            public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
            {
                return new OrganizationResponse();
            }

            public Type GetResponsibleRequestType()
            {
                return typeof(AssociateRequest);
            }
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
