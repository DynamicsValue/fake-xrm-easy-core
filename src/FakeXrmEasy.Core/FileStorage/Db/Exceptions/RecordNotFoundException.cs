using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when a file upload is initiated against a record that doesn't exists
    /// </summary>
    public class RecordNotFoundException: Exception
    {
        public RecordNotFoundException(EntityReference reference) : base($"The entity reference record for logical name '{reference.LogicalName}' and '{reference.Id}' does not exist.")
        {
            
        }
    }
}