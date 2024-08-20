using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when InitializeFiles is called more than once.
    /// </summary>
    public class AlreadyInitializedFilesException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AlreadyInitializedFilesException() : base("InitializeFiles has been called more than once. Always initialize all the files you need in a single call to .InitializeFiles()")
        {
            
        }
        
    }
}