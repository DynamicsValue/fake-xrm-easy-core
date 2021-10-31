using System;

namespace FakeXrmEasy.Core.Exceptions.Query.FetchXml.Aggregations
{
    /// <summary>
    /// Exception thrown when there is an aggregate function against an attribute type which is not supported
    /// </summary>
    public class UnhandledPropertyTypeException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unhandledType">The unsupported attribute type</param>
        /// <param name="agrFn">The aggregate function ('min')</param>
        public UnhandledPropertyTypeException(string unhandledType, string agrFn) : base($"Unhandled property type '{unhandledType}' in '{agrFn}' aggregate function")
        {

        }
    }
}
