using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Xunit;

namespace FakeXrmEasy.Tests
{
    public class FakeXrmEasyTestRetrieve : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_retrieve_is_invoked_with_an_empty_logical_name_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _service.Retrieve(null, Guid.Empty, new ColumnSet()));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");

            ex = Assert.Throws<InvalidOperationException>(() => _service.Retrieve("", Guid.Empty, new ColumnSet()));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");

            ex = Assert.Throws<InvalidOperationException>(() => _service.Retrieve("     ", Guid.Empty, new ColumnSet()));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");
        }

        [Fact]
        public void When_retrieve_is_invoked_with_an_empty_guid_an_exception_is_thrown()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("account", Guid.Empty, new ColumnSet(true)));
            Assert.Equal("account With Id = 00000000-0000-0000-0000-000000000000 Does Not Exist", ex.Message);
        }

        [Fact]
        public void When_retrieve_is_invoked_with_a_null_columnset_exception_is_thrown()
        {
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("account", Guid.NewGuid(), null));
            Assert.Equal(ex.Message, "Required field 'ColumnSet' is missing");
        }

        [Fact]
        public void When_retrieve_is_invoked_with_a_non_existing_logical_name_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _service.Retrieve("account", Guid.NewGuid(), new ColumnSet(true)));
            Assert.Equal("The entity logical name account is not valid.", ex.Message);
        }

        [Fact]
        public void When_retrieve_is_invoked_with_non_existing_entity_null_is_returned()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("account", Guid.NewGuid(), new ColumnSet()));
            Assert.Equal<uint>((uint)0x80040217, (uint)ex.Detail.ErrorCode);
        }

        [Fact]
        public void When_retrieve_is_invoked_with_an_existing_entity_that_entity_is_returned()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);
            var result = _service.Retrieve("account", guid, new ColumnSet());
            Assert.Equal(result.Id, data.FirstOrDefault().Id);
        }

        [Fact]
        public void When_retrieve_is_invoked_with_an_existing_entity_and_all_columns_all_the_attributes_are_returned()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);

            var result = _service.Retrieve("account", guid, new ColumnSet(true));
            Assert.Equal(result.Id, data.FirstOrDefault().Id);
            Assert.Equal(result.Attributes.Count, 7);
        }

        [Fact]
        public void When_retrieve_is_invoked_with_an_existing_entity_and_only_one_column_only_that_one_is_retrieved()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var entity = new Entity("account") { Id = guid };
            entity["name"] = "Test account";
            entity["createdon"] = DateTime.UtcNow;

            var data = new List<Entity>() { entity }.AsQueryable();
            _context.Initialize(data);

            var result = _service.Retrieve("account", guid, new ColumnSet(new string[] { "name" }));
            Assert.Equal(result.Id, data.FirstOrDefault().Id);
            Assert.True(result.Attributes.Count == 1);
            Assert.Equal(result["name"], "Test account");
        }

        [Fact]
        public void When_retrieve_is_invoked_with_an_existing_entity_and_proxy_types_the_returned_entity_must_be_of_the_appropiate_subclass()
        {
            _context.EnableProxyTypes(Assembly.GetExecutingAssembly());

            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var account = new Account() { Id = guid };
            account.Name = "Test account";

            var data = new List<Entity>() { account }.AsQueryable();
            _context.Initialize(data);

            var result = _service.Retrieve("account", guid, new ColumnSet(new string[] { "name" }));

            Assert.True(result is Account);
        }

        [Fact]
        public void When_retrieving_entity_that_does_not_exist_with_proxy_types_entity_name_should_be_known()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));
            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("account", Guid.NewGuid(), new ColumnSet(true)));
        }

        [Fact]
        public void Should_Not_Fail_On_Retrieving_Entity_With_Entity_Collection_Attributes()
        {
            var party = new ActivityParty
            {
                PartyId = new EntityReference("systemuser", Guid.NewGuid())
            };

            var email = new Email
            {
                Id = Guid.NewGuid(),
                To = new[] { party }
            };

            _service.Create(email);

            var ex = Record.Exception(() => _service.Retrieve(email.LogicalName, email.Id, new ColumnSet(true)));
            Assert.Null(ex);
        }
    }
}