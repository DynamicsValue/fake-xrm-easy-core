using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Xunit;
using FakeItEasy;
using System;
using Crm;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Tests.Middleware
{
    public class MiddlewareBuilderTests
    {
        [Fact]
        public void Should_create_new_instance() 
        {
            var builder = MiddlewareBuilder.New();
            Assert.NotNull(builder);
        }

        [Fact]
        public void Should_build_a_default_pull_request_exception_pipeline_if_no_other_delegates_are_added() 
        {
            var builder = MiddlewareBuilder.New();
            var ctx = builder.Build();
            Assert.IsType<XrmFakedContext>(ctx);

            var dummyRequest = new OrganizationRequest("DummyRequest");
            var service = ctx.GetOrganizationService();

            Assert.Throws<PullRequestException>(() => service.Execute(dummyRequest));
        }

        [Fact]
        public void Should_be_able_to_add_a_new_mock_when_calling_add() 
        {
            var builder = MiddlewareBuilder.New();
            builder.Add(ctx => {
                var fakeService = ctx.GetOrganizationService();
                A.CallTo(() => fakeService.Create(A<Entity>.Ignored))
                    .ReturnsLazily(() => {
                        return Guid.NewGuid();
                    });
            });

            var context = builder.Build();
            var service = context.GetOrganizationService();

            var guid = service.Create(new Account());
            Assert.NotEqual(Guid.Empty, guid);
        }

        [Fact]
        public void Should_use_middleware_when_added_to_the_pipeline() 
        {
            var builder = MiddlewareBuilder.New();
            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {
                return (IXrmFakedContext ctx, OrganizationRequest request) => {
                    return new OrganizationResponse() 
                    {
                        ResponseName = "DummyResponse"
                    };
                };
            };
            builder.Use(middleware);

            var context = builder.Build();
            var service = context.GetOrganizationService();

            var response = service.Execute(new OrganizationRequest());
            Assert.Equal("DummyResponse", response.ResponseName);
        }

        [Fact]
        public void Should_shortcircuit_pipeline_when_middleware_requires() 
        {
            var builder = MiddlewareBuilder.New();
            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {
                return (IXrmFakedContext ctx, OrganizationRequest request) => {
                    if(request.RequestName.Equals("DummyRequest")) 
                    {
                        return new OrganizationResponse() 
                        {
                            ResponseName = "DummyResponse"
                        };
                    }
                    else
                    {
                        return next.Invoke(ctx, request);
                    }
                    
                };
            };
            builder.Use(middleware);

            var context = builder.Build();
            var service = context.GetOrganizationService();

            var response = service.Execute(new OrganizationRequest("DummyRequest"));
            Assert.Equal("DummyResponse", response.ResponseName);

            Assert.Throws<PullRequestException>(() => service.Execute(new OrganizationRequest("UnknownRequest")));
        }
    }
}
