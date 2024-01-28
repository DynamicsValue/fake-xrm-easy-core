using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class SubscriptionUsageManagerTests
    {
        private readonly SubscriptionUsageManager _usageManager;
        private readonly ISubscriptionStorageProvider _subscriptionStorageProvider;
        private readonly IUserReader _userReader;
        private const string cUserName = "CurrentDomain\\CurrentUser";
        private readonly ISubscriptionInfo _subscriptionInfo;
        
        public SubscriptionUsageManagerTests()
        {
            _subscriptionStorageProvider = A.Fake<ISubscriptionStorageProvider>();
            _userReader = A.Fake<IUserReader>();
            A.CallTo(() => _userReader.GetCurrentUserName()).ReturnsLazily(() => cUserName);
            _usageManager = new SubscriptionUsageManager();
            _subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 10
            };
        }
        
        [Fact]
        public void Should_initialize_new_subscription_usage_data_if_the_provider_returns_null_and_add_current_user()
        {
            A.CallTo(() => _subscriptionStorageProvider.Read()).ReturnsLazily(() => null);
            
            var usage = _usageManager.ReadAndUpdateUsage(_subscriptionInfo, _subscriptionStorageProvider, _userReader, false);
            
            Assert.NotNull(usage);
            Assert.Single(usage.Users);

            var userInfo = usage.Users.First();
            Assert.Equal(cUserName, userInfo.UserName);
            Assert.True(userInfo.LastTimeUsed > DateTime.UtcNow.AddDays(-1));

            A.CallTo(() => _subscriptionStorageProvider.Write(usage))
                .MustHaveHappened();
        }
        
        [Fact]
        public void Should_update_last_used_date_if_current_user_already_exists()
        {
            A.CallTo(() => _subscriptionStorageProvider.Read()).ReturnsLazily(() => new SubscriptionUsage()
            {
                Users = new List<ISubscriptionUserInfo>()
                {
                    new SubscriptionUserInfo()
                    {
                        UserName = cUserName,
                        LastTimeUsed = DateTime.UtcNow.AddMonths(-2)
                    }
                }
            });
            
            var usage = _usageManager.ReadAndUpdateUsage(_subscriptionInfo, _subscriptionStorageProvider, _userReader, false);
            
            Assert.NotNull(usage);
            Assert.Single(usage.Users);

            var userInfo = usage.Users.First();
            Assert.Equal(cUserName, userInfo.UserName);
            Assert.True(userInfo.LastTimeUsed > DateTime.UtcNow.AddDays(-1));
            
            A.CallTo(() => _subscriptionStorageProvider.Write(usage))
                .MustHaveHappened();
        }
        
        [Fact]
        public void Should_add_upgrade_requested_info_upgrade_requested_and_no_previous_upgrade_info_existed()
        {
            A.CallTo(() => _subscriptionStorageProvider.Read()).ReturnsLazily(() => null);
            
            var usage = _usageManager.ReadAndUpdateUsage(_subscriptionInfo, _subscriptionStorageProvider, _userReader, true);
            
            Assert.NotNull(usage);
            Assert.Single(usage.Users);

            var userInfo = usage.Users.First();
            Assert.Equal(cUserName, userInfo.UserName);
            Assert.True(userInfo.LastTimeUsed > DateTime.UtcNow.AddDays(-1));

            var upgradeInfo = usage.UpgradeInfo;
            Assert.NotNull(upgradeInfo);
            Assert.Equal(_subscriptionInfo.NumberOfUsers, upgradeInfo.PreviousNumberOfUsers);
            
            A.CallTo(() => _subscriptionStorageProvider.Write(usage))
                .MustHaveHappened();
        }
        
        [Fact]
        public void Should_not_update_upgrade_info_if_upgrade_info_existed_previously()
        {
            var upgradeDate = DateTime.UtcNow.AddMonths(-1);
            
            A.CallTo(() => _subscriptionStorageProvider.Read()).ReturnsLazily(() => new SubscriptionUsage()
            {
                UpgradeInfo = new SubscriptionUpgradeRequest()
                {
                    FirstRequestDate = upgradeDate,
                    PreviousNumberOfUsers = _subscriptionInfo.NumberOfUsers
                },
                Users = new List<ISubscriptionUserInfo>()
                {
                    new SubscriptionUserInfo()
                    {
                        UserName = cUserName,
                        LastTimeUsed = DateTime.UtcNow.AddMonths(-2)
                    }
                }
            });
            
            var usage = _usageManager.ReadAndUpdateUsage(_subscriptionInfo, _subscriptionStorageProvider, _userReader, true);
            
            Assert.NotNull(usage);
            Assert.Single(usage.Users);

            var upgradeInfo = usage.UpgradeInfo;
            Assert.NotNull(upgradeInfo);
            Assert.Equal(_subscriptionInfo.NumberOfUsers, upgradeInfo.PreviousNumberOfUsers);
            Assert.Equal(upgradeDate, upgradeInfo.FirstRequestDate);
            
            A.CallTo(() => _subscriptionStorageProvider.Write(usage))
                .MustHaveHappened();
        }
    }
}