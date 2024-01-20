using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception thrown if the info about the current subscription plan is unknown
    /// </summary>
    public class NoSubscriptionPlanInfoException: Exception 
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoSubscriptionPlanInfoException() : base("The current subscription info is unknown")
        {
            
        }
    }
}