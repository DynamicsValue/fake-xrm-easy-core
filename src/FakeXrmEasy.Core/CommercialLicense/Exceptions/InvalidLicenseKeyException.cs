using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when the license key is invalid or malformed
    /// </summary>
    public class InvalidLicenseKeyException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidLicenseKeyException() : base("The license key is invalid")
        {
            
        }
    }
}