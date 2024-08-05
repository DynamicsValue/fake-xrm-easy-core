using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    /// <summary>
    /// Contains information about the file contents and file metadata associated to an existing record (Target) and field (AttributeName)
    /// </summary>
    internal class FileAttachment
    {
        internal string Id { get; set; }
        internal string FileName { get; set; }
        internal string MimeType { get; set; }
        internal byte[] Content { get; set; }
        internal EntityReference Target { get; set; }
        internal string AttributeName { get; set; }
    }
}