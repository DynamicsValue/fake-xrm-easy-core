#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions.Settings;
using FakeXrmEasy.Core.Query.Aggregations;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class EntityKeyGroupDateTimeGroupingEqualsTests: FakeXrmEasyTestsBase
    {
        private readonly Entity _entity;
        private readonly Entity _otherEntity;
        private List<XrmAttributeExpression> _attributeExpressions;
        private readonly XrmAttributeExpression _dateTimeGrouping;
        
        public EntityKeyGroupDateTimeGroupingEqualsTests()
        {
            _dateTimeGrouping = new XrmAttributeExpression()
            {
                AttributeName = "createdon",
                Alias = "createdOnAlias",
                HasGroupBy = true,
                DateTimeGrouping = XrmDateTimeGrouping.Year
            };
            _entity = new Entity();
            _otherEntity = new Entity();
        }

        [Theory]
        [InlineData(true, 2025, 2025)]
        [InlineData(false, 2023, 2022)]
        [InlineData(false, 2022, 2023)]
        [InlineData(true, null, null)]
        [InlineData(false, null, 2026)]
        [InlineData(false, 2026, null)]
        public void Should_return_correct_result_when_using_date_time_grouping_by_year(bool areEqual, int? year1, int? year2)
        {
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (year1 != null)
            {
                value1 = new DateTime(year1.Value, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (year2 != null)
            {
                value2 = new DateTime(year2.Value, 2, 2, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1)]
        [InlineData(true, 12, 12)]
        [InlineData(false, 1, 2)]
        [InlineData(false, 4, 3)]
        [InlineData(true, null, null)]
        [InlineData(false, null, 2)]
        [InlineData(false, 12, null)]
        public void Should_return_correct_result_when_using_date_time_grouping_by_month(bool areEqual, int? month1, int? month2)
        {
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.Month;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (month1 != null)
            {
                value1 = new DateTime(2026, month1.Value, 1, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (month2 != null)
            {
                value2 = new DateTime(2026, month2.Value, 2, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context,_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1)]
        [InlineData(true, 28, 28)]
        [InlineData(false, 1, 2)]
        [InlineData(false, 4, 3)]
        [InlineData(true, null, null)]
        [InlineData(false, null, 28)]
        [InlineData(false, 31, null)]
        public void Should_return_correct_result_when_using_date_time_grouping_by_day(bool areEqual, int? day1, int? day2)
        {
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.Day;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (day1 != null)
            {
                value1 = new DateTime(2026, 1, day1.Value, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (day2 != null)
            {
                value2 = new DateTime(2026, 2, day2.Value, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context,_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1, 1, 1)]
        [InlineData(true, 1, 3, 1, 1)]
        [InlineData(true, 4, 10, 1, 1)]
        [InlineData(false, 4, 11, 1, 1)]
        [InlineData(false, 1, 8, 1, 1)]
        [InlineData(false, 1, 1, 1, 2)]
        [InlineData(true, null, null, 1, 1)]
        [InlineData(false, null, 1, 1,1)]
        public void Should_return_correct_result_when_using_date_time_grouping_by_week(bool areEqual, int? day1, int? day2, int month1, int month2)
        {
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.Week;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (day1 != null)
            {
                value1 = new DateTime(2026, month1, day1.Value, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (day2 != null)
            {
                value2 = new DateTime(2026, month2, day2.Value, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context, _otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1, 1, 1)] //Same date
        [InlineData(true, 1, 1, 31, 3)] // 1/1 => 31/3 == Q1
        [InlineData(false, 1, 1, 1, 4)] // 1/1 (Q1) => 1/4 (Q2)
        [InlineData(true, 1, 4, 30, 6)] // 1/4 => 30/6 == Q2
        [InlineData(false, 1, 4, 1, 7)] // 1/4 (Q2) => 1/7 (Q3)
        [InlineData(true, 1, 7, 30, 9)] // 1/7 => 30/9 == Q3
        [InlineData(false, 1, 7, 1, 10)] // 1/7 (Q3) => 1/10 (Q4)
        [InlineData(true, 1, 10, 31, 12)] // 1/10 => 31/12 == Q4
        [InlineData(false, 1, 10, 1, 1)] // 1/10 (Q4) => 1/1 (Q1)
        [InlineData(true, null, 1, null, 1)]
        [InlineData(false, null, 1, 1,1)]
        public void Should_return_correct_result_when_using_date_time_grouping_by_quarter(bool areEqual, int? day1, int month1, int? day2, int month2)
        {
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.Quarter;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (day1 != null)
            {
                value1 = new DateTime(2026, month1, day1.Value, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (day2 != null)
            {
                value2 = new DateTime(2026, month2, day2.Value, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context,_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1, 2026,1, 1, 2026)] //Same date
        [InlineData(true, 1, 1, 2026, 31, 3, 2026)] //Dates in same fy
        [InlineData(true, 1, 1, 2026,1, 4, 2026)] //Dates in same fy
        [InlineData(true, 1, 1, 2026,30, 6, 2026)] //Dates in same fy
        [InlineData(true, 1, 7, 2026,31, 12, 2026)] //Dates in same fy
        [InlineData(true, 30, 6, 2026,1, 7, 2026)] //Dates in same fy
        [InlineData(true, 1, 1, 2026,31, 12, 2026)] // Same year 1/1 , 31/12 
        [InlineData(false, 1, 1, 2025,31, 3, 2026)] // Different years 1/1/2025  31/3/2026
        [InlineData(false, 31, 12, 2025,1, 1, 2026)] // Different years 31/12/2025  1/1/2026
        [InlineData(true, null, 1, 2026,null, 1, 2026)] //null and null
        [InlineData(false, null, 1,2026, 1,1, 2026)] //null and not null
        public void Should_return_correct_result_when_using_date_time_grouping_by_fiscal_year_when_fiscal_year_starts_on_Jan_1st(bool areEqual, int? day1, int month1, int year1, int? day2, int month2, int year2)
        {
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.FiscalYear;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (day1 != null)
            {
                value1 = new DateTime(year1, month1, day1.Value, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (day2 != null)
            {
                value2 = new DateTime(year2, month2, day2.Value, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context,_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Theory]
        [InlineData(true, 1, 1, 2026,1, 1, 2026)] //Same date (fy 2025)
        [InlineData(true, 1, 1, 2026, 31, 3, 2026)] //Dates in same fy (2025)
        [InlineData(true, 1, 1, 2026,1, 4, 2026)] //Dates in same fy (2025)
        [InlineData(true, 1, 1, 2026,30, 6, 2026)] //Dates in same fy (2025)
        [InlineData(true, 1, 7, 2026,31, 12, 2026)] //Dates in same fy (2026)
        [InlineData(false, 1, 7, 2025,1, 7, 2026)] //Dates different fy (2025, 2026)
        [InlineData(false, 30, 6, 2026,1, 7, 2026)] //Dates different fy (2025, 2026)
        [InlineData(false, 1, 1, 2026,31, 12, 2026)] // Dates different fy (2025, 2026) 
        [InlineData(false, 1, 1, 2025,31, 12, 2026)] // Different years 1/1/2025 (2024) 31/3/2026 (2026)
        [InlineData(true, 31, 12, 2025,1, 1, 2026)] // Different same fy 31/12/2025  1/1/2026 (2025)
        [InlineData(true, null, 1, 2026,null, 1, 2026)] //null and null
        [InlineData(false, null, 1,2026, 1,1, 2026)] //null and not null
        public void Should_return_correct_result_when_using_date_time_grouping_by_fiscal_year_when_fiscal_year_starts_on_July_1st(bool areEqual, int? day1, int month1, int year1, int? day2, int month2, int year2)
        {
            _context.SetProperty(new FiscalYearSettings()
            {
                StartDate = new DateTime(2026, 7, 1) //July 1st
            });
            
            _dateTimeGrouping.DateTimeGrouping = XrmDateTimeGrouping.FiscalYear;
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                _dateTimeGrouping
            };
            
            object value1 = null;
            if (day1 != null)
            {
                value1 = new DateTime(year1, month1, day1.Value, 1, 1, 1, DateTimeKind.Utc);
            }

            object value2 = null;
            if (day2 != null)
            {
                value2 = new DateTime(year2, month2, day2.Value, 2, 2, 2, DateTimeKind.Utc);
            }
            _entity["createdon"] = value1;
            _otherEntity["createdon"]  = value2;
            
            var entityGroupKey = new EntityGroupKey(_context,_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context,_otherEntity, _attributeExpressions);
            
            Assert.Equal(areEqual, entityGroupKey.Equals(otherEntityGroupKey));
        }
    }
}
#endif