using System;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public partial class SubscriptionValidatorTests
    {
        private ISubscriptionUsage _subscriptionUsage;

        [Fact]
        public void Should_return_no_usage_found_exception()
        {
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, null, null, false);
            Assert.Throws<NoUsageFoundException>(() => _subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_return_consider_upgrading_exception_if_the_number_of_users_exceeds_the_current_subscription()
        {
            _subscriptionUsage = new SubscriptionUsage() //3 valid users
            {
                 Users = new SubscriptionUserInfo[]
                 {
                     new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                     new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                     new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                 }
            };
            _subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 2
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, _subscriptionUsage, false);
            Assert.Throws<ConsiderUpgradingPlanException>(() => _subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_not_return_consider_upgrading_exception_if_the_number_of_users_exceeds_the_current_subscription_and_upgrade_was_requested_within_30days()
        {
            _subscriptionUsage = new SubscriptionUsage() //3 valid users
            {
                UpgradeInfo = new SubscriptionUpgradeRequest()
                {
                    FirstRequestDate = DateTime.UtcNow.AddMonths(-1).AddDays(1),
                    PreviousNumberOfUsers = 2
                },
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            _subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 2
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, _subscriptionUsage, false);
            Assert.True(_subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_return_upgrade_request_expired_exception_if_the_number_of_users_exceeds_the_current_subscription_and_upgrade_was_requested_but_took_longer_thab_30days()
        {
            _subscriptionUsage = new SubscriptionUsage() //3 valid users
            {
                UpgradeInfo = new SubscriptionUpgradeRequest()
                {
                    FirstRequestDate = DateTime.UtcNow.AddMonths(-1).AddDays(-3),
                    PreviousNumberOfUsers = 2
                },
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            _subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 2
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, _subscriptionUsage, false);
            Assert.Throws<UpgradeRequestExpiredException>(() => _subscriptionValidator.IsUsageValid());
        }
        
        
        [Fact]
        public void Should_not_count_users_where_the_last_time_used_is_greater_than_one_month()
        {
            _subscriptionUsage = new SubscriptionUsage() //3 valid users
            {
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddMonths(-1).AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            _subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 2
            };
            
            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, _subscriptionUsage, false);
            Assert.True(_subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_return_usage_is_valid_if_it_is_within_the_allowed_range()
        {
            _subscriptionUsage = new SubscriptionUsage() //3 existing valid users
            {
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            
            _subscriptionInfo = new SubscriptionInfo
            {
                NumberOfUsers = 3
            };

            _subscriptionValidator = new SubscriptionValidator(_defaultEnvironmentReader, _subscriptionInfo, _subscriptionUsage, false);
            Assert.True(_subscriptionValidator.IsUsageValid());
        }
        
        [Theory]
        [InlineData("FAKE_XRM_EASY_CI", "1")]
        [InlineData("TF_BUILD", "True")]
        public void Should_ignore_usage_if_running_inside_ci(string envVariableName, string envVariableValue)
        {
            var continuousIntegrationEnvironmentReader = new FakeEnvironmentReader();
            continuousIntegrationEnvironmentReader.SetEnvironmentVariable(envVariableName, envVariableValue);
            
            _subscriptionValidator = new SubscriptionValidator(continuousIntegrationEnvironmentReader, null, _subscriptionUsage, false);
            Assert.True(_subscriptionValidator.IsUsageValid());
        }
    }
}