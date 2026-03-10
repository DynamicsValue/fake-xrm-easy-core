#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class GroupByTests: FakeXrmEasyTestsBase
    {
        private List<Entity> _entities;

        private void InitEntities()
        {
            // Arrange
            _entities = new List<Entity>();
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 10,
                CreditLimit = new Money(10m),
                Address1_Latitude = 10,
                Address1_City = "Barcelona",
                Address1_PostalCode = "08620"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 5,
                CreditLimit = null,
                Address1_Latitude = 5,
                Address1_City = "Barcelona",
                Address1_PostalCode = "08620"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 1,
                CreditLimit = new Money(1m),
                Address1_Latitude = 1,
                Address1_City = "Tarragona",
                Address1_PostalCode = "43008"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 2,
                CreditLimit = new Money(2m),
                Address1_Latitude = 2,
                Address1_City = "Tarragona",
                Address1_PostalCode = "43008"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 0,
                CreditLimit = new Money(0m),
                Address1_Latitude = 0,
                Address1_City = "Tarragona",
                Address1_PostalCode = "43003"
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 40,
                CreditLimit = new Money(10m),
                Address1_Latitude = 10,
                Address1_City = null,
                Address1_PostalCode = "08620"
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
            Assert.Equal(3, entityCollection.Entities.Count);

            var barcelonaAccounts = entityCollection.Entities[0];
            var barcelonaSumEmployees = barcelonaAccounts["sumofemployees"];
            var barcelonaCity = barcelonaAccounts["city"];
            Assert.IsType<AliasedValue>(barcelonaSumEmployees);
            Assert.Equal(15, ((AliasedValue) barcelonaSumEmployees).Value);
            Assert.Equal("Barcelona", ((AliasedValue) barcelonaCity).Value);
            
            var tarragonaAccounts = entityCollection.Entities[1];
            var tarragonaSumEmployees = tarragonaAccounts["sumofemployees"];
            var tarragonaCity = tarragonaAccounts["city"];
            Assert.IsType<AliasedValue>(tarragonaSumEmployees);
            Assert.Equal(3, ((AliasedValue) tarragonaSumEmployees).Value);
            Assert.Equal("Tarragona", ((AliasedValue) tarragonaCity).Value);
            
            var nullAccounts = entityCollection.Entities[2];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            Assert.False(nullAccounts.Contains("city"));
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_city_and_postalcode()
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
                        },
                        new XrmAttributeExpression{
                            AttributeName = "address1_postalcode",
                            Alias = "zipcode",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(4, entityCollection.Entities.Count);

            var barcelonaAccounts = entityCollection.Entities[0];
            var barcelonaSumEmployees = barcelonaAccounts["sumofemployees"];
            var barcelonaCity = barcelonaAccounts["city"];
            var barcelonaZipCode = barcelonaAccounts["zipcode"];
            Assert.IsType<AliasedValue>(barcelonaSumEmployees);
            Assert.IsType<AliasedValue>(barcelonaCity);
            Assert.IsType<AliasedValue>(barcelonaZipCode);
            
            Assert.Equal(15, ((AliasedValue) barcelonaSumEmployees).Value);
            Assert.Equal("numberofemployees", ((AliasedValue) barcelonaSumEmployees).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) barcelonaSumEmployees).EntityLogicalName);
            
            Assert.Equal("Barcelona", ((AliasedValue) barcelonaCity).Value);
            Assert.Equal("address1_city", ((AliasedValue) barcelonaCity).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) barcelonaCity).EntityLogicalName);
            
            Assert.Equal("08620", ((AliasedValue) barcelonaZipCode).Value);
            Assert.Equal("address1_postalcode", ((AliasedValue) barcelonaZipCode).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) barcelonaZipCode).EntityLogicalName);
            
            var tarragonaAccounts1 = entityCollection.Entities[1];
            var tarragonaSumEmployees1 = tarragonaAccounts1["sumofemployees"];
            var tarragonaCity1 = tarragonaAccounts1["city"];
            var tarragonaZipCode1 = tarragonaAccounts1["zipcode"];
            Assert.IsType<AliasedValue>(tarragonaSumEmployees1);
            Assert.IsType<AliasedValue>(tarragonaCity1);
            Assert.IsType<AliasedValue>(tarragonaZipCode1);
            
            Assert.Equal(3, ((AliasedValue) tarragonaSumEmployees1).Value);
            Assert.Equal("numberofemployees", ((AliasedValue) tarragonaSumEmployees1).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaSumEmployees1).EntityLogicalName);
            
            Assert.Equal("Tarragona", ((AliasedValue) tarragonaCity1).Value);
            Assert.Equal("address1_city", ((AliasedValue) tarragonaCity1).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaCity1).EntityLogicalName);
            
            Assert.Equal("43008", ((AliasedValue) tarragonaZipCode1).Value);
            Assert.Equal("address1_postalcode", ((AliasedValue) tarragonaZipCode1).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaZipCode1).EntityLogicalName);
            
            var tarragonaAccounts2 = entityCollection.Entities[2];
            var tarragonaSumEmployees2 = tarragonaAccounts2["sumofemployees"];
            var tarragonaCity2 = tarragonaAccounts2["city"];
            var tarragonaZipCode2 = tarragonaAccounts2["zipcode"];
            Assert.IsType<AliasedValue>(tarragonaSumEmployees2);
            Assert.IsType<AliasedValue>(tarragonaCity2);
            Assert.IsType<AliasedValue>(tarragonaZipCode2);
            
            Assert.Equal(0, ((AliasedValue) tarragonaSumEmployees2).Value);
            Assert.Equal("numberofemployees", ((AliasedValue) tarragonaSumEmployees2).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaSumEmployees2).EntityLogicalName);
            
            Assert.Equal("Tarragona", ((AliasedValue) tarragonaCity2).Value);
            Assert.Equal("address1_city", ((AliasedValue) tarragonaCity2).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaCity2).EntityLogicalName);
            
            Assert.Equal("43003", ((AliasedValue) tarragonaZipCode2).Value);
            Assert.Equal("address1_postalcode", ((AliasedValue) tarragonaZipCode2).AttributeLogicalName);
            Assert.Equal("account", ((AliasedValue) tarragonaZipCode2).EntityLogicalName);
        }
        
        [Fact]
        public void Should_return_correct_count_when_grouping_by_city()
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
                            Alias = "countofaccounts",
                            AggregateType = XrmAggregateType.Count
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
            Assert.Equal(3, entityCollection.Entities.Count);

            var barcelonaAccounts = entityCollection.Entities[0];
            var barcelonaNumberOfAccounts = barcelonaAccounts["countofaccounts"];
            var barcelonaCity = barcelonaAccounts["city"];
            Assert.IsType<AliasedValue>(barcelonaNumberOfAccounts);
            Assert.Equal(2, ((AliasedValue) barcelonaNumberOfAccounts).Value);
            Assert.Equal("Barcelona", ((AliasedValue) barcelonaCity).Value);
            
            var tarragonaAccounts = entityCollection.Entities[1];
            var tarragonaNumberOfAccounts = tarragonaAccounts["countofaccounts"];
            var tarragonaCity = tarragonaAccounts["city"];
            Assert.IsType<AliasedValue>(tarragonaNumberOfAccounts);
            Assert.Equal(3, ((AliasedValue) tarragonaNumberOfAccounts).Value);
            Assert.Equal("Tarragona", ((AliasedValue) tarragonaCity).Value);
        }
        
        [Fact]
        public void Should_return_correct_count_column_when_grouping_by_city()
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
                            Alias = "countofcreditlimit",
                            AggregateType = XrmAggregateType.CountColumn
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
            Assert.Equal(3, entityCollection.Entities.Count);

            var barcelonaAccounts = entityCollection.Entities[0];
            var barcelonaNumberOfAccounts = barcelonaAccounts["countofcreditlimit"];
            var barcelonaCity = barcelonaAccounts["city"];
            Assert.IsType<AliasedValue>(barcelonaNumberOfAccounts);
            Assert.Equal(1, ((AliasedValue) barcelonaNumberOfAccounts).Value);
            Assert.Equal("Barcelona", ((AliasedValue) barcelonaCity).Value);
            
            var tarragonaAccounts = entityCollection.Entities[1];
            var tarragonaNumberOfAccounts = tarragonaAccounts["countofcreditlimit"];
            var tarragonaCity = tarragonaAccounts["city"];
            Assert.IsType<AliasedValue>(tarragonaNumberOfAccounts);
            Assert.Equal(3, ((AliasedValue) tarragonaNumberOfAccounts).Value);
            Assert.Equal("Tarragona",  ((AliasedValue) tarragonaCity).Value);
        }
    }
}
#endif