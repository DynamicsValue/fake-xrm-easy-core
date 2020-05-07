using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Query
{
    public static class IXrmFakedContextQueryExtensions
    {
        internal static void EnsureEntityNameExistsInMetadata(this IXrmFakedContext context, string sEntityName)
        {
            if (context.Relationships.Any(value => new[] { value.Entity1LogicalName, value.Entity2LogicalName, value.IntersectEntity }.Contains(sEntityName, StringComparer.InvariantCultureIgnoreCase)))
            {
                return;
            }

            // Entity metadata is checked differently when we are using a ProxyTypesAssembly => we can infer that from the generated types assembly
            if (context.ProxyTypesAssemblies.Count() > 0)
            {
                var subClassType = context.FindReflectedType(sEntityName);
                if (subClassType == null)
                {
                    throw new Exception($"Entity {sEntityName} does not exist in the metadata cache");
                }
            }
        }

        
    }
}
