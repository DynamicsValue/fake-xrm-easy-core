using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class SumTests: FakeXrmEasyTestsBase
    {
        private int _totalSum = 0;
        private List<Entity> _entities;
        
        private void InitEntities()
        {
            // Arrange
            var numberOfAccounts = 10;
            _entities = new List<Entity>();
            for (var i = 0; i < numberOfAccounts; i++)
            {
                _entities.Add(new Account()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test {i}",
                    NumberOfEmployees = 1 + i,
                    CreditLimit = new Money(1m + i),
                    Address1_Latitude = 1 + i
                });
                
                _entities.Add(new dv_test()
                {
                    Id = Guid.NewGuid(),
                    dv_decimal = 1 + i
                });
                _totalSum += 1 + i;
            }
        }
        
        [Fact]
        public void Should_return_correct_int_sum_value_as_an_aliased_value()
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
                            Alias = "accountsum",
                            AggregateType = XrmAggregateType.Sum
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountsum"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(_totalSum, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_double_sum_value_as_an_aliased_value()
        {
            InitEntities();
            _context.Initialize(_entities);
            
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "address1_latitude",
                            Alias = "accountsum",
                            AggregateType = XrmAggregateType.Sum
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountsum"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal((double) _totalSum, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_money_sum_value_as_an_aliased_value()
        {
            InitEntities();
            _context.Initialize(_entities);
            
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "creditlimit",
                            Alias = "accountsum",
                            AggregateType = XrmAggregateType.Sum
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountsum"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(_totalSum, ((Money)((AliasedValue) aggregatedField).Value).Value);
        }
        
        [Fact]
        public void Should_return_correct_decimal_sum_value_as_an_aliased_value()
        {
            InitEntities();
            _context.Initialize(_entities);
            
            QueryExpression query = new QueryExpression(dv_test.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "dv_decimal",
                            Alias = "testsum",
                            AggregateType = XrmAggregateType.Sum
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["testsum"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal((decimal) _totalSum, ((AliasedValue) aggregatedField).Value);
        }
    }
}