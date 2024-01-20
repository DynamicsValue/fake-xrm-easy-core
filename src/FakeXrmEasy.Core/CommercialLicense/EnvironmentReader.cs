using System;

namespace FakeXrmEasy.Core.CommercialLicense
{
    internal interface IEnvironmentReader
    {
        string GetEnvironmentVariable(string variableName);
    }
    
    internal class EnvironmentReader: IEnvironmentReader
    {
        public string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }
    }
}