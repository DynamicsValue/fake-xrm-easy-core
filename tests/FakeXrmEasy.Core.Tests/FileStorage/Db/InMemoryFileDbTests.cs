using System;
using System.Linq;
using System.Threading.Tasks;
using DataverseEntities;
using FakeXrmEasy.Core.FileStorage;
using FakeXrmEasy.Core.FileStorage.Db;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db
{
    public class InMemoryFileDbTests
    {
        private readonly InMemoryFileDb _fileDb;
        private readonly FileUploadProperties _fileUploadProperties;
        
        public InMemoryFileDbTests()
        {
            _fileDb = new InMemoryFileDb();
            _fileUploadProperties = new FileUploadProperties()
            {
                Target = new EntityReference(dv_test.EntityLogicalName, Guid.NewGuid()),
                FileName = "FileName.pdf",
                FileAttributeName = "dv_file"
            };
        }
        
        [Fact]
        public void Should_create_an_empty_in_memory_file_db()
        {
            Assert.Empty(_fileDb.GetAllFileUploadSessions());
            Assert.Empty(_fileDb.GetAllFiles());
        }
        
        [Fact]
        public void Should_init_and_store_file_upload_session()
        {
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
        }

        [Fact]
        public void Should_init_and_upload_multiple_blocks_concurrently_and_commit_file()
        {
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
    }
}