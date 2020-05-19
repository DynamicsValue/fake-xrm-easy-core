using Xunit;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using FakeXrmEasy.Extensions;
using System;

namespace FakeXrmEasy.Tests.FakeContextTests.RetrieveAttributeRequestTests
{
    public class RetrieveAttributeTests: FakeXrmEasyTests
    {
        [Fact]
        public void When_retrieve_attribute_request_is_called_correctly_attribute_is_returned()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            var nameAttribute = new StringAttributeMetadata()
            {
                LogicalName = "name",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
            };
            entityMetadata.SetAttributeCollection(new[] { nameAttribute });

            _context.InitializeMetadata(entityMetadata);

            RetrieveAttributeRequest req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = "account",
                LogicalName = "name"
            };

            var response = _service.Execute(req) as RetrieveAttributeResponse;
            Assert.NotNull(response.AttributeMetadata);
            Assert.Equal(AttributeRequiredLevel.ApplicationRequired, response.AttributeMetadata.RequiredLevel.Value);
            Assert.Equal("name", response.AttributeMetadata.LogicalName);
        }

        [Fact]
        public void When_retrieve_attribute_request_is_without_entity_logical_name_exception_is_raised()
        {
            RetrieveAttributeRequest req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = null,
                LogicalName = "name"
            };

            Assert.Throws<Exception>(() => _service.Execute(req));
        }

        [Fact]
        public void When_retrieve_attribute_request_is_without_logical_name_exception_is_raised()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            _context.InitializeMetadata(entityMetadata);

            RetrieveAttributeRequest req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = "account",
                LogicalName = null
            };

            Assert.Throws<Exception>(() => _service.Execute(req));
        }

        [Fact]
        public void When_retrieve_attribute_request_is_without_being_initialised_exception_is_raised()
        {
            
            

            RetrieveAttributeRequest req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = "account",
                LogicalName = "name"
            };

            Assert.Throws<Exception>(() => _service.Execute(req));
        }

        [Fact]
        public void When_retrieve_attribute_request_is_initialised_but_attribute_doesnt_exists_exception_is_raised()
        {
            
            

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            _context.InitializeMetadata(entityMetadata);

            RetrieveAttributeRequest req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = "account",
                LogicalName = "name"
            };

            Assert.Throws<Exception>(() => _service.Execute(req));
        }


    }
}
