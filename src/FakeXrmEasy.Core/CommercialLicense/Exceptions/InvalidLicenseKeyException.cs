using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when the license key is invalid or malformed
    /// </summary>
    public class InvalidLicenseKeyException: Exception
    {
        private const string _url =
            CommercialLicenseTroubleshootingLinks.BaseUrl + "/invalid-license-key-exception/";
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidLicenseKeyException() : base($"The license key is invalid. More info at {_url}.")
        {
            
        }
    }
}