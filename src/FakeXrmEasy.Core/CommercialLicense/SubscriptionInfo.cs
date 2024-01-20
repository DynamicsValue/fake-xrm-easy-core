using System;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Contains info about the current subscription
    /// </summary>
    internal class SubscriptionInfo
    {
        /// <summary>
        /// True if the current subscription auto-renews 
        /// </summary>
        internal bool AutoRenews { get; set; }
        
        /// <summary>
        /// The current billing cycle type
        /// </summary>
        internal SubscriptionBillingCycleType BillingType { get; set; }
        
        /// <summary>
        /// Max number of users allowed in the current subscription
        /// </summary>
        internal long NumberOfUsers { get; set; }
        
        /// <summary>
        /// The subscription start date
        /// </summary>
        internal DateTime StartDate { get; set; }
        
        /// <summary>
        /// The subscription's end date
        /// </summary>
        internal DateTime EndDate { get; set; }
    }
}