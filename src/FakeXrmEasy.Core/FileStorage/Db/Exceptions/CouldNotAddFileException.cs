using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Internal exception used for internal testing only thrown when a file could not be added to the InMemoryFileDb
    /// </summary>
    public class CouldNotAddFileException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal CouldNotAddFileException(): base("A file could not be added")
        {
            
        }
    }
}