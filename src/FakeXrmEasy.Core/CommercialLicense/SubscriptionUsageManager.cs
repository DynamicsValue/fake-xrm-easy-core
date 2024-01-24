using System;
using System.Linq;
using FakeXrmEasy.Abstractions.CommercialLicense;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal class SubscriptionUsageManager
    {
        internal ISubscriptionUsage _subscriptionUsage;

        internal ISubscriptionUsage ReadAndUpdateUsage(ISubscriptionStorageProvider subscriptionStorageProvider,
            IUserReader userReader)
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
                
            subscriptionStorageProvider.Write(_subscriptionUsage);

            return _subscriptionUsage;
        }
    }
}