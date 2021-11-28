using System;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Extensions;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Services
{
    /// <summary>
    /// 
    /// </summary>
    public enum EntityInitializationLevel
    {
        /// <summary>
        /// Minimal initialization of common attributes
        /// </summary>
        Default = 0,
        /// <summary>
        /// More detailed initialization of entities, on an entity per entity basis
        /// </summary>
        PerEntity = 1
    }

    /// <summary>
    /// 
    /// </summary>
    public class DefaultEntityInitializerService : IEntityInitializerService
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, IEntityInitializerService> InitializerServiceDictionary { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public DefaultEntityInitializerService()
        {
            InitializerServiceDictionary = new Dictionary<string, IEntityInitializerService>()
            {
                { InvoiceDetailInitializerService.EntityLogicalName, new InvoiceDetailInitializerService() },
                { InvoiceInitializerService.EntityLogicalName, new InvoiceInitializerService() }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="gCallerId"></param>
        /// <param name="ctx"></param>
        /// <param name="isManyToManyRelationshipEntity"></param>
        /// <returns></returns>
        public Entity Initialize(Entity e, Guid gCallerId, XrmFakedContext ctx, bool isManyToManyRelationshipEntity = false)
        {
            //Validate primary key for dynamic entities
            var primaryKey = string.Format("{0}id", e.LogicalName);
            if (!e.Attributes.ContainsKey(primaryKey))
            {
                e[primaryKey] = e.Id;
            }

            if (isManyToManyRelationshipEntity)
            {
                return e;
            }

            var CallerId = new EntityReference("systemuser", gCallerId); //Create a new instance by default

            var now = DateTime.UtcNow;

            e.SetValueIfEmpty("createdon", now);

            // Overridden created on should replace created on
            if (e.Contains("overriddencreatedon"))
            {
                e["createdon"] = e["overriddencreatedon"];
            }

            e.SetValueIfEmpty("modifiedon", now);
            e.SetValueIfEmpty("createdby", CallerId);
            e.SetValueIfEmpty("modifiedby", CallerId);
            e.SetValueIfEmpty("ownerid", CallerId);
            e.SetValueIfEmpty("statecode", new OptionSetValue(0)); //Active by default

            if (ctx.InitializationLevel == EntityInitializationLevel.PerEntity)
            {
                if (!string.IsNullOrEmpty(e.LogicalName) && InitializerServiceDictionary.ContainsKey(e.LogicalName))
                    InitializerServiceDictionary[e.LogicalName].Initialize(e, gCallerId, ctx, isManyToManyRelationshipEntity);
            }

            return e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ctx"></param>
        /// <param name="isManyToManyRelationshipEntity"></param>
        /// <returns></returns>
        public Entity Initialize(Entity e, XrmFakedContext ctx, bool isManyToManyRelationshipEntity = false)
        {
            return this.Initialize(e, Guid.NewGuid(), ctx, isManyToManyRelationshipEntity);
        }
    }
}