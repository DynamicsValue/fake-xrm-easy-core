using System;

using Xunit;

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Threading;
using Crm;
using System.Reflection;

namespace FakeXrmEasy.Core.Tests
{
    public class FakeContextCoreTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_a_fake_context_is_created_the_data_is_initialized()
        {
            Assert.True((_context as XrmFakedContext).Db != null);
        }

        [Fact]
        public void When_initializing_the_context_with_a_null_list_of_entities_an_exception_is_thrown()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _context.Initialize(entities: null));
            Assert.Equal("The entities parameter must be not null", ex.Message);
        }

        [Fact]
        public void When_initializing_the_context_with_an_entity_with_an_empty_logical_name_an_exception_is_thrown()
        {
            IEnumerable<Entity> data = new List<Entity>() {
                new Entity() { Id = Guid.NewGuid()}
            };

            var ex = Assert.Throws<InvalidOperationException>(() => _context.Initialize(data));
            Assert.Equal("The LogicalName property must not be empty", ex.Message);
        }

        [Fact]
        public void When_initializing_the_context_with_an_entity_with_an_empty_guid_an_exception_is_thrown()
        {
            IEnumerable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = Guid.Empty }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => _context.Initialize(data));
            Assert.Equal("The Id property must not be empty", ex.Message);
        }

        [Fact]
        public void When_initializing_the_context_with_a_dynamic_entity_without_a_primary_key_but_id_entity_is_added()
        {
            IEnumerable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = Guid.NewGuid() }
            };

            _context.Initialize(data);
            Assert.True(_context.CreateQuery("account").Count() == 1);
        }


        [Fact]
        public void When_initializing_the_context_with_a_dynamic_entity_with_a_primary_key_is_added_to_the_context()
        {
            IEnumerable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = Guid.NewGuid(), Attributes = new AttributeCollection { { "accountid", Guid.NewGuid() } }  }
            };

            _context.Initialize(data);
            Assert.True(_context.CreateQuery("account").Count() == 1);
        }

        [Fact]
        public void When_initializing_the_context_with_an_entity_the_context_has_that_entity()
        {
            var guid = Guid.NewGuid();

            IQueryable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);
            Assert.True(_context.CreateQuery("account").Count() == 1);
            Assert.Equal(guid, _context.CreateQuery("account").FirstOrDefault().Id);
        }

        [Fact]
        public void When_initializing_the_context_with_the_single_entity_overload_the_context_has_that_entity()
        {
            var guid = Guid.NewGuid();

            _context.Initialize(new Entity("account") { Id = guid });

            Assert.True(_context.CreateQuery("account").Count() == 1);
            Assert.Equal(guid, _context.CreateQuery("account").FirstOrDefault().Id);
        }

        [Fact]
        public void When_initializing_with_two_entities_with_the_same_guid_only_the_latest_will_be_in_the_context()
        {
            var guid = Guid.NewGuid();

            IQueryable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = guid },
                new Entity("account") { Id = guid }
            }.AsQueryable();

            _context.Initialize(data);

            Assert.True(_context.CreateQuery("account").Count() == 1);
            Assert.Equal(guid, _context.CreateQuery("account").FirstOrDefault().Id);
        }

        [Fact]
        public void When_initializing_with_two_entities_with_two_different_guids_the_context_will_have_both()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            IQueryable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = guid1 },
                new Entity("account") { Id = guid2 }
            }.AsQueryable();

            _context.Initialize(data);

            Assert.True(_context.CreateQuery("account").Count() == 2);
            Assert.Equal(guid1, _context.CreateQuery("account").FirstOrDefault().Id);
            Assert.Equal(guid2, _context.CreateQuery("account").LastOrDefault().Id);
        }
        [Fact]
        public void When_initializing_with_two_entities_of_same_logical_name_and_another_one_the_context_will_have_all_three()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            IQueryable<Entity> data = new List<Entity>() {
                new Entity("account") { Id = guid1 },
                new Entity("account") { Id = guid2 },
                new Entity("contact") { Id = guid3 }
            }.AsQueryable();

            _context.Initialize(data);
            Assert.True(_context.CreateQuery("account").Count() == 2);
            Assert.True(_context.CreateQuery("contact").Count() == 1);

            Assert.Equal(guid1, _context.CreateQuery("account").FirstOrDefault().Id);
            Assert.Equal(guid2, _context.CreateQuery("account").LastOrDefault().Id);
            Assert.Equal(guid3, _context.CreateQuery("contact").FirstOrDefault().Id);

        }


        [Fact]
        public void When_initializing_the_context_with_an_entity_it_should_have_default_createdon_createdby_modifiedon_and_modifiedby_attributes()
        {
            var e = new Entity("account") { Id = Guid.NewGuid() };
            _context.Initialize(new List<Entity>() { e });

            var createdEntity = _context.CreateQuery("account").FirstOrDefault();
            Assert.True(createdEntity.Attributes.ContainsKey("createdon"));
            Assert.True(createdEntity.Attributes.ContainsKey("modifiedon"));
            Assert.True(createdEntity.Attributes.ContainsKey("createdby"));
            Assert.True(createdEntity.Attributes.ContainsKey("modifiedby"));
        }

        [Fact]
        public void When_updating_an_entity_Modified_On_Should_Also_Be_Updated()
        {
            var e = new Entity("account") { Id = Guid.NewGuid() };
            _context.Initialize(new List<Entity>() { e });

            var oldModifiedOn = _context.CreateQuery<Account>()
                                        .FirstOrDefault()
                                        .ModifiedOn;

            Thread.Sleep(1000);

            _service.Update(e);
            var newModifiedOn = _context.CreateQuery<Account>()
                                        .FirstOrDefault()
                                        .ModifiedOn;

            Assert.NotEqual(oldModifiedOn, newModifiedOn);
        }

        [Fact]
        public void When_using_typed_entities_ProxyTypesAssembly_is_not_mandatory()
        {
            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Jordi" };
            _context.Initialize(new List<Entity>() { c });

            // Linq 2 Query Expression
            using (var ctx = new XrmServiceContext(_service))
            {
                var contacts = (from con in ctx.CreateQuery<Contact>()
                                select con).ToList();

                Assert.Single(contacts);
            }

            //Query faked context directly
            var ex = Record.Exception(() => _context.CreateQuery<Contact>());
            Assert.Null(ex);

        }

        [Fact]
        public void When_initializing_the_entities_a_proxy_types_assembly_is_not_mandatory()
        {
            //This will make tests much more simple as we won't need to specify the ProxyTypesAssembly every single time if 
            //we use early bound entities

            var assembly = Assembly.GetAssembly(typeof(Contact));

            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Jordi" };
            _context.Initialize(new List<Entity>() { c });

            Assert.Equal(assembly, (_context as XrmFakedContext).ProxyTypesAssemblies.FirstOrDefault());
        }

        [Fact]
        public void When_using_proxy_types_entity_names_are_validated()
        {
            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Jordi" };
            _context.Initialize(new List<Entity>() { c });

            Assert.Throws<Exception>(() => _service.Create(new Entity("thisDoesntExist")));
        }

        [Fact]
        public void When_initialising_the_context_once_exception_is_not_thrown()
        {
            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Lionel" };
            var ex = Record.Exception(() => _context.Initialize(new List<Entity>() { c }));
            Assert.Null(ex);
        }

        [Fact]
        public void When_initialising_the_context_twice_exception_is_thrown()
        {
            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Lionel" };
            var ex = Record.Exception(() => _context.Initialize(new List<Entity>() { c }));
            Assert.Null(ex);
            Assert.Throws<Exception>(() => _context.Initialize(new List<Entity>() { c }));
        }

        [Fact]
        public void When_getting_a_fake_service_reference_it_uses_a_singleton_pattern()
        {
            var service2 = _context.GetOrganizationService();
            Assert.Equal(_service, service2);
        }

        [Fact]
        public void When_enabling_proxy_types_exception_is_not_thrown() {

            var assembly = typeof(Crm.Account).Assembly;
            var ex = Record.Exception(() => _context.EnableProxyTypes(assembly));
            Assert.Null(ex);
        }

        [Fact]
        public void When_enabling_proxy_types_twice_for_same_assembly_an_exception_is_thrown() {

            var assembly = typeof(Crm.Account).Assembly;

            _context.EnableProxyTypes(assembly);
            Assert.Throws<InvalidOperationException>(() => _context.EnableProxyTypes(assembly));
        }

        [Fact]
        public void When_initialising_the_context_after_enabling_proxy_types_exception_is_not_thrown()
        {
            var assembly = typeof(Crm.Account).Assembly;

            _context.EnableProxyTypes(assembly);
            var c = new Contact() { Id = Guid.NewGuid(), FirstName = "Lionel" };
            var ex = Record.Exception(() => _context.Initialize(new List<Entity>() { c }));
            Assert.Null(ex);
        }

        [Fact]
        public void Should_return_entity_by_id() 
        {
            var contact = new Contact() 
            { 
                Id = Guid.NewGuid(),
                FirstName = "Steve",
                LastName = "Vai"
            };
            _context.Initialize(contact);

            var retrievedContact = _context.GetEntityById<Contact>(contact.Id);
            Assert.Equal(contact.Id, retrievedContact.Id);
            Assert.Equal(contact.FirstName, retrievedContact.FirstName);
            Assert.Equal(contact.LastName, retrievedContact.LastName);   
        }

        [Fact]
        public void Should_return_error_if_entity_logical_name_doesnt_exists() 
        {
            Assert.Throws<InvalidOperationException>(() =>_context.GetEntityById("doesNotExist", Guid.NewGuid())); 
        }

        [Fact]
        public void Should_return_error_if_entity_id_does_not_exists() 
        {
            var contact = new Contact() 
            { 
                Id = Guid.NewGuid(),
                FirstName = "Steve",
                LastName = "Vai"
            };
            _context.Initialize(contact);

            Assert.Throws<InvalidOperationException>(() =>_context.GetEntityById("contact", Guid.NewGuid())); 
        }
        
        [Fact]
        public void Should_find_reflected_type_by_entity_type_code() {

            var assembly = typeof(Crm.Account).Assembly;
            _context.EnableProxyTypes(assembly);

            var type = _context.FindReflectedType(1);
            Assert.Equal(typeof(Account), type);
        }
    }
}
