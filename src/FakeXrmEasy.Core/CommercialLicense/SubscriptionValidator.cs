using System;
using System.Linq;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Validates the current subscription usage is within the current subscription plan
    /// </summary>
    public sealed class SubscriptionValidator
    {
        private readonly IEnvironmentReader _environmentReader;
        
        public SubscriptionValidator(IEnvironmentReader environmentReader)
        {
            _environmentReader = environmentReader;
        }
        
        /// <summary>
        /// The current subscription plan
        /// </summary>
        internal SubscriptionInfo SubscriptionPlan { get; set; }
        
        /// <summary>
        /// The current usage of the subscription
        /// </summary>
        internal SubscriptionUsage CurrentUsage { get; set; }

        /// <summary>
        /// Validates if the current usage is within the subscription limits
        /// </summary>
        /// <returns></returns>
        internal bool IsValid()
        {
            var isSubscriptionPlanValid = IsSubscriptionPlanValid();
            if (!isSubscriptionPlanValid)
            {
                return false;
            }
            
            var isSubscriptionUsageValid = IsUsageValid();
            if (!isSubscriptionUsageValid)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Returns valid if the current subscription didn't expire yet
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NoSubscriptionPlanInfoException"></exception>
        /// <exception cref="SubscriptionExpiredException"></exception>
        internal bool IsSubscriptionPlanValid()
        {
            if (SubscriptionPlan == null)
            {
                throw new NoSubscriptionPlanInfoException();
            }

            if (SubscriptionPlan.AutoRenews)
            {
                return true;
            }
            
            var expiryDate = SubscriptionPlan.BillingType == SubscriptionBillingCycleType.Annual
                ? SubscriptionPlan.StartDate.AddYears(1)
                : SubscriptionPlan.StartDate.AddMonths(1);

            if (expiryDate < DateTime.UtcNow)
            {
                throw new SubscriptionExpiredException(expiryDate);
            }

            return true;
        }

        internal bool IsUsageValid()
        {
            if (IsRunningInContinuousIntegration())
            {
                return true;
            }
            if (CurrentUsage == null)
            {
                throw new NoUsageFoundException();
            }

            var currentNumberOfUsers = CurrentUsage
                .Users
                .Count(userInfo => userInfo.LastTimeUsed >= DateTime.UtcNow.AddMonths(-1));

            if (currentNumberOfUsers > SubscriptionPlan.NumberOfUsers)
            {
                throw new ConsiderUpgradingPlanException(currentNumberOfUsers, SubscriptionPlan.NumberOfUsers);
            }
            return true;
        }

        private bool IsRunningInContinuousIntegration()
        {
            return "1".Equals(_environmentReader.GetEnvironmentVariable("FAKE_XRM_EASY_CI"))
                || "True".Equals(_environmentReader.GetEnvironmentVariable(("TF_BUILD")));
        }
    }
}