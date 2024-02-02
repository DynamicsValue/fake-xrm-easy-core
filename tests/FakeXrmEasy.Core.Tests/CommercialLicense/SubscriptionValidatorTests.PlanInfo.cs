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
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, null, null, false);
            Assert.Throws<NoSubscriptionPlanInfoException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }

        [Fact]
        public void Should_not_return_subscription_expired_if_still_valid()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow.AddDays(20),
                AutoRenews = true
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null, false);
            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_expired_but_autorenew_is_enabled()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow.AddDays(-1),
                AutoRenews = true
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null, false);
            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_subscription_expired_exception_if_expired_with_no_autorenewal()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow.AddDays(-1),
                AutoRenews = false
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null, false);
            Assert.Throws<SubscriptionExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_not_return_subscription_expired_exception_if_expired_but_a_renewal_was_requested_within_a_valid_time_frame()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow.AddDays(-15),
                AutoRenews = false
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null, true);
            Assert.True(_subscriptionValidator.IsSubscriptionPlanValid());
        }
        
        [Fact]
        public void Should_return_renewal_request_expired_exception_if_expired_and_exceeded_the_valid_time_frame_for_renewal()
        {
            _subscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow.AddMonths(-1).AddDays(-1),
                AutoRenews = false
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, null, true);
            Assert.Throws<RenewalRequestExpiredException>(() => _subscriptionValidator.IsSubscriptionPlanValid());
        }
    }
}