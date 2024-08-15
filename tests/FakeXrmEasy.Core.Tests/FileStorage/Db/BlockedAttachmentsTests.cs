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

namespace FakeXrmEasy.Core.Tests.FileStorage.Db
{
    public class BlockedAttachmentsTests: FakeXrmEasyTestsBase
    {
        private const string FILE_ATTRIBUTE_NAME = "dv_file";
        
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly Entity _entity;
        private readonly FileAttachment _file;

        private readonly Entity _organization;

        private const string BLOCKED_ATTACHMENTS_ATTRIBUTE = "blockedattachments";
        
        public BlockedAttachmentsTests()
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
        public void Should_throw_blocked_attachment_exception_if_a_file_extension_is_not_allowed()
        {
            _file.FileName = "TestFile.ade";
            Assert.Throws<BlockedAttachmentException>(() => _fileDb.AddFile(_file));
        }
        
        [Theory]
        [InlineData(null, "File.ade")]
        [InlineData("asmx", "File.ade")]
        [InlineData("asmx;php", "File.ade")]
        [InlineData("asmx;php", "File.pdf")]
        public void Should_not_throw_blocked_attachment_exception_with_a_custom_blocked_attachments_file_extension(string blockedAttachments, string fileName)
        {
            _organization[BLOCKED_ATTACHMENTS_ATTRIBUTE] = blockedAttachments;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.FileName = fileName;
            _fileDb.AddFile(_file);

            var fileAdded = _fileDb.GetAllFiles().FirstOrDefault(f => f.Id == _file.Id);
            Assert.NotNull(fileAdded);
        }
        
        [Theory]
        [InlineData("asmx", "File.asmx")]
        [InlineData("asmx;php", "File.asmx")]
        [InlineData("asmx;php", "File.php")]
        public void Should_throw_blocked_attachment_exception_with_a_custom_blocked_attachments_file_extension(string blockedAttachments, string fileName)
        {
            _organization[BLOCKED_ATTACHMENTS_ATTRIBUTE] = blockedAttachments;
            
            _context.Initialize(new List<Entity>()
            {
                _organization, _entity
            });
            
            _file.FileName = fileName;
            Assert.Throws<BlockedAttachmentException>(() => _fileDb.AddFile(_file));
        }
    }
}