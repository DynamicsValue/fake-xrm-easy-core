using Crm;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests.QueryTranslationTests
{
    public class ProjectionTests : FakeXrmEasyTestsBase
    {
        private readonly Account _account;

        public ProjectionTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Some name"
            };
        }

        [Fact]
        public void Should_return_primary_key_attribute_even_if_not_specified_in_column_set()
        {
            _context.Initialize(_account);
            var account = _service.Retrieve(Account.EntityLogicalName, _account.Id, new ColumnSet(new string[] { "name" }));
            Assert.True(account.Attributes.ContainsKey("accountid"));
        }

        [Fact]
        public void Should_return_primary_key_attribute_when_retrieving_using_all_columns()
        {
            _context.Initialize(_account);
            var account = _service.Retrieve(Account.EntityLogicalName, _account.Id, new ColumnSet(true));
            Assert.True(account.Attributes.ContainsKey("accountid"));
        }
    }
}
