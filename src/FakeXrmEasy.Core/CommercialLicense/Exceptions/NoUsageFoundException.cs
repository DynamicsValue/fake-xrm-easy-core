using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Throws an exception when your current usage of FakeXrmEasy could not be retrieved
    /// </summary>
    public class NoUsageFoundException: Exception
    {
        private const string _url =
            "https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/commercial-license/troubleshooting/no-usage-found-exception/";
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoUsageFoundException() : base($"No info about your current usage of FakeXrmEasy was found. More info at {_url}.")
        {
            
        }
    }
}