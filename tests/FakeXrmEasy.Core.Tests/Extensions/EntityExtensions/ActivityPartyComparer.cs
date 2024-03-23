using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    internal class ActivityPartyComparer: EqualityComparer<Entity>
    {
        public override bool Equals(Entity x, Entity y)
        {
            var partyId_X = x["partyid"] as EntityReference;
            var partyId_Y = y["partyid"] as EntityReference;

            if (partyId_X?.LogicalName != partyId_Y?.LogicalName) return false;
            if (partyId_X?.Id != partyId_Y?.Id) return false;

            return true;
        }

        public override int GetHashCode(Entity obj)
        {
            return obj.LogicalName.GetHashCode() * obj["partyid"].GetHashCode();
        }
    }
}