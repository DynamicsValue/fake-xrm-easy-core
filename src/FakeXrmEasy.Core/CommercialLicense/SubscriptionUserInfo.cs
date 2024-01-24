using System;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Info about the last time a given user used FakeXrmEasy
    /// </summary>
    internal class SubscriptionUserInfo: ISubscriptionUserInfo
    {
        /// <summary>
        /// The last time this user used FakeXrmEasy
        /// </summary>
        public DateTime LastTimeUsed { get; set; }
        
        /// <summary>
        /// The user's username
        /// </summary>
        public string UserName { get; set; }
    }
}