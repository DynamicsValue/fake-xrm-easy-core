using System;
using FakeXrmEasy.Core.FileStorage.Download;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    /// <summary>
    /// Exception raised when an Offset property is not within the range of a given file length
    /// </summary>
    public class InvalidOffsetException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="actualLength"></param>
        internal InvalidOffsetException(DownloadBlockProperties properties, long actualLength) 
            : base($"The Offset '{properties.Offset.ToString()}' and block lengh '{properties.BlockLength.ToString()}' property values are not within the file's size of [0..{actualLength.ToString()}]")
        {
            
        }
    }
}