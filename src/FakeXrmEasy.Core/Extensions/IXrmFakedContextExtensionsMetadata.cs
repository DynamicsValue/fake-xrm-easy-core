using System;
using System.Linq;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Extensions
{
    public static class IXrmFakedContextExtensionsMetadata
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

        internal static bool AttributeExistsInMetadata(this IXrmFakedContext context, string sEntityName, string sAttributeName)
        {
            var relationships = context.Relationships.Where(value => new[] { value.Entity1LogicalName, value.Entity2LogicalName, value.IntersectEntity }.Contains(sEntityName, StringComparer.InvariantCultureIgnoreCase)).ToArray();
            if (relationships.Any(e => e.Entity1Attribute == sAttributeName || e.Entity2Attribute == sAttributeName))
            {
                return true;
            }

            //Early bound types
            if (context.ProxyTypesAssemblies.Count() > 0)
            {
                //Check if attribute exists in the early bound type 
                var earlyBoundType = context.FindReflectedType(sEntityName);
                if (earlyBoundType != null)
                {
                    //Get that type properties
                    var attributeFound = earlyBoundType
                        .GetProperties()
                        .Where(pi => pi.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true).Length > 0)
                        .Where(pi => (pi.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true)[0] as AttributeLogicalNameAttribute).LogicalName.Equals(sAttributeName))
                        .FirstOrDefault();

                    if (attributeFound != null)
                        return true;

                    if (attributeFound == null && context.GetEntityMetadataByName(sEntityName) != null)
                    {
                        //Try with metadata
                        return context.AttributeExistsInInjectedMetadata(sEntityName, sAttributeName);
                    }
                    else
                    {
                        return false;
                    }
                }
                //Try with metadata
                return false;
            }

            if (context.GetEntityMetadataByName(sEntityName) != null)
            {
                //Try with metadata
                return context.AttributeExistsInInjectedMetadata(sEntityName, sAttributeName);
            }

            //Dynamic entities and not entity metadata injected for entity => just return true if not found
            return true;
        }

        internal static bool AttributeExistsInInjectedMetadata(this IXrmFakedContext context, string sEntityName, string sAttributeName)
        {
            var attributeInMetadata = context.FindAttributeTypeInInjectedMetadata(sEntityName, sAttributeName);
            return attributeInMetadata != null;
        }

        internal static Type FindAttributeTypeInInjectedMetadata(this IXrmFakedContext context, string sEntityName, string sAttributeName)
        {
            var entityMetadata = context.GetEntityMetadataByName(sEntityName);
            if(entityMetadata == null)
                return null;

            if (entityMetadata.Attributes == null)
                return null;

            var attribute = entityMetadata.Attributes
                                .Where(a => a.LogicalName == sAttributeName)
                                .FirstOrDefault();

            if (attribute == null)
                return null;

            if (attribute.AttributeType == null)
                return null;

            switch (attribute.AttributeType.Value)
            {
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.BigInt:
                    return typeof(long);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Integer:
                    return typeof(int);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Boolean:
                    return typeof(bool);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.CalendarRules:
                    throw new Exception("CalendarRules: Type not yet supported");

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Lookup:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Customer:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Owner:
                    return typeof(EntityReference);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.DateTime:
                    return typeof(DateTime);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Decimal:
                    return typeof(decimal);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Double:
                    return typeof(double);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.EntityName:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Memo:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.String:
                    return typeof(string);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Money:
                    return typeof(Money);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.PartyList:
                    return typeof(EntityReferenceCollection);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Picklist:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.State:
                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Status:
                    return typeof(OptionSetValue);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Uniqueidentifier:
                    return typeof(Guid);

                case Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Virtual:
#if FAKE_XRM_EASY_9
                    if (attribute.AttributeTypeName.Value == "MultiSelectPicklistType")
                    {
                        return typeof(OptionSetValueCollection);
                    }
#endif
                    throw new Exception("Virtual: Type not yet supported");

                default:
                    return typeof(string);

            }

        }
    }
}
