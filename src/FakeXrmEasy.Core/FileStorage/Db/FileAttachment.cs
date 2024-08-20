using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Abstractions.FileStorage;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    /// <summary>
    /// Contains information about the file contents and file metadata associated to an existing record (Target) and field (AttributeName)
    /// </summary>
    public class FileAttachment: IFileAttachment
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
        public EntityReference Target { get; set; }
        public string AttributeName { get; set; }
    }
}