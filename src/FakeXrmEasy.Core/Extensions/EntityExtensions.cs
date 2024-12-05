﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Extension method to add an attribute and return the entity itself
        /// </summary>
        /// <param name="e"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Entity AddAttribute(this Entity e, string key, object value)
        {
            e.Attributes.Add(key, value);
            return e;
        }

        /// <summary>
        /// Projects the attributes of entity e so that only the attributes specified in the columnSet are returned
        /// </summary>
        /// <param name="e"></param>
        /// <param name="columnSet"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Entity ProjectAttributes(this Entity e, ColumnSet columnSet, IXrmFakedContext context)
        {
            return ProjectAttributes(e, new QueryExpression() { ColumnSet = columnSet }, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        public static void ApplyDateBehaviour(this Entity e, XrmFakedContext context)
        {
#if FAKE_XRM_EASY || FAKE_XRM_EASY_2013
            return; //Do nothing... DateBehavior wasn't available for versions <= 2013
#else

            var entityMetadata = context.GetEntityMetadataByName(e.LogicalName);

            if (e.LogicalName == null || entityMetadata == null || entityMetadata?.Attributes == null)
            {
                return;
            }

            var dateTimeAttributes = entityMetadata?.Attributes
                        .Where(a => a is DateTimeAttributeMetadata)
                        .Select(a => a as DateTimeAttributeMetadata)
                        .ToList();

            foreach (var attribute in dateTimeAttributes)
            {
                if (!e.Attributes.ContainsKey(attribute.LogicalName))
                {
                    continue;
                }

                if(attribute.DateTimeBehavior == DateTimeBehavior.DateOnly)
                {
                    var currentValue = (DateTime)e[attribute.LogicalName];
                    e[attribute.LogicalName] = new DateTime(currentValue.Year, currentValue.Month, currentValue.Day, 0, 0, 0, DateTimeKind.Utc);
                    break;
                }
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="projected"></param>
        /// <param name="le"></param>
        /// <param name="context"></param>
        public static void ProjectAttributes(Entity e, Entity projected, LinkEntity le, IXrmFakedContext context)
        {
            var sAlias = string.IsNullOrWhiteSpace(le.EntityAlias) ? le.LinkToEntityName : le.EntityAlias;

            if (le.Columns.AllColumns)
            {
                foreach (var attKey in e.Attributes.Keys)
                {
                    if (attKey.StartsWith(sAlias + "."))
                    {
                        projected[attKey] = e[attKey];
                    }
                }

                foreach (var attKey in e.FormattedValues.Keys)
                {
                    if (attKey.StartsWith(sAlias + "."))
                    {
                        projected.FormattedValues[attKey] = e.FormattedValues[attKey];
                    }
                }
            }
            else
            {
                foreach (var attKey in le.Columns.Columns)
                {
                    var linkedAttKey = sAlias + "." + attKey;
                    if (e.Attributes.ContainsKey(linkedAttKey))
                        projected[linkedAttKey] = e[linkedAttKey];

                    if (e.FormattedValues.ContainsKey(linkedAttKey))
                        projected.FormattedValues[linkedAttKey] = e.FormattedValues[linkedAttKey];
                }

            }

            foreach (var nestedLinkedEntity in le.LinkEntities)
            {
                ProjectAttributes(e, projected, nestedLinkedEntity, context);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="qe"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Entity ProjectAttributes(this Entity e, QueryExpression qe, IXrmFakedContext context)
        {
            if (qe.ColumnSet == null || qe.ColumnSet.AllColumns)
            {
                return RemoveNullAttributes(e); //return all the original attributes
            }
            else
            {
                //Return selected list of attributes in a projected entity
                Entity projected = null;

                //However, if we are using proxy types, we must create a instance of the appropiate class
                if (context.ProxyTypesAssemblies.Count() > 0)
                {
                    var subClassType = context.FindReflectedType(e.LogicalName);
                    if (subClassType != null)
                    {
                        var instance = Activator.CreateInstance(subClassType);
                        projected = (Entity)instance;
                        projected.Id = e.Id;
                    }
                    else
                        projected = new Entity(e.LogicalName) { Id = e.Id }; //fallback to generic type if type not found
                }
                else
                    projected = new Entity(e.LogicalName) { Id = e.Id };


                foreach (var attKey in qe.ColumnSet.Columns)
                {
                    //Check if attribute really exists in metadata
                    if (!context.AttributeExistsInMetadata(e.LogicalName, attKey))
                    {
                        throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute, string.Format("The attribute {0} does not exist on this entity.", attKey));
                    }

                    if (e.Attributes.ContainsKey(attKey) && e.Attributes[attKey] != null)
                    {
                        projected[attKey] = CloneAttribute(e[attKey], context);

                        string formattedValue = "";

                        if (e.FormattedValues.TryGetValue(attKey, out formattedValue))
                        {
                            projected.FormattedValues[attKey] = formattedValue;
                        }
                    }
                }


                //Plus attributes from joins
                foreach (var le in qe.LinkEntities)
                {
                    ProjectAttributes(RemoveNullAttributes(e), projected, le, context);
                }
                return RemoveNullAttributes(projected);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Entity RemoveNullAttributes(Entity entity)
        {
            IList<string> nullAttributes = entity.Attributes
                .Where(attribute => attribute.Value == null ||
                                  (attribute.Value is AliasedValue && (attribute.Value as AliasedValue).Value == null))
                .Select(attribute => attribute.Key).ToList();
            foreach (var nullAttribute in nullAttributes)
            {
                entity.Attributes.Remove(nullAttribute);
            }
            return entity;
        }

        /// <summary>
        /// Clones an attribute value to make sure the object reference in memory is different to the original attribute present in the In-Memory database
        /// </summary>
        /// <param name="attributeValue"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static object CloneAttribute(object attributeValue, IXrmFakedContext context = null)
        {
            if (attributeValue == null)
                return null;

            var type = attributeValue.GetType();
            if (type == typeof(string)) 
            {
                return new string((attributeValue as string).ToCharArray());
            }
            else if (type == typeof(EntityReference))
            {
                var original = (attributeValue as EntityReference);
                var clone = new EntityReference(original.LogicalName, original.Id);

                EntityMetadata entityMetadata = null;
                bool containsEntity = false;

                if(context != null)
                {
                    entityMetadata = context.GetEntityMetadataByName(original.LogicalName);
                    containsEntity = context.ContainsEntity(original.LogicalName, original.Id);
                }
                
                if (context != null 
                        && !string.IsNullOrEmpty(original.LogicalName) 
                        && entityMetadata != null 
                        && !string.IsNullOrEmpty(entityMetadata.PrimaryNameAttribute) 
                        && containsEntity)
                {
                    var entity = (context as XrmFakedContext).GetEntityById_Internal(original.LogicalName, original.Id);
                    clone.Name = entity.GetAttributeValue<string>(entityMetadata.PrimaryNameAttribute);
                }
                else
                {
                    clone.Name = CloneAttribute(original.Name) as string;
                }

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
                if (original.KeyAttributes != null)
                {
                    clone.KeyAttributes = new KeyAttributeCollection();
                    clone.KeyAttributes.AddRange(original.KeyAttributes.Select(kvp => new KeyValuePair<string, object>(CloneAttribute(kvp.Key) as string, kvp.Value)).ToArray());
                }
#endif
                return clone;
            }
            else if (type == typeof(BooleanManagedProperty))
            {
                var original = (attributeValue as BooleanManagedProperty);
                return new BooleanManagedProperty(original.Value);
            }
            else if (type == typeof(OptionSetValue))
            {
                var original = (attributeValue as OptionSetValue);
                return new OptionSetValue(original.Value);
            }
            else if (type == typeof(AliasedValue))
            {
                var original = (attributeValue as AliasedValue);
                return new AliasedValue(original.EntityLogicalName, original.AttributeLogicalName, CloneAttribute(original.Value));
            }
            else if (type == typeof(Money))
            {
                var original = (attributeValue as Money);
                return new Money(original.Value);
            }
            else if (attributeValue.GetType() == typeof(EntityCollection))
            {
                var collection = attributeValue as EntityCollection;
                return new EntityCollection(collection.Entities.Select(e => e.Clone(e.GetType())).ToList());
            }
            else if (attributeValue is IEnumerable<Entity>)
            {
                var enumerable = attributeValue as IEnumerable<Entity>;
                return enumerable.Select(e => e.Clone(e.GetType())).ToArray();
            }
#if !FAKE_XRM_EASY
            else if (type == typeof(byte[]))
            {
                var original = (attributeValue as byte[]);
                var copy = new byte[original.Length];
                original.CopyTo(copy, 0);
                return copy;
            }
#endif
#if FAKE_XRM_EASY_9
            else if (attributeValue is OptionSetValueCollection)
            {
                var original = (attributeValue as OptionSetValueCollection);
                var copy = new OptionSetValueCollection(original.ToArray());
                return copy;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var genericTypeArguments = type.GenericTypeArguments;
                    if (genericTypeArguments.Length == 1 && genericTypeArguments[0].IsEnum)
                    {
                        //MultiOption set value
                        return (attributeValue as IEnumerable).Copy();
                    }
                }
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    if (elementType.IsEnum)
                    {
                        return attributeValue.Copy();
                    }
                }
            }
#endif
            else if (type == typeof(int) || type == typeof(Int64))
                return attributeValue; //Not a reference type
            else if (type == typeof(decimal))
                return attributeValue; //Not a reference type
            else if (type == typeof(double))
                return attributeValue; //Not a reference type
            else if (type == typeof(float))
                return attributeValue; //Not a reference type
            else if (type == typeof(byte))
                return attributeValue; //Not a reference type
            else if (type == typeof(bool))
                return attributeValue; //Not a reference type
            else if (type == typeof(Guid))
                return attributeValue; //Not a reference type
            else if (type == typeof(DateTime))
                return attributeValue; //Not a reference type
            else if (attributeValue is Enum)
                return attributeValue; //Not a reference type

            throw new Exception(string.Format("Attribute type not supported when trying to clone attribute '{0}'", type.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Entity Clone(this Entity e, IXrmFakedContext context = null)
        {
            var cloned = new Entity(e.LogicalName);
            cloned.Id = e.Id;
            cloned.LogicalName = e.LogicalName;

            CloneEntity(e, cloned, context);

            return cloned;
        }

        /// <summary>
        /// Clones source entity data into cloned entity
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cloned"></param>
        /// <param name="context">The IXrmFakedContext where the source Entity lives, if any</param>
        internal static void CloneEntity(Entity source, Entity cloned, IXrmFakedContext context = null)
        {
            if (source.FormattedValues != null)
            {
                var formattedValues = new FormattedValueCollection();
                foreach (var key in source.FormattedValues.Keys)
                    formattedValues.Add(key, source.FormattedValues[key]);

                cloned.Inject("FormattedValues", formattedValues);
            }

            foreach (var attKey in source.Attributes.Keys)
            {
                cloned[attKey] = source[attKey] != null ? CloneAttribute(source[attKey], context) : null;
            }
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
            foreach (var attKey in source.KeyAttributes.Keys)
            {
                cloned.KeyAttributes[attKey] = source.KeyAttributes[attKey] != null ? CloneAttribute(source.KeyAttributes[attKey]) : null;
            }
#endif
            foreach (var relatedEntityKeyValuePair in source.RelatedEntities)
            {
                var relationShip = CloneRelationship(relatedEntityKeyValuePair.Key);
                var newKeyValuePair = new KeyValuePair<Relationship, EntityCollection>(relationShip,
                    CloneEntityCollection(relatedEntityKeyValuePair.Value));
                cloned.RelatedEntities.Add(newKeyValuePair);
            }
        }

        /// <summary>
        /// Clones an entire EntityCollection
        /// </summary>
        /// <param name="entityCollection">The source entity collection to clone</param>
        /// <param name="context">A reference to an in-memory context</param>
        /// <returns></returns>
        internal static EntityCollection CloneEntityCollection(EntityCollection entityCollection, IXrmFakedContext context = null)
        {
            var listOfClones = new List<Entity>();

            foreach (var source in entityCollection.Entities)
            {
                listOfClones.Add(source.Clone(context));
            }

            return new EntityCollection(listOfClones);
        }

        /// <summary>
        /// Clones an existing relationship object
        /// </summary>
        /// <param name="relationShip"></param>
        /// <returns></returns>
        internal static Relationship CloneRelationship(Relationship relationShip)
        {
            return new Relationship(relationShip.SchemaName)
            {
                PrimaryEntityRole = relationShip.PrimaryEntityRole
            };
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static T Clone<T>(this Entity e) where T : Entity
        {
            return (T)e.Clone(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Entity Clone(this Entity e, Type t, IXrmFakedContext context = null)
        {
            if (t == null)
                return e.Clone(context);

            var cloned = Activator.CreateInstance(t) as Entity;
            cloned.Id = e.Id;
            cloned.LogicalName = e.LogicalName;

            CloneEntity(e, cloned, context);
            
            return cloned;
        }

        /// <summary>
        /// Extension method to join the attributes of entity e and otherEntity
        /// </summary>
        /// <param name="e"></param>
        /// <param name="otherEntity"></param>
        /// <param name="columnSet"></param>
        /// <param name="alias"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Entity JoinAttributes(this Entity e, Entity otherEntity, ColumnSet columnSet, string alias, IXrmFakedContext context)
        {
            if (otherEntity == null) return e; //Left Join where otherEntity was not matched

            otherEntity = otherEntity.Clone(); //To avoid joining entities from/to the same entities, which would cause collection modified exceptions

            if (columnSet.AllColumns)
            {
                foreach (var attKey in otherEntity.Attributes.Keys)
                {
                    e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, otherEntity[attKey]);
                }

                foreach (var attKey in otherEntity.FormattedValues.Keys)
                {
                    e.FormattedValues[alias + "." + attKey] = otherEntity.FormattedValues[attKey];
                }
            }
            else
            {
                //Return selected list of attributes
                foreach (var attKey in columnSet.Columns)
                {
                    if (!context.AttributeExistsInMetadata(otherEntity.LogicalName, attKey))
                    {
                        throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute, string.Format("The attribute {0} does not exist on this entity.", attKey));
                    }

                    if (otherEntity.Attributes.ContainsKey(attKey))
                    {
                        e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, otherEntity[attKey]);
                    }
                    else
                    {
                        e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, null);
                    }

                    if (otherEntity.FormattedValues.ContainsKey(attKey))
                    {
                        e.FormattedValues[alias + "." + attKey] = otherEntity.FormattedValues[attKey];
                    }
                }
            }
            return e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="otherEntities"></param>
        /// <param name="columnSet"></param>
        /// <param name="alias"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Entity JoinAttributes(this Entity e, IEnumerable<Entity> otherEntities, ColumnSet columnSet, string alias, IXrmFakedContext context)
        {
            foreach (var otherEntity in otherEntities)
            {
                var otherClonedEntity = otherEntity.Clone(); //To avoid joining entities from/to the same entities, which would cause collection modified exceptions

                if (columnSet.AllColumns)
                {
                    foreach (var attKey in otherClonedEntity.Attributes.Keys)
                    {
                        e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, otherClonedEntity[attKey]);
                    }

                    foreach (var attKey in otherEntity.FormattedValues.Keys)
                    {
                        e.FormattedValues[alias + "." + attKey] = otherEntity.FormattedValues[attKey];
                    }
                }
                else
                {
                    //Return selected list of attributes
                    foreach (var attKey in columnSet.Columns)
                    {
                        if (!context.AttributeExistsInMetadata(otherEntity.LogicalName, attKey))
                        {
                            throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute, string.Format("The attribute {0} does not exist on this entity.", attKey));
                        }

                        if (otherClonedEntity.Attributes.ContainsKey(attKey))
                        {
                            e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, otherClonedEntity[attKey]);
                        }
                        else
                        {
                            e[alias + "." + attKey] = new AliasedValue(otherEntity.LogicalName, attKey, null);
                        }

                        if (otherEntity.FormattedValues.ContainsKey(attKey))
                        {
                            e.FormattedValues[alias + "." + attKey] = otherEntity.FormattedValues[attKey];
                        }
                    }
                }
            }
            return e;
        }

        /// <summary>
        /// Returns the key for the attribute name selected (could an entity reference or a primary key or a guid)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sAttributeName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static object KeySelector(this Entity e, string sAttributeName, IXrmFakedContext context)
        {
            if (sAttributeName.Contains("."))
            {
                //Do not lowercase the alias prefix
                var splitted = sAttributeName.Split('.');
                sAttributeName = string.Format("{0}.{1}", splitted[0], splitted[1].ToLower());
            }
            else
            {
                sAttributeName = sAttributeName.ToLower();
            }

            if (!e.Attributes.ContainsKey(sAttributeName))
            {
                //Check if it is the primary key
                if (sAttributeName.Contains("id") &&
                   e.LogicalName.ToLower().Equals(sAttributeName.Substring(0, sAttributeName.Length - 2)))
                {
                    return e.Id;
                }
                return Guid.Empty; //Atrribute is null or doesn´t exists so it can´t be joined
            }

            object keyValue = null;
            AliasedValue aliasedValue;
            if ((aliasedValue = e[sAttributeName] as AliasedValue) != null)
            {
                keyValue = aliasedValue.Value;
            }
            else
            {
                keyValue = e[sAttributeName];
            }

            EntityReference entityReference = keyValue as EntityReference;
            if (entityReference != null)
                return entityReference.Id;

            OptionSetValue optionSetValue = keyValue as OptionSetValue;
            if (optionSetValue != null)
                return optionSetValue.Value;

            Money money = keyValue as Money;
            if (money != null)
                return money.Value;

            return keyValue;
        }

        /// <summary>
        /// Extension method to "hack" internal set properties on sealed classes via reflection
        /// </summary>
        /// <param name="e"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void Inject(this Entity e, string property, object value)
        {
            e.GetType().GetProperty(property).SetValue(e, value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetValueIfEmpty(this Entity e, string property, object value)
        {
            var containsKey = e.Attributes.ContainsKey(property);
            if (!containsKey || containsKey && e[property] == null)
            {
                e[property] = value;
            }
        }

        /// <summary>
        /// ToEntityReference implementation that converts an entity into an entity reference with key attribute info as well
        /// </summary>
        /// <param name="e">Entity to convert to an Entity Reference</param>
        /// <returns></returns>
        public static EntityReference ToEntityReferenceWithKeyAttributes(this Entity e)
        {
            var result = e.ToEntityReference();
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
            result.KeyAttributes = e.KeyAttributes;
#endif
            return result;
        }
    }
}