using FakeXrmEasy.Abstractions.CommercialLicense;
using FakeXrmEasy.Core.CommercialLicense;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class FakeSubscriptionStorageProvider: ISubscriptionStorageProvider
    {
        public string GetLicenseKey()
        {
            return "license-key";
        }

        public ISubscriptionUsage Read()
        {
            return new SubscriptionUsage();
        }

        public void Write(ISubscriptionUsage currentUsage)
        {
            //Do nothing
        }
    }
}