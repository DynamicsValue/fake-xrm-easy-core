using System;
using System.Linq;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal class SubscriptionUsageManager
    {
        internal ISubscriptionUsage _subscriptionUsage;

        internal ISubscriptionUsage ReadAndUpdateUsage(
            ISubscriptionInfo subscriptionInfo,
            ISubscriptionStorageProvider subscriptionStorageProvider,
            IUserReader userReader,
            bool upgradeRequested)
        {
            _subscriptionUsage = subscriptionStorageProvider.Read();
            if (_subscriptionUsage == null)
            {
                _subscriptionUsage = new SubscriptionUsage();
            }
                    
            var currentUserName = userReader.GetCurrentUserName();
                
            var existingUser = _subscriptionUsage
                .Users
                .FirstOrDefault(user => currentUserName.Equals(user.UserName));

            if (existingUser == null)
            {
                _subscriptionUsage.Users.Add(new SubscriptionUserInfo()
                {
                    UserName = currentUserName,
                    LastTimeUsed = DateTime.UtcNow
                });
            }
            else
            {
                existingUser.LastTimeUsed = DateTime.UtcNow;
            }

            if (upgradeRequested && _subscriptionUsage.UpgradeInfo == null)
            {
                _subscriptionUsage.UpgradeInfo = new SubscriptionUpgradeRequest()
                {
                    FirstRequestDate = DateTime.UtcNow,
                    PreviousNumberOfUsers = subscriptionInfo.NumberOfUsers
                };
            }
            
            subscriptionStorageProvider.Write(_subscriptionUsage);

            return _subscriptionUsage;
        }
    }
}