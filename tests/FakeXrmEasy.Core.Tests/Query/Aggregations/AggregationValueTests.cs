using System;
using FakeXrmEasy.Core.Exceptions.Query.Aggregations;
using FakeXrmEasy.Core.Query.Aggregations;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class AggregationValueTests
    {
        [Fact]
        public void Should_throw_exception_when_aggregating_values_of_different_types()
        {
            var value1 = new IntAggregationValue(1);
            var value2 = new DoubleAggregationValue(2.0);

            Assert.Throws<DifferentAggregationValueTypeException>(() => value1 + value2);
        }
    }
}