using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception thrown when a file continuation token could not be found or invalid, either when initiating a file upload or a file download
    /// </summary>
    public class FileTokenContinuationNotFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileContinuationTokenId">The file continuation token that could not be found</param>
        internal FileTokenContinuationNotFoundException(string fileContinuationTokenId) 
            : base($"The file continuation token with Id '{fileContinuationTokenId}' could not be found or has already been committed")
        {
            
        }
    }
}