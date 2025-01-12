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
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Abstractions.Enums;

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
            if (!Db.ContainsTable(record.LogicalName) && !Db.ContainsTableMetadata(record.LogicalName))
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
                if (Db.ContainsTableMetadata(record.LogicalName))
                {
                    var entityMetadata = Db.GetTableMetadata(record.LogicalName);
                    if (entityMetadata.Keys == null && validate)
                    {
                        throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidEntityKeyOperation, $"Invalid EntityKey Operation performed : Entity {record.LogicalName} does not contain an attribute named {record.KeyAttributes.First().Key}");
                    }
                    foreach (var key in entityMetadata.Keys)
                    {
                        if (record.KeyAttributes.Keys.Count == key.KeyAttributes.Length && key.KeyAttributes.All(x => record.KeyAttributes.Keys.Contains(x)))
                        {
                            if (Db.ContainsTable(record.LogicalName))
                            {
                                var table = Db.GetTable(record.LogicalName);
                                var matchedRecord = table.GetByKeyAttributeCollection(record.KeyAttributes);
                                if (matchedRecord != null)
                                {
                                    return matchedRecord.Id;
                                }
                            }
                            if (validate)
                            {
                                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidEntityKeyOperation, $"Invalid EntityKey Operation performed : Entity {record.LogicalName} does not contain an attribute named {record.KeyAttributes.First().Key}");
                            }
                        }
                    }
                }
                if (validate)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidEntityKeyOperation, $"Invalid EntityKey Operation performed : Entity {record.LogicalName} does not contain an attribute named {record.KeyAttributes.First().Key}");
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
            var clone = e.Clone(e.GetType());
            var reference = clone.ToEntityReferenceWithKeyAttributes();
            clone.Id = GetRecordUniqueId(reference);

            // Update specific validations: The entity record must exist in the context
            if (!ContainsEntity(clone.LogicalName, clone.Id))
            {
                throw FakeOrganizationServiceFaultFactory.New($"{clone.LogicalName} with Id {clone.Id} Does Not Exist");
            }
            
            var integrityOptions = GetProperty<IIntegrityOptions>();

            var table = Db.GetTable(e.LogicalName);

            // Add as many attributes to the entity as the ones received (this will keep existing ones)
            var cachedEntity = table.GetById(clone.Id);
            foreach (var sAttributeName in clone.Attributes.Keys.ToList())
            {
                var attribute = clone[sAttributeName];
                if (attribute == null)
                {
                    cachedEntity.Attributes.Remove(sAttributeName);
                }
                else if (attribute is DateTime)
                {
                    cachedEntity[sAttributeName] = ConvertToUtc((DateTime)clone[sAttributeName]);
                }
                else
                {
                    if (attribute is EntityReference && integrityOptions.ValidateEntityReferences)
                    {
                        var target = (EntityReference) clone[sAttributeName];
                        attribute = ResolveEntityReference(target);
                    }
                    cachedEntity[sAttributeName] = attribute;
                }
            }

            // Update ModifiedOn
            cachedEntity["modifiedon"] = DateTime.UtcNow;
            cachedEntity["modifiedby"] = CallerProperties.CallerId;

            if (clone.RelatedEntities.Count > 0)
            {
                foreach (var relationshipSet in clone.RelatedEntities)
                {
                    var relationship = relationshipSet.Key;

                    var entityReferenceCollection = new EntityReferenceCollection();

                    foreach (var relatedEntity in relationshipSet.Value.Entities)
                    {
                        UpdateEntity(relatedEntity);
                        entityReferenceCollection.Add(new EntityReference(relatedEntity.LogicalName, relatedEntity.Id));
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
            
            DeleteAssociatedFilesAfterUpdate(e);
        }

        /// <summary>
        /// Deletes any associated files to an entity that has their file attributes as null
        /// </summary>
        /// <param name="e"></param>
        private void DeleteAssociatedFilesAfterUpdate(Entity e)
        {
            var associatedFiles = FileDb.GetFilesForTarget(e.ToEntityReference());
            
            foreach (var updatedAttribute in e.Attributes.Keys)
            {
                if (e[updatedAttribute] == null)
                {
                    var associatedFile = associatedFiles.FirstOrDefault(f => f.AttributeName.Equals(updatedAttribute));
                    if (associatedFile != null)
                    {
                        FileDb.DeleteFile(associatedFile.Id);
                    }
                }
            }
        }
        
        /// <summary>
        /// Deletes any associated files after a Delete message
        /// </summary>
        /// <param name="er">The entity reference that was deleted</param>
        private void DeleteAssociatedFiles(EntityReference er)
        {
            var associatedFiles = FileDb.GetFilesForTarget(er);
            
            foreach (var associatedFile in associatedFiles)
            {
                FileDb.DeleteFile(associatedFile.Id);
            }
        }
        
        /// <summary>
        /// Returns an entity record by logical name and primary key
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Entity GetEntityById(string logicalName, Guid id)
        {
            var entity = GetEntityById_Internal(logicalName, id);
            //return entity;
            return entity.Clone(null, this);
        }

        internal Entity GetEntityById_Internal(string sLogicalName, Guid id, Type t = null)
        {
            if (!Db.ContainsTable(sLogicalName))
            {
                throw new InvalidOperationException($"The entity logical name '{sLogicalName}' is not valid.");
            }

            var table = Db.GetTable(sLogicalName);

            if (!table.Contains(id))
            {
                throw new InvalidOperationException($"The id parameter '{id.ToString()}' for entity logical name '{sLogicalName}' is not valid.");
            }

            return table.GetById(id);
        }

        /// <summary>
        /// Returns true if the entity record with the specified logical name and id exists in the InMemory database
        /// </summary>
        /// <param name="sLogicalName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsEntity(string sLogicalName, Guid id)
        {
            if(!Db.ContainsTable(sLogicalName)) 
            {
                return false;
            }

            var table = Db.GetTable(sLogicalName);
            if(!table.Contains(id)) 
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a strongly-typed entity record by Id and its class name
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

            var entity = GetEntityById_Internal(logicalName, id, typeParameter);

            //return entity as T;
            return entity.Clone(typeParameter, this) as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        protected EntityReference ResolveEntityReference(EntityReference er)
        {
            if (!Db.ContainsTable(er.LogicalName) || !ContainsEntity(er.LogicalName, er.Id))
            {
                if (er.Id == Guid.Empty && er.HasKeyAttributes())
                {
                    return ResolveEntityReferenceByAlternateKeys(er);
                }
                else
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, $"{er.LogicalName} With Ids = {er.Id:D} Do Not Exist");
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
            if (!Db.ContainsTable(er.LogicalName))
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
            if (Db.ContainsTable(er.LogicalName) && ContainsEntity(er.LogicalName, er.Id))
            {
                // Entity found => remove entity
                var table = Db.GetTable(er.LogicalName);
                table.Remove(er.Id);

                DeleteAssociatedFiles(er);
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
            var integrityOptions = GetProperty<IIntegrityOptions>();

            if (integrityOptions.ValidateEntityReferences)
            {
                var caller = new Entity("systemuser") { Id = CallerProperties.CallerId.Id };

                AddEntityRecordInternal(caller);
            }

            var isManyToManyRelationshipEntity = e.LogicalName != null && this._relationships.ContainsKey(e.LogicalName);

            EntityInitializerService.Initialize(e, CallerProperties.CallerId.Id, this, isManyToManyRelationshipEntity);
        }

        /// <summary>
        /// Performs basic validation of the entity record which is common across all CRUD operations
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
            
        }

        

        /// <summary>
        /// Adds an entity record to the in-memory database with some default values for out of the box attributes
        /// </summary>
        /// <param name="e">Entity record to add</param>
        /// <param name="clone">True if it should clone the entity record before adding it</param>
        public void AddEntityWithDefaults(Entity e, bool clone = false)
        {
            // Create the entity with defaults
            AddEntityDefaultAttributes(e);
            // Store
            AddEntity(clone ? e.Clone(e.GetType()) : e);
        }

        internal void AddEntityRecordInternal(Entity e)
        {
            //Add the entity collection
            Db.AddOrReplaceEntityRecord(e);
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

            AddEntityRecordInternal(e);

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
