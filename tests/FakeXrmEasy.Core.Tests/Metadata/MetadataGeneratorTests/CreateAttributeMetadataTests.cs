using System;
using DataverseEntities;
using FakeXrmEasy.Core.Exceptions.Metadata;
using FakeXrmEasy.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Metadata.MetadataGeneratorTests
{
    public class CreateAttributeMetadataTests: FakeXrmEasyTestsBase
    {
        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_generate_file_type()
        {
            var attributeMetadata = MetadataGenerator.CreateAttributeMetadata(dv_test.EntityLogicalName, "dv_file", typeof(object), _context);
            Assert.NotNull(attributeMetadata);
            Assert.IsType<FileAttributeMetadata>(attributeMetadata);
        }
        #endif
        
        [Fact]
        public void Should_throw_attribute_metadata_could_not_be_mapped()
        {
            Assert.Throws<AttributeMetadataGenerationException>(() => MetadataGenerator.CreateAttributeMetadata(DummyEntity.EntityLogicalName, "dummy", typeof(UnknownAttributeType), _context));
        }
    }
}