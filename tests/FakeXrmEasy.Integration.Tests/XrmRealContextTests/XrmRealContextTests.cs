using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Exceptions;
using System;
using Xunit;

namespace FakeXrmEasy.Integration.Tests.XrmRealContextTests
{
    public class XrmRealContextTests: FakeXrmEasyTestsBase
    {
        private readonly XrmRealContext _realContext;

        public XrmRealContextTests() : base()
        {
            _realContext = new XrmRealContext(_service);
            _realContext.LicenseContext = FakeXrmEasyLicense.RPL_1_5;
        }

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
            Assert.Equal(_service, _realContext.GetOrganizationService());
        }

        [Fact]
        public void Should_set_property()
        {
            var customProperty = new CustomProperty();
            _realContext.SetProperty(customProperty);

            Assert.True(_realContext.HasProperty<CustomProperty>());

            var property = _realContext.GetProperty<CustomProperty>();
            Assert.Equal(customProperty, property);
        }

        [Fact]
        public void Should_throw_type_access_exception_if_property_was_not_found()
        {
            Assert.Throws<TypeAccessException>(() => _realContext.GetProperty<CustomProperty>());
        }

        [Fact]
        public void Should_update_property_if_it_was_set()
        {
            var customProperty = new CustomProperty();
            _realContext.SetProperty(customProperty);

            var newProperty = new CustomProperty();
            _realContext.SetProperty(newProperty);

            var property = _realContext.GetProperty<CustomProperty>();
            Assert.Equal(newProperty, property);
        }

        [Fact]
        public void Should_return_fake_tracing_service()
        {
            var tracingService = _realContext.GetTracingService();
            Assert.IsType<XrmFakedTracingService>(tracingService);
        }

        [Fact]
        public void Should_set_default_caller_properties()
        {
            Assert.NotNull(_realContext.CallerProperties);
        }

        [Fact]
        public void Should_return_license_exception_if_not_set_when_getting_an_organization_service()
        {
            var ctx = new XrmRealContext(_service);
            Assert.Throws<LicenseException>(() => ctx.GetOrganizationService());
        }
    }
}
