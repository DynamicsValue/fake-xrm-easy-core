using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

#if FAKE_XRM_EASY_9
namespace FakeXrmEasy.Core.Tests.FakeContextTests.FetchXml.JoinOperatorTests
{
    public class NotAllOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        private readonly Account _contosoAccount;
        private readonly Account _contAccount;
        
        public NotAllOperatorTests()
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
        public void Should_translate_link_entity_with_any_operator_that_matches_an_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var fetchXml = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                        <filter type='or'>
                            <link-entity name='account' to='contactid' from='primarycontactid' link-type='not-all'>
                                <filter type='and'>
                                    <condition attribute='name' operator='eq' value='Contoso' />
                                </filter>
                            </link-entity>
                        <condition attribute='statecode' operator='eq' value='1' />
                        </filter>
                    </entity>
                   </fetch>
                ";
            
            var query = fetchXml.ToQueryExpression(_context);
            
            Assert.NotNull(query);
            Assert.NotNull(query.Criteria.AnyAllFilterLinkEntity);

            var linkEntity = query.Criteria.AnyAllFilterLinkEntity;
            Assert.Equal(JoinOperator.NotAll, query.Criteria.AnyAllFilterLinkEntity.JoinOperator);
            Assert.Equal("contact", query.Criteria.AnyAllFilterLinkEntity.LinkFromEntityName);
            Assert.Equal("account", query.Criteria.AnyAllFilterLinkEntity.LinkToEntityName);
            Assert.Equal("contactid", query.Criteria.AnyAllFilterLinkEntity.LinkFromAttributeName);
            Assert.Equal("primarycontactid", query.Criteria.AnyAllFilterLinkEntity.LinkToAttributeName);

            Assert.NotNull(linkEntity.LinkCriteria);
            
            var linkCriteria = linkEntity.LinkCriteria;
            Assert.Single(linkCriteria.Conditions);
            
            Assert.Equal("name", linkCriteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Equal, linkCriteria.Conditions[0].Operator);
            Assert.Equal("Contoso", linkCriteria.Conditions[0].Values.FirstOrDefault());
        }
        
        [Fact]
        public void Should_return_contact_with_any_operator_that_matches_an_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var fetch = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                        <filter type='or'>
                            <link-entity name='account' to='contactid' from='primarycontactid' link-type='not-all'>
                                <filter type='and'>
                                    <condition attribute='name' operator='eq' value='Contoso' />
                                </filter>
                            </link-entity>
                        <condition attribute='statecode' operator='eq' value='1' />
                        </filter>
                    </entity>
                   </fetch>
                ";
            
            var result = _service.RetrieveMultiple(new FetchExpression(fetch));
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
        
        [Fact]
        public void Should_not_return_duplicate_contacts_with_any_operator_that_matches_more_than_one_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount, _contAccount });
            
            var fetch = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                        <filter type='or'>
                            <link-entity name='account' to='contactid' from='primarycontactid' link-type='not-all'>
                                <filter type='and'>
                                    <condition attribute='name' operator='begins-with' value='Cont' />
                                </filter>
                            </link-entity>
                        <condition attribute='statecode' operator='eq' value='1' />
                        </filter>
                    </entity>
                   </fetch>
                ";
            
            var result = _service.RetrieveMultiple(new FetchExpression(fetch));
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
        
        [Fact]
        public void Should_not_return_contact_records_with_any_operator_if_link_criteria_with_the_and_operator_does_not_match()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            //It's impossible the same account record can have 2 different names, it's used to return no matching records
            
            var fetch = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                        <filter type='or'>
                            <link-entity name='account' to='contactid' from='primarycontactid' link-type='not-all'>
                                <filter type='and'>
                                    <condition attribute='name' operator='eq' value='Cont' />
                                    <condition attribute='name' operator='eq' value='Some other' />
                                </filter>
                            </link-entity>
                        <condition attribute='statecode' operator='eq' value='1' />
                        </filter>
                    </entity>
                   </fetch>
                ";
            
            var result = _service.RetrieveMultiple(new FetchExpression(fetch));
            Assert.Empty(result.Entities);
        }
    }
}
#endif
