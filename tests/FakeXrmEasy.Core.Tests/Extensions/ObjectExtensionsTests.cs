using System;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void Should_throw_exception_when_setting_an_unknown_field()
        {
            var metadata = new EntityMetadata();
            Assert.Throws<ArgumentOutOfRangeException>(() => metadata.SetFieldValue("non-existing", 1));
        }
        
        [Fact]
        public void Should_throw_exception_when_getting_an_unknown_field()
        {
            var metadata = new EntityMetadata();
            Assert.Throws<ArgumentOutOfRangeException>(() => metadata.GetFieldValue("non-existing"));
        }
     
        [Fact]
        public void Should_set_entity_metadata_field_value()
        {
            var metadata = new EntityMetadata();
            metadata.SetFieldValue("_objectTypeCode", 1);
            
            Assert.Equal(1, metadata.ObjectTypeCode);
        }
        
        [Fact]
        public void Should_get_entity_metadata_field_value()
        {
            var metadata = new EntityMetadata();
            metadata.SetFieldValue("_objectTypeCode", 1);
            
            var objectTypeCode = metadata.GetFieldValue("_objectTypeCode");
            Assert.Equal(1, objectTypeCode);
        }
    }
}