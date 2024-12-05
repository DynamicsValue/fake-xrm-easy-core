using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when a file is uploaded and its file MIME type is blocked by either the BlockedMimeType or AllowedMimeType settings
    /// </summary>
    public class BlockedMimeTypeException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal BlockedMimeTypeException(string fileName, string mimeType) : base($"The MIME Type '{mimeType}' for file name '{fileName}' is not a valid")
        {
            
        }
    }
}