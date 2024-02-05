using System;
using System.Globalization;
using System.Linq;
using System.Text;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal static class SubscriptionManager
    {
        internal static ISubscriptionInfo _subscriptionInfo;
        internal static readonly object _subscriptionInfoLock = new object();

        internal static ISubscriptionUsage _subscriptionUsage;
        internal static readonly object _subscriptionUsageLock = new object();

        internal static bool _renewalRequested = false;
        internal static bool _upgradeRequested = false;
        
        private static void SetLicenseKey(string licenseKey)
        {
            lock (_subscriptionInfoLock)
            {
                if (_subscriptionInfo == null)
                {
                    var subscriptionPlanManager = new SubscriptionPlanManager();
                    _subscriptionInfo = subscriptionPlanManager.GetSubscriptionInfoFromKey(licenseKey);
                }
            }
        }
        
        internal static void SetSubscriptionStorageProvider(ISubscriptionStorageProvider subscriptionStorageProvider, 
            IUserReader userReader,
            bool upgradeRequested,
            bool renewalRequested)
        {
            SetLicenseKey(subscriptionStorageProvider.GetLicenseKey());
            
            lock (_subscriptionUsageLock)
            {
                if (_subscriptionUsage == null)
                {
                    _upgradeRequested = upgradeRequested;
                    _renewalRequested = renewalRequested;
                    
                    var usageManager = new SubscriptionUsageManager();
                    _subscriptionUsage = usageManager.ReadAndUpdateUsage(_subscriptionInfo, subscriptionStorageProvider, userReader, _upgradeRequested);
                }
            }
        }
    }
}