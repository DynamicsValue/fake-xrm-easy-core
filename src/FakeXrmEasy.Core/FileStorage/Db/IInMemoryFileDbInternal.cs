using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal interface IInMemoryFileDbInternal
    {
        List<FileAttachment> GetAllFiles();
        void AddFile(FileAttachment fileAttachment);
        void DeleteFile(string fileId);
        List<FileAttachment> GetFilesForTarget(EntityReference target);
    }
}