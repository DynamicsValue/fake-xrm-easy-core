using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when a file is uploaded and its file extension is blocked by the BlockedAttachment settings
    /// </summary>
    public class BlockedAttachmentException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal BlockedAttachmentException(string fileName) : base($"The attachment with file name '{fileName}' is not a valid file extension type")
        {
            
        }
    }
}