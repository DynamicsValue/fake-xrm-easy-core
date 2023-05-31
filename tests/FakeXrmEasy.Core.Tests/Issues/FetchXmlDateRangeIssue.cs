using Crm;
using FakeXrmEasy.Tests;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class FetchXmlDateRangeIssue : FakeXrmEasyTestsBase
    {
        protected readonly Task _taskInRange;
        protected readonly Task _taskOutsideRange;

        public FetchXmlDateRangeIssue() : base()
        {
            _taskInRange = new Task()
            {
                Id = Guid.NewGuid()
            };

            _taskOutsideRange = new Task()
            {
                Id = Guid.NewGuid()
            };

            _taskInRange.ActualStart = new DateTime(2023, 4, 1, 00, 00, 00);
            _taskInRange.ActualEnd = new DateTime(2023, 6, 30, 23, 59, 59);

            _taskOutsideRange.ActualStart = new DateTime(2023, 1, 1, 00, 00, 00);
            _taskOutsideRange.ActualEnd = new DateTime(2023, 2, 1, 23, 59, 59);
        }

        [Fact]
        public void Should_return_record_within_a_valid_date_range_date_only_string()
        {
            _context.Initialize(new List<Entity>()
            {
                _taskInRange, _taskOutsideRange
            });

            var dateRangeValue = new DateTime(2023, 5, 1, 00, 00, 00);

            var fetchXml = $@"
                <fetch>
                    <entity name='{Task.EntityLogicalName}'>
                        <all-attributes />
                        <filter type='and'>
                            <condition attribute='actualend' operator='on-or-after' value='{dateRangeValue.ToString("yyyy-MM-dd")}' />
                            <condition attribute='actualstart' operator='on-or-before' value='{dateRangeValue.ToString("yyyy-MM-dd")}' />
                        </filter>
                    </entity>
                </fetch>
            ";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();
            Assert.Single(results);

        }

        [Fact]
        public void Should_return_record_within_a_valid_date_range_date()
        {
            _context.Initialize(new List<Entity>()
            {
                _taskInRange, _taskOutsideRange
            });

            var dateRangeValue = new DateTime(2023, 5, 1, 00, 00, 00);

            var fetchXml = $@"
                <fetch>
                    <entity name='{Task.EntityLogicalName}'>
                        <all-attributes />
                        <filter type='and'>
                            <condition attribute='actualend' operator='on-or-after' value='{dateRangeValue}' />
                            <condition attribute='actualstart' operator='on-or-before' value='{dateRangeValue}' />
                        </filter>
                    </entity>
                </fetch>
            ";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();
            Assert.Single(results);

        }

        [Theory]
        [InlineData(2023, 5, 1)]
        [InlineData(2023, 4, 1)]
        [InlineData(2026, 4, 1)]
        public void Should_return_record_on_or_before_a_given_date(int year, int month, int day)
        {
            _context.Initialize(new List<Entity>()
            {
                _taskInRange
            });

            var dateRangeValue = new DateTime(year, month, day, 00, 00, 00);

            var fetchXml = $@"
                <fetch>
                    <entity name='{Task.EntityLogicalName}'>
                        <all-attributes />
                        <filter type='and'>
                            <condition attribute='actualstart' operator='on-or-before' value='{dateRangeValue.ToString("yyyy-MM-dd")}' />
                        </filter>
                    </entity>
                </fetch>
            ";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();
            Assert.Single(results);
        }

        [Theory]
        [InlineData(2022, 5, 1)]
        [InlineData(2023, 4, 1)]
        [InlineData(2023, 3, 1)]
        public void Should_return_record_on_or_after_a_given_date(int year, int month, int day)
        {
            _context.Initialize(new List<Entity>()
            {
                _taskInRange
            });

            var dateRangeValue = new DateTime(year, month, day, 00, 00, 00);

            var fetchXml = $@"
                <fetch>
                    <entity name='{Task.EntityLogicalName}'>
                        <all-attributes />
                        <filter type='and'>
                            <condition attribute='actualstart' operator='on-or-after' value='{dateRangeValue.ToString("yyyy-MM-dd")}' />
                        </filter>
                    </entity>
                </fetch>
            ";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();
            Assert.Single(results);
        }

        [Fact]
        public void Should_return_both_records_without_any_conditions_in_the_and_filter()
        {
            _context.Initialize(new List<Entity>()
            {
                _taskInRange, _taskOutsideRange
            });

            var dateRangeValue = new DateTime(2023, 5, 1, 00, 00, 00);

            var fetchXml = $@"
                <fetch>
                    <entity name='{Task.EntityLogicalName}'>
                        <all-attributes />
                        <filter type='and'>
                        </filter>
                    </entity>
                </fetch>
            ";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList();
            Assert.Equal(2, results.Count);
        }
    }
}
