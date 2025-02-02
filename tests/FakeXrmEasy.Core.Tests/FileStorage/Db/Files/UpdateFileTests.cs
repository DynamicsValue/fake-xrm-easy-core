using System;
using DataverseEntities;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db;
using Microsoft.Xrm.Sdk;
using Xunit;
using FileAttachment = FakeXrmEasy.Core.FileStorage.Db.FileAttachment;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db.Files
{
    public class UpdateFileTests: FakeXrmEasyTestsBase
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity;
        private readonly FileAttachment _file;
        
        public UpdateFileTests()
        {
            _db = (_context as XrmFakedContext).Db;
            _fileDb = (_context as XrmFakedContext).FileDb;
            
            _entity = new Entity(dv_test.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
            };
            
            _file = new FileAttachment()
            {
                Id = Guid.NewGuid().ToString(),
                MimeType = "application/pdf",
                FileName = "TestFile.pdf",
                Target = _entity.ToEntityReference(),
                AttributeName = FILE_ATTRIBUTE_NAME,
                Content = new byte[] { 1, 2, 3, 4 }
            };

            _entity[FILE_ATTRIBUTE_NAME] = _file.Id;
        }

        [Fact]
        public void Should_remove_file_when_updating_file_attribute_to_null()
        {
            _fileDb.AddFile(_file);
            _context.Initialize(_entity);

            var entityToUpdate = new Entity(_entity.LogicalName)
            {
                Id = _entity.Id,
                [FILE_ATTRIBUTE_NAME] = null
            };
            
            _service.Update(entityToUpdate);
            
            Assert.Empty(_fileDb.GetAllFiles());
        }
        
        [Fact]
        public void Should_keep_file_when_updating_another_attribute_to_null()
        {
            _fileDb.AddFile(_file);
            _context.Initialize(_entity);

            var entityToUpdate = new Entity(_entity.LogicalName)
            {
                Id = _entity.Id,
                ["dv_name"] = null
            };
            
            _service.Update(entityToUpdate);
            
            Assert.Single(_fileDb.GetAllFiles());
        }
    }
}