using System;

namespace FakeXrmEasy.Core.Db.Exceptions
{
    /// <summary>
    /// Exception thrown when a table with the same logical name already exists in the current InMemoryDb
    /// </summary>
    public class TableAlreadyExistsException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logicalName"></param>
        public TableAlreadyExistsException(string logicalName) : base($"Table with logical name '{logicalName}' already exists")
        {

        }
    }
}
