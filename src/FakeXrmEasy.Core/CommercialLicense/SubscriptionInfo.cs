using System;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Contains info about the current subscription
    /// </summary>
    internal class SubscriptionInfo: ISubscriptionInfo
    {
        /// <summary>
        /// The CustomerId
        /// </summary>
        public string CustomerId { get; set; }
        
        /// <summary>
        /// SKU
        /// </summary>
        public StockKeepingUnits SKU { get; set; }

        /// <summary>
        /// True if the current subscription auto-renews 
        /// </summary>
        public bool AutoRenews { get; set; }
        
        /// <summary>
        /// The current billing cycle type
        /// </summary>
        public SubscriptionBillingCycleType BillingType { get; set; }
        
        /// <summary>
        /// Max number of users allowed in the current subscription
        /// </summary>
        public long NumberOfUsers { get; set; }
        
        /// <summary>
        /// The subscription start date
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// The subscription's end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseKey"></param>
        internal void FromLicenseKey(string licenseKey)
        {
            
        }
    }
}