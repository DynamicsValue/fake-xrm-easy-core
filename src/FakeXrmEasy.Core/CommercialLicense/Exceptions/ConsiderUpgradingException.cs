using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception raised when the current number of users calculated based on the usage of your current subscription is greater than the maximum number of users in your current subscription
    /// </summary>
    public class ConsiderUpgradingPlanException: Exception 
    {
        private const string _url =
            CommercialLicenseTroubleshootingLinks.BaseUrl + "/consider-upgrading-exception/";
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="currentNumberOfUsers"></param>
        /// <param name="allowedNumberOfUsers"></param>
        public ConsiderUpgradingPlanException(long currentNumberOfUsers, long allowedNumberOfUsers) :
            base($"Your current subscription allows up to {allowedNumberOfUsers.ToString()} users, however, {currentNumberOfUsers.ToString()} users are currently using it. Please consider upgrading your current plan. More info at {_url}.")
        {
            
        }
    }
}