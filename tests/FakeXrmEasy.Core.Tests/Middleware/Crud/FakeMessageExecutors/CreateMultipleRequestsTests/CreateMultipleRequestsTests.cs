#if FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Reflection;
using DataverseEntities;
using Xunit;
using Account = Crm.Account;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.CreateMultipleRequestTests
{
    public class CreateMultipleRequestTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_throw_exception_if_targets_was_not_set()
        {
            var request = new CreateMultipleRequest();
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal("Required field 'Targets' is missing", ex.Detail.Message);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_throw_exception_if_create_multiple_is_called_with_null_entity_name(string entityLogicalName)
        {
            List<Entity> recordsToCreate = new List<Entity>();
            
            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = entityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal("Required member 'EntityName' missing for field 'Targets'", ex.Detail.Message);
        }

        [Theory]
        [InlineData("asdasdasd")]
        public void Should_throw_exception_if_create_multiple_is_called_with_invalid_entity_name_and_early_bound_types_are_used(string entityLogicalName)
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));
            
            List<Entity> recordsToCreate = new List<Entity>();
            
            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = entityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoEntity, () => _service.Execute(request));
            Assert.StartsWith($"The entity with a name = '{entityLogicalName}' with namemapping = 'Logical' was not found in the MetadataCache.", ex.Detail.Message);
        }

        [Theory]
        [InlineData("asdasdasd")]
        public void Should_throw_exception_if_create_multiple_is_called_with_invalid_entity_name_and_metadata_is_used(string entityLogicalName)
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(Account)));
            
            List<Entity> recordsToCreate = new List<Entity>();
            
            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = entityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoEntity, () => _service.Execute(request));
            Assert.StartsWith($"The entity with a name = '{entityLogicalName}' with namemapping = 'Logical' was not found in the MetadataCache.", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_throw_exception_if_create_multiple_is_called_with_an_empty_list()
        {
            List<Entity> recordsToCreate = new List<Entity>();

            // Create an EntityCollection populated with the list of entities.
            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.UnExpected, () => _service.Execute(request));
            Assert.Equal("System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_throw_exception_if_create_multiple_is_called_with_an_existing_entity_record()
        {
            var id = _service.Create(new dv_test());

            List<Entity> recordsToCreate = new List<Entity>() { new dv_test() { Id = id } };

            // Create an EntityCollection populated with the list of entities.
            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.DuplicateRecord, () => _service.Execute(request));
            Assert.Equal("Cannot insert duplicate key.", ex.Detail.Message);
        }
    }
}
#endif