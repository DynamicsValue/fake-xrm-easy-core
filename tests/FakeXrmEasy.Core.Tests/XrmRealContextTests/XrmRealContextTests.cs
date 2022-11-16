using System;
using Xunit;

namespace FakeXrmEasy.Tests.XrmRealContextTests
{
    public class XrmRealContextTests: FakeXrmEasyTestsBase
    {
        private class CustomProperty
        {

        }

        [Fact]
        public void Should_connect_to_CRM_with_given_ConnectionString()
        {
            var ctx = new XrmRealContext("myfirstconnectionstring");
            Assert.Equal("myfirstconnectionstring", ctx.ConnectionStringName);
        }

        [Fact]
        public void Should_return_service_that_was_injected_in_the_constructor()
        {
            var ctx = new XrmRealContext(_service);
            Assert.Equal(_service, ctx.GetOrganizationService());
        }

        [Fact]
        public void Should_set_property()
        {
            var ctx = new XrmRealContext(_service);
            var customProperty = new CustomProperty();
            ctx.SetProperty(customProperty);

            Assert.True(ctx.HasProperty<CustomProperty>());

            var property = ctx.GetProperty<CustomProperty>();
            Assert.Equal(customProperty, property);
        }

        [Fact]
        public void Should_throw_type_access_exception_if_property_was_not_found()
        {
            var ctx = new XrmRealContext(_service);
            Assert.Throws<TypeAccessException>(() => ctx.GetProperty<CustomProperty>());
        }

        [Fact]
        public void Should_update_property_if_it_was_set()
        {
            var ctx = new XrmRealContext(_service);
            var customProperty = new CustomProperty();
            ctx.SetProperty(customProperty);

            var newProperty = new CustomProperty();
            ctx.SetProperty(newProperty);

            var property = ctx.GetProperty<CustomProperty>();
            Assert.Equal(newProperty, property);
        }

        [Fact]
        public void Should_return_fake_tracing_service()
        {
            var ctx = new XrmRealContext(_service);
            var tracingService = ctx.GetTracingService();
            Assert.IsType<XrmFakedTracingService>(tracingService);

        }
    }
}