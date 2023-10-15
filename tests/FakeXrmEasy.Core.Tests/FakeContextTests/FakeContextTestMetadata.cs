using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class FakeContextTestMetadata : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_throw_exception_if_null_was_used_to_initialise()
        {
            Assert.Throws<Exception>(() => _context.InitializeMetadata(entityMetadataList: null));
        }

        [Fact]
        public void Should_throw_exception_if_logical_name_is_null_during_initialisation()
        {
            var entityMetadata = new EntityMetadata()
            {

            };
            Assert.Throws<Exception>(() =>
                _context.InitializeMetadata(new List<EntityMetadata>() {
                    entityMetadata
                }));
        }

        [Fact]
        public void Should_throw_exception_if_logical_name_is_empty_during_initialisation()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = ""
            };
            Assert.Throws<Exception>(() =>
                _context.InitializeMetadata(new List<EntityMetadata>() {
                    entityMetadata
                }));
        }

        [Fact]
        public void Should_throw_exception_if_entity_name_is_duplicated_during_initialisation()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            Assert.Throws<Exception>(() =>
                _context.InitializeMetadata(new List<EntityMetadata>() {
                    entityMetadata,
                    entityMetadata
                }));
        }

        [Fact]
        public void Should_contain_one_entity_metadata_after_initialisation()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            _context.InitializeMetadata(new List<EntityMetadata>() { entityMetadata });

            var metadatas = _context.CreateMetadataQuery().ToList();
            Assert.True(metadatas.Count == 1);
            Assert.Equal("account", metadatas[0].LogicalName);
        }

        [Fact]
        public void Should_store_a_clone_after_initialisation()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            _context.InitializeMetadata(new List<EntityMetadata>() { entityMetadata });

            var metadatas = _context.CreateMetadataQuery().ToList();
            Assert.True(metadatas[0] != entityMetadata);
        }

        [Fact]
        public void Should_return_a_clone_when_querying_entity_metadatas()
        {
            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };
            _context.InitializeMetadata(new List<EntityMetadata>() { entityMetadata });

            var metadata1 = _context.CreateMetadataQuery().FirstOrDefault();
            var metadata2 = _context.CreateMetadataQuery().FirstOrDefault();
            Assert.True(metadata1 != metadata2);
        }

        [Fact]
        public void Should_initialize_metadata_from_early_bound_assembly()
        {
            _context.InitializeMetadata(typeof(Crm.Account).Assembly);

            var accountMetadata = _context.CreateMetadataQuery().Where(x => x.LogicalName == "account").FirstOrDefault();

            Assert.NotNull(accountMetadata);

            var accountid = accountMetadata.Attributes.FirstOrDefault(x => x.LogicalName == "accountid");


            Assert.Equal("accountid", accountMetadata.PrimaryIdAttribute);
            Assert.NotNull(accountid);
            Assert.Equal(AttributeTypeCode.Uniqueidentifier, accountid.AttributeType);
        }
    }
}
