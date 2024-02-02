using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when the grace period for requesting an upgrade has expired
    /// </summary>
    public class UpgradeRequestExpiredException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="firstRequested"></param>
        public UpgradeRequestExpiredException(DateTime firstRequested) :
            base($"You requested a subscription upgrade on '{firstRequested.ToShortDateString()}', however, the new subscription details or upgrade progress has not been completed within the allowed upgrade window. Please contact your line manager and raise a support ticket")
        {
            
        }
    }
}