using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class GroupBySumTests: FakeXrmEasyTestsBase
    {
        private int _totalSum = 0;
        private List<Entity> _entities;

        private void InitEntities()
        {
            // Arrange
            var numberOfAccounts = 10;
            _entities = new List<Entity>();
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 10,
                CreditLimit = new Money(10m),
                Address1_Latitude = 10,
                Address1_City = "Barcelona"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 5,
                CreditLimit = new Money(5m),
                Address1_Latitude = 5,
                Address1_City = "Barcelona"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 1,
                CreditLimit = new Money(1m),
                Address1_Latitude = 1,
                Address1_City = "Tarragona"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 2,
                CreditLimit = new Money(2m),
                Address1_Latitude = 2,
                Address1_City = "Tarragona"
            });
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_city()
        {
            InitEntities();
            _context.Initialize(_entities);
            
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "numberofemployees",
                            Alias = "sumofemployees",
                            AggregateType = XrmAggregateType.Sum
                        },
                        new XrmAttributeExpression{
                            AttributeName = "address1_city",
                            Alias = "city",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(2, entityCollection.Entities.Count);

            var barcelonaAccounts = entityCollection.Entities[0];
            var barcelonaSumEmployees = barcelonaAccounts["sumofemployees"];
            Assert.IsType<AliasedValue>(barcelonaSumEmployees);
            Assert.Equal(15, ((AliasedValue) barcelonaSumEmployees).Value);
            
            var tarragonaAccounts = entityCollection.Entities[1];
            var tarragonaSumEmployees = tarragonaAccounts["sumofemployees"];
            Assert.IsType<AliasedValue>(tarragonaSumEmployees);
            Assert.Equal(3, ((AliasedValue) tarragonaSumEmployees).Value);
        }
    }
}