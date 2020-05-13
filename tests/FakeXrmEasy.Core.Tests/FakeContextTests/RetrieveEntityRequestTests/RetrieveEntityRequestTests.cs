using Crm;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;

namespace FakeXrmEasy.Tests.FakeContextTests.RetrieveEntityRequestTests
{
    public class RetrieveEntityRequestTests
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public RetrieveEntityRequestTests()
        {
            _context = XrmFakedContextFactory.New();
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_calling_retrieve_entity_without_proxy_types_assembly_exception_is_thrown()
        {
            var request = new RetrieveEntityRequest()
            {
                LogicalName = Account.EntityLogicalName
            };
            Assert.Throws<Exception>(() => _service.Execute(request));
        }

        [Fact]
        public void When_calling_retrieve_entity_with_a_null_or_empty_logicalname_exception_is_thrown()
        {
            var request = new RetrieveEntityRequest()
            {
                LogicalName = ""
            };
            Assert.Throws<Exception>(() => _service.Execute(request));
        }

        [Fact]
        public void When_calling_retrieve_entity_without_a_fake_entity_metadata_exception_is_thrown()
        {
            var request = new RetrieveEntityRequest()
            {
                LogicalName = Account.EntityLogicalName
            };
            Assert.Throws<Exception>(() => _service.Execute(request));
        }

        [Fact]
        public void When_calling_retrieve_entity_with_a_fake_entity_metadata_that_one_is_returned()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = Account.EntityLogicalName,
                IsCustomizable = new BooleanManagedProperty(true)
            };
            _context.InitializeMetadata(entityMetadata);

            var request = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = Account.EntityLogicalName
            };

            var response = _service.Execute(request);
            Assert.IsType<RetrieveEntityResponse>(response);
            Assert.True((response as RetrieveEntityResponse).EntityMetadata.IsCustomizable.Value);
        }

        [Fact]
        public void When_calling_retrieve_entity_with_a_fake_attribute_definition_it_is_returned()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = Account.EntityLogicalName,
                IsCustomizable = new BooleanManagedProperty(true)
            };
            var stringMetadata = new StringAttributeMetadata()
            {
                SchemaName = "name",
                MaxLength = 200, 
            };
            stringMetadata.SetSealedPropertyValue("IsValidForCreate", new Nullable<bool>(true));
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { stringMetadata });
            _context.InitializeMetadata(entityMetadata);

            var request = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = Account.EntityLogicalName
            };

            var response = _service.Execute(request);
            Assert.IsType<RetrieveEntityResponse>(response);

            var nameAttribute = (response as RetrieveEntityResponse).EntityMetadata.Attributes
                                .Where(a => a.SchemaName.Equals("name"))
                                .FirstOrDefault();

            Assert.True(nameAttribute.IsValidForCreate.Value);
        }
    }
}