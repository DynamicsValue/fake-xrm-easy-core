using System;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal class SubscriptionUpgradeRequest: ISubscriptionUpgradeRequest
    {
        public DateTime FirstRequestDate { get; set; }
        public long PreviousNumberOfUsers { get; set; }
    }
}