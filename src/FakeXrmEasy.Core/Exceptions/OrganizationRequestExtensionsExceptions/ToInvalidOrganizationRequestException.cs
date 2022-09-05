using System;

namespace FakeXrmEasy.Exceptions.OrganizationRequestExtensionsExceptions
{
    /// <summary>
    /// Exception thrown when trying to use one of the OrganizationRequest 
    /// extension methods to convert generic requests to its strongly-typed versions
    /// when the type your trying to convert to doesn't match the original
    /// </summary>
    public class ToInvalidOrganizationRequestException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ToInvalidOrganizationRequestException() : base("The current OrganizationRequest object can't be converted to the specified OrganizationRequest")
        {

        }
    }
}
