using System;

namespace FakeXrmEasy.Core.Exceptions
{
    /// <summary>
    /// Exception when the of a specified attribute could not be determined
    /// </summary>
    public class FindReflectedAttributeTypeNotFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="attributeLogicalName"></param>
        public FindReflectedAttributeTypeNotFoundException(string entityLogicalName, string attributeLogicalName) :
            base($"The type of the attribute with name '{attributeLogicalName}' in  entity '{entityLogicalName}' was not found. Please consider using a generated early bound assembly, or entity metadata with the necessary type information.")
        {
            
        }
    }
}