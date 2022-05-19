using System;

namespace FakeXrmEasy.Core.Exceptions.Query
{
    /// <summary>
    /// Exception thrown when both PageInfo and TopCount properties are set in a query.
    /// More info: https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.query.querybyattribute.topcount?view=dynamics-general-ce-9
    /// </summary>
    public class CantSetBothPageInfoAndTopCountException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CantSetBothPageInfoAndTopCountException() : base("You can't set both a PageInfo and a TopCount property at the same time. Choose one.")
        {

        }
    }
}
