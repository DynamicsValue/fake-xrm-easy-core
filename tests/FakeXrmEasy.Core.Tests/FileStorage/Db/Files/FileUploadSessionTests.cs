using System;
using FakeXrmEasy.Core.FileStorage.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using FakeXrmEasy.Core.FileStorage.Upload;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db.Files
{
    public class FileUploadSessionTests
    {
        private readonly FileUploadSession _session;

        public FileUploadSessionTests()
        {
            _session = new FileUploadSession() { FileUploadSessionId = new Guid().ToString() };
        }

        [Fact]
        public void Should_be_empty_when_created()
        {
            Assert.Empty(_session.GetAllBlocks());
        }

        [Fact]
        public void Should_add_new_block_to_existing_session()
        {
            var blockId = Guid.NewGuid().ToString();
            _session.AddFileBlock(new UploadBlockProperties()
            {
                BlockId = blockId,
                BlockContents = new byte[] { 1, 2, 3, 4 },
                FileContinuationToken = _session.FileUploadSessionId
            });

            var fileBlock = _session.GetFileBlock(blockId);
            Assert.NotNull(fileBlock);
            Assert.Equal(new byte[] {1 , 2, 3, 4}, fileBlock.Content);
        }

        [Fact]
        public void Should_throw_exception_if_a_block_already_exists()
        {
            var blockId = Guid.NewGuid().ToString();
            var uploadBlockProperties = new UploadBlockProperties()
            {
                BlockId = blockId,
                BlockContents = new byte[] { 1, 2, 3, 4 },
                FileContinuationToken = _session.FileUploadSessionId
            };
            
            //First attempt ok
            _session.AddFileBlock(uploadBlockProperties);

            //Second fails
            Assert.Throws<DuplicateFileBlockException>(() => _session.AddFileBlock(uploadBlockProperties));
        }

        [Fact]
        public void Should_convert_file_upload_session_to_file_attachment()
        {
            //Add two blocks
            var blockId1 = Guid.NewGuid().ToString();
            var uploadBlockProperties = new UploadBlockProperties()
            {
                BlockId = blockId1,
                BlockContents = new byte[] { 1, 2, 3, 4 },
                FileContinuationToken = _session.FileUploadSessionId
            };
            _session.AddFileBlock(uploadBlockProperties);
            
            var blockId2 = Guid.NewGuid().ToString();
            uploadBlockProperties = new UploadBlockProperties()
            {
                BlockId = blockId2,
                BlockContents = new byte[] { 5, 6, 7, 8 },
                FileContinuationToken = _session.FileUploadSessionId
            };
            _session.AddFileBlock(uploadBlockProperties);

            var commitProperties = new CommitFileUploadSessionProperties()
            {
                FileUploadSessionId = _session.FileUploadSessionId,
                FileName = "Output.pdf",
                MimeType = "application/pdf",
                BlockIdsListSequence = new[] { blockId2, blockId1 } //2 comes first
            };

            var fileAttachment = _session.ToFileAttachment(commitProperties);
            Assert.NotNull(fileAttachment);
            Assert.Equal(new byte[] { 5, 6, 7, 8, 1, 2, 3, 4 }, fileAttachment.Content);
            Assert.Equal(commitProperties.FileName, fileAttachment.FileName);
            Assert.Equal(commitProperties.MimeType, fileAttachment.MimeType);
        }
    }
}