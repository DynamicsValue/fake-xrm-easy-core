using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// EntityMetadata Extensions
    /// </summary>
    public static class EntityMetadataExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="attributes"></param>
        public static void SetAttributeCollection(this EntityMetadata entityMetadata, AttributeMetadata[] attributes)
        {
            //AttributeMetadata is internal set in a sealed class so... just doing this

            entityMetadata.GetType().GetProperty("Attributes").SetValue(entityMetadata, attributes, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="attribute"></param>
        public static void SetAttribute(this EntityMetadata entityMetadata, AttributeMetadata attribute)
        {
            var currentAttributes = entityMetadata.Attributes;
            if (currentAttributes == null)
            {
                currentAttributes = new AttributeMetadata[0];
            }
            var newAttributesList = currentAttributes.Where(a => a.LogicalName != attribute.LogicalName).ToList();
            newAttributesList.Add(attribute);
            var newAttributesArray = newAttributesList.ToArray();

            entityMetadata.GetType().GetProperty("Attributes").SetValue(entityMetadata, newAttributesArray, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="attributes"></param>
        public static void SetAttributeCollection(this EntityMetadata entityMetadata, IEnumerable<AttributeMetadata> attributes)
        {
            entityMetadata.GetType().GetProperty("Attributes").SetValue(entityMetadata, attributes.ToList().ToArray(), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="sPropertyName"></param>
        /// <param name="value"></param>
        public static void SetSealedPropertyValue(this EntityMetadata entityMetadata, string sPropertyName, object value)
        {
            entityMetadata.GetType().GetProperty(sPropertyName).SetValue(entityMetadata, value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeMetadata"></param>
        /// <param name="sPropertyName"></param>
        /// <param name="value"></param>
        public static void SetSealedPropertyValue(this AttributeMetadata attributeMetadata, string sPropertyName, object value)
        {
            attributeMetadata.GetType().GetProperty(sPropertyName).SetValue(attributeMetadata, value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manyToManyRelationshipMetadata"></param>
        /// <param name="sPropertyName"></param>
        /// <param name="value"></param>
        public static void SetSealedPropertyValue(this ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, string sPropertyName, object value)
        {
            manyToManyRelationshipMetadata.GetType().GetProperty(sPropertyName).SetValue(manyToManyRelationshipMetadata, value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oneToManyRelationshipMetadata"></param>
        /// <param name="sPropertyName"></param>
        /// <param name="value"></param>
        public static void SetSealedPropertyValue(this OneToManyRelationshipMetadata oneToManyRelationshipMetadata, string sPropertyName, object value)
        {
            oneToManyRelationshipMetadata.GetType().GetProperty(sPropertyName).SetValue(oneToManyRelationshipMetadata, value, null);
        }
    }
}
