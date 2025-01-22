using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception thrown when InitializeFiles is called without calling Initialize first, as at least one entity record is needed for any file upload
    /// </summary>
    public class InitializeNotCalledException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InitializeNotCalledException() : base("Initialize method must be called before calling InitializeFiles.")
        {
            
        }
    }
}