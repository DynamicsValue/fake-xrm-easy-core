using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when the max file size is exceeded for a given column
    /// </summary>
    public class MaxSizeExceededException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logicalName">The table with the column that has the max file size limitation</param>
        /// <param name="attributeName">The column with the max file size limitation</param>
        public MaxSizeExceededException(string logicalName, string attributeName, int currentMaxSize) : 
            base($"Could not commit the file upload because the sum of the block sizes exceeds the current allowed max file size of {currentMaxSize.ToString()}KB for column '{attributeName}' in table '{logicalName}'")
        {
            
        }
    }
}