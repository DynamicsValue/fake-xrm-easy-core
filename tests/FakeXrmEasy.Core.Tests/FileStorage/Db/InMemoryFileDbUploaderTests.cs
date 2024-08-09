using System;
using System.Linq;
using System.Threading.Tasks;
using DataverseEntities;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using FakeXrmEasy.Core.FileStorage.Upload;
using Microsoft.Xrm.Sdk;
using Xunit;

#if FAKE_XRM_EASY_9
using System.Collections.Generic;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
#endif

namespace FakeXrmEasy.Core.Tests.FileStorage.Db
{
    public class InMemoryFileDbUploaderTests
    {
        private readonly InMemoryDb _db;
        private readonly InMemoryFileDb _fileDb;
        private readonly FileUploadProperties _fileUploadProperties;
        private readonly Entity _entity;
        public InMemoryFileDbUploaderTests()
        {
            _db = new InMemoryDb();
            _fileDb = new InMemoryFileDb(_db);

            SetMetadata();
            
            _entity = new Entity(dv_test.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            
            _fileUploadProperties = new FileUploadProperties()
            {
                Target = _entity.ToEntityReference(),
                FileName = "FileName.pdf",
                FileAttributeName = "dv_file"
            };
        }
        
        private void SetMetadata()
        {
            _db.AddTable(dv_test.EntityLogicalName, out var table);

            #if FAKE_XRM_EASY_9
            
            var fileAttributeMetadata = new FileAttributeMetadata()
            {
                LogicalName = "dv_file"
            };
            fileAttributeMetadata.MaxSizeInKB = 1; //1 KB
            var tableEntityMetadata = new EntityMetadata()
            {
                LogicalName = dv_test.EntityLogicalName
            };
            tableEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                fileAttributeMetadata  
            });
            table.SetMetadata(tableEntityMetadata);
            
            #endif
            
        }
        
        [Fact]
        public void Should_create_an_empty_in_memory_file_db()
        {
            Assert.Empty(_fileDb.GetAllFileUploadSessions());
            Assert.Empty(_fileDb.GetAllFiles());
        }
        
        [Fact]
        public void Should_throw_exception_if_a_file_upload_session_is_initiated_for_a_non_existing_record()
        {
            Assert.Throws<RecordNotFoundException>( () => _fileDb.InitFileUploadSession(_fileUploadProperties));
        }
        
        [Fact]
        public void Should_init_and_store_file_upload_session()
        {
            _db.AddEntityRecord(_entity);
            var fileContinuationToken = _fileDb.InitFileUploadSession(_fileUploadProperties);

            var fileUploadSession = _fileDb.GetFileUploadSession(fileContinuationToken);
            
            Assert.NotNull(fileUploadSession);

            Assert.Equal(fileContinuationToken, fileUploadSession.FileUploadSessionId);
            Assert.Equal(_fileUploadProperties.Target.LogicalName, fileUploadSession.Properties.Target.LogicalName);
            Assert.Equal(_fileUploadProperties.Target.Id, fileUploadSession.Properties.Target.Id);
            Assert.Equal(_fileUploadProperties.FileName, fileUploadSession.Properties.FileName);
            Assert.Equal(_fileUploadProperties.FileAttributeName, fileUploadSession.Properties.FileAttributeName);
        }

        [Fact]
        public void Should_init_and_commit_file()
        {
            _db.AddEntityRecord(_entity);
            var fileContinuationToken = _fileDb.InitFileUploadSession(_fileUploadProperties);
            var fileUploadSession = _fileDb.GetFileUploadSession(fileContinuationToken);

            var fileBlockProperties = new UploadBlockProperties()
            {
                BlockId = Guid.NewGuid().ToString(),
                BlockContents = new byte[] { 1, 2, 3, 4 }
            };
            
            fileUploadSession.AddFileBlock(fileBlockProperties);

            var commitProperties = new CommitFileUploadSessionProperties()
            {
                FileUploadSessionId = fileContinuationToken,
                FileName = "Output.pdf",
                MimeType = "application/pdf",
                BlockIdsListSequence = new[] { fileBlockProperties.BlockId }
            };

            _fileDb.CommitFileUploadSession(commitProperties);

            var allFiles = _fileDb.GetAllFiles();
            
            Assert.Single(allFiles);

            var createdFile = allFiles.FirstOrDefault();
            
            Assert.Equal(commitProperties.FileName, createdFile.FileName);
            Assert.Equal(commitProperties.MimeType, createdFile.MimeType);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, createdFile.Content);
            
