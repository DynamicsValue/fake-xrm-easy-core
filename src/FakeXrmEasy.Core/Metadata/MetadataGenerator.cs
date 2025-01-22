using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Core.Extensions;
using FakeXrmEasy.Core.FileStorage;

namespace FakeXrmEasy.Metadata
{
    internal static class MetadataGenerator
    {
        public static IEnumerable<EntityMetadata> FromEarlyBoundEntities(Assembly earlyBoundEntitiesAssembly, IXrmFakedContext context)
        {
            var types = earlyBoundEntitiesAssembly.GetTypes();
            return FromTypes(types, context);
        }

        internal static IEnumerable<EntityMetadata> FromTypes(Type[] types, IXrmFakedContext context)
        {
            var entityMetadatas = new List<EntityMetadata>();

            foreach (var possibleEarlyBoundEntity in types)
            {
                var metadata = FromType(possibleEarlyBoundEntity, context);
                if(metadata != null)
                    entityMetadatas.Add(metadata);
            }

            return entityMetadatas;
        }

        internal static EntityMetadata FromType(Type possibleEarlyBoundEntity, IXrmFakedContext context)
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

            var attributeMetadatas = PopulateAttributeProperties(metadata, attributeMetadataProperties, context);
            if (attributeMetadatas.Any())
            {
                metadata.SetSealedPropertyValue("Attributes", attributeMetadatas.ToArray());
            }

            var relationshipMetadatas = PopulateRelationshipProperties(metadata, possibleEarlyBoundEntity, relationshipMetadataProperties);
            
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

        private static List<AttributeMetadata> PopulateAttributeProperties(EntityMetadata metadata, IEnumerable<PropertyInfo> properties, IXrmFakedContext context)
        {
            List<AttributeMetadata> attributeMetadatas = new List<AttributeMetadata>();

            foreach (var property in properties)
            {
                var attributeMetadata = PopulateAttributeProperty(metadata, property, context);
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

        private static AllRelationShips PopulateRelationshipProperties(EntityMetadata entityMetadata,
                                                                                    Type possibleEarlyBoundEntityType, 
                                                                                    IEnumerable<PropertyInfo> relationshipPropertyInfos)
        {
            var allRelationships = new AllRelationShips();

            foreach (var property in relationshipPropertyInfos)
            {
                PopulateRelationshipProperty(entityMetadata, possibleEarlyBoundEntityType, property, allRelationships);
            }

            return allRelationships;
        }

        private static AttributeMetadata PopulateAttributeProperty(EntityMetadata metadata, PropertyInfo property, IXrmFakedContext context)
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
                attributeMetadata = CreateAttributeMetadata(property.PropertyType, context);
            }

            attributeMetadata.SetFieldValue("_entityLogicalName", metadata.LogicalName);
            attributeMetadata.SetFieldValue("_logicalName", attributeLogicalNameAttribute.LogicalName);

            return attributeMetadata;
        }
        private static void PopulateRelationshipProperty(EntityMetadata entityMetadata, 
            Type possibleEarlyBoundEntityType, 
            PropertyInfo relationshipProperty, 
            AllRelationShips allRelationships)
        {
            RelationshipSchemaNameAttribute relationshipSchemaNameAttribute = GetCustomAttribute<RelationshipSchemaNameAttribute>(relationshipProperty);
            
            if (relationshipProperty.IsEnumerable())
            {
                //Could be 1:N or N:N
                var enumerableGenericArgumentType = relationshipProperty.PropertyType.GetGenericArguments()[0];
                PropertyInfo peerProperty = enumerableGenericArgumentType.GetProperties()
                    .SingleOrDefault(x => x.PropertyType == possibleEarlyBoundEntityType && GetCustomAttribute<RelationshipSchemaNameAttribute>(x)?.SchemaName == relationshipSchemaNameAttribute.SchemaName);

                if (peerProperty == null || peerProperty.IsEnumerable())
                {
                    //N:N    
                    var manyToManyRelationship = CreateManyToManyRelationshipMetadata(relationshipSchemaNameAttribute,
                        possibleEarlyBoundEntityType, enumerableGenericArgumentType);
                    allRelationships.ManyToManyRelationships.Add(manyToManyRelationship);
                }
                else
                {
                    //1:N relationship
                    var relationShipMetadata = CreateOneToManyRelationshipMetadata(entityMetadata, enumerableGenericArgumentType, peerProperty, possibleEarlyBoundEntityType, entityMetadata.PrimaryIdAttribute);
                    allRelationships.OneToManyRelationships.Add(relationShipMetadata);
                }

            }
            else
            {
                //N:1 relationship
                var relationShipMetadata = CreateOneToManyRelationshipMetadata(entityMetadata, possibleEarlyBoundEntityType, 
                    relationshipProperty, 
                    relationshipProperty.PropertyType,
                    relationshipProperty.PropertyType.GetPrimaryIdFieldName()
                );
                
                allRelationships.ManyToOneRelationships.Add(relationShipMetadata);
            }
        }

