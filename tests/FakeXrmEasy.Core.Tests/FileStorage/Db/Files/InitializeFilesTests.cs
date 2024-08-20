using System;
using System.Collections.Generic;
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
    public class InitializeFilesTests: FakeXrmEasyTestsBase
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity;
        private readonly FileAttachment _file;

        private readonly EntityMetadata _entityMetadata;
        
        public InitializeFilesTests()
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
        public void Should_throw_exception_if_initialize_metadata_is_not_called_before_initialize_files()
        {
            Assert.Throws<InitializeMetadataNotCalledException>(() => _context.InitializeFiles(new [] { _file }));
        }
        
        [Fact]
        public void Should_throw_exception_if_initialize_is_not_called_before_initialize_files()
        {
            _context.InitializeMetadata(_entityMetadata);
            Assert.Throws<InitializeNotCalledException>(() => _context.InitializeFiles(new [] { _file }));
        }
        
        [Fact]
        public void Should_init_file()
        {
            _context.InitializeMetadata(_entityMetadata);
            _context.Initialize(_entity);
            _context.InitializeFiles(new [] { _file });

            var allFiles = _fileDb.GetAllFiles();
            Assert.Single(allFiles);
        }
    }
}