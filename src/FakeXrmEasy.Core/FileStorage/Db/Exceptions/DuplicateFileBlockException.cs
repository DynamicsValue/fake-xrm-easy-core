using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when a block id already exists in the current file upload session
    /// </summary>
    public class DuplicateFileBlockException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="blockId">The block Id that already existed</param>
        /// <param name="fileUploadSessionId">The Id of the FileUploadSession where that block existed</param>
        public DuplicateFileBlockException(string blockId, string fileUploadSessionId) : base($"A block was already uploaded with Id {blockId} against the current file continuation token: {fileUploadSessionId}")
        {
            
        }
    }
}