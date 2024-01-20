using System;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Info about the last time a given user used FakeXrmEasy
    /// </summary>
    internal class SubscriptionUserInfo
    {
        /// <summary>
        /// The last time this user used FakeXrmEasy
        /// </summary>
        internal DateTime LastTimeUsed { get; set; }
        
        /// <summary>
        /// The user's username
        /// </summary>
        internal string UserName { get; set; }
        
        /// <summary>
        /// This user that runs as part of a CI process (Continuous Integration, i.e. either a build or release pipeline)
        /// </summary>
        internal bool IsCI { get; set; }
    }
}