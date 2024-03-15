using System.Linq;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    internal class BulkOperationsCommon
    {
        internal static void ValidateEntityName(string collectionEntityName, IXrmFakedContext ctx)
        {
            if (ctx.ProxyTypesAssemblies.Any())
            {
                var earlyBoundType = ctx.FindReflectedType(collectionEntityName);
                if (earlyBoundType == null)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoEntity,
                        $"The entity with a name = '{collectionEntityName}' with namemapping = 'Logical' was not found in the MetadataCache.");
                }
            }

            if (ctx.CreateMetadataQuery().Any())
            {
                var entityMetadata = ctx.CreateMetadataQuery().FirstOrDefault(m => m.LogicalName == collectionEntityName);
                if (entityMetadata == null)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoEntity,
                        $"The entity with a name = '{collectionEntityName}' with namemapping = 'Logical' was not found in the MetadataCache.");
                }
            }
        }
    }
}