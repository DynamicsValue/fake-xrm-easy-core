using System;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class EntityReferenceExtensionsTests
    {
        [Fact]
        public void Should_clone_entity_reference()
        {
            var entityRef = new EntityReference()
            {
                Id = Guid.NewGuid(),
                LogicalName = "account"
            };

            var clone = entityRef.Clone();
            Assert.NotSame(entityRef, clone);
            Assert.Equal(entityRef, clone);
        }
        
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
        [Fact]
        public void Should_clone_entity_reference_with_key_attributes()
        {
            var entityRef = new EntityReference()
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                KeyAttributes = new KeyAttributeCollection()
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                }
            };

            var clone = entityRef.Clone();
            Assert.NotSame(entityRef, clone);
            Assert.Equal(entityRef, clone);
        }
#endif
    }
}