using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Client;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Exceptions;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Stores the current license context (the current selected license of the 3 available licenses)
        /// </summary>
        public FakeXrmEasyLicense? LicenseContext { get; set; }

        /// <summary>
        /// Entity Active StateCode
        /// </summary>
        protected const int EntityActiveStateCode = 0;

        /// <summary>
        /// Entity Inactive StateCode
        /// </summary>
        protected const int EntityInactiveStateCode = 1;

        #region CRUD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="validate"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Guid GetRecordUniqueId(EntityReference record, bool validate = true)
        {
            if (string.IsNullOrWhiteSpace(record.LogicalName))
            {
                throw new InvalidOperationException("The entity logical name must not be null or empty.");
            }

            // Don't fail with invalid operation exception, if no record of this entity exists, but entity is known
            if (!Data.ContainsKey(record.LogicalName) && !EntityMetadata.ContainsKey(record.LogicalName))
            {
                if (ProxyTypesAssemblies == null || !ProxyTypesAssemblies.Any())
                {
                    throw new InvalidOperationException($"The entity logical name {record.LogicalName} is not valid.");
                }

                if (!ProxyTypesAssemblies.SelectMany(p=> p.GetTypes()).Any(type => FindReflectedType(record.LogicalName) != null))
                {
                    throw new InvalidOperationException($"The entity logical name {record.LogicalName} is not valid.");
                }
            }

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
            if (record.Id == Guid.Empty && record.HasKeyAttributes())
            {
                if (EntityMetadata.ContainsKey(record.LogicalName))
                {
                    var entityMetadata = EntityMetadata[record.LogicalName];
                    foreach (var key in entityMetadata.Keys)
                    {
                        if (record.KeyAttributes.Keys.Count == key.KeyAttributes.Length && key.KeyAttributes.All(x => record.KeyAttributes.Keys.Contains(x)))
                        {
                            if (Data.ContainsKey(record.LogicalName))
                            {
                                var matchedRecord = Data[record.LogicalName].Values.SingleOrDefault(x => record.KeyAttributes.All(k => x.Attributes.ContainsKey(k.Key) && x.Attributes[k.Key] != null && x.Attributes[k.Key].Equals(k.Value)));
                                if (matchedRecord != null)
                                {
                                    return matchedRecord.Id;
                                }
                            }
                            if (validate)
                            {
                                throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { Message = $"{record.LogicalName} with the specified Alternate Keys Does Not Exist"});
                            }
                        }
                    }
                }
                if (validate)
                {
                    throw new InvalidOperationException($"The requested key attributes do not exist for the entity {record.LogicalName}");
                }
            }
