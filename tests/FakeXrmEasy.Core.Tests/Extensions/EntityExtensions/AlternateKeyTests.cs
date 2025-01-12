using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class AlternateKeyTests
    {
        private const string KEY = "dv_code";
        private readonly EntityKeyMetadata _keyMetadata;

        public AlternateKeyTests()
        {
            _keyMetadata = new EntityKeyMetadata()
            {
                KeyAttributes = new [] { KEY }
            };
        }
        
        [Fact]
        public void Should_return_null_if_it_doesnt_have_any_key_values()
        {
            var e = new Entity("account");
            e["name"] = "Some name";

            var keyAttributes = e.ToAlternateKeyAttributeCollection(_keyMetadata);
            Assert.Null(keyAttributes);
        }
        
        [Fact]
        public void Should_return_key_attributes_if_it_does_have_key_values()
        {
            var e = new dv_test();
            e.dv_code = "C00001";

            var keyAttributes = e.ToAlternateKeyAttributeCollection(_keyMetadata);
            Assert.NotNull(keyAttributes);

            Assert.Single(keyAttributes.Keys);
            Assert.Equal(KEY, keyAttributes.Keys.First());
            Assert.Equal(e.dv_code, keyAttributes.Values.First());
        }
    }
}