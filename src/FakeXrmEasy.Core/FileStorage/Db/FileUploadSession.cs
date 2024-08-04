using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

        internal void AddFileBlock(UploadBlockProperties uploadBlockProperties)
        {
            var existed = _fileBlocks.TryAdd(uploadBlockProperties.BlockId, new FileBlock()
            {
                BlockId = uploadBlockProperties.BlockId,
                Content = uploadBlockProperties.BlockContents
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