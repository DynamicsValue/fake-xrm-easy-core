using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeXrmEasy.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class InvoiceInitializerService : IEntityInitializerService
    {
        /// <summary>
        /// Entity LogicalName
        /// </summary>
        public const string EntityLogicalName = "invoice";

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
            if (string.IsNullOrEmpty(e.GetAttributeValue<string>("invoicenumber")))
            {
                //first FakeXrmEasy auto-numbering emulation
                e["invoicenumber"] = "INV-" + DateTime.Now.Ticks;
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
