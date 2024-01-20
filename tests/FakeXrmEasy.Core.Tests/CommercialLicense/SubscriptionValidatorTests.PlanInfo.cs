using System;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public partial class SubscriptionValidatorTests
    {
        private readonly IEnvironmentReader _defaultEnvironmentReader;
        private readonly SubscriptionValidator _subscriptionValidator;
        
        public SubscriptionValidatorTests()
        {
            _defaultEnvironmentReader = new FakeEnvironmentReader();
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader);
        }
        [Fact]
        public void Should_return_error_if_current_subscription_is_unknown()
        {
            Assert.Throws<NoSubscriptionPlanInfoException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_monthly_expired()
        {
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1);
            _subscriptionValidator.SubscriptionPlan.BillingType = SubscriptionBillingCycleType.Monthly;
            _subscriptionValidator.SubscriptionPlan.AutoRenews = false;

            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_monthly_expired_but_autorenew_is_enabled()
        {
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1);
            _subscriptionValidator.SubscriptionPlan.BillingType = SubscriptionBillingCycleType.Monthly;
            _subscriptionValidator.SubscriptionPlan.AutoRenews = true;

            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_annual_expired()
        {
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.StartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1);
            _subscriptionValidator.SubscriptionPlan.BillingType = SubscriptionBillingCycleType.Annual;
            _subscriptionValidator.SubscriptionPlan.AutoRenews = false;

            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_annual_expired_but_autorenew_is_enabled()
        {
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.StartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1);
            _subscriptionValidator.SubscriptionPlan.BillingType = SubscriptionBillingCycleType.Annual;
            _subscriptionValidator.SubscriptionPlan.AutoRenews = true;

            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_prepaid_expired()
        {
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.StartDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1);
            _subscriptionValidator.SubscriptionPlan.BillingType = SubscriptionBillingCycleType.PrePaid;
            _subscriptionValidator.SubscriptionPlan.AutoRenews = false;

            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
    }
}