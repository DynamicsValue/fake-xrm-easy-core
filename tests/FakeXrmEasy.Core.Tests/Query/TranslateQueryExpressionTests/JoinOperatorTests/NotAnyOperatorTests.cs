#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.JoinOperatorTests
{
    public class NotAnyOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        private readonly Account _contosoAccount;
        private readonly Account _contAccount;
        
        public NotAnyOperatorTests()
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
        public void Should_return_contact_with_not_any_operator_that_matches_an_account_record_with_a_different_name()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                Criteria = new FilterExpression(filterOperator: LogicalOperator.Or)
                {
                    AnyAllFilterLinkEntity = new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.NotAny)
                    {
                        LinkCriteria = new FilterExpression(filterOperator: LogicalOperator.And)
                        {
                            Conditions = {
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.Equal,
                                    value: "Other name")
                            }
                        }
                    },
                    Conditions = {
                        new ConditionExpression(
                            attributeName:"statecode",
                            conditionOperator: ConditionOperator.Equal,
                            value: 1)
                    }
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
        
        [Fact]
        public void Should_not_return_contact_with_not_any_operator_if_the_account_matches_that_name()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                Criteria = new FilterExpression(filterOperator: LogicalOperator.Or)
                {
                    AnyAllFilterLinkEntity = new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.NotAny)
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
                    Conditions = {
                        new ConditionExpression(
                            attributeName:"statecode",
                            conditionOperator: ConditionOperator.Equal,
                            value: 1)
                    }
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
        }
        
        [Fact]
        public void Should_not_return_duplicate_contacts_with_not_any_operator_that_does_not_match_more_than_one_account_record()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount, _contAccount });
            
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                Criteria = new FilterExpression(filterOperator: LogicalOperator.Or)
                {
                    AnyAllFilterLinkEntity = new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.NotAny)
                    {
                        LinkCriteria = new FilterExpression(filterOperator: LogicalOperator.And)
                        {
                            Conditions = {
                                new ConditionExpression(
                                    attributeName: "name",
                                    conditionOperator: ConditionOperator.BeginsWith,
                                    value: "Other name")
                            }
                        }
                    },
                    Conditions = {
                        new ConditionExpression(
                            attributeName:"statecode",
                            conditionOperator: ConditionOperator.Equal,
                            value: 1)
                    }
                }
            };
            
            var result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.Equal("Joe", result.Entities[0]["firstname"]);
        }
    }
}
#endif