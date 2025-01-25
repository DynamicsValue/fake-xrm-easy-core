#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Integrity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.BulkOperations
{
    public class UpdateMultipleRequestTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_throw_exception_if_targets_was_not_set()
        {
            var request = new UpdateMultipleRequest();

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal("Required field 'Targets' is missing", ex.Detail.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_throw_exception_if_update_multiple_is_called_with_null_entity_name(string entityLogicalName)
        {
            List<Entity> recordsToUpdate = new List<Entity>();

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = entityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal("Required member 'EntityName' missing for field 'Targets'", ex.Detail.Message);
        }

        [Theory]
        [InlineData("asdasdasd")]
        public void Should_throw_exception_if_update_multiple_is_called_with_invalid_entity_name_and_early_bound_types_are_used(string entityLogicalName)
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));
            
            List<Entity> recordsToUpdate = new List<Entity>();

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = entityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoEntity, () => _service.Execute(request));
            Assert.StartsWith($"The entity with a name = '{entityLogicalName}' with namemapping = 'Logical' was not found in the MetadataCache.", ex.Detail.Message);
        }
        
        [Theory]
        [InlineData("asdasdasd")]
        public void Should_throw_exception_if_update_multiple_is_called_with_invalid_entity_name_and_metadata_is_used(string entityLogicalName)
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            List<Entity> recordsToUpdate = new List<Entity>();

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = entityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoEntity, () => _service.Execute(request));
            Assert.StartsWith($"The entity with a name = '{entityLogicalName}' with namemapping = 'Logical' was not found in the MetadataCache.", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_an_empty_list()
        {
            List<Entity> recordsToUpdate = new List<Entity>();

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.UnExpected, () => _service.Execute(request));
            Assert.Equal("System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_a_non_existing_entity_record()
        {
            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpdate = new List<Entity>() { new dv_test() { Id = nonExistingGuid } };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"{dv_test.EntityLogicalName} With Ids = {nonExistingGuid} Do Not Exist", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_without_entity_id()
        {
            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpdate = new List<Entity>() { new dv_test() };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"Entity Id must be specified for Operation", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_an_entity_record_with_a_logical_name_different_than_the_main_logical_name()
        {
            var guid1 = _service.Create(new dv_test());

            List<Entity> recordsToUpdate = new List<Entity>() 
            {
                new dv_test() { Id = guid1 }
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = Account.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoAttribute, () => _service.Execute(request));
            Assert.Equal($"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {Account.EntityLogicalName} while this entity has Platform Name: {dv_test.EntityLogicalName}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_non_existing_related_entity()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var nonExistingGuid = Guid.NewGuid();

            var guid1 = _service.Create(new dv_test());

            List<Entity> recordsToUpdate = new List<Entity>() {
                new dv_test()
                {
                    Id = guid1,
                    dv_accountid = new EntityReference(Account.EntityLogicalName, nonExistingGuid)
                }
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"account With Ids = {nonExistingGuid} Do Not Exist", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_non_existing_related_entity_by_alternate_key()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var guid1 = _service.Create(new dv_test());

            var nonExistingGuid = Guid.NewGuid();
            var missingAttribute = "dv_key_number";

            List<Entity> recordsToUpdate = new List<Entity>() {
                new dv_test()
                {
                    Id = guid1,
                    dv_accountid = new EntityReference(Account.EntityLogicalName, missingAttribute, "Missing number")
                }
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {missingAttribute}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_a_non_existing_single_alternate_key()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var guid1 = _service.Create(new dv_test());

            var dummy_attribute_name = "dv_dummy_attribute";

            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpdate = new List<Entity>() {
                new dv_test()
                {
                    Id = guid1,
                    dv_accountid = new EntityReference(Account.EntityLogicalName, dummy_attribute_name, "Microsoft")
                }
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };
            
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_update_multiple_is_called_with_a_non_existing_multiple_alternate_key()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var guid1 = _service.Create(new dv_test());

            var dummy_attribute_name1 = "dv_dummy_attribute1";
            var dummy_attribute_name2 = "dv_dummy_attribute2";

            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpdate = new List<Entity>() {
                new dv_test()
                {
                    Id = guid1,
                    dv_accountid = new EntityReference(Account.EntityLogicalName,
                    new KeyAttributeCollection 
                    {
                        { dummy_attribute_name2, "value2" },
                        { dummy_attribute_name1, "value1" },
                    })
                }
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name2}", ex.Detail.Message);
        }

        [Fact]
        public void Should_update_two_records_in_update_multiple()
        {
            var guid1 = _service.Create(new dv_test());
            var guid2 = _service.Create(new dv_test());

            List<Entity> recordsToUpdate = new List<Entity>() 
            { 
                new dv_test()
                {
                    Id = guid1,
                    dv_string = "Record 1"
                }, 
                new dv_test() {
                    Id = guid2,
                    dv_string = "Record 2"
                } 
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var response = _service.Execute(request) as UpdateMultipleResponse;

            var updatedRecord1 = _service.Retrieve(dv_test.EntityLogicalName, guid1, new ColumnSet(true));
            var updatedRecord2 = _service.Retrieve(dv_test.EntityLogicalName, guid2, new ColumnSet(true));

            Assert.NotNull(updatedRecord1);
            Assert.NotNull(updatedRecord2);

            Assert.Equal("Record 1", updatedRecord1["dv_string"]);
            Assert.Equal("Record 2", updatedRecord2["dv_string"]);
        }
        
        [Fact]
        public void Should_throw_exception_in_update_multiple_when_using_an_alternate_key_without_a_primary_key()
        {
            var guid1 = _service.Create(new dv_test(){ dv_code = "C0001"});
            var guid2 = _service.Create(new dv_test() { dv_code = "C0002"});

            List<Entity> recordsToUpdate = new List<Entity>() 
            { 
                new dv_test()
                {
                    dv_code = "C0001",
                    dv_string = "Record 1"
                }, 
                new dv_test() {
                    dv_code = "C0002",
                    dv_string = "Record 2"
                } 
            };

            var entities = new EntityCollection(recordsToUpdate)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpdateMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"Entity Id must be specified for Operation", ex.Detail.Message);
        }
    }
}
#endif