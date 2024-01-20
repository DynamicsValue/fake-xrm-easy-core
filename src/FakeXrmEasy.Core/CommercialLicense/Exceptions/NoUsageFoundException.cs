using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Throws an exception when your current usage of FakeXrmEasy could not be retrieved
    /// </summary>
    public class NoUsageFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoUsageFoundException() : base("No info about your current usage of FakeXrmEasy was found")
        {
            
        }
    }
}