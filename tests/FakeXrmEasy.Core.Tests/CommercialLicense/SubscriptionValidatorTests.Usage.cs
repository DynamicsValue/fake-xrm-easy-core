using System;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public partial class SubscriptionValidatorTests
    {
        [Fact]
        public void Should_return_no_usage_found_exception()
        {
            _subscriptionValidator.CurrentUsage = null;
            Assert.Throws<NoUsageFoundException>(() => _subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_return_consider_upgrading_exception_if_the_number_of_users_exceeds_the_current_subscription()
        {
            _subscriptionValidator.CurrentUsage = new SubscriptionUsage() //3 valid users
            {
                 Users = new SubscriptionUserInfo[]
                 {
                     new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                     new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                     new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                 }
            };
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.NumberOfUsers = 2;

            Assert.Throws<ConsiderUpgradingPlanException>(() => _subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_not_count_users_where_the_last_time_used_is_greater_than_one_month()
        {
            _subscriptionValidator.CurrentUsage = new SubscriptionUsage() //3 valid users
            {
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddMonths(-1).AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo();
            _subscriptionValidator.SubscriptionPlan.NumberOfUsers = 2;

            Assert.True(_subscriptionValidator.IsUsageValid());
        }
        
        [Fact]
        public void Should_return_usage_is_valid_if_it_is_within_the_allowed_range()
        {
            _subscriptionValidator.CurrentUsage = new SubscriptionUsage() //3 valid users
            {
                Users = new SubscriptionUserInfo[]
                {
                    new SubscriptionUserInfo() { UserName = "user1", LastTimeUsed = DateTime.UtcNow.AddDays(-1) },
                    new SubscriptionUserInfo() { UserName = "user2", LastTimeUsed = DateTime.UtcNow.AddDays(-10) },
                    new SubscriptionUserInfo() { UserName = "user3", LastTimeUsed = DateTime.UtcNow.AddDays(-3) },
                }
            };
            
            _subscriptionValidator.SubscriptionPlan = new SubscriptionInfo
            {
                NumberOfUsers = 3
            };

            Assert.True(_subscriptionValidator.IsUsageValid());
        }
        
        [Theory]
        [InlineData("FAKE_XRM_EASY_CI", "1")]
        [InlineData("TF_BUILD", "True")]
        public void Should_ignore_usage_if_running_inside_ci(string envVariableName, string envVariableValue)
        {
            var continuousIntegrationEnvironmentReader = new FakeEnvironmentReader();
            continuousIntegrationEnvironmentReader.SetEnvironmentVariable(envVariableName, envVariableValue);
            
            var currentSubscription = new SubscriptionValidator(continuousIntegrationEnvironmentReader)
            {
                CurrentUsage = null
            };

            Assert.True(currentSubscription.IsUsageValid());
        }
    }
}