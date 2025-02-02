using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception thrown when a particular file could not be deleted or it doesn't exists
    /// </summary>
    public class CouldNotDeleteFileException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileId"></param>
        internal CouldNotDeleteFileException(string fileId) : base($"Could not delete file with Id '{fileId}'")
        {
            
        }
    }
}