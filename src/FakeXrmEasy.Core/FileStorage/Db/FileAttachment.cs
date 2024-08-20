using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Abstractions.FileStorage;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    /// <summary>
    /// Represents a file associated to an existing Dataverse record.
    /// </summary>
    public class FileAttachment: IFileAttachment
    {
        /// <summary>
        /// The unique identifier of the file to be uploaded(i.e. the Guid of the fileattachment record)
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The file name, including the file extension
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// The file MIME type
        /// </summary>
        public string MimeType { get; set; }
        
        /// <summary>
        /// The actual file contents as a byte array
        /// </summary>
        public byte[] Content { get; set; }
        
        /// <summary>
        /// The associated record to which this file belongs
        /// </summary>
        public EntityReference Target { get; set; }
        
        /// <summary>
        /// The logical name of the File or Image column / field of the Target EntityReference
        /// </summary>
        public string AttributeName { get; set; }
    }
}