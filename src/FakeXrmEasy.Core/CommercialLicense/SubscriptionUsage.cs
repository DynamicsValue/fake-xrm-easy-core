using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Contains info about the current subscription usage
    /// </summary>
    internal class SubscriptionUsage
    {
        /// <summary>
        /// The last time the current subscription usage was checked
        /// </summary>
        internal DateTime LastTimeChecked { get; set; }
        
        /// <summary>
        /// Information about all the users
        /// </summary>
        internal IEnumerable<SubscriptionUserInfo> Users { get; set; }
    }
}