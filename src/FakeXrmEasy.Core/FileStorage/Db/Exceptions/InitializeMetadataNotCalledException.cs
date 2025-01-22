using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception thrown when InitializeFiles is called without calling InitializeMetadata first, as entity metadata information is needed for file storage
    /// </summary>
    public class InitializeMetadataNotCalledException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InitializeMetadataNotCalledException() : base("InitializeMetadata has not been called before calling InitializeFiles.")
        {
            
        }
    }
}