#endif          
            return record.Id;
        }   

        /// <summary>
        /// Updates an entity in the context directly (i.e. skips any middleware setup)
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpdateEntity(Entity e)
        {
            if (e == null)
            {
                throw new InvalidOperationException("The entity must not be null");
            }
            e = e.Clone(e.GetType());
            var reference = e.ToEntityReferenceWithKeyAttributes();
            e.Id = GetRecordUniqueId(reference);

            // Update specific validations: The entity record must exist in the context
            if (Data.ContainsKey(e.LogicalName) &&
                Data[e.LogicalName].ContainsKey(e.Id))
            {
                var integrityOptions = GetProperty<IIntegrityOptions>();

                // Add as many attributes to the entity as the ones received (this will keep existing ones)
                var cachedEntity = Data[e.LogicalName][e.Id];
                foreach (var sAttributeName in e.Attributes.Keys.ToList())
                {
                    var attribute = e[sAttributeName];
                    if (attribute == null)
                    {
                        cachedEntity.Attributes.Remove(sAttributeName);
                    }
                    else if (attribute is DateTime)
                    {
                        cachedEntity[sAttributeName] = ConvertToUtc((DateTime)e[sAttributeName]);
                    }
                    else
                    {
                        if (attribute is EntityReference && integrityOptions.ValidateEntityReferences)
                        {
                            var target = (EntityReference)e[sAttributeName];
                            attribute = ResolveEntityReference(target);
                        }
                        cachedEntity[sAttributeName] = attribute;
                    }
                }

                // Update ModifiedOn
                cachedEntity["modifiedon"] = DateTime.UtcNow;
                cachedEntity["modifiedby"] = CallerProperties.CallerId;
            }
            else
            {
                // The entity record was not found, return a CRM-ish update error message
                throw FakeOrganizationServiceFaultFactory.New($"{e.LogicalName} with Id {e.Id} Does Not Exist");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sLogicalName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Entity GetEntityById(string sLogicalName, Guid id)
        {
            if(!Data.ContainsKey(sLogicalName)) 
            {
                throw new InvalidOperationException($"The entity logical name '{sLogicalName}' is not valid.");
            }

            if(!Data[sLogicalName].ContainsKey(id)) 
            {
                throw new InvalidOperationException($"The id parameter '{id.ToString()}' for entity logical name '{sLogicalName}' is not valid.");
            }

            return Data[sLogicalName][id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sLogicalName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsEntity(string sLogicalName, Guid id)
        {
            if(!Data.ContainsKey(sLogicalName)) 
            {
                return false;
            }

            if(!Data[sLogicalName].ContainsKey(id)) 
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntityById<T>(Guid id) where T: Entity
        {
            var typeParameter = typeof(T);

            var logicalName = "";

            if (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).Length > 0)
            {
                logicalName = (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)[0] as EntityLogicalNameAttribute).LogicalName;
            }

            return GetEntityById(logicalName, id) as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        protected EntityReference ResolveEntityReference(EntityReference er)
        {
            if (!Data.ContainsKey(er.LogicalName) || !Data[er.LogicalName].ContainsKey(er.Id))
            {
                if (er.Id == Guid.Empty && er.HasKeyAttributes())
                {
                    return ResolveEntityReferenceByAlternateKeys(er);
                }
                else
                {
                    throw FakeOrganizationServiceFaultFactory.New($"{er.LogicalName} With Id = {er.Id:D} Does Not Exist");
                }
            }
            return er;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        protected EntityReference ResolveEntityReferenceByAlternateKeys(EntityReference er)
        {
            var resolvedId = GetRecordUniqueId(er);

            return new EntityReference()
            {
                LogicalName = er.LogicalName,
                Id = resolvedId
            };
        }

        /// <summary>
        /// Fakes the delete method. Very similar to the Retrieve one
        /// </summary>
        /// <param name="er"></param>
        public void DeleteEntity(EntityReference er)
        {
            // Don't fail with invalid operation exception, if no record of this entity exists, but entity is known
            if (!this.Data.ContainsKey(er.LogicalName))
            {
                if (ProxyTypesAssemblies.Count() == 0)
                {
                    throw new InvalidOperationException($"The entity logical name {er.LogicalName} is not valid.");
                }

                if (FindReflectedType(er.LogicalName) == null)
                {
                    throw new InvalidOperationException($"The entity logical name {er.LogicalName} is not valid.");
                }
            }

            // Entity logical name exists, so , check if the requested entity exists
            if (this.Data.ContainsKey(er.LogicalName) && this.Data[er.LogicalName] != null &&
                this.Data[er.LogicalName].ContainsKey(er.Id))
            {
                // Entity found => return only the subset of columns specified or all of them
                this.Data[er.LogicalName].Remove(er.Id);
            }
            else
            {
                // Entity not found in the context => throw not found exception
                // The entity record was not found, return a CRM-ish update error message
                throw FakeOrganizationServiceFaultFactory.New($"{er.LogicalName} with Id {er.Id} Does Not Exist");
            }
        }
        #endregion

        #region Other protected methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void AddEntityDefaultAttributes(Entity e)
        {
            // Add createdon, modifiedon, createdby, modifiedby properties
            if (CallerProperties.CallerId == null)
            {
                CallerProperties.CallerId = new EntityReference("systemuser", Guid.NewGuid()); // Create a new instance by default
            }

            var integrityOptions = GetProperty<IIntegrityOptions>();

            if (integrityOptions.ValidateEntityReferences)
            {
                if (!Data.ContainsKey("systemuser"))
                {
                    Data.Add("systemuser", new Dictionary<Guid, Entity>());
                }
                if (!Data["systemuser"].ContainsKey(CallerProperties.CallerId.Id))
                {
                    Data["systemuser"].Add(CallerProperties.CallerId.Id, new Entity("systemuser") { Id = CallerProperties.CallerId.Id });
                }
            }

            var isManyToManyRelationshipEntity = e.LogicalName != null && this._relationships.ContainsKey(e.LogicalName);

            EntityInitializerService.Initialize(e, CallerProperties.CallerId.Id, this, isManyToManyRelationshipEntity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected internal void ValidateEntity(Entity e)
        {
            if (e == null)
            {
                throw new InvalidOperationException("The entity must not be null");
            }

            // Validate the entity
            if (string.IsNullOrWhiteSpace(e.LogicalName))
            {
                throw new InvalidOperationException("The LogicalName property must not be empty");
            }

            if (e.Id == Guid.Empty)
            {
                throw new InvalidOperationException("The Id property must not be empty");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Guid CreateEntity(Entity e)
        {
            if (e == null)
            {
                throw new InvalidOperationException("The entity must not be null");
            }

            var clone = e.Clone(e.GetType());

            if (clone.Id == Guid.Empty)
            {
                clone.Id = Guid.NewGuid(); // Add default guid if none present
            }

            // Hack for Dynamic Entities where the Id property doesn't populate the "entitynameid" primary key
            var primaryKeyAttribute = $"{e.LogicalName}id";
            if (!clone.Attributes.ContainsKey(primaryKeyAttribute))
            {
                clone[primaryKeyAttribute] = clone.Id;
            }

            ValidateEntity(clone);

            // Create specific validations
            if (clone.Id != Guid.Empty && Data.ContainsKey(clone.LogicalName) &&
                Data[clone.LogicalName].ContainsKey(clone.Id))
            {
                throw new InvalidOperationException($"There is already a record of entity {clone.LogicalName} with id {clone.Id}, can't create with this Id.");
            }

            // Create specific validations
            if (clone.Attributes.ContainsKey("statecode"))
            {
                throw new InvalidOperationException($"When creating an entity with logical name '{clone.LogicalName}', or any other entity, it is not possible to create records with the statecode property. Statecode must be set after creation.");
            }

            AddEntityWithDefaults(clone, false);

            if (e.RelatedEntities.Count > 0)
            {
                foreach (var relationshipSet in e.RelatedEntities)
                {
                    var relationship = relationshipSet.Key;

                    var entityReferenceCollection = new EntityReferenceCollection();

                    foreach (var relatedEntity in relationshipSet.Value.Entities)
                    {
                        var relatedId = CreateEntity(relatedEntity);
                        entityReferenceCollection.Add(new EntityReference(relatedEntity.LogicalName, relatedId));
                    }

                    var request = new AssociateRequest
                    {
                        Target = clone.ToEntityReference(),
                        Relationship = relationship,
                        RelatedEntities = entityReferenceCollection
                    };
                    _service.Execute(request);
                }
            }

            return clone.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="clone"></param>
        /// <param name="deprecatedAndToBeRemoved"></param>
        public void AddEntityWithDefaults(Entity e, bool clone = false, bool deprecatedAndToBeRemoved = false)
        {
            // Create the entity with defaults
            AddEntityDefaultAttributes(e);
            // Store
            AddEntity(clone ? e.Clone(e.GetType()) : e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        public void AddEntity(Entity e)
        {
            //Automatically detect proxy types assembly if an early bound type was used.
            if (ProxyTypesAssemblies.Count() == 0 &&
                e.GetType().IsSubclassOf(typeof(Entity)))
            {
                EnableProxyTypes(Assembly.GetAssembly(e.GetType()));
            }

            ValidateEntity(e); //Entity must have a logical name and an Id

            var integrityOptions = GetProperty<IIntegrityOptions>();

            foreach (var sAttributeName in e.Attributes.Keys.ToList())
            {
                var attribute = e[sAttributeName];
                if (attribute is DateTime)
                {
                    e[sAttributeName] = ConvertToUtc((DateTime)e[sAttributeName]);
                }
                if (attribute is EntityReference && integrityOptions.ValidateEntityReferences)
                {
                    var target = (EntityReference)e[sAttributeName];
                    e[sAttributeName] = ResolveEntityReference(target);
                }
            }

            //Add the entity collection
            if (!Data.ContainsKey(e.LogicalName))
            {
                Data.Add(e.LogicalName, new Dictionary<Guid, Entity>());
            }

            if (Data[e.LogicalName].ContainsKey(e.Id))
            {
                Data[e.LogicalName][e.Id] = e;
            }
            else
            {
                Data[e.LogicalName].Add(e.Id, e);
            }

            //Update metadata for that entity
            if (!AttributeMetadataNames.ContainsKey(e.LogicalName))
                AttributeMetadataNames.Add(e.LogicalName, new Dictionary<string, string>());

            //Update attribute metadata
            if (ProxyTypesAssemblies.Count() > 0)
            {
                //If the context is using a proxy types assembly then we can just guess the metadata from the generated attributes
                var type = FindReflectedType(e.LogicalName);
                if (type != null)
                {
                    var props = type.GetProperties();
                    foreach (var p in props)
                    {
                        if (!AttributeMetadataNames[e.LogicalName].ContainsKey(p.Name))
                            AttributeMetadataNames[e.LogicalName].Add(p.Name, p.Name);
                    }
                }
                else
                    throw new Exception(string.Format("Couldnt find reflected type for {0}", e.LogicalName));

            }
            else
            {
                //If dynamic entities are being used, then the only way of guessing if a property exists is just by checking
                //if the entity has the attribute in the dictionary
                foreach (var attKey in e.Attributes.Keys)
                {
                    if (!AttributeMetadataNames[e.LogicalName].ContainsKey(attKey))
                        AttributeMetadataNames[e.LogicalName].Add(attKey, attKey);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        protected internal DateTime ConvertToUtc(DateTime attribute)
        {
            return DateTime.SpecifyKind(attribute, DateTimeKind.Utc);
        }
        #endregion
    }
}
