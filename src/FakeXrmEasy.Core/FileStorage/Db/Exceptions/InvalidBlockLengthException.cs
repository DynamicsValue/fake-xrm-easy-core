using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when the BlockLength property is not valid
    /// </summary>
    public class InvalidBlockLengthException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal InvalidBlockLengthException() : base($"The BlockLength property must be greater than zero.")
        {
            
        }
    }
}