using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.JoinOperatorTests
{
    public class AnyOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        private readonly Account _account;
        
        public AnyOperatorTests()
        {
            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Joe"
            };
            
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                PrimaryContactId = _contact.ToEntityReference()
            };
            
        }
        
        [Fact]
        public void Should_filter_records_with_any_operator()
        {
            _context.Initialize(new List<Entity>() {_contact, _account });
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("fullname"),
                Criteria = new FilterExpression(filterOperator: LogicalOperator.Or)
                {
                    AnyAllFilterLinkEntity = new LinkEntity(
                        linkFromEntityName: "contact",
                        linkToEntityName: "account",
                        linkFromAttributeName: "contactid",
                        linkToAttributeName: "primarycontactid",
                        joinOperator: JoinOperator.Any)
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
            Assert.Single(result.Entities);
        }
    }
    
    
}