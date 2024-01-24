using System;
using System.Linq;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;

namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Validates the current subscription usage is within the current subscription plan
    /// </summary>
    internal sealed class SubscriptionValidator
    {
        private readonly IEnvironmentReader _environmentReader;
        private readonly ISubscriptionInfo _subscriptionInfo;
        private readonly ISubscriptionUsage _subscriptionUsage;

        internal SubscriptionValidator(
            IEnvironmentReader environmentReader,
            ISubscriptionInfo subscriptionInfo,
            ISubscriptionUsage subscriptionUsage)
        {
            _environmentReader = environmentReader;
            _subscriptionInfo = subscriptionInfo;
            _subscriptionUsage = subscriptionUsage;
        }
        
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
            if (_subscriptionInfo == null)
            {
                throw new NoSubscriptionPlanInfoException();
            }

            if (_subscriptionInfo.AutoRenews)
            {
                return true;
            }

            var expiryDate = _subscriptionInfo.EndDate;

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
            if (_subscriptionUsage == null)
            {
                throw new NoUsageFoundException();
            }

            var currentNumberOfUsers = _subscriptionUsage
                .Users
                .Count(userInfo => userInfo.LastTimeUsed >= DateTime.UtcNow.AddMonths(-1));

            if (currentNumberOfUsers > _subscriptionInfo.NumberOfUsers)
            {
                throw new ConsiderUpgradingPlanException(currentNumberOfUsers, _subscriptionInfo.NumberOfUsers);
            }
            return true;
        }

        private bool IsRunningInContinuousIntegration()
        {
            return "1".Equals(_environmentReader.GetEnvironmentVariable("FAKE_XRM_EASY_CI"))
                || "True".Equals(_environmentReader.GetEnvironmentVariable("TF_BUILD"));
        }
    }
}