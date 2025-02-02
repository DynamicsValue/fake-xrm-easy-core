#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;
using FileAttachment = FakeXrmEasy.Core.FileStorage.Db.FileAttachment;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db.Files
{
    public class DeleteFileTests: FakeXrmEasyTestsBase
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity, _entity2;
        private readonly FileAttachment _file1, _file2;

        private readonly EntityMetadata _entityMetadata;
        
        public DeleteFileTests()
        {
            _db = (_context as XrmFakedContext).Db;
            _fileDb = (_context as XrmFakedContext).FileDb;
            
            _entity = new Entity(dv_test.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
            };
            
            _entity2 = new Entity(dv_test.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
            };
            
            _file1 = new FileAttachment()
            {
                Id = Guid.NewGuid().ToString(),
                MimeType = "application/pdf",
                FileName = "TestFile.pdf",
                Target = _entity.ToEntityReference(),
                AttributeName = FILE_ATTRIBUTE_NAME,
                Content = new byte[] { 1, 2, 3, 4 }
            };
            
            _file2 = new FileAttachment()
            {
                Id = Guid.NewGuid().ToString(),
                MimeType = "application/pdf",
                FileName = "TestFile2.pdf",
                Target = _entity2.ToEntityReference(),
                AttributeName = FILE_ATTRIBUTE_NAME,
                Content = new byte[] { 1, 2, 3, 4 }
            };

            _entity[FILE_ATTRIBUTE_NAME] = _file1.Id;
            _entity2[FILE_ATTRIBUTE_NAME] = _file2.Id;
            
            var fileAttributeMetadata = new FileAttributeMetadata()
            {
                LogicalName = "dv_file"
            };
            fileAttributeMetadata.MaxSizeInKB = 1; //1 KB
            
            _entityMetadata = new EntityMetadata()
            {
                LogicalName = dv_test.EntityLogicalName
            };
            _entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                fileAttributeMetadata  
            });
        }

        [Fact]
        public void Should_be_empty_when_no_files_are_added()
        {
            Assert.Empty(_fileDb.GetAllFiles());
        }
        
        [Fact]
        public void Should_throw_exception_when_a_file_doesnt_exists()
        {
            Assert.Throws<CouldNotDeleteFileException>(() => _fileDb.DeleteFile("invalid id"));
        }
        
        [Fact]
        public void Should_delete_an_existing_file()
        {
            _context.InitializeMetadata(_entityMetadata);
            _context.Initialize(_entity);
            _context.InitializeFiles(new [] {_file1 });
            
            _fileDb.DeleteFile(_file1.Id);
            
            Assert.Empty(_fileDb.GetAllFiles());

            var entityAfter = _db.GetTable(_entity.LogicalName).GetById(_entity.Id);
            Assert.Null(entityAfter[_file1.AttributeName]);
        }
        
        
        [Fact]
        public void Should_delete_file_when_the_associated_record_is_deleted()
        {
            _context.InitializeMetadata(_entityMetadata);
            _context.Initialize(_entity);
            _context.InitializeFiles(new [] {_file1 });
            
            _service.Delete(_entity.LogicalName, _entity.Id);
            
            Assert.Empty(_fileDb.GetAllFiles());
        }
        
        [Fact]
        public void Should_not_delete_file_that_is_not_associated_with_the_deleted_record()
        {
            _context.InitializeMetadata(_entityMetadata);
            _context.Initialize(new [] {_entity, _entity2 });
            _context.InitializeFiles(new [] { _file1, _file2 });
            
            _service.Delete(_entity.LogicalName, _entity.Id);
            
            Assert.Single(_fileDb.GetAllFiles());

            var existingFile = _fileDb.GetAllFiles().FirstOrDefault();
            Assert.Equal(_file2.Id, existingFile.Id);
        }
        
    }
}
#endif