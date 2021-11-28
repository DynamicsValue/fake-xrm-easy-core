using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityInitializerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ctx"></param>
        /// <param name="isManyToManyRelationshipEntity"></param>
        /// <returns></returns>
        Entity Initialize(Entity e, XrmFakedContext ctx, bool isManyToManyRelationshipEntity = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="gCallerId"></param>
        /// <param name="ctx"></param>
        /// <param name="isManyToManyRelationshipEntity"></param>
        /// <returns></returns>
        Entity Initialize(Entity e, Guid gCallerId, XrmFakedContext ctx, bool isManyToManyRelationshipEntity = false);
    }



}
