using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when your current subscription expired and you exceeded the allowed renewal time window 
    /// </summary>
    public class RenewalRequestExpiredException: Exception
    {
        private const string _url =
            CommercialLicenseTroubleshootingLinks.BaseUrl + "/renewal-request-expired-exception/";
        
        /// <summary>
        /// Throws an exception where the current subscription expired
        /// </summary>
        /// <param name="expiredOn"></param>
        public RenewalRequestExpiredException(DateTime expiredOn) : base($"The current subscription expired on '{expiredOn.ToLongDateString()}' and a renewal license was not applied on time. Please request a new subscription license. More info at {_url}.")
        {
            
        }
    }
}