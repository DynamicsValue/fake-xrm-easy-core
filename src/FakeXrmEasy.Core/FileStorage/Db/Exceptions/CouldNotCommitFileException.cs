using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Throws an exception when a file could not be committed due to an unknown error
    /// </summary>
    public class CouldNotCommitFileException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileUploadContinuationToken"></param>
        internal CouldNotCommitFileException(string fileUploadContinuationToken) : base($"The file associated to continuation token '{fileUploadContinuationToken}' could not be committed due to an unknown error")
        {
            
        }
    }
}