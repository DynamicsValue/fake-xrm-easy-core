using System;
using FakeXrmEasy.Core.Query.Aggregations;

namespace FakeXrmEasy.Core.Exceptions.Query.Aggregations
{
    /// <summary>
    /// Exception thrown when aggregating two different values that have different data types
    /// </summary>
    internal class DifferentAggregationValueTypeException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public DifferentAggregationValueTypeException(AggregationValue value1, AggregationValue value2) : base(
            $"The types of the AggregationValue must match but they don't ({value1.ValueType} vs. {value2.ValueType})")
        {
            
        }
    }
}