﻿using Crm;
using FakeXrmEasy.Metadata;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Metadata
{
    public class MetadataGeneratorTests
    {
        private readonly Type[] _typesWithAccountType;
        public MetadataGeneratorTests()
        {
            _typesWithAccountType = new Type[] { typeof(Account) };
        }

        [Fact]
        public void Should_return_one_metadata_from_one_early_bound_type()
        {
            var metadatas = MetadataGenerator.FromTypes(_typesWithAccountType);
            Assert.Single(metadatas);
        }

        [Fact]
        public void Should_set_primary_id_attribute()
        {
            var accountMetadata = MetadataGenerator.FromTypes(_typesWithAccountType).First();
            Assert.Equal("accountid", accountMetadata.PrimaryIdAttribute);
        }

        [Fact]
        public void Should_set_entity_type_code()
        {
            var accountMetadata = MetadataGenerator.FromTypes(_typesWithAccountType).First();
            Assert.Equal(Account.EntityTypeCode, accountMetadata.ObjectTypeCode);
        }

    }
}
