using System;
using System.Globalization;
using System.Linq;
using System.Text;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal class SubscriptionManager
    {
        internal ISubscriptionInfo _subscriptionInfo;
        internal ISubscriptionUsage _subscriptionUsage;
        internal readonly SubscriptionUsageManager _subscriptionUsageManager;
        
        private bool _renewalRequested = false;
        private bool _upgradeRequested = false;

        private static readonly object _subscriptionManagerLock = new object();
        private static SubscriptionManager _instance = null;
        private readonly IEnvironmentReader _environmentReader;
        internal SubscriptionManager(IEnvironmentReader environmentReader,
                                    ISubscriptionInfo subscriptionInfo,
                                    ISubscriptionUsage subscriptionUsage,
                                    SubscriptionUsageManager subscriptionUsageManager)
        {
            _environmentReader = environmentReader;
            _subscriptionInfo = subscriptionInfo;
            _subscriptionUsage = subscriptionUsage;
            _subscriptionUsageManager = subscriptionUsageManager;
        }
        
        internal static SubscriptionManager Instance
        {
            get
            {
                lock (_subscriptionManagerLock)
                {
                    if (_instance == null)
                    {
                        _instance = new SubscriptionManager(
                            new EnvironmentReader(), 
                            null, 
                            null, 
                            new SubscriptionUsageManager());
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// For testing
        /// </summary>
        /// <param name="instance"></param>
        internal static void SetFakeInstance(SubscriptionManager instance)
        {
            _instance = instance;
        }
        
        internal ISubscriptionInfo SubscriptionInfo => _subscriptionInfo;
        internal ISubscriptionUsage SubscriptionUsage => _subscriptionUsage;
        internal bool RenewalRequested => _renewalRequested;
        
        internal void SetLicenseKey(string licenseKey)
        {
            if (_subscriptionInfo == null)
            {
                var subscriptionPlanManager = new SubscriptionPlanManager();
                _subscriptionInfo = subscriptionPlanManager.GetSubscriptionInfoFromKey(licenseKey);
            }
        }
        
        internal void SetSubscriptionStorageProvider_Internal(ISubscriptionStorageProvider subscriptionStorageProvider,
            IUserReader userReader,
            bool upgradeRequested,
            bool renewalRequested)

        {
            SetLicenseKey(subscriptionStorageProvider.GetLicenseKey());
            if (_subscriptionUsage == null)
            {
                if (_environmentReader.IsRunningInContinuousIntegration())
                {
                    _subscriptionUsage = new SubscriptionUsage();
                    return;
                }
                    
                _upgradeRequested = upgradeRequested;
                _renewalRequested = renewalRequested;
                
                _subscriptionUsage = _subscriptionUsageManager.ReadAndUpdateUsage(_subscriptionInfo, subscriptionStorageProvider, userReader, _upgradeRequested);
            }
        }
        internal void SetSubscriptionStorageProvider(ISubscriptionStorageProvider subscriptionStorageProvider, 
            IUserReader userReader,
            bool upgradeRequested,
            bool renewalRequested)
        {
            lock (_subscriptionManagerLock)
            {
                SetLicenseKey(subscriptionStorageProvider.GetLicenseKey());
                SetSubscriptionStorageProvider_Internal(subscriptionStorageProvider, userReader, upgradeRequested,
                    renewalRequested);
            }
        }
    }
}