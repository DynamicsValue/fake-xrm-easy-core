using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    /// <summary>
    /// Contains information about a current file upload session and their blocks
    /// </summary>
    internal class FileUploadSession
    {
        private ConcurrentDictionary<string, FileBlock> _fileBlocks;
        
        internal string FileUploadSessionId { get; set; }
        internal FileUploadProperties Properties { get; set; }

        internal FileUploadSession()
        {
            _fileBlocks = new ConcurrentDictionary<string, FileBlock>();
        }

        internal void AddFileBlock(UploadBlockRequest uploadBlockRequest)
        {
            var existed = _fileBlocks.TryAdd(uploadBlockRequest.BlockId, new FileBlock()
            {
                BlockId = uploadBlockRequest.BlockId,
                Content = uploadBlockRequest.BlockData
            });
        }

        internal FileBlock GetFileBlock(string blockId)
        {
            FileBlock fileBlock;
            var exists = _fileBlocks.TryGetValue(blockId, out fileBlock);
            if (exists)
            {
                return fileBlock;
            }

            return null;
        }

        internal List<FileBlock> GetAllBlocks()
        {
            return _fileBlocks.Values.ToList();
        }
    }
}