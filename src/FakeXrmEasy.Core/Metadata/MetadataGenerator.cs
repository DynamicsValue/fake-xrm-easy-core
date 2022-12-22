using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FakeXrmEasy.Metadata
{
    internal class MetadataGenerator
    {
        public static IEnumerable<EntityMetadata> FromEarlyBoundEntities(Assembly earlyBoundEntitiesAssembly)
        {
            var types = earlyBoundEntitiesAssembly.GetTypes();
            return FromTypes(types);
        }

        internal static IEnumerable<EntityMetadata> FromTypes(Type[] types)
        {
            var entityMetadatas = new List<EntityMetadata>();

            foreach (var possibleEarlyBoundEntity in types)
            {
                var metadata = FromType(possibleEarlyBoundEntity);
                if(metadata != null)
                    entityMetadatas.Add(metadata);
            }

            return entityMetadatas;
        }

        internal static EntityMetadata FromType(Type possibleEarlyBoundEntity)
        {
            EntityLogicalNameAttribute entityLogicalNameAttribute = GetCustomAttribute<EntityLogicalNameAttribute>(possibleEarlyBoundEntity);
            if (entityLogicalNameAttribute == null) return null;

            EntityMetadata metadata = new EntityMetadata();
            metadata.LogicalName = entityLogicalNameAttribute.LogicalName;

            FieldInfo entityTypeCode = possibleEarlyBoundEntity.GetField("EntityTypeCode", BindingFlags.Static | BindingFlags.Public);
            if (entityTypeCode != null)
            {
                metadata.SetFieldValue("_objectTypeCode", entityTypeCode.GetValue(null));
            }

            List<ManyToManyRelationshipMetadata> manyToManyRelationshipMetadatas = new List<ManyToManyRelationshipMetadata>();
            List<OneToManyRelationshipMetadata> manyToOneRelationshipMetadatas = new List<OneToManyRelationshipMetadata>();

            var idProperty = possibleEarlyBoundEntity.GetProperty("Id");
            AttributeLogicalNameAttribute attributeLogicalNameAttribute;
            if (idProperty != null && (attributeLogicalNameAttribute = GetCustomAttribute<AttributeLogicalNameAttribute>(idProperty)) != null)
            {
                metadata.SetFieldValue("_primaryIdAttribute", attributeLogicalNameAttribute.LogicalName);
            }

            var attributeMetadataProperties = possibleEarlyBoundEntity.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(x => x.Name != "Id" && Attribute.IsDefined(x, typeof(AttributeLogicalNameAttribute))
                                                        && !Attribute.IsDefined(x, typeof(RelationshipSchemaNameAttribute)));

            var relationshipMetadataProperties = possibleEarlyBoundEntity.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(x => Attribute.IsDefined(x, typeof(RelationshipSchemaNameAttribute)));

            var attributeMetadatas = PopulateAttributeProperties(metadata, attributeMetadataProperties);
            if (attributeMetadatas.Any())
            {
                metadata.SetSealedPropertyValue("Attributes", attributeMetadatas.ToArray());
            }

            var relationshipMetadatas = PopulateRelationshipProperties(possibleEarlyBoundEntity, metadata, relationshipMetadataProperties);
            
            if (relationshipMetadatas.ManyToManyRelationships.Any())
            {
                metadata.SetSealedPropertyValue("ManyToManyRelationships", relationshipMetadatas.ManyToManyRelationships.ToArray());
            }
            if (relationshipMetadatas.ManyToOneRelationships.Any())
            {
                metadata.SetSealedPropertyValue("ManyToOneRelationships", relationshipMetadatas.ManyToOneRelationships.ToArray());
            }
            if (relationshipMetadatas.OneToManyRelationships.Any())
            {
                metadata.SetSealedPropertyValue("OneToManyRelationships", relationshipMetadatas.OneToManyRelationships.ToArray());
            }
            return metadata;
        }

        private static List<AttributeMetadata> PopulateProperties(EntityMetadata metadata, IEnumerable<PropertyInfo> properties)
        {
            List<AttributeMetadata> attributeMetadatas = new List<AttributeMetadata>();

            foreach (var property in properties)
            {
                var attributeMetadata = PopulateAttributeProperty(metadata, property);
                if(attributeMetadata != null)
                {
                    attributeMetadatas.Add(attributeMetadata);
                }
            }

            return attributeMetadatas;
        }

        private static List<AttributeMetadata> PopulateAttributeProperties(EntityMetadata metadata, IEnumerable<PropertyInfo> properties)
        {
            List<AttributeMetadata> attributeMetadatas = new List<AttributeMetadata>();

            foreach (var property in properties)
            {
                var attributeMetadata = PopulateAttributeProperty(metadata, property);
                if (attributeMetadata != null)
                {
                    attributeMetadatas.Add(attributeMetadata);
                }
            }

            return attributeMetadatas;
        }

        internal class AllRelationShips
        {
            internal List<OneToManyRelationshipMetadata> ManyToOneRelationships { get; set; }
            internal List<OneToManyRelationshipMetadata> OneToManyRelationships { get; set; }
            internal List<ManyToManyRelationshipMetadata> ManyToManyRelationships { get; set; }

            public AllRelationShips()
            {
                ManyToOneRelationships = new List<OneToManyRelationshipMetadata>();
                OneToManyRelationships = new List<OneToManyRelationshipMetadata>();
                ManyToManyRelationships = new List<ManyToManyRelationshipMetadata>();
            }
        }

        private static AllRelationShips PopulateRelationshipProperties(Type possibleEarlyBoundEntityType, 
                                                                                    EntityMetadata metadata, 
                                                                                    IEnumerable<PropertyInfo> properties)
        {
            var allRelationships = new AllRelationShips();

            foreach (var property in properties)
            {
                PopulateRelationshipProperty(possibleEarlyBoundEntityType, metadata, property, allRelationships);
            }

            return allRelationships;
        }

        private static AttributeMetadata PopulateAttributeProperty(EntityMetadata metadata, PropertyInfo property)
        {
            var attributeLogicalNameAttribute = GetCustomAttribute<AttributeLogicalNameAttribute>(property);
#if !FAKE_XRM_EASY
            if (property.PropertyType == typeof(byte[]))
            {
                metadata.SetFieldValue("_primaryImageAttribute", attributeLogicalNameAttribute.LogicalName);
            }
#endif
            AttributeMetadata attributeMetadata;
            if (attributeLogicalNameAttribute.LogicalName == "statecode")
            {
                attributeMetadata = new StateAttributeMetadata();
            }
            else if (attributeLogicalNameAttribute.LogicalName == "statuscode")
            {
                attributeMetadata = new StatusAttributeMetadata();
            }
            else if (attributeLogicalNameAttribute.LogicalName == metadata.PrimaryIdAttribute)
            {
                attributeMetadata = new AttributeMetadata();
                attributeMetadata.SetSealedPropertyValue("AttributeType", AttributeTypeCode.Uniqueidentifier);
            }
            else
            {
                attributeMetadata = CreateAttributeMetadata(property.PropertyType);
            }

            attributeMetadata.SetFieldValue("_entityLogicalName", metadata.LogicalName);
            attributeMetadata.SetFieldValue("_logicalName", attributeLogicalNameAttribute.LogicalName);

            return attributeMetadata;
        }
        private static void PopulateRelationshipProperty(Type possibleEarlyBoundEntity, EntityMetadata metadata, PropertyInfo property, AllRelationShips allRelationships)
        {
            RelationshipSchemaNameAttribute relationshipSchemaNameAttribute = GetCustomAttribute<RelationshipSchemaNameAttribute>(property);

            if (property.PropertyType.Name == "IEnumerable`1")
            {
                PropertyInfo peerProperty = property.PropertyType.GetGenericArguments()[0].GetProperties().SingleOrDefault(x => x.PropertyType == possibleEarlyBoundEntity && GetCustomAttribute<RelationshipSchemaNameAttribute>(x)?.SchemaName == relationshipSchemaNameAttribute.SchemaName);
                if (peerProperty == null || peerProperty.PropertyType.Name == "IEnumerable`1") // N:N relationship
                {
                    ManyToManyRelationshipMetadata relationshipMetadata = new ManyToManyRelationshipMetadata();
                    relationshipMetadata.SchemaName = relationshipSchemaNameAttribute.SchemaName;
                    allRelationships.ManyToManyRelationships.Add(relationshipMetadata);
                }
                else // 1:N relationship
                {
                    var relationShipMetadata = CreateOneToManyRelationshipMetadata(possibleEarlyBoundEntity, property, property.PropertyType.GetGenericArguments()[0], peerProperty);
                    allRelationships.OneToManyRelationships.Add(relationShipMetadata);
                }
            }
            else //N:1 Property
            {
                var relationShipMetadata = CreateOneToManyRelationshipMetadata(property.PropertyType, 
                    property.PropertyType.GetProperties().SingleOrDefault(x => x.PropertyType.GetGenericArguments().SingleOrDefault() == possibleEarlyBoundEntity && GetCustomAttribute<RelationshipSchemaNameAttribute>(x)?.SchemaName == relationshipSchemaNameAttribute.SchemaName), 
                    possibleEarlyBoundEntity, 
                    property);
                allRelationships.ManyToOneRelationships.Add(relationShipMetadata);
            }
        }

        private static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T));
        }

        private static AttributeMetadata CreateAttributeMetadata(Type propertyType)
        {
            if (typeof(string) == propertyType)
            {
                return new StringAttributeMetadata();
            }
            else if (typeof(EntityReference).IsAssignableFrom(propertyType))
            {
                return new LookupAttributeMetadata();
            }
            else if (typeof(OptionSetValue).IsAssignableFrom(propertyType))
            {
                return new PicklistAttributeMetadata();
            }
            else if (typeof(Money).IsAssignableFrom(propertyType))
            {
                return new MoneyAttributeMetadata();
            }
            else if (propertyType.IsGenericType)
            {
                Type genericType = propertyType.GetGenericArguments().FirstOrDefault();
                if (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (typeof(int) == genericType)
                    {
                        return new IntegerAttributeMetadata();
                    }
                    else if (typeof(double) == genericType)
                    {
                        return new DoubleAttributeMetadata();
                    }
                    else if (typeof(bool) == genericType)
                    {
                        return new BooleanAttributeMetadata();
                    }
                    else if (typeof(decimal) == genericType)
                    {
                        return new DecimalAttributeMetadata();
                    }
                    else if (typeof(DateTime) == genericType)
                    {
                        return new DateTimeAttributeMetadata();
                    }
                    else if (typeof(Guid) == genericType)
                    {
                        return new LookupAttributeMetadata();
                    }
                    else if (typeof(long) == genericType)
                    {
                        return new BigIntAttributeMetadata();
                    }
                    else if (typeof(Enum).IsAssignableFrom(genericType))
                    {
                        return new StateAttributeMetadata();
                    }
                    else
                    {
                        throw new Exception($"Type {propertyType.Name}{genericType?.Name} has not been mapped to an AttributeMetadata.");
                    }
                }
                else if (propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var partyList = new LookupAttributeMetadata();
                    partyList.SetSealedPropertyValue("AttributeType", AttributeTypeCode.PartyList);
                    return partyList;
                }
                else
                {
                    throw new Exception($"Type {propertyType.Name}{genericType?.Name} has not been mapped to an AttributeMetadata.");
                }
            }
            else if (typeof(BooleanManagedProperty) == propertyType)
            {
                var booleanManaged = new BooleanAttributeMetadata();
                booleanManaged.SetSealedPropertyValue("AttributeType", AttributeTypeCode.ManagedProperty);
                return booleanManaged;
            }
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013
            else if (typeof(Guid) == propertyType)
            {
                return new UniqueIdentifierAttributeMetadata();
            }
#endif
#if !FAKE_XRM_EASY
            else if (typeof(byte[]) == propertyType)
            {

                return new ImageAttributeMetadata();
            }
#endif
#if FAKE_XRM_EASY_9
            else if (typeof(OptionSetValueCollection).IsAssignableFrom(propertyType))
            {
                return new MultiSelectPicklistAttributeMetadata();
            }
#endif
            else
            {
                throw new Exception($"Type {propertyType.Name} has not been mapped to an AttributeMetadata.");
            }
        }

        private static OneToManyRelationshipMetadata CreateOneToManyRelationshipMetadata(Type referencingEntity, 
                                                        PropertyInfo referencingAttribute, 
                                                        Type referencedEntity, 
                                                        PropertyInfo referencedAttribute)
        {
            if (referencingEntity == null || referencingAttribute == null || referencedEntity == null || referencedAttribute == null) 
                return null;

            OneToManyRelationshipMetadata relationshipMetadata = new OneToManyRelationshipMetadata();
            relationshipMetadata.SchemaName = GetCustomAttribute<RelationshipSchemaNameAttribute>(referencingAttribute).SchemaName;
            relationshipMetadata.ReferencingEntity = GetCustomAttribute<EntityLogicalNameAttribute>(referencingEntity).LogicalName;
            relationshipMetadata.ReferencingAttribute = GetCustomAttribute<AttributeLogicalNameAttribute>(referencingAttribute)?.LogicalName;
            relationshipMetadata.ReferencedEntity = GetCustomAttribute<EntityLogicalNameAttribute>(referencedEntity).LogicalName;
            relationshipMetadata.ReferencedAttribute = GetCustomAttribute<AttributeLogicalNameAttribute>(referencedAttribute).LogicalName;

            return relationshipMetadata;
        }
    }
}
