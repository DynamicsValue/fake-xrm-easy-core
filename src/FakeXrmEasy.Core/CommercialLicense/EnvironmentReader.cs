using System;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal interface IEnvironmentReader
    {
        string GetEnvironmentVariable(string variableName);
        bool IsRunningInContinuousIntegration();
    }
    
    internal class EnvironmentReader: IEnvironmentReader
    {
        /// <summary>
        /// Gets the value of environment variable with name variableName
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }
        
        /// <summary>
        /// Checks if the test run is running in a CI environment
        /// </summary>
        /// <returns></returns>
        public bool IsRunningInContinuousIntegration()
        {
            return "1".Equals(GetEnvironmentVariable("FAKE_XRM_EASY_CI"))
                   || "True".Equals(GetEnvironmentVariable("TF_BUILD"));
        }
    }
}