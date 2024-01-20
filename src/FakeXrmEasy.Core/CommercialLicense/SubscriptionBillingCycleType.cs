namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Contains info about the current subscription billing cycle type
    /// </summary>
    public enum SubscriptionBillingCycleType
    {
        /// <summary>
        /// Monthly: subscription billed per user/month
        /// </summary>
        Monthly = 0,
        /// <summary>
        /// Annual: subscription billed per user/year
        /// </summary>
        Annual = 1,
        /// <summary>
        /// PrePaid: The subscription is prepaid for a period different than one month or one year and is set in the StartDate and EndDate fields
        /// </summary>
        PrePaid = 2
    }
}