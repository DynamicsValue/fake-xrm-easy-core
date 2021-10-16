using Crm;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Xunit;

namespace FakeXrmEasy.Tests
{
    public class FakeXrmEasyTestDelete : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_delete_is_invoked_with_an_empty_logical_name_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _service.Delete(null, Guid.Empty));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");

            ex = Assert.Throws<InvalidOperationException>(() => _service.Delete("", Guid.Empty));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");

            ex = Assert.Throws<InvalidOperationException>(() => _service.Delete("     ", Guid.Empty));
            Assert.Equal(ex.Message, "The entity logical name must not be null or empty.");
        }

        [Fact]
        public void When_delete_is_invoked_with_an_empty_guid_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _service.Delete("account", Guid.Empty));
            Assert.Equal(ex.Message, "The id must not be empty.");
        }

        [Fact]
        public void When_delete_is_invoked_with_non_existing_entity_an_exception_is_thrown()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var nonExistingGuid = Guid.NewGuid();
            var data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Delete("account", nonExistingGuid));
            Assert.Equal(ex.Message, string.Format("account with Id {0} Does Not Exist", nonExistingGuid));
        }

        [Fact]
        public void When_delete_is_invoked_with_non_existing_entity_and_nothing_has_been_initalised_an_exception_is_thrown()
        {
            //Initialize the context with a single entity
            var nonExistingGuid = Guid.NewGuid();

            var ex = Assert.Throws<InvalidOperationException>(() => _service.Delete("account", nonExistingGuid));
            Assert.Equal(ex.Message.ToLower(), "the entity logical name account is not valid.");
        }

        [Fact]
        public void When_delete_is_invoked_with_non_existing_entity_and_nothing_has_been_initalised_using_proxytypes_assembly_an_exception_is_thrown()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            //Initialize the context with a single entity
            var nonExistingGuid = Guid.NewGuid();

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Delete("account", nonExistingGuid));
            Assert.Equal(ex.Message, string.Format("account with Id {0} Does Not Exist", nonExistingGuid));
        }

        [Fact]
        public void When_delete_is_invoked_with_an_existing_entity_that_entity_is_delete_from_the_context()
        {
            //Initialize the context with a single entity
            var guid = Guid.NewGuid();
            var data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);

            _service.Delete("account", guid);
            Assert.True(_context.CreateQuery("acount").Count() == 0);
        }

        [Fact]
        public void When_Deleting_Using_Organization_Context_Record_Should_Be_Deleted()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var account = new Account() { Id = Guid.NewGuid(), Name = "Super Great Customer", AccountNumber = "69" };

            using (var ctx = new OrganizationServiceContext(_service))
            {
                ctx.AddObject(account);
                ctx.SaveChanges();
            }

            Assert.NotNull(_service.Retrieve(Account.EntityLogicalName, account.Id, new ColumnSet(true)));

            using (var ctx = new OrganizationServiceContext(_service))
            {
                ctx.Attach(account);
                ctx.DeleteObject(account);
                ctx.SaveChanges();

                var retrievedAccount = ctx.CreateQuery<Account>().SingleOrDefault(acc => acc.Id == account.Id);
                Assert.Null(retrievedAccount);
            }
        }

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
        [Fact]
        public void When_delete_is_invoked_with_an_existing_entity_by_alternate_key_that_entity_is_delete_from_the_context()
        {
            var accountMetadata = new Microsoft.Xrm.Sdk.Metadata.EntityMetadata();
            accountMetadata.LogicalName = Account.EntityLogicalName;
            var alternateKeyMetadata = new Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata();
            alternateKeyMetadata.KeyAttributes = new string[] { "AccountNumber" };
            accountMetadata.SetFieldValue("_keys", new Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata[]
                 {
                 alternateKeyMetadata
                 });
            _context.InitializeMetadata(accountMetadata);

            //Initialize the context with a single entity
            var account = new Entity("account");
            account.Id = Guid.NewGuid();
            account.Attributes.Add("AccountNumber", 9000);

            _context.Initialize(account);

            var delete = new DeleteRequest
            {
                Target = new EntityReference("account", "AccountNumber", 9000)
            };
            _service.Execute(delete);

            Assert.True(_context.CreateQuery("account").Count() == 0);
        }
#endif

    }
}