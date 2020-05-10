using Crm;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;

namespace FakeXrmEasy.Tests.Issues
{
    public class Issue178
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public Issue178() 
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void Reproduce_issue_178_ManyToMany()
        {
            Arrange();
            using (var ctx = new XrmServiceContext(_service))
            {
                // ACT
                var contact = ctx.ContactSet.First();
                ctx.LoadProperty(contact, "gbp_gbp_customaddress_contact");   // THis will trigger the RetrieveRelationshipRequest

                // ASSERT
                Assert.NotNull(contact.gbp_gbp_customaddress_contact);
            }
        }
        [Fact]
        public void Reproduce_issue_178_ManyToOne()
        {
            Arrange();
            using (var ctx = new XrmServiceContext(_service))
            {
                // ACT
                var contact = ctx.ContactSet.First();
                ctx.LoadProperty(contact, "contact_customer_accounts");   // THis will trigger the RetrieveRelationshipRequest

                // ASSERT
                Assert.NotNull(contact.contact_customer_accounts);
            }
        }
        [Fact]
        public void Reproduce_issue_178_OneToMany()
        {
            Arrange();
            using (var ctx = new XrmServiceContext(_service))
            {
                // ACT
                var account = ctx.AccountSet.First();
                ctx.LoadProperty(account, "contact_customer_accounts");   // THis will trigger the RetrieveRelationshipRequest

                // ASSERT
                Assert.NotNull(account.contact_customer_accounts);
            }
        }


        private void Arrange()
        {
            Account account = new Account();
            account.Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            account.Name = "Goggle ltd";

            Contact contact = new Contact();
            contact.Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
            contact.FirstName = "Test";
            contact.LastName = "Contact 1";
            contact.ParentCustomerId = account.ToEntityReference();

            gbp_globecountry country = new gbp_globecountry()
            {
                Id = Guid.NewGuid(),
                gbp_name = "United Kingdom",
                gbp_code = "GB"
            };

            gbp_customaddress customAddress = new gbp_customaddress()
            {
                Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                gbp_addresstype = new OptionSetValue(3),
                gbp_country = country.ToEntityReference(),
            };
            var ugh = new gbp_gbp_customaddress_contact() { Id = new Guid("12345678-0000-0000-0000-000000000000") };
            ugh.Attributes["contactid"] = contact.Id;
            ugh.Attributes["gbp_customaddressid"] = customAddress.Id;

            _context.AddRelationship("gbp_gbp_customaddress_contact",
                new XrmFakedRelationship()
                {
                    RelationshipType = XrmFakedRelationship.FakeRelationshipType.ManyToMany,
                    IntersectEntity = "gbp_gbp_customaddress_contact",
                    Entity1LogicalName = gbp_customaddress.EntityLogicalName,
                    Entity1Attribute = "gbp_customaddressid",
                    Entity2LogicalName = Contact.EntityLogicalName,
                    Entity2Attribute = "contactid"
                });


            /*
              this doen't work, need to step through the code to see what the query is doing
              or maybe determine if it's an n:1
             */
            _context.AddRelationship("contact_customer_accounts",
                new XrmFakedRelationship()
                {
                    RelationshipType = XrmFakedRelationship.FakeRelationshipType.OneToMany,
                    IntersectEntity = "contact_customer_accounts",
                    Entity1LogicalName = Contact.EntityLogicalName,
                    Entity1Attribute = "parentcustomerid",
                    Entity2LogicalName = Account.EntityLogicalName,
                    Entity2Attribute = "accountid",
                });


            _context.Initialize(new List<Entity>()
            {
                account, contact, customAddress, ugh
            });
        }
    }
}

namespace Crm
{
    partial class Contact
    {
        /// <summary>
		/// N:N gbp_gbp_customaddress_contact
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("gbp_gbp_customaddress_contact")]
        public System.Collections.Generic.IEnumerable<gbp_customaddress> gbp_gbp_customaddress_contact
        {
            get
            {
                return this.GetRelatedEntities<gbp_customaddress>("gbp_gbp_customaddress_contact", null);
            }
            set
            {
                this.OnPropertyChanging("gbp_gbp_customaddress_contact");
                this.SetRelatedEntities<gbp_customaddress>("gbp_gbp_customaddress_contact", null, value);
                this.OnPropertyChanged("gbp_gbp_customaddress_contact");
            }
        }
    }


}