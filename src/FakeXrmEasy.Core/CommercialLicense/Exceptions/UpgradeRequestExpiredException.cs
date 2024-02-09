using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when the grace period for requesting an upgrade has expired: https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/commercial-license/troubleshooting/upgrade-request-expired-exception/
    /// </summary>
    public class UpgradeRequestExpiredException: Exception
    {
        private const string _url =
            "https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/commercial-license/troubleshooting/upgrade-request-expired-exception/";
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="firstRequested"></param>
        public UpgradeRequestExpiredException(DateTime firstRequested) :
            base($"You requested a subscription upgrade on '{firstRequested.ToShortDateString()}', however, the new subscription details or upgrade progress has not been completed within the allowed upgrade window. More info at {_url}.")
        {
            
        }
    }
}