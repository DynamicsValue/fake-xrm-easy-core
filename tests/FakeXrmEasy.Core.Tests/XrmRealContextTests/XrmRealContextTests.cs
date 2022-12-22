using FakeXrmEasy.Abstractions.Enums;
using Microsoft.PowerPlatform.Dataverse.Client;
using FakeXrmEasy.Abstractions.Exceptions;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.XrmRealContextTests
{
    public class XrmRealContextTests: FakeXrmEasyTestsBase
    {
        private readonly XrmRealContext _realContext;
        private readonly IOrganizationServiceAsync _serviceAsync;
        private readonly IOrganizationServiceAsync2 _serviceAsync2;

        public XrmRealContextTests() : base()
        {
            _realContext = new XrmRealContext(_service);
            _serviceAsync = _context.GetAsyncOrganizationService();
            _serviceAsync2 = _context.GetAsyncOrganizationService2();
            _realContext.LicenseContext = FakeXrmEasyLicense.RPL_1_5;
        }

        private class CustomProperty
        {

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
        public void Should_return_license_exception_if_not_set_when_getting_an_organization_service()
        {
            var ctx = new XrmRealContext(_service);
            Assert.Throws<LicenseException>(() => ctx.GetOrganizationService());
        }

        [Fact]
        public void Should_retrieve_fake_organization_service() 
        {           
            var ctx = new XrmRealContext(_service, _serviceAsync, _serviceAsync2);
            ctx.LicenseContext = FakeXrmEasyLicense.RPL_1_5;

            Assert.Equal(_service, ctx.GetOrganizationService());
            Assert.Equal(_serviceAsync, ctx.GetAsyncOrganizationService());
            Assert.Equal(_serviceAsync2, ctx.GetAsyncOrganizationService2());
        }

        [Fact]
        public void Should_return_false_if_context_doesnt_have_a_property()
        {
            var ctx = new XrmRealContext(_service, _serviceAsync, _serviceAsync2);
            Assert.False(ctx.HasProperty<CustomProperty>());
        }

        [Fact]
        public void Should_return_true_if_context_does_have_a_property()
        {
            var ctx = new XrmRealContext(_service, _serviceAsync, _serviceAsync2);
            ctx.SetProperty<CustomProperty>(new CustomProperty());
            Assert.True(ctx.HasProperty<CustomProperty>());
        }

        [Fact]
        public void Should_retrieve_property_that_was_previously_set()
        {
            var ctx = new XrmRealContext(_service, _serviceAsync, _serviceAsync2);
            
            var prop = new CustomProperty();
            ctx.SetProperty<CustomProperty>(prop);

            var retrieved = ctx.GetProperty<CustomProperty>(); 
            Assert.Equal(prop, retrieved);
        }
    }
}