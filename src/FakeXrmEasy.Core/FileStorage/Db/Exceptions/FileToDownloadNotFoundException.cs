using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when downloading a file block for a database record and column that doesn't actually have a file against it
    /// </summary>
    public class FileToDownloadNotFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="entityReference">An entity reference of the record</param>
        /// <param name="fileAttributeName">The column where a file was not found</param>
        internal FileToDownloadNotFoundException(EntityReference entityReference, string fileAttributeName) 
            : base($"A file was not found for record with logical name '{entityReference.LogicalName}' and Id '{entityReference.Id.ToString()}' in column '{fileAttributeName}'")
        {
            
        }
    }
}