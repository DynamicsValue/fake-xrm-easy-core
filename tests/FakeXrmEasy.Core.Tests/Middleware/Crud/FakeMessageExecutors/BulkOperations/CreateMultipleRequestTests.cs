#if FAKE_XRM_EASY_9
using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Integrity;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using Account = Crm.Account;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.BulkOperations
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
        
        [Fact]
        public void Should_throw_exception_if_create_multiple_is_called_with_an_entity_record_with_a_logical_name_different_than_the_main_logical_name()
        {
            List<Entity> recordsToCreate = new List<Entity>() { new dv_test() };

            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = Account.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal($"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {Account.EntityLogicalName} while this entity has Platform Name: {dv_test.EntityLogicalName}", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_throw_exception_if_create_multiple_is_called_with_non_existing_related_entity_and_integrity_options_is_enabled()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToCreate = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName, nonExistingGuid)
                }
            };

            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"account With Ids = {nonExistingGuid.ToString()} Do Not Exist", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_throw_exception_if_create_multiple_is_called_with_a_non_existing_alternate_key()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var dummy_attribute_name = "dv_dummy_attribute";

            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToCreate = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName, dummy_attribute_name, "Microsoft")
                }
            };

            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name}", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_create_two_records_with_create_multiple()
        {
            var record1 = new dv_test() { Id = Guid.NewGuid() };
            var record2 = new dv_test() { Id = Guid.NewGuid() };

            List<Entity> recordsToCreate = new List<Entity>() { record1, record2 };

            var entities = new EntityCollection(recordsToCreate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new CreateMultipleRequest()
            {
                Targets = entities
            };

            var response = _service.Execute(request) as CreateMultipleResponse;

            Assert.Equal(2, response.Ids.Length);
            Assert.Equal(record1.Id, response.Ids[0]);
            Assert.Equal(record2.Id, response.Ids[1]);

            var createdRecord1 = _service.Retrieve(dv_test.EntityLogicalName, record1.Id, new ColumnSet(true));
            var createdRecord2 = _service.Retrieve(dv_test.EntityLogicalName, record2.Id, new ColumnSet(true));

            Assert.NotNull(createdRecord1);
            Assert.NotNull(createdRecord2);
        }
    }
}
#endif