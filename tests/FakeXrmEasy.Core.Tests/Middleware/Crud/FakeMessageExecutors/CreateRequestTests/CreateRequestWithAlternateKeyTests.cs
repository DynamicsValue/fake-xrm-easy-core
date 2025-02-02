#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013
using System;
using System.Linq;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.CreateRequestTests
{
    public class CreateRequestWithAlternateKeyTests: FakeXrmEasyAlternateKeyTestsBase
    {
        private const string KEY = "C0001";
        private readonly Entity _record;

        public CreateRequestWithAlternateKeyTests(): base()
        {
            _record = new dv_test()
            {
                Id = Guid.NewGuid(),
                dv_code = KEY
            };
        }
        [Fact]
        public void Should_create_test_entity_record_and_ignore_alternate_key_if_no_key_attributes_are_set()
        {
            _context.Initialize(_record);
            
            var test = new dv_test()
            {
                dv_string = "Test Create"
            };

            var createdGuid = _service.Create(test);

            var createdRecord = _service.Retrieve(dv_test.EntityLogicalName, createdGuid, new ColumnSet(true));

            Assert.NotNull(createdRecord);
        }

        [Fact]
        public void Should_throw_duplicate_record_sql_exception_if_alternate_key_already_exists_when_using_a_key_attribute()
        {
            _context.Initialize(_record);
            
            var test = new dv_test()
            {
                dv_string = "Test Create",
                KeyAttributes = new KeyAttributeCollection()
                {
                    { "dv_code", KEY }
                }
            };

            var ex = XAssert.ThrowsFaultCode(Abstractions.ErrorCodes.DuplicateRecord, () => _service.Create(test));
            Assert.Contains("Cannot insert duplicate key", ex.Message);
        }

        [Fact]
        public void Should_throw_duplicate_record_entity_key_exception_if_alternate_key_already_exists_without_using_a_key_attribute()
        {
            _context.Initialize(_record);
            
            var test = new dv_test()
            {
                dv_string = "Test Create",
                dv_code = KEY
            };

            var ex = XAssert.ThrowsFaultCode(Abstractions.ErrorCodes.DuplicateRecordEntityKey, () => _service.Create(test));
            Assert.Contains("A record that has the attribute values Code already exists. The entity key Code Key requires that this set of attributes contains unique values. Select unique values and try again.", ex.Message);
        }

        [Fact]
        public void Should_throw_exception_if_key_attributes_are_used_despite_not_duplicate()
        {
            var key = Guid.NewGuid().ToString();

            var test = new dv_test()
            {
                dv_string = "Test Create",
                KeyAttributes = new KeyAttributeCollection()
                {
                    { "dv_code", key }
                }
            };

            var ex = XAssert.ThrowsFaultCode(Abstractions.ErrorCodes.RecordNotFoundByEntityKey, () => _service.Create(test));
            Assert.Contains("A record with the specified key values does not exist in dv_test entity", ex.Message);
        }

        [Fact]
        public void Should_create_test_entity_record_with_a_non_existing_key()
        {
            var key = Guid.NewGuid().ToString();

            var test = new dv_test()
            {
                dv_code = key,
                dv_string = "Test Create",
            };

            var createdGuid = _service.Create(test);

            var createdRecord = _service.Retrieve(dv_test.EntityLogicalName, createdGuid, new ColumnSet(true)).ToEntity<dv_test>();

            Assert.NotNull(createdRecord);
            Assert.Equal(key, createdRecord.dv_code);
        }
    }
}
#endif