            //Uncommited session is removed
            Assert.Null(_fileDb.GetFileUploadSession(fileContinuationToken));
            
            //File attribute is updated with file id and file name
            var entityAfter = _db.GetTable(_entity.LogicalName).GetById(_entity.Id);
            Assert.Equal(createdFile.Id, entityAfter[_fileUploadProperties.FileAttributeName]);
            Assert.Equal(createdFile.FileName, entityAfter[$"{_fileUploadProperties.FileAttributeName}_name"]);
            
            //New file attachment record is created
            var fileAttachment = _db
                .GetTable(InMemoryFileDb.FILE_ATTACHMENT_TABLE_NAME)
                .GetById(new Guid(createdFile.Id));
            
            Assert.NotNull(fileAttachment);
        }
        
        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_throw_exception_if_file_exceeds_size()
        {
            _db.AddEntityRecord(_entity);
            var fileContinuationToken = _fileDb.InitFileUploadSession(_fileUploadProperties);
            var fileUploadSession = _fileDb.GetFileUploadSession(fileContinuationToken);

            var blob = new byte[1025]; //edge case where it exceeds 1 byte
            for (var i = 0; i < 1025; i++)
            {
                blob[i] = (byte)(i % 254);
            }
                
            var fileBlockProperties = new UploadBlockProperties()
            {
                BlockId = Guid.NewGuid().ToString(),
                BlockContents = blob
            };
            
            fileUploadSession.AddFileBlock(fileBlockProperties);

            var commitProperties = new CommitFileUploadSessionProperties()
            {
                FileUploadSessionId = fileContinuationToken,
                FileName = "Output.pdf",
                MimeType = "application/pdf",
                BlockIdsListSequence = new[] { fileBlockProperties.BlockId }
            };

            Assert.Throws<MaxSizeExceededException>(() => _fileDb.CommitFileUploadSession(commitProperties));
        }
        #endif
        
        [Fact]
        public void Should_init_and_upload_multiple_blocks_concurrently_and_commit_file()
        {
            _db.AddEntityRecord(_entity);
            var fileContinuationToken = _fileDb.InitFileUploadSession(_fileUploadProperties);
            var fileUploadSession = _fileDb.GetFileUploadSession(fileContinuationToken);

            var blockIds = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            Parallel.ForEach(blockIds, (blockId) =>
            {
                fileUploadSession.AddFileBlock(new UploadBlockProperties()
                {
                    BlockId = blockId.ToString(),
                    BlockContents = new byte[] { 
                        Convert.ToByte(10 + blockId), 
                        Convert.ToByte(20 + blockId), 
                        Convert.ToByte(30 + blockId),
                        Convert.ToByte(40 + blockId)
                    }
                });
            });
            
            var commitProperties = new CommitFileUploadSessionProperties()
            {
                FileUploadSessionId = fileContinuationToken,
                FileName = "Output.pdf",
                MimeType = "application/pdf",
                BlockIdsListSequence = blockIds.Select(id => id.ToString()).ToArray()
            };

            _fileDb.CommitFileUploadSession(commitProperties);

            var allFiles = _fileDb.GetAllFiles();
            
            Assert.Single(allFiles);

            var createdFile = allFiles.FirstOrDefault();
            
            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(Convert.ToByte(10 + i + 1), createdFile.Content[i * 4]);
                Assert.Equal(Convert.ToByte(20 + i + 1), createdFile.Content[i * 4 + 1]);
                Assert.Equal(Convert.ToByte(30 + i + 1), createdFile.Content[i * 4 + 2]);
                Assert.Equal(Convert.ToByte(40 + i + 1), createdFile.Content[i * 4 + 3]);
            }
        }

        [Fact]
        public void Should_throw_exception_if_file_upload_session_does_not_exist_when_commiting_a_file_upload_session()
        {
            _db.AddEntityRecord(_entity);
            var commitProperties = new CommitFileUploadSessionProperties()
            {
                FileUploadSessionId = "asdasdasd",
                FileName = "Output.pdf",
                MimeType = "application/pdf",
                BlockIdsListSequence = new string[] {}
            };
            Assert.Throws<FileTokenContinuationNotFoundException>(() =>
                _fileDb.CommitFileUploadSession(commitProperties));
        }
    }
}