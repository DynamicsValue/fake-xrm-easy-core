using System;
using System.Linq;
using DataverseEntities;
using DataverseEntities2;
using FakeXrmEasy.Core.Exceptions.Metadata;
using FakeXrmEasy.Metadata;
using Xunit;
using Account = Crm.Account;

namespace FakeXrmEasy.Core.Tests.Metadata.MetadataGeneratorTests
{
    public class MetadataGeneratorTests: FakeXrmEasyTestsBase
    {
        private readonly Type[] _typesWithAccountType;
        public MetadataGeneratorTests()
        {
            _typesWithAccountType = new Type[] { typeof(Account) };
        }

        [Fact]
        public void Should_return_one_metadata_from_one_early_bound_type()
        {
            var metadatas = MetadataGenerator.FromTypes(_typesWithAccountType, _context);
            Assert.Single(metadatas);
        }

        [Fact]
        public void Should_set_primary_id_attribute()
        {
            var accountMetadata = MetadataGenerator.FromTypes(_typesWithAccountType, _context).First();
            Assert.Equal("accountid", accountMetadata.PrimaryIdAttribute);
        }

        [Fact]
        public void Should_set_entity_type_code()
        {
            var accountMetadata = MetadataGenerator.FromTypes(_typesWithAccountType, _context).First();
            Assert.Equal(Account.EntityTypeCode, accountMetadata.ObjectTypeCode);
        }

        #if !FAKE_XRM_EASY_9
        [Fact]
        public void Should_throw_exception_with_attribute_name_details_when_an_attribute_can_not_generated()
        {
            var ex = Assert.Throws<AttributeMetadataGenerationException>(() => _context.InitializeMetadata(typeof(dv_test_without_precompilation).Assembly));
            Assert.Contains("dv_test_without_precompilation", ex.Message);
            Assert.Contains("dv_file", ex.Message);
            Assert.Contains("Object", ex.Message);
        }
        #endif

    }
}
