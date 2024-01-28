using FakeXrmEasy.Core.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class SubscriptionPlanManagerTests
    {
        private readonly SubscriptionPlanManager _subscriptionPlanManager;

        public SubscriptionPlanManagerTests()
        {
            _subscriptionPlanManager = new SubscriptionPlanManager();
        }
        
        [Fact]
        public void Should_raise_invalid_license_key_exception()
        {
            var invalidKey =
                "asdasdkjakdhu38768a79aysdaiushdakjshdajshda79878s97d89as7d9a87sda98sdyausydusydausdajbdahsdjhasgdahsgda78sda8s7d6a986d98as6d9a8d";

            Assert.Throws<InvalidLicenseKeyException>(() => _subscriptionPlanManager.GetSubscriptionInfoFromKey(invalidKey));
        }
    }
}