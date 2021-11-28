using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy
{
    /// <summary>
    /// Caller Properties
    /// </summary>
    public class CallerProperties : ICallerProperties
    {
        /// <summary>
        /// CallerId
        /// </summary>
        public EntityReference CallerId { get; set; }

        /// <summary>
        /// BusinessUnitId
        /// </summary>
        public EntityReference BusinessUnitId { get; set; }

        /// <summary>
        /// Caller Properties
        /// </summary>
        public CallerProperties() 
        {
            CallerId = new EntityReference("systemuser", Guid.NewGuid());
            BusinessUnitId = new EntityReference("businessunit", Guid.NewGuid());
        }
    }
}
