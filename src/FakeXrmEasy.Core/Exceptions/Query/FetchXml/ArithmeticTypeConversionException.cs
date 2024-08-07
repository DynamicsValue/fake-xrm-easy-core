using System;

namespace FakeXrmEasy.Core.Exceptions.Query.FetchXml
{
    public class ArithmeticTypeConversionException: Exception
    {
        public ArithmeticTypeConversionException(string entityName, string attributeName) : 
            base($"When using arithmetic values in a condition of attribute '{attributeName}' of entity '{entityName}' in a Fetch a ProxyTypesAssembly must be used in order to know which types to cast values to. If you are using early bound types, please make sure the early bound type was generated for entity '{entityName}'")
        {
            
        }
    }
}