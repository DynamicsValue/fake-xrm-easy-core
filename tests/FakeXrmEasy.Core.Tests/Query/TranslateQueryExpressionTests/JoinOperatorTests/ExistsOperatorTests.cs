#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.JoinOperatorTests
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
        public void Should_return_contact_with_exists_operator_that_matches_an_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                LinkEntities =
                {
                    new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.Exists)
                    {
                        LinkCriteria = new FilterExpression(filterOperator: LogicalOperator.And)
                        {
                            Conditions = {
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.Equal,
                                    value: "Contoso")
                            }
                        }
                    },
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
        
        [Fact]
        public void Should_not_return_duplicate_contacts_with_exists_operator_that_matches_more_than_one_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount, _contAccount });
            
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                LinkEntities =
                {
                    new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.Exists)
                    {
                        LinkCriteria = new FilterExpression(filterOperator: LogicalOperator.And)
                        {
                            Conditions = {
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.BeginsWith,
                                    value: "Cont")
                            }
                        }
                    },
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
        
        [Fact]
        public void Should_not_return_contact_records_with_exists_operator_if_link_criteria_with_the_and_operator_does_not_match()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            //It's impossible the same account record can have 2 different names, it's used to return no matching records
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                LinkEntities =
                {
                    new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.Exists)
                    {
                        LinkCriteria = new FilterExpression(filterOperator: LogicalOperator.And)
                        {
                            Conditions = {
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.Equal,
                                    value: "Contoso"),
                                
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.Equal,
                                    value: "Non existing name")
                            }
                        }
                    }
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
        }
    }
}
#endif