using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class AvgTests: FakeXrmEasyTestsBase
    {
        private int _totalSum = 0;
        private int _numberOfAccounts = 3;
        private List<Entity> _entities;
        
        private void InitEntities()
        {
            // Arrange
            _entities = new List<Entity>();
            for (var i = 1; i <= _numberOfAccounts; i++)
            {
                _entities.Add(new Account()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test {i}",
                    NumberOfEmployees = i,
                    CreditLimit = new Money(i),
                    Address1_Latitude = i
                });
                
                _entities.Add(new dv_test()
                {
                    Id = Guid.NewGuid(),
                    dv_decimal = i
                });
                _totalSum += i;
            }
            
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                Name = $"Test 11",
                NumberOfEmployees = null,
                CreditLimit = null,
                Address1_Latitude = null
            });
                
            _entities.Add(new dv_test()
            {
                Id = Guid.NewGuid(),
                dv_decimal = null
            });
        }
        
        [Fact]
        public void Should_return_correct_int_avg_value_as_an_aliased_value()
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
                            Alias = "accountavg",
                            AggregateType = XrmAggregateType.Avg
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountavg"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(_totalSum / _numberOfAccounts, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_double_avg_value_as_an_aliased_value()
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
                            Alias = "accountavg",
                            AggregateType = XrmAggregateType.Avg
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountavg"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal((double) _totalSum / _numberOfAccounts, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_money_avg_value_as_an_aliased_value()
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
                            Alias = "accountavg",
                            AggregateType = XrmAggregateType.Avg
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountavg"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal((decimal) _totalSum / _numberOfAccounts, ((Money)((AliasedValue) aggregatedField).Value).Value);
        }
        
        [Fact]
        public void Should_return_correct_decimal_avg_value_as_an_aliased_value()
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
                            Alias = "testavg",
                            AggregateType = XrmAggregateType.Avg
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["testavg"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal((decimal) _totalSum / _numberOfAccounts, ((AliasedValue) aggregatedField).Value);
        }
    }
}