        private static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T));
        }

        internal static AttributeMetadata CreateAttributeMetadata(Type propertyType, IXrmFakedContext context)
        {
            var fileStorageSettings = context.GetProperty<IFileStorageSettings>();
            
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
                return CreateAttributeMetadataFromGenericType(propertyType);
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
                #if FAKE_XRM_EASY_9
                return new ImageAttributeMetadata()
                {
                    MaxSizeInKB = fileStorageSettings.ImageMaxSizeInKB
                };
                #else
                return new ImageAttributeMetadata();
                #endif
                
            }
#endif
#if FAKE_XRM_EASY_9
            else if (typeof(OptionSetValueCollection).IsAssignableFrom(propertyType))
            {
                return new MultiSelectPicklistAttributeMetadata();
            }
            else if (typeof(object) == propertyType)
            {
                return new FileAttributeMetadata()
                {
                    MaxSizeInKB = fileStorageSettings.MaxSizeInKB
                };
            }
#endif
            else
            {
                throw new Exception($"Type {propertyType.Name} has not been mapped to an AttributeMetadata.");
            }
        }

        internal static AttributeMetadata CreateAttributeMetadataFromGenericType(Type propertyType)
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
        private static OneToManyRelationshipMetadata CreateOneToManyRelationshipMetadata(
                                                        EntityMetadata entityMetadata,
                                                        Type referencingEntity, 
                                                        PropertyInfo referencingAttribute, 
                                                        Type referencedEntity, 
                                                        string referencedAttribute)
        {
            if (referencingEntity == null || referencingAttribute == null || referencedEntity == null || referencedAttribute == null) 
                return null;

            OneToManyRelationshipMetadata relationshipMetadata = new OneToManyRelationshipMetadata();
            relationshipMetadata.SchemaName = GetCustomAttribute<RelationshipSchemaNameAttribute>(referencingAttribute).SchemaName;
            relationshipMetadata.ReferencingEntity = GetCustomAttribute<EntityLogicalNameAttribute>(referencingEntity).LogicalName;
            relationshipMetadata.ReferencingAttribute = GetCustomAttribute<AttributeLogicalNameAttribute>(referencingAttribute)?.LogicalName;
            relationshipMetadata.ReferencedEntity = GetCustomAttribute<EntityLogicalNameAttribute>(referencedEntity).LogicalName;
            relationshipMetadata.ReferencedAttribute = referencedAttribute;
            
            return relationshipMetadata;
        }
        
        private static ManyToManyRelationshipMetadata CreateManyToManyRelationshipMetadata(
            RelationshipSchemaNameAttribute relationshipSchemaNameAttribute,
            Type referencingEntity,
            Type referencedEntity)
        {
            if (referencingEntity == null || referencedEntity == null) 
                return null;

            ManyToManyRelationshipMetadata relationshipMetadata = new ManyToManyRelationshipMetadata();
            relationshipMetadata.SchemaName = relationshipSchemaNameAttribute.SchemaName;
            relationshipMetadata.Entity1LogicalName = GetCustomAttribute<EntityLogicalNameAttribute>(referencingEntity).LogicalName;
            relationshipMetadata.Entity1IntersectAttribute = referencingEntity.GetPrimaryIdFieldName();

            relationshipMetadata.Entity2LogicalName = GetCustomAttribute<EntityLogicalNameAttribute>(referencedEntity).LogicalName;
            relationshipMetadata.Entity2IntersectAttribute = referencedEntity.GetPrimaryIdFieldName();

            return relationshipMetadata;
        }
    }
}
