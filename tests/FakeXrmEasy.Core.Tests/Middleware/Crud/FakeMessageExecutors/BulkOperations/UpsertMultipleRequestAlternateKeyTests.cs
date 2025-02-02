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
    public class UpsertMultipleRequestAlternateKeyTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_non_existing_related_entity_by_alternate_key()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var nonExistingGuid = Guid.NewGuid();
            var missingAttribute = "dv_key_number";

            List<Entity> recordsToUpsert = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName, missingAttribute, "Missing number")
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

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {missingAttribute}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_a_non_existing_single_alternate_key()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var dummy_attribute_name = "dv_dummy_attribute";

            var nonExistingGuid = Guid.NewGuid();

            List<Entity> recordsToUpsert = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName, dummy_attribute_name, "Microsoft")
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

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name}", ex.Detail.Message);
        }

        [Fact]
        public void Should_throw_exception_if_upsert_multiple_is_called_with_a_non_existing_multiple_alternate_key()
        {
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));
            _context.SetProperty<IIntegrityOptions>(new IntegrityOptions());
            
            var dummy_attribute_name1 = "dv_dummy_attribute1";
            var dummy_attribute_name2 = "dv_dummy_attribute2";

            List<Entity> recordsToUpsert = new List<Entity>() {
                new dv_test()
                {
                    dv_accountid = new EntityReference(Account.EntityLogicalName,
                    new KeyAttributeCollection 
                    {
                        { dummy_attribute_name2, "value2" },
                        { dummy_attribute_name1, "value1" },
                    })
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

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _service.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name2}", ex.Detail.Message);
        }
        
        [Fact]
        public void Should_upsert_two_records_in_upsert_multiple_with_alternate_keys_and_explicit_key_attributes()
        {
            var assembly = Assembly.GetAssembly(typeof(dv_test));
            _context.EnableProxyTypes(assembly);
            _context.InitializeMetadata(assembly);
            
            var metadata = _context.GetEntityMetadataByName("dv_test");
            metadata.SetFieldValue("_keys", new EntityKeyMetadata[]
            {
                new EntityKeyMetadata()
                {
                    KeyAttributes = new string[]{"dv_code"}
                }
            });
            _context.SetEntityMetadata(metadata);
            
            var guid1 = _service.Create(new dv_test(){ dv_code = "C0001"});

            List<Entity> recordsToUpdate = new List<Entity>() 
            { 
                new dv_test()
                {
                    dv_string = "Record 1",
                    KeyAttributes = new KeyAttributeCollection()
                    {
                        {"dv_code", "C0001"}
                    }
                }, 
                new dv_test() {
                    dv_string = "Record 2",
                    KeyAttributes = new KeyAttributeCollection()
                    {
                        {"dv_code", "C0002"}
                    }
                } 
            };

            var entities = new EntityCollection(recordsToUpdate)
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