using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy
{
    public class CallerProperties : ICallerProperties
    {
        public EntityReference CallerId { get; set; }
        public EntityReference BusinessUnitId { get; set; }

        public CallerProperties() 
        {
            CallerId = new EntityReference("systemuser", Guid.NewGuid());
            BusinessUnitId = new EntityReference("businessunit", Guid.NewGuid());
        }
    }
}
