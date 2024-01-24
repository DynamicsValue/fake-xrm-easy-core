using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Contains info about the current subscription usage
    /// </summary>
    internal class SubscriptionUsage: ISubscriptionUsage
    {
        /// <summary>
        /// The last time the current subscription usage was checked
        /// </summary>
        public DateTime LastTimeChecked { get; set; }
        
        /// <summary>
        /// Information about all the users
        /// </summary>
        public ICollection<ISubscriptionUserInfo> Users { get; set; }

        internal SubscriptionUsage()
        {
            Users = new List<ISubscriptionUserInfo>();
            LastTimeChecked = DateTime.UtcNow;
        }
    }
}