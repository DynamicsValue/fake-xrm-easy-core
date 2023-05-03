using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Core.PipelineTypes
{
    internal sealed class EntityOptionSetEnum
    {
        public static int? GetEnum(Entity entity, string attributeLogicalName)
        {
            if (entity.Attributes.ContainsKey(attributeLogicalName))
            {
                OptionSetValue value = entity.GetAttributeValue<OptionSetValue>(attributeLogicalName);
                if (value != null)
                {
                    return value.Value;
                }
            }
            return null;
        }

        public static IEnumerable<T> GetMultiEnum<T>(Entity entity, string attributeLogicalName)

        {
            OptionSetValueCollection value = entity.GetAttributeValue<OptionSetValueCollection>(attributeLogicalName);
            List<T> list = new List<T>();
            if (value == null)
            {
                return list;
            }
            list.AddRange(Enumerable.Select(value, v => (T)(object)v.Value));
            return list;
        }

        public static OptionSetValueCollection GetMultiEnum<T>(Entity entity, string attributeLogicalName, IEnumerable<T> values)
        {
            if (values == null)
            {
                return null;
            }
            OptionSetValueCollection collection = new OptionSetValueCollection();
            collection.AddRange(Enumerable.Select(values, v => new OptionSetValue((int)(object)v)));
            return collection;
        }
    }
}
