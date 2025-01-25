using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions.FileStorage;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal interface IInMemoryFileDbInternal
    {
        IFileAttachment GetFileById(string fileId);
        List<IFileAttachment> GetAllFiles();
        IQueryable<IFileAttachment> CreateQuery();
        
        void AddFile(IFileAttachment fileAttachment);
        void DeleteFile(string fileId);
        List<IFileAttachment> GetFilesForTarget(EntityReference target);

        OrganizationFileSettings GetOrganizationFileSettings();

        bool IsFileExtensionBlocked(string fileName, OrganizationFileSettings fileSettings);
        bool IsMimeTypeAllowed(string mimeType, OrganizationFileSettings fileSettings);
    }
}