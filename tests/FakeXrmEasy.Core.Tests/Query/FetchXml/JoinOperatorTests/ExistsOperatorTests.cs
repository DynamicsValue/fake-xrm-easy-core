using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

#if FAKE_XRM_EASY_9
namespace FakeXrmEasy.Core.Tests.Query.FetchXml.JoinOperatorTests
{
    public class ExistsOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        private readonly Account _contosoAccount;
        private readonly Account _contAccount;
        
        public ExistsOperatorTests()
        {
            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Joe"
            };
            
            _contosoAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                PrimaryContactId = _contact.ToEntityReference()
            };

            _contAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Cont",
                PrimaryContactId = _contact.ToEntityReference()
            };

        }
        
        [Fact]
        public void Should_translate_link_entity_with_exists_operator_that_matches_an_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var fetchXml = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                            <link-entity name='account' to='contactid' from='primarycontactid' link-type='exists'>
                                <filter type='and'>
                                    <condition attribute='name' operator='eq' value='Contoso' />
                                </filter>
                            </link-entity>
                    </entity>
                   </fetch>
                ";
            
            var query = fetchXml.ToQueryExpression(_context);
            Assert.NotNull(query);
            Assert.Single(query.LinkEntities);

            var linkEntity = query.LinkEntities[0];
            Assert.Equal(JoinOperator.Exists, linkEntity.JoinOperator);
            Assert.Equal("contact", linkEntity.LinkFromEntityName);
            Assert.Equal("account", linkEntity.LinkToEntityName);
            Assert.Equal("contactid", linkEntity.LinkFromAttributeName);
            Assert.Equal("primarycontactid", linkEntity.LinkToAttributeName);

            Assert.NotNull(linkEntity.LinkCriteria);
            
            var linkCriteria = linkEntity.LinkCriteria;
            Assert.Single(linkCriteria.Conditions);
            
            Assert.Equal("name", linkCriteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Equal, linkCriteria.Conditions[0].Operator);
            Assert.Equal("Contoso", linkCriteria.Conditions[0].Values.FirstOrDefault());
        }
    }
}
#endif
