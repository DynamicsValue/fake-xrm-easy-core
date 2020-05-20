using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Tests.Issues
{
    public class Issue312: FakeXrmEasyTests
    {
        [Fact]
        public void Reproduce_issue_312()
        {
            var accountId = Guid.NewGuid();

            Account account = new Account();
            account.Id = accountId;
            account.Name = "Test Account";

            Contact contact = new Contact();
            contact.FirstName = "Dave";
            contact.LastName = "Contact";
            contact.ParentCustomerId = new EntityReference("account", accountId);
            contact.JobTitle = "Developer";

            Contact contact2 = new Contact();
            contact2.FirstName = "Simon";
            contact2.LastName = "Contact";
            contact2.ParentCustomerId = new EntityReference("account", accountId);
            contact2.JobTitle = "Tester2";


            _service.Create(account);
            _service.Create(contact);
            _service.Create(contact2);


            string fetchXml = $@"
        <fetch distinct='true' no-lock='true' >
            <entity name='account' >
                <attribute name='name' />
                <filter>
                    <condition attribute='accountid' operator='eq' value='{accountId}' />
                </filter>
                <link-entity name='contact' from='parentcustomerid' to='accountid' alias='dev' >
                    <attribute name='firstname' />
                    <filter>
                        <condition attribute='jobtitle' operator='eq' value='Developer' />
                    </filter>
                </link-entity>
                <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='outer' alias='tester' >
                    <attribute name='firstname' />
                    <filter>
                        <condition attribute='jobtitle' operator='eq' value='Tester' />
                    </filter>
                </link-entity>
            </entity>
        </fetch>";

            string fetchXml2 = $@"
        <fetch distinct='true' no-lock='true' >
            <entity name='account' >
                <attribute name='name' />
                <filter>
                    <condition attribute='accountid' operator='eq' value='{accountId}' />
                </filter>
                <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='outer' alias='tester' >
                    <attribute name='firstname' />
                    <filter>
                        <condition attribute='jobtitle' operator='eq' value='Tester' />
                    </filter>
                </link-entity>
            </entity>
        </fetch>";

            EntityCollection result2 = _service.RetrieveMultiple(new FetchExpression(fetchXml2));
            Assert.Equal(1, result2.Entities.Count);
            Assert.Equal(2, result2.Entities[0].Attributes.Count);
            Assert.Equal("Test Account", result2.Entities[0].Attributes["name"].ToString());

            EntityCollection result = _service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Equal(1, result.Entities.Count);
            Assert.Equal(3, result.Entities[0].Attributes.Count);
            Assert.Equal("Test Account", result.Entities[0].Attributes["name"].ToString());
            Assert.Equal("Dave", ((AliasedValue)result.Entities[0].Attributes["dev.firstname"]).Value);

        }
    }
}
