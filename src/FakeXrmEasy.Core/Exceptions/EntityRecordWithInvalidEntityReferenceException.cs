using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.Exceptions
{
    /// <summary>
    /// Throws an exception when an entity record has an entity reference attribute with a null logical name
    /// </summary>
    public class NullLogicalNameEntityReferenceException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="record">The entity record that has the invalid EntityReference attribute</param>
        /// <param name="attributeLogicalName">The name of the attribute in the entity record with the invalid entity reference</param>
        public NullLogicalNameEntityReferenceException(Entity record, string attributeLogicalName) : 
            base ($"The entity record with logical name '{record.LogicalName}' and Id '{record.Id}' has an entity reference attribute '{attributeLogicalName}' with a null logical name.")
        {
            
        }
    }
}