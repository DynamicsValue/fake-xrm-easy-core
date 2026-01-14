#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class MinTests: FakeXrmEasyTestsBase
    {
        private int _numberOfAccounts = 3;
        private List<Entity> _entities;
        private int _minValue = 3;
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
                    Address1_Latitude = i,
                    LastOnHoldTime = DateTime.UtcNow.AddDays(i)
                });
                
                _entities.Add(new dv_test()
                {
                    Id = Guid.NewGuid(),
                    dv_decimal = i
                });
            }
        }
        
        [Fact]
        public void Should_return_correct_int_min_value_as_an_aliased_value()
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
                            Alias = "accountmin",
                            AggregateType = XrmAggregateType.Min
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountmin"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(1, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_double_min_value_as_an_aliased_value()
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
                            Alias = "accountmin",
                            AggregateType = XrmAggregateType.Min
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountmin"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(1.0, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_money_min_value_as_an_aliased_value()
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
                            Alias = "accountmin",
                            AggregateType = XrmAggregateType.Min
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountmin"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(1m, ((Money)((AliasedValue) aggregatedField).Value).Value);
        }
        
        [Fact]
        public void Should_return_correct_decimal_min_value_as_an_aliased_value()
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
                            Alias = "testmin",
                            AggregateType = XrmAggregateType.Min
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["testmin"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(1m, ((AliasedValue) aggregatedField).Value);
        }
        
        [Fact]
        public void Should_return_correct_datetime_min_value_as_an_aliased_value()
        {
            InitEntities();
            _context.Initialize(_entities);
            
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "lastonholdtime",
                            Alias = "accountmin",
                            AggregateType = XrmAggregateType.Min
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountmin"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(((Account)_entities[0]).LastOnHoldTime.Value.Date, ((DateTime) ((AliasedValue) aggregatedField).Value).Date);
        }
    }
}
#endif