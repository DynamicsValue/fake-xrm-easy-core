using System;
using FakeItEasy;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public partial class SubscriptionValidatorTests
    {
        private readonly IEnvironmentReader _defaultEnvironmentReader;
        private ISubscriptionInfo _subscriptionInfo;
        private SubscriptionValidator _subscriptionValidator;

        
        public SubscriptionValidatorTests()
        {
            _defaultEnvironmentReader = new FakeEnvironmentReader();
        }
        
        [Fact]
        public void Should_return_error_if_current_subscription_is_unknown()
        {
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, null, null);
            Assert.Throws<NoSubscriptionPlanInfoException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_monthly_expired()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1),
                BillingType = SubscriptionBillingCycleType.Monthly,
                AutoRenews = false
            };
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null);
            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_monthly_expired_but_autorenew_is_enabled()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1),
                BillingType = SubscriptionBillingCycleType.Monthly,
                AutoRenews = true
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null);
            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_annual_expired()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                StartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1),
                BillingType = SubscriptionBillingCycleType.Annual,
                AutoRenews = false
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null);
            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_annual_expired_but_autorenew_is_enabled()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                StartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1),
                BillingType = SubscriptionBillingCycleType.Annual,
                AutoRenews = true
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null);
            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_prepaid_expired()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1),
                BillingType = SubscriptionBillingCycleType.PrePaid,
                AutoRenews = false
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null);
            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
    }
}