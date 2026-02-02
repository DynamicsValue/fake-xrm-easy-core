#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class GroupByDateTimeGroupingTests: FakeXrmEasyTestsBase
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
                LastOnHoldTime = new DateTime(2025, 1, 1)
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 5,
                LastOnHoldTime = new DateTime(2026, 1, 1)

            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 1,
                LastOnHoldTime = new DateTime(2026, 1, 2)
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 2,
                LastOnHoldTime = new DateTime(2026, 2, 1)
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 0,
                LastOnHoldTime = new DateTime(2026, 1, 8)
            });
            _entities.Add(new Account()
            {
                Id = Guid.NewGuid(),
                NumberOfEmployees = 40,
                LastOnHoldTime = null
            });
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_year()
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
                            AttributeName = "lastonholdtime",
                            Alias = "year",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true,
                            DateTimeGrouping = XrmDateTimeGrouping.Year
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And),
                Orders = { new OrderExpression("lastonholdtime", OrderType.Ascending, "year") }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(3, entityCollection.Entities.Count);

            var nullAccounts = entityCollection.Entities[0];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.False(nullAccounts.Contains("year"));
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            
            var lastYearAccounts = entityCollection.Entities[1];
            var lastYearSumEmployees = lastYearAccounts["sumofemployees"];
            var lastYear = lastYearAccounts["year"];
            Assert.IsType<AliasedValue>(lastYearSumEmployees);
            Assert.IsType<AliasedValue>(lastYear);
            Assert.Equal(10, ((AliasedValue) lastYearSumEmployees).Value);
            Assert.Equal(2025, ((AliasedValue) lastYear).Value);
            
            var thisYearAccounts = entityCollection.Entities[2];
            var thisYearSumEmployees = thisYearAccounts["sumofemployees"];
            var thisYear = thisYearAccounts["year"];
            Assert.IsType<AliasedValue>(thisYearSumEmployees);
            Assert.IsType<AliasedValue>(thisYear);
            Assert.Equal(8, ((AliasedValue) thisYearSumEmployees).Value);
            Assert.Equal(2026, ((AliasedValue) thisYear).Value);
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_month()
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
                            AttributeName = "lastonholdtime",
                            Alias = "month",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true,
                            DateTimeGrouping = XrmDateTimeGrouping.Month
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And),
                Orders = { new OrderExpression("lastonholdtime", OrderType.Ascending, "month") }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(3, entityCollection.Entities.Count);

            var nullAccounts = entityCollection.Entities[0];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.False(nullAccounts.Contains("month"));
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            
            var thisMonthAccounts = entityCollection.Entities[1];
            var thisMonthSumEmployees = thisMonthAccounts["sumofemployees"];
            var thisMonth = thisMonthAccounts["month"];
            Assert.IsType<AliasedValue>(thisMonthSumEmployees);
            Assert.IsType<AliasedValue>(thisMonth);
            Assert.Equal(16, ((AliasedValue) thisMonthSumEmployees).Value);
            Assert.Equal(1, ((AliasedValue) thisMonth).Value);
            
            var nextMonthAccounts = entityCollection.Entities[2];
            var nextMonthSumEmployees = nextMonthAccounts["sumofemployees"];
            var nextMonth = nextMonthAccounts["month"];
            Assert.IsType<AliasedValue>(nextMonthSumEmployees);
            Assert.IsType<AliasedValue>(nextMonth);
            Assert.Equal(2, ((AliasedValue) nextMonthSumEmployees).Value);
            Assert.Equal(2, ((AliasedValue) nextMonth).Value);
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_day()
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
                            AttributeName = "lastonholdtime",
                            Alias = "day",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true,
                            DateTimeGrouping = XrmDateTimeGrouping.Day
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And),
                Orders = { new OrderExpression("lastonholdtime", OrderType.Ascending, "day") }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(4, entityCollection.Entities.Count);

            var nullAccounts = entityCollection.Entities[0];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.False(nullAccounts.Contains("day"));
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            
            var thisDayAccounts = entityCollection.Entities[1];
            var thisDaySumEmployees = thisDayAccounts["sumofemployees"];
            var thisDay = thisDayAccounts["day"];
            Assert.IsType<AliasedValue>(thisDaySumEmployees);
            Assert.IsType<AliasedValue>(thisDay);
            Assert.Equal(17, ((AliasedValue) thisDaySumEmployees).Value);
            Assert.Equal(1, ((AliasedValue) thisDay).Value);
            
            var nextDayAccounts = entityCollection.Entities[2];
            var nextDaySumEmployees = nextDayAccounts["sumofemployees"];
            var nextDay = nextDayAccounts["day"];
            Assert.IsType<AliasedValue>(nextDaySumEmployees);
            Assert.IsType<AliasedValue>(nextDay);
            Assert.Equal(1, ((AliasedValue) nextDaySumEmployees).Value);
            Assert.Equal(2, ((AliasedValue) nextDay).Value);
            
            var nextWeekDayAccounts = entityCollection.Entities[3];
            var nextWeekDaySumEmployees = nextWeekDayAccounts["sumofemployees"];
            var nextWeekDay = nextWeekDayAccounts["day"];
            Assert.IsType<AliasedValue>(nextWeekDaySumEmployees);
            Assert.IsType<AliasedValue>(nextWeekDay);
            Assert.Equal(0, ((AliasedValue) nextWeekDaySumEmployees).Value);
            Assert.Equal(8, ((AliasedValue) nextWeekDay).Value);
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_week()
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
                            AttributeName = "lastonholdtime",
                            Alias = "week",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true,
                            DateTimeGrouping = XrmDateTimeGrouping.Week
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And),
                Orders = { new OrderExpression("lastonholdtime", OrderType.Ascending, "week") }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(4, entityCollection.Entities.Count);

            var nullAccounts = entityCollection.Entities[0];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.False(nullAccounts.Contains("week"));
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            
            var thisWeekAccounts = entityCollection.Entities[1];
            var thisWeekSumEmployees = thisWeekAccounts["sumofemployees"];
            var thisWeek = thisWeekAccounts["week"];
            Assert.IsType<AliasedValue>(thisWeekSumEmployees);
            Assert.IsType<AliasedValue>(thisWeek);
            Assert.Equal(16, ((AliasedValue) thisWeekSumEmployees).Value);
            Assert.Equal(1, ((AliasedValue) thisWeek).Value);
            
            var nextWeekAccounts = entityCollection.Entities[2];
            var nextWeekSumEmployees = nextWeekAccounts["sumofemployees"];
            var nextWeek = nextWeekAccounts["week"];
            Assert.IsType<AliasedValue>(nextWeekSumEmployees);
            Assert.IsType<AliasedValue>(nextWeek);
            Assert.Equal(0, ((AliasedValue) nextWeekSumEmployees).Value);
            Assert.Equal(2, ((AliasedValue) nextWeek).Value);
            
            var weekInNextMonthAccounts = entityCollection.Entities[3];
            var weekInNextMonthSumEmployees = weekInNextMonthAccounts["sumofemployees"];
            var weekInNextMonth = weekInNextMonthAccounts["week"];
            Assert.IsType<AliasedValue>(weekInNextMonthSumEmployees);
            Assert.IsType<AliasedValue>(weekInNextMonth);
            Assert.Equal(2, ((AliasedValue) weekInNextMonthSumEmployees).Value);
            Assert.Equal(6, ((AliasedValue) weekInNextMonth).Value);
        }
        
        [Fact]
        public void Should_return_correct_sum_when_grouping_by_quarter()
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
                            AttributeName = "lastonholdtime",
                            Alias = "quarter",
                            AggregateType = XrmAggregateType.None,
                            HasGroupBy = true,
                            DateTimeGrouping = XrmDateTimeGrouping.Quarter
                        }
                    }
                },
                Criteria = new FilterExpression(LogicalOperator.And),
                Orders = { new OrderExpression("lastonholdtime", OrderType.Ascending, "quarter") }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Equal(2, entityCollection.Entities.Count);

            var nullAccounts = entityCollection.Entities[0];
            var nullAccountSumEmployees = nullAccounts["sumofemployees"];
            Assert.False(nullAccounts.Contains("quarter"));
            Assert.IsType<AliasedValue>(nullAccountSumEmployees);
            Assert.Equal(40, ((AliasedValue) nullAccountSumEmployees).Value);
            
            var quarterAccounts = entityCollection.Entities[1];
            var quarterSumEmployees = quarterAccounts["sumofemployees"];
            var quarterValue = quarterAccounts["quarter"];
            Assert.IsType<AliasedValue>(quarterSumEmployees);
            Assert.IsType<AliasedValue>(quarterValue);
            Assert.Equal(18, ((AliasedValue) quarterSumEmployees).Value);
            Assert.Equal(1, ((AliasedValue) quarterValue).Value);
        }
    }
}
#endif