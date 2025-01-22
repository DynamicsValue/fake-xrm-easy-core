using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using Microsoft.Xrm.Sdk;
using Xunit;
using FileAttachment = FakeXrmEasy.Core.FileStorage.Db.FileAttachment;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db.Files
{
    public class BlockedMimeTypeTests: FakeXrmEasyTestsBase
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity;
        private readonly FileAttachment _file;

        private readonly Entity _organization;

        private const string BLOCKED_MIME_TYPES_ATTRIBUTE = "blockedmimetypes";
        private const string ALLOWED_MIME_TYPES_ATTRIBUTE = "allowedmimetypes";
        
        public BlockedMimeTypeTests()
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

            _organization = new Entity(Organization.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
        }

        [Fact]
        public void Should_allow_any_mime_type_by_default()
        {
            _fileDb.AddFile(_file);

            var fileAdded = _fileDb.GetAllFiles().FirstOrDefault(f => f.Id == _file.Id);
            Assert.NotNull(fileAdded);
        }
        
        [Theory]
        [InlineData(null, "application/pdf")]
        [InlineData("", "application/pdf")]
        [InlineData("audio/aac", "application/pdf")]
        [InlineData("audio/aac;application/x-csh", "application/pdf")]
        public void Should_not_throw_blocked_mime_type_exception_when_is_not_blocked(string blockedMimeTypes, string mimeType)
        {
            _organization[BLOCKED_MIME_TYPES_ATTRIBUTE] = blockedMimeTypes;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.MimeType = mimeType;
            _fileDb.AddFile(_file);

            var fileAdded = _fileDb.GetAllFiles().FirstOrDefault(f => f.Id == _file.Id);
            Assert.NotNull(fileAdded);
        }
        
        [Theory]
        [InlineData("audio/aac", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "application/x-csh")]
        public void Should_throw_blocked_mime_type_exception_when_is_blocked(string blockedMimeTypes, string mimeType)
        {
            _organization[BLOCKED_MIME_TYPES_ATTRIBUTE] = blockedMimeTypes;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.MimeType = mimeType;
            Assert.Throws<BlockedMimeTypeException>(() => _fileDb.AddFile(_file));
        }
        
        [Theory]
        [InlineData("audio/aac", "application/pdf")]
        [InlineData("audio/aac;application/x-csh", "application/pdf")]
        public void Should_throw_blocked_mime_type_exception_when_is_not_allowed(string allowedMimeTypes, string mimeType)
        {
            _organization[ALLOWED_MIME_TYPES_ATTRIBUTE] = allowedMimeTypes;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.MimeType = mimeType;
            Assert.Throws<BlockedMimeTypeException>(() => _fileDb.AddFile(_file));
        }
        
        [Theory]
        [InlineData(null, "audio/aac")]
        [InlineData("", "audio/aac")]
        [InlineData("audio/aac", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "application/x-csh")]
        public void Should_not_throw_blocked_mime_type_exception_when_is_allowed(string allowedMimeTypes, string mimeType)
        {
            _organization[ALLOWED_MIME_TYPES_ATTRIBUTE] = allowedMimeTypes;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.MimeType = mimeType;
            _fileDb.AddFile(_file);

            var fileAdded = _fileDb.GetAllFiles().FirstOrDefault(f => f.Id == _file.Id);
            Assert.NotNull(fileAdded);
        }
        
        [Theory]
        [InlineData("audio/aac", "audio/aac", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "audio/aac", "audio/aac;application/x-csh")]
        [InlineData("audio/aac;application/x-csh", "application/x-csh", "audio/aac;application/x-csh")]
        [InlineData("audio/aac;application/x-csh", "audio/aac", "audio/aac")]
        [InlineData("audio/aac;application/x-csh", "application/x-csh", "application/x-csh")]
        public void Should_ignore_blocked_mime_types_when_allow_mime_types_are_used(string allowedMimeTypes, string mimeType, string blockedMimeTypes)
        {
            _organization[ALLOWED_MIME_TYPES_ATTRIBUTE] = allowedMimeTypes;
            _organization[BLOCKED_MIME_TYPES_ATTRIBUTE] = blockedMimeTypes;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.MimeType = mimeType;
            _fileDb.AddFile(_file);

            var fileAdded = _fileDb.GetAllFiles().FirstOrDefault(f => f.Id == _file.Id);
            Assert.NotNull(fileAdded);
        }
    }
}