using System;

namespace FakeXrmEasy.Core.FileStorage.Db.Exceptions
{
    public class CouldNotAddFileException: Exception
    {
        public CouldNotAddFileException(): base("A file could not be added")
        {
            
        }
    }
}