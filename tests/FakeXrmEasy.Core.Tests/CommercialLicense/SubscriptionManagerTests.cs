using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class SubscriptionManagerTests
    {
        private SubscriptionManager _subscriptionManager;
        private readonly FakeEnvironmentReader _environmentReader;
        private readonly ISubscriptionInfo _subscriptionInfo;
        private readonly ISubscriptionUsage _subscriptionUsage;
        private readonly SubscriptionUsageManager _subscriptionUsageManager;
        private readonly ISubscriptionStorageProvider _fakeSubscriptionStorageProvider;
        public SubscriptionManagerTests()
        {
            _environmentReader = new FakeEnvironmentReader();
            _subscriptionUsageManager = new SubscriptionUsageManager();
            _fakeSubscriptionStorageProvider = A.Fake<ISubscriptionStorageProvider>();
        }
        
        [Fact]
        public void Should_set_license_key_and_throw_exception_when_invalid()
        {
            _subscriptionManager = new SubscriptionManager(_environmentReader, null, null, _subscriptionUsageManager);
            Assert.Throws<InvalidLicenseKeyException>(() => _subscriptionManager.SetLicenseKey("dummy"));
        }
        
        [Fact]
        public void Should_not_read_usage_when_running_in_CI()
        {
            var subscriptionInfo = new SubscriptionInfo()
            {
                NumberOfUsers = 1,
                EndDate = DateTime.UtcNow.AddMonths(1)
            };
            _environmentReader.SetEnvironmentVariable("FAKE_XRM_EASY_CI", "1");
            
            _subscriptionManager = new SubscriptionManager(_environmentReader, subscriptionInfo, null, _subscriptionUsageManager);
            _subscriptionManager.SetSubscriptionStorageProvider_Internal(_fakeSubscriptionStorageProvider, new UserReader(), false, false);
            
            Assert.NotNull(_subscriptionManager._subscriptionUsage);
            A.CallTo(() => _fakeSubscriptionStorageProvider.Read()).MustNotHaveHappened();
        }
        
        
        
    }
}