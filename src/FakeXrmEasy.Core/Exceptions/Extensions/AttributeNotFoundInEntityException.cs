using System;

namespace FakeXrmEasy.Core.Exceptions.Extensions
{
    /// <summary>
    /// Exception thrown when attempting to access an attribute in an entity record that doesn't exist
    /// </summary>
    public class AttributeNotFoundInEntityException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="attributeName">The missing attribute</param>
        public AttributeNotFoundInEntityException(string attributeName) : base(
            $"Attribute '{attributeName}' was not found in entity record")
        {
            
        }
    }
}