using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Extensions;
using System.Reflection;
using FakeXrmEasy.Metadata;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Stores some minimal metadata info if dynamic entities are used and no injected metadata was used
        /// </summary>
        protected internal Dictionary<string, Dictionary<string, string>> AttributeMetadataNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadataList"></param>
        /// <exception cref="Exception"></exception>
        public void InitializeMetadata(IEnumerable<EntityMetadata> entityMetadataList)
        {
            if (entityMetadataList == null)
            {
                throw new Exception("Entity metadata parameter can not be null");
            }

            //  this.EntityMetadata = new Dictionary<string, EntityMetadata>();
            foreach (var eMetadata in entityMetadataList)
            {
                if (string.IsNullOrWhiteSpace(eMetadata.LogicalName))
                {
                    throw new Exception("An entity metadata record must have a LogicalName property.");
                }

                if (Db.ContainsTableMetadata(eMetadata.LogicalName))
                {
                    throw new Exception("An entity metadata record with the same logical name was previously added. ");
                }
                Db.AddOrUpdateMetadata(eMetadata.LogicalName, eMetadata);
            }

            MetadataInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityMetadata"></param>
        public void InitializeMetadata(EntityMetadata entityMetadata)
        {
            InitializeMetadata(new List<EntityMetadata>() { entityMetadata });
        }

        /// <summary>
        /// Initializes Metadata from an early bound assembly
        /// </summary>
        /// <param name="earlyBoundEntitiesAssembly"></param>
        public void InitializeMetadata(Assembly earlyBoundEntitiesAssembly)
        {
            IEnumerable<EntityMetadata> entityMetadatas = MetadataGenerator.FromEarlyBoundEntities(earlyBoundEntitiesAssembly, this);
            if (entityMetadatas.Any())
            {
                InitializeMetadata(entityMetadatas);
            }
            MetadataInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<EntityMetadata> CreateMetadataQuery()
        {
            return Db
                    .AllMetadata
                    .AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sLogicalName"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadataByName(string sLogicalName)
        {
            if (Db.ContainsTableMetadata(sLogicalName))
                return Db.GetTableMetadata(sLogicalName);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="em"></param>
        public void SetEntityMetadata(EntityMetadata em)
        {
            Db.AddOrUpdateMetadata(em.LogicalName, em);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sEntityName"></param>
        /// <param name="sAttributeName"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public AttributeMetadata GetAttributeMetadataFor(string sEntityName, string sAttributeName, Type attributeType)
        {
            if (Db.ContainsTableMetadata(sEntityName))
            {
                var entityMetadata = GetEntityMetadataByName(sEntityName);
                var attribute = entityMetadata.Attributes
                                .Where(a => a.LogicalName.Equals(sAttributeName))
                                .FirstOrDefault();

                if (attribute != null)
                    return attribute;
            }

            if (attributeType == typeof(string))
            {
                return new StringAttributeMetadata(sAttributeName);
            }
            //Default
            return new StringAttributeMetadata(sAttributeName);
        }

    }
}