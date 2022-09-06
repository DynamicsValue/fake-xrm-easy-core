using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Core.Db
{
    /// <summary>
    /// An InMemoryTableMetadata contains info about all the metadata of a given table
    /// Column definitions, data ranges, and so on...
    /// </summary>
    internal class InMemoryTableMetadata
    {
        protected internal EntityMetadata _entityMetadata;
        protected internal int? _entityTypeCode;
    }
}
