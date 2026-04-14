using System;

namespace FakeXrmEasy.Core.Exceptions.Metadata
{
    /// <summary>
    /// Exception thrown when a specific attribute metadata could not be generated
    /// </summary>
    public class AttributeMetadataGenerationException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="attributeName"></param>
        /// <param name="entityName"></param>
        public AttributeMetadataGenerationException(string typeName, string attributeName, string entityName) :
            base($"Type '{typeName}' can not be mapped to an AttributeMetadata for attribute '{attributeName}' on entity '{entityName}'.")
        {
            
        }
    }
}