using System;

namespace FakeXrmEasy.Core.Exceptions.Query.FetchXml.Aggregations
{
    /// <summary>
    /// Exception thrown when the aggregation function in FetchXml aggregation is unknown
    /// </summary>
    public class UnknownAggregateFunctionException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agrFn">The name of the unknown aggregate function</param>
        public UnknownAggregateFunctionException(string agrFn) : base($"Unknown aggregate function '{agrFn}'")
        {

        }
    }
}
