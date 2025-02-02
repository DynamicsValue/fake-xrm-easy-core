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
    public class InMemoryFileDbDownloaderTests
    {
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly FileDownloadProperties _fileDownloadProperties;
        private readonly Entity _entity;
        private readonly FileAttachment _file;
        private readonly DownloadBlockProperties _downloadBlockProperties;
        
        public InMemoryFileDbDownloaderTests()
        {
            _db = new InMemoryDb();
            _fileDb = new InMemoryFileDb(_db);
            
            _entity = new Entity(dv_test.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            
            _fileDownloadProperties = new FileDownloadProperties()
            {
                Target = _entity.ToEntityReference(),
                FileAttributeName = "dv_file"
            };

            _file = new FileAttachment()
            {
                Id = Guid.NewGuid().ToString(),
                MimeType = "application/pdf",
                FileName = "TestFile.pdf",
                Target = _entity.ToEntityReference(),
                AttributeName = "dv_file",
                Content = new byte[] { 1, 2, 3, 4 }
            };

            _downloadBlockProperties = new DownloadBlockProperties()
            {
                FileDownloadSessionId = ""
            };
        }
        
        [Fact]
        public void Should_create_an_empty_in_memory_file_db()
        {
            Assert.Empty(_fileDb.GetAllFileDownloadSessions());
            Assert.Empty(_fileDb.GetAllFiles());
        }

        [Fact]
        public void Should_throw_exception_if_record_doesnt_exists()
        {
            Assert.Throws<RecordNotFoundException>( () => _fileDb.InitFileDownloadSession(_fileDownloadProperties));
        }

        [Fact]
        public void Should_return_file_not_found_exception_if_the_record_doesnt_have_a_file()
        {
            _db.AddEntityRecord(_entity);
            Assert.Throws<FileToDownloadNotFoundException>(() => _fileDb.InitFileDownloadSession(_fileDownloadProperties));
        }
        
        [Fact]
        public void Should_return_file_not_found_exception_if_the_record_has_an_invalid_file_reference()
        {
            _entity[_fileDownloadProperties.FileAttributeName] = "invalid file id";
            
            _db.AddEntityRecord(_entity);
            Assert.Throws<FileToDownloadNotFoundException>(() => _fileDb.InitFileDownloadSession(_fileDownloadProperties));
        }
        
        [Fact]
        public void Should_return_file_download_session_if_properties_are_valid()
        {
            _fileDb.AddFile(_file);
            
            _entity[_fileDownloadProperties.FileAttributeName] = _file.Id;
            _db.AddEntityRecord(_entity);

            var fileContinuationToken = _fileDb.InitFileDownloadSession(_fileDownloadProperties);
            Assert.NotNull(fileContinuationToken);

            var fileDownloadSession = _fileDb.GetFileDownloadSession(fileContinuationToken);
            Assert.NotNull(fileDownloadSession);
            
            Assert.Equal(_file.Id, fileDownloadSession.File.Id);
            Assert.Equal(_fileDownloadProperties.FileAttributeName, fileDownloadSession.Properties.FileAttributeName);
            Assert.Equal(_fileDownloadProperties.Target.LogicalName, _entity.LogicalName);
            Assert.Equal(_fileDownloadProperties.Target.Id, _entity.Id);
        }

        [Fact]
        public void Should_throw_file_token_continuation_not_found_exception()
        {
            _downloadBlockProperties.FileDownloadSessionId = "invalid id";
            Assert.Throws<FileTokenContinuationNotFoundException>(() =>
                _fileDb.DownloadFileBlock(_downloadBlockProperties));
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-23456)]
        public void Should_throw_invalid_length_exception(long blockLength)
        {
            _fileDb.AddFile(_file);
            
            _entity[_fileDownloadProperties.FileAttributeName] = _file.Id;
            _db.AddEntityRecord(_entity);
            
            var fileContinuationToken = _fileDb.InitFileDownloadSession(_fileDownloadProperties);
            Assert.NotNull(fileContinuationToken);
            
            _downloadBlockProperties.FileDownloadSessionId = fileContinuationToken;
            _downloadBlockProperties.BlockLength = blockLength;
            Assert.Throws<InvalidBlockLengthException>(() => _fileDb.DownloadFileBlock(_downloadBlockProperties));
        }
        
        [Theory]
        [InlineData(-1, 2)]
        [InlineData(0, 5)] 
        [InlineData(2, 25)]
        [InlineData(4, 2)]
        public void Should_throw_invalid_offset_exception_if_exceeds_file_length(long offset, long blockLength)
        {
            _fileDb.AddFile(_file);
            
            _entity[_fileDownloadProperties.FileAttributeName] = _file.Id;
            _db.AddEntityRecord(_entity);
            
            var fileContinuationToken = _fileDb.InitFileDownloadSession(_fileDownloadProperties);
            Assert.NotNull(fileContinuationToken);
            
            _downloadBlockProperties.FileDownloadSessionId = fileContinuationToken;
            _downloadBlockProperties.Offset = offset;
            _downloadBlockProperties.BlockLength = blockLength;
            Assert.Throws<InvalidOffsetException>(() => _fileDb.DownloadFileBlock(_downloadBlockProperties));
        }
        
        [Theory]
        [InlineData(0, 4)]
        [InlineData(1, 3)] 
        [InlineData(3, 1)]
        public void Should_download_a_valid_file_block(long offset, long blockLength)
        {
            _fileDb.AddFile(_file);
            
            _entity[_fileDownloadProperties.FileAttributeName] = _file.Id;
            _db.AddEntityRecord(_entity);
            
            var fileContinuationToken = _fileDb.InitFileDownloadSession(_fileDownloadProperties);
            Assert.NotNull(fileContinuationToken);
            
            _downloadBlockProperties.FileDownloadSessionId = fileContinuationToken;
            _downloadBlockProperties.Offset = offset;
            _downloadBlockProperties.BlockLength = blockLength;
            var data = _fileDb.DownloadFileBlock(_downloadBlockProperties);
            
            Assert.Equal(blockLength, data.Length);
        }
    }
}