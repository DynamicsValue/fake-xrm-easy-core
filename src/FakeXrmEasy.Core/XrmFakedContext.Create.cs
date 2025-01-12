using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Core.Extensions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        private void ValidateEntityForCreate(Entity e, bool isUpsert)
        {
            ValidateEntity(e);
            ValidateAlternateKeysForCreate(e, isUpsert);
        }

        private void ValidateAlternateKeysForCreate(Entity e, bool isUpsert)
        {
            //1 check if entity metadata has any keys
            //2 for each key, check if there are matching attribute values
            //3 throw exception if there is any match
            if (!Db.ContainsTableMetadata(e.LogicalName) || !Db.ContainsTable(e.LogicalName))
            {
                return;
            }
            
            var table = Db.GetTable(e.LogicalName);
            
            
            //Check the keyCollection present in the actual entity record key attributes (not necessarily the ones in metadata)
            if (!isUpsert && e.KeyAttributes.Count > 0)
            {
                var matchedRecord = table.GetByKeyAttributeCollection(e.KeyAttributes);
                if (matchedRecord != null)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.DuplicateRecord, 
                        $"Cannot insert duplicate key");
                }
                else
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.RecordNotFoundByEntityKey, 
                        $"A record with the specified key values does not exist in {e.LogicalName} entity");
                
                }
            }
            
            EntityKeyMetadata matchedKeyMetadata;
            var existingRecord = table.GetByAlternateKeys(e, out matchedKeyMetadata);
            if (existingRecord != null)
            {
                var keyLabel = matchedKeyMetadata.GetDisplayName();
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.DuplicateRecordEntityKey, 
                    $"A record that has the attribute values {keyLabel} already exists. The entity key {keyLabel} Key requires that this set of attributes contains unique values. Select unique values and try again.");
            }
        }
        
        /// <summary>
        /// Creates a new entity record
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Guid CreateEntity(Entity e, bool isUpsert = false)
        {
            ValidateEntityForCreate(e, isUpsert);
            
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

            // Create specific validations
            if (clone.Id != Guid.Empty && ContainsEntity(clone.LogicalName, clone.Id))
            {
                throw new InvalidOperationException($"There is already a record of entity {clone.LogicalName} with id {clone.Id}, can't create with this Id.");
            }

            if(clone.Attributes.ContainsKey("statecode"))
            {
                clone["statecode"] = new OptionSetValue(0);  //Always active by default regardless of value on Create
            }

            AddEntityWithDefaults(clone, false);

            if (clone.RelatedEntities.Count > 0)
            {
                foreach (var relationshipSet in clone.RelatedEntities)
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
    }
}