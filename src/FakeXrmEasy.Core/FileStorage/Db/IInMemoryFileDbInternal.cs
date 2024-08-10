using System.Collections.Generic;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal interface IInMemoryFileDbInternal
    {
        List<FileAttachment> GetAllFiles();
        void AddFile(FileAttachment fileAttachment);
        void DeleteFile(string fileId);
    }
}