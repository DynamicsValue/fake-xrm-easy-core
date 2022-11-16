using Crm;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Xunit;

#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using Xunit.Sdk;
#endif

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
            Assert.Equal(property, customProperty);
        }
    }
}