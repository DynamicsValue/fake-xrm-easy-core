#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Core.Exceptions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class SumTestsLateBound: FakeXrmEasyTestsBase
    {
        private int _totalSum = 0;
        private List<Entity> _entities;
        private List<EntityMetadata> _entityMetadatas;
        private void InitEntities()
        {
            // Arrange
            var numberOfAccounts = 10;
            _entities = new List<Entity>();
            for (var i = 0; i < numberOfAccounts; i++)
            {
                _entities.Add(new Entity("account")
                {
                    Id = Guid.NewGuid(),
                    ["name"] = $"Test {i}",
                    ["numberofemployees"] = 1 + i,
                    ["creditlimit"] = new Money(1m + i),
                    ["address1_latitude"] = 1.0 + i
                });
                
                _entities.Add(new Entity("dv_test")
                {
                    Id = Guid.NewGuid(),
                    ["dv_decimal"] = 1m + i
                });
                _totalSum += 1 + i;
            }
        }

        private void InitMetadata()
        {
            var accountEntityMetadata = new EntityMetadata()
            {
                LogicalName = "account"
            };

            var numberOfEmployeesMetadata = new IntegerAttributeMetadata() { LogicalName = "numberofemployees" };
            var creditLimitMetadata = new MoneyAttributeMetadata() { LogicalName = "creditlimit" };
            var addressLatitudeMetadata = new DoubleAttributeMetadata() { LogicalName = "address1_latitude" };
            
            accountEntityMetadata.SetAttributeCollection(new AttributeMetadata[]
            {
                numberOfEmployeesMetadata, creditLimitMetadata, addressLatitudeMetadata
            });
            
            var dvTestEntityMetadata = new EntityMetadata()
            {
                LogicalName = "dv_test"
            };
            var dvDecimalMetadata = new DecimalAttributeMetadata() { LogicalName = "dv_decimal" };
            
            dvTestEntityMetadata.SetAttributeCollection(new AttributeMetadata[]
            {
                dvDecimalMetadata
            });

            _entityMetadatas = new List<EntityMetadata>()
            {
                accountEntityMetadata, dvTestEntityMetadata
            };
        }
        [Fact]
        public void Should_throw_exception_when_both_early_bound_types_and_metadata_are_not_present()
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
            
            Assert.Throws<FindReflectedAttributeTypeNotFoundException>(() => _service.RetrieveMultiple(query));
        }
        
        [Fact]
        public void Should_return_correct_int_sum_value_as_an_aliased_value_when_metadata_is_present()
        {
            InitMetadata();
            _context.InitializeMetadata(_entityMetadatas);
            
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
        public void Should_return_correct_double_sum_value_as_an_aliased_value_when_metadata_is_present()
        {
            InitMetadata();
            _context.InitializeMetadata(_entityMetadatas);
            
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
        public void Should_return_correct_money_sum_value_as_an_aliased_value_when_metadata_is_present()
        {
            InitMetadata();
            _context.InitializeMetadata(_entityMetadatas);
            
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
        public void Should_return_correct_decimal_sum_value_as_an_aliased_value_when_metadata_is_present()
        {
            InitMetadata();
            _context.InitializeMetadata(_entityMetadatas);
            
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
#endif