using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    public class FileTokenContinuationNotFoundException: Exception
    {
        public FileTokenContinuationNotFoundException(string fileContinuationTokenId) 
            : base($"The file continuation token with Id '{fileContinuationTokenId}' could not be found or has already been committed")
        {
            
        }
    }
}