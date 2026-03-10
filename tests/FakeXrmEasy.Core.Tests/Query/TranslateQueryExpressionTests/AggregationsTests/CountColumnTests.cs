#if FAKE_XRM_EASY_9
using System;
using System.Linq;
using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class CountColumnTests: FakeXrmEasyTestsBase
    {
        
        [Fact]
        public void Should_return_correct_count_column_value_as_an_aliased_value()
        {
            // Arrange
            var numberOfAccounts = 10;
            var accounts = Enumerable.Range(1, numberOfAccounts)
                .Select(i => new Account() {
                    Id = Guid.NewGuid(),
                    Name = $"Test {i}",
                    NumberOfEmployees = i
                })
                .ToList();
            
            //Adds null value (which should be excluded)
            accounts.Add(new Account()
            {
                Id = Guid.NewGuid(),
                Name = $"Test 11",
                NumberOfEmployees = null
            });
            
            _context.Initialize(accounts);
            
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "numberofemployees",
                            Alias = "accountcount",
                            AggregateType = XrmAggregateType.CountColumn
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultingEntity = entityCollection.Entities[0];
            var aggregatedField = resultingEntity["accountcount"];
            Assert.IsType<AliasedValue>(aggregatedField);

            Assert.Equal(numberOfAccounts, ((AliasedValue) aggregatedField).Value);
        }
    }
}
#endif