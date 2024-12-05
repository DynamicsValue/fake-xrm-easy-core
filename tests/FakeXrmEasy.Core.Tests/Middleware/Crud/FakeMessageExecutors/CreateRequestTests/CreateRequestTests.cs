﻿using Crm;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.CreateRequestTests
{
    public class CreateRequestTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_a_null_entity_is_created_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _service.Create(null));
            Assert.Equal("The entity must not be null", ex.Message);
        }

        [Fact]
        public void When_an_entity_is_created_with_an_empty_logical_name_an_exception_is_thrown()
        {
            var e = new Entity("") { Id = Guid.Empty };

            var ex = Assert.Throws<InvalidOperationException>(() => _service.Create(e));
            Assert.Equal("The LogicalName property must not be empty", ex.Message);
        }

        [Fact]
        public void When_adding_an_entity_the_returned_guid_must_not_be_empty_and_the_context_should_have_it()
        {
            var e = new Entity("account") { Id = Guid.Empty };
            var guid = _service.Create(e);

            Assert.True(guid != Guid.Empty);
            Assert.True(_context.CreateQuery("account").Count() == 1);
        }

        [Fact]
        public void When_Creating_Without_Id_It_should_Be_set_Automatically()
        {
            var account = new Account
            {
                Name = "TestAcc"
            };

            account.Id = _service.Create(account);

            Assert.NotEqual(Guid.Empty, account.Id);
        }

        [Fact]
        public void When_Creating_With_Id_It_should_Be_set()
        {
            var service = _context.GetOrganizationService();
            var accId = Guid.NewGuid();

            var account = new Account
            {
                Name = "TestAcc",
                Id = accId
            };

            var createdId = service.Create(account);

            Assert.Equal(accId, createdId);
        }

        [Fact]
        public void When_Creating_With_Already_Existing_Id_Exception_Should_Be_Thrown()
        {
            var accId = Guid.NewGuid();

            var account = new Account
            {
                Name = "TestAcc",
                Id = accId
            };
            _service.Create(account);

            Assert.Throws<InvalidOperationException>(() => _service.Create(account));
        }

        [Fact]
        public void When_Creating_With_A_StateCode_Property_In_v9_Exception_Is_NotThrown()
        {
            var accId = Guid.NewGuid();

            var account = new Account
            {
                Name = "TestAcc",
                Id = accId
            };
            account["statecode"] = new OptionSetValue(1);

            _service.Create(account);

            var accountCreated = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(accountCreated);

            Assert.Equal(AccountState.Active, accountCreated.StateCode.Value);
        }


        [Fact]
        public void When_Creating_Using_Organization_Context_Record_Should_Be_Created()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var account = new Account() { Id = Guid.NewGuid(), Name = "Super Great Customer", AccountNumber = "69" };

            using (var ctx = new OrganizationServiceContext(_service))
            {
                ctx.AddObject(account);
                ctx.SaveChanges();
            }

            Assert.NotNull(_service.Retrieve(Account.EntityLogicalName, account.Id, new ColumnSet(true)));
        }

        [Fact]
        public void When_Creating_Using_Organization_Context_Without_Saving_Changes_Record_Should_Not_Be_Created()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var account = new Account() { Id = Guid.NewGuid(), Name = "Super Great Customer", AccountNumber = "69" };

            using (var ctx = new OrganizationServiceContext(_service))
            {
                ctx.AddObject(account);

                var retrievedAccount = ctx.CreateQuery<Account>().SingleOrDefault(acc => acc.Id == account.Id);
                Assert.Null(retrievedAccount);
            }
        }

        [Fact]
        public void When_creating_a_record_using_early_bound_entities_primary_key_should_be_populated()
        {
            var c = new Contact();

            var id = _service.Create(c);

            //Retrieve the record created
            var contact = (from con in _context.CreateQuery<Contact>()
                           select con).FirstOrDefault();

            Assert.True(contact.Attributes.ContainsKey("contactid"));
            Assert.Equal(id, contact["contactid"]);
        }

        [Fact]
        public void When_creating_a_record_using_dynamic_entities_primary_key_should_be_populated()
        {
            Entity e = new Entity("new_myentity");

            var id = _service.Create(e);

            //Retrieve the record created
            var record = (from r in _context.CreateQuery("new_myentity")
                          select r).FirstOrDefault();

            Assert.True(record.Attributes.ContainsKey("new_myentityid"));
            Assert.Equal(id, record["new_myentityid"]);
        }

        [Fact]
        public void When_creating_a_record_using_early_bound_entities_and_proxytypes_primary_key_should_be_populated()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));
            var c = new Contact();
            c.Id = Guid.NewGuid();

            IOrganizationService service = _context.GetOrganizationService();

            _context.Initialize(new List<Entity>() { c });

            //Retrieve the record created
            var contact = (from con in _context.CreateQuery<Contact>()
                           select con).FirstOrDefault();

            Assert.True(contact.Attributes.ContainsKey("contactid"));
            Assert.Equal(c.Id, contact["contactid"]);
        }

        

        [Fact]
        public void Should_not_store_references_to_variables_but_actual_clones()
        {
            //create an account and then retrieve it with no changes
            Entity newAccount = new Entity("account");
            newAccount["name"] = "New Account";

            newAccount.Id = _service.Create(newAccount);

            Entity retrievedAccount = _service.Retrieve("account", newAccount.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.True(retrievedAccount.Attributes.Contains("name"));

            //do the same as above, but this time clear the attributes - see that when retrieved, the retrieved entity does not contain the name attribute
            Entity newAccount1 = new Entity("account");
            newAccount1["name"] = "New Account1";

            newAccount1.Id = _service.Create(newAccount1);
            newAccount1.Attributes.Clear();

            Entity retrievedAccount1 = _service.Retrieve("account", newAccount1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.True(retrievedAccount1.Attributes.Contains("name"));

            //third time around, change the name to something new, the retrieved entity should not reflect this change
            Entity newAccount2 = new Entity("account");
            newAccount2["name"] = "New Account2";

            newAccount2.Id = _service.Create(newAccount2);
            newAccount2["name"] = "Changed name";

            Entity retrievedAccount2 = _service.Retrieve("account", newAccount2.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.True(retrievedAccount2["name"].ToString() == "New Account2", $"'{retrievedAccount2["name"]}' was not the expected result");
        }

        [Fact]
        public void Shouldnt_modify_objects_passed_to_the_service()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));
            var account = new Account { Id = Guid.NewGuid(), Name = "Test account" };

            _context.Initialize(new List<Entity>() { account });

            //Retrieve the record created
            Contact c = new Contact
            {
                ParentCustomerId = account.ToEntityReference(),
                LastName = "Duck",
            };
            foreach (var name in new[] { "Huey", "Dewey", "Louie"})
            {
                c.FirstName = name;
                _service.Create(c);
            }

            var createdContacts = _context.CreateQuery<Contact>().ToList();

            Assert.Equal(Guid.Empty, c.Id);
            Assert.Null(c.ContactId);
            Assert.Null(c.CreatedOn);

            Assert.Equal(3, createdContacts.Count);
        }

        [Fact]
        public void When_Creating_Without_Default_Attributes_They_Should_Be_Set_By_Default()
        {
            var account = new Account
            {
                Name = "test"
            };

            _service.Create(account);
            var createdAccount = _context.CreateQuery<Account>().FirstOrDefault();

            Assert.True(createdAccount.Attributes.ContainsKey("createdon"));
            Assert.True(createdAccount.Attributes.ContainsKey("createdby"));
            Assert.True(createdAccount.Attributes.ContainsKey("modifiedon"));
            Assert.True(createdAccount.Attributes.ContainsKey("modifiedby"));
            Assert.True(createdAccount.Attributes.ContainsKey("statecode"));
        }

        [Fact]
        public void When_Creating_Without_Default_Attributes_They_Should_Be_Set_By_Default_With_Early_Bound()
        {
            var service = _context.GetOrganizationService();
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var account = new Account
            {
                Name = "test"
            };

            service.Create(account);
            var createdAccount = _context.CreateQuery<Account>().FirstOrDefault();

            Assert.True(createdAccount.Attributes.ContainsKey("createdon"));
            Assert.True(createdAccount.Attributes.ContainsKey("createdby"));
            Assert.True(createdAccount.Attributes.ContainsKey("modifiedon"));
            Assert.True(createdAccount.Attributes.ContainsKey("modifiedby"));
            Assert.True(createdAccount.Attributes.ContainsKey("statecode"));
        }

        [Fact]
        public void When_creating_a_record_overridencreatedon_should_override_created_on()
        {
            var now = DateTime.Now.Date;

            var account = new Account()
            {
                OverriddenCreatedOn = now,
                ["createdon"] = now.AddDays(-1)
            };

            _service.Create(account);

            var createdAccount = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.Equal(now, createdAccount.CreatedOn);
        }

        [Fact]
        public void When_creating_a_record_with_a_generic_organization_request_record_should_also_be_created()
        {
            var account = new Account() { Name = "test" };
            var request = new OrganizationRequest()
            {
                RequestName = "Create",
                Parameters = new ParameterCollection()
                {
                    { "Target", account }
                }
            };

            _service.Execute(request);

            var createdAccount = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.Equal(account.Name, createdAccount.Name);
        }
    }
}