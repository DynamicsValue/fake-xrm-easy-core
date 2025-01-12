#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Integrity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.BulkOperations
{
    public class UpsertMultipleRequestTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_throw_exception_if_targets_was_not_set()
        {
            var request = new UpsertMultipleRequest();

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal("'Targets' should be one of the parameters for UpsertMultiple.", ex.Detail.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_null_entity_name(string entityLogicalName)
        {
            List<Entity> recordsToUpsert = new List<Entity>();

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = entityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.UnExpected, () => _service.Execute(request));
            Assert.Equal("System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.", ex.Detail.Message);
        }

        [Theory]
        [InlineData("asdasdasd")]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_invalid_entity_name(string entityLogicalName)
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            List<Entity> recordsToUpsert = new List<Entity>();

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = entityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderNoEntity, () => _service.Execute(request));
            Assert.StartsWith($"The entity with a name = '{entityLogicalName}' with namemapping = 'Logical' was not found in the MetadataCache.", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_an_empty_list()
        {
            List<Entity> recordsToUpsert = new List<Entity>();

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.UnExpected, () => _service.Execute(request));
            Assert.Equal("System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.", ex.Detail.Message);
        }

        [Fact]
        public void Should_create_record_if_upsert_multiple_is_called_with_a_non_existing_entity_record()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpsert = new List<Entity>() { new dv_test() { Id = nonExistingGuid } };

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var response = _service.Execute(request) as UpsertMultipleResponse;

            Assert.Single(response.Results);

            var upsertResponse = response.Results[0];
            var createdRecordId = upsertResponse.Target.Id;
            Assert.Equal(nonExistingGuid, createdRecordId);

            var entity = _service.Retrieve(dv_test.EntityLogicalName, createdRecordId, new ColumnSet(true));
            Assert.NotNull(entity);
        }

        [Fact]
        public void Should_create_record_if_upsert_multiple_is_called_without_entity_id()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            List<Entity> recordsToUpsert = new List<Entity>() { new dv_test() };

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var response = _service.Execute(request) as UpsertMultipleResponse;

            Assert.Single(response.Results);

            var upsertResponse = response.Results[0];
            var createdRecordId = upsertResponse.Target.Id;

            var entity = _service.Retrieve(dv_test.EntityLogicalName, createdRecordId, new ColumnSet(true));
            Assert.NotNull(entity);
        }

        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_an_entity_record_with_a_logical_name_different_than_the_main_logical_name()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            List<Entity> recordsToUpsert = new List<Entity>() 
            {
                new dv_test()
            };

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = Account.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.Execute(request));
            Assert.Equal($"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = account while this entity has Platform Name: {dv_test.EntityLogicalName}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_non_existing_related_entity()
        {
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            
            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpsert = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName, nonExistingGuid)
                }
            };

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.ObjectDoesNotExist, () => _service.Execute(request));
            Assert.Equal($"account With Ids = {nonExistingGuid} Do Not Exist", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_upsert_two_records_in_upsert_multiple()
        {
            var guid1 = _service.Create(new dv_test());

            List<Entity> recordsToUpsert = new List<Entity>() 
            { 
                new dv_test()
                {
                    Id = guid1,
                    dv_string = "Record 1"
                }, 
                new dv_test() {
                    dv_string = "Record 2"
                } 
            };

            var entities = new EntityCollection(recordsToUpsert)
            {
                EntityName = dv_test.EntityLogicalName
            };

            var request = new UpsertMultipleRequest()
            {
                Targets = entities
            };

            var response = _service.Execute(request) as UpsertMultipleResponse;
            var upsertResponse1 = response.Results[0];
            var upsertResponse2 = response.Results[1];

            Assert.Equal(guid1, upsertResponse1.Target.Id);

            var updatedRecord = _service.Retrieve(dv_test.EntityLogicalName, guid1, new ColumnSet(true));
            var createdRecord = _service.Retrieve(dv_test.EntityLogicalName, upsertResponse2.Target.Id, new ColumnSet(true));

            Assert.NotNull(updatedRecord);
            Assert.NotNull(createdRecord);

            Assert.Equal("Record 1", updatedRecord["dv_string"]);
            Assert.Equal("Record 2", createdRecord["dv_string"]);

        }
        
        
    }
}
#endif