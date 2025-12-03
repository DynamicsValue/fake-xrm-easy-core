using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Abstractions.Enums;

namespace FakeXrmEasy.Core.Tests.Query.FetchXml
{
    /// <summary>
    /// Tests for FetchXML conditions with entityname attribute that references linked entity aliases
    /// </summary>
    public class FetchXmlAliasedConditionTests : FakeXrmEasyTestsBase
    {
        public FetchXmlAliasedConditionTests() : base()
        {
        }

        /// <summary>
        /// This test reproduces the bug where a FetchXML condition with entityname="alias"
        /// fails when the attribute doesn't exist on the main entity but does exist on the linked entity.
        /// 
        /// The bug occurs because the attribute type lookup uses the parent entity name instead of
        /// resolving the alias to the linked entity's name.
        /// </summary>
        [Fact]
        public void FetchXml_WithEntityNameAlias_ShouldFilterOnLinkedEntity_NotMainEntity()
        {
            // This test uses Contact and Account entities from GeneratedCode.cs
            // Contact will have an attribute that Account doesn't have
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            // Create test data
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Name = "Test Account"
            };

            var contactId = Guid.NewGuid();
            var contact = new Contact
            {
                Id = contactId,
                FirstName = "John",
                LastName = "Doe",
                // Contact has attributes that Account doesn't have
                BirthDate = DateTime.Now.AddYears(-30)
            };

            // Link account to contact via ParentCustomerId (Contact can have parent account)
            contact.ParentCustomerId = new EntityReference("account", accountId);

            _context.Initialize(new List<Entity> { account, contact });

            var service = _context.GetOrganizationService();

            // FetchXML with entityname pointing to linked entity alias
            // The condition filters on contact.birthdate using entityname="linkedContact"
            // Account doesn't have birthdate attribute - only Contact does
            var fetchXml = $@"
                <fetch>
                    <entity name='account'>
                        <attribute name='name' />
                        <filter>
                            <condition attribute='birthdate' operator='not-null' 
                                       entityname='linkedContact' />
                        </filter>
                        <link-entity name='contact' 
                                     from='parentcustomerid' 
                                     to='accountid' 
                                     alias='linkedContact' 
                                     link-type='inner'>
                            <attribute name='firstname' />
                            <attribute name='birthdate' />
                        </link-entity>
                    </entity>
                </fetch>";

            // This should succeed - the entityname="linkedContact" means check linkedContact.birthdate
            // NOT account.birthdate. Account doesn't need to have the birthdate attribute.
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            Assert.Equal(accountId, result.Entities[0].Id);
        }

        /// <summary>
        /// Test with entityname referencing entity name directly (not alias)
        /// This is an existing supported scenario that should continue to work
        /// </summary>
        [Fact]
        public void FetchXml_WithEntityNameNoAlias_ShouldWork()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Name = "Test Account"
            };

            var contactId = Guid.NewGuid();
            var contact = new Contact
            {
                Id = contactId,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = DateTime.Now.AddYears(-30),
                ParentCustomerId = new EntityReference("account", accountId)
            };

            _context.Initialize(new List<Entity> { account, contact });

            var service = _context.GetOrganizationService();

            // FetchXML with entityname pointing to entity name (not alias)
            var fetchXml = $@"
                <fetch>
                    <entity name='account'>
                        <attribute name='name' />
                        <filter>
                            <condition attribute='birthdate' operator='not-null' 
                                       entityname='contact' />
                        </filter>
                        <link-entity name='contact' 
                                     from='parentcustomerid' 
                                     to='accountid' 
                                     link-type='inner'>
                            <attribute name='firstname' />
                            <attribute name='birthdate' />
                        </link-entity>
                    </entity>
                </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Single(result.Entities);
            Assert.Equal(accountId, result.Entities[0].Id);
        }
    }
}
