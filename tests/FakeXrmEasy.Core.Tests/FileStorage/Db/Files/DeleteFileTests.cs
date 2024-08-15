using System;
using DataverseEntities;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using FakeXrmEasy.Core.FileStorage.Download;
using Microsoft.Xrm.Sdk;
using Xunit;
using FileAttachment = FakeXrmEasy.Core.FileStorage.Db.FileAttachment;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db.Files
{
    public class DeleteFileTests
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity;
        private readonly FileAttachment _file;

        public DeleteFileTests()
        {
            _db = new InMemoryDb();
            _fileDb = new InMemoryFileDb(_db);
            
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
            _db.AddEntityRecord(_entity);
            _fileDb.AddFile(_file);
            
            _fileDb.DeleteFile(_file.Id);
            
            Assert.Empty(_fileDb.GetAllFiles());

            var entityAfter = _db.GetTable(_entity.LogicalName).GetById(_entity.Id);
            Assert.Null(entityAfter[_file.AttributeName]);
        }
    }
}