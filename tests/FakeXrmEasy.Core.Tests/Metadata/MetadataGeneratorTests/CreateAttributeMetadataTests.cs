using System;
using DataverseEntities;
using FakeXrmEasy.Metadata;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Metadata
{
    public class CreateAttributeMetadataTests
    {
        private readonly Type[] _typesTestType;
        public CreateAttributeMetadataTests()
        {
            _typesTestType = new Type[] { typeof(dv_test) };
        }
        
        [Fact]
        public void Should_generate_file_type()
        {
            var attributeMetadata = MetadataGenerator.CreateAttributeMetadata(typeof(object));
            Assert.NotNull(attributeMetadata);
            Assert.IsType<FileAttributeMetadata>(attributeMetadata);
        }
    }
}