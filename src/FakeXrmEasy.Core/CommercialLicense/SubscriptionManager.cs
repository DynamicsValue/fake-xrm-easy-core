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
        
        private static string GenerateHash(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        
        private static ISubscriptionInfo GetSubscriptionInfoFromKey(string licenseKey)
        {
            try
            {
                var encodedBaseKey = licenseKey.Substring(0, licenseKey.Length - 32);
                var hash = licenseKey.Substring(licenseKey.Length - 32, 32);
                var computedHash = GenerateHash(encodedBaseKey);

                if (!computedHash.Equals(hash))
                {
                    throw new InvalidLicenseKeyException();
                }
            
                var decodedBaseKey = Encoding.UTF8.GetString(Convert.FromBase64String(encodedBaseKey));
                var baseKeyParts = decodedBaseKey.Split('-');

                var expiryDate = DateTime.ParseExact(baseKeyParts[4], "yyyyMMdd", CultureInfo.InvariantCulture);
                var numberOfUsers = int.Parse(baseKeyParts[3]);
            
                var sku = (StockKeepingUnits) Enum.Parse(typeof(StockKeepingUnits), baseKeyParts[0]);
                var autoRenews = "1".Equals(baseKeyParts[2]);
            
                return new SubscriptionInfo()
                {
                    SKU = sku,
                    CustomerId = baseKeyParts[1],
                    NumberOfUsers = numberOfUsers,
                    EndDate = expiryDate,
                    AutoRenews = autoRenews
                };
            }
            catch
            {
                throw new InvalidLicenseKeyException();
            }
        }

        internal static void SetLicense(string licenseKey)
        {
            lock (_subscriptionInfoLock)
            {
                if (_subscriptionInfo == null)
                {
                    _subscriptionInfo = SubscriptionManager.GetSubscriptionInfoFromKey(licenseKey);
                }
            }
        }
        
        internal static void SetSubscriptionUsageStoreProvider(ISubscriptionStorageProvider subscriptionStorageProvider, IUserReader userReader)
        {
            lock (_subscriptionUsageLock)
            {
                if (_subscriptionUsage == null)
                {
                    var usageManager = new SubscriptionUsageManager();
                    _subscriptionUsage = usageManager.ReadAndUpdateUsage(subscriptionStorageProvider, userReader);
                }
            }
        }
    }
}