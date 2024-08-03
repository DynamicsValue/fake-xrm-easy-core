using System;
using FakeXrmEasy.Core.FileStorage.Db;
using Microsoft.Crm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db
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
            _session.AddFileBlock(new UploadBlockRequest()
            {
                BlockId = blockId,
                BlockData = new byte[] { 1, 2, 3, 4 },
                FileContinuationToken = _session.FileUploadSessionId
            });

            var fileBlock = _session.GetFileBlock(blockId);
            Assert.NotNull(fileBlock);
            Assert.Equal(new byte[] {1 , 2, 3, 4}, fileBlock.Content);
        }
    }
}