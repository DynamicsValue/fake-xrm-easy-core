using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    /// <summary>
    /// Contains information about a current file upload session and their associated file blocks
    /// </summary>
    internal class FileUploadSession
    {
        private readonly ConcurrentDictionary<string, FileBlock> _fileBlocks;
        
        internal string FileUploadSessionId { get; set; }
        internal FileUploadProperties Properties { get; set; }

        internal FileUploadSession()
        {
            _fileBlocks = new ConcurrentDictionary<string, FileBlock>();
        }

        /// <summary>
        /// Adds a new FileBlock to the current FileUploadSession
        /// </summary>
        /// <param name="uploadBlockProperties"></param>
        /// <exception cref="DuplicateFileBlockException"></exception>
        internal void AddFileBlock(UploadBlockProperties uploadBlockProperties)
        {
            var addedSuccessfully = _fileBlocks.TryAdd(uploadBlockProperties.BlockId, new FileBlock()
            {
                BlockId = uploadBlockProperties.BlockId,
                Content = uploadBlockProperties.BlockContents
            });

            if (!addedSuccessfully)
            {
                throw new DuplicateFileBlockException(uploadBlockProperties.BlockId, FileUploadSessionId);
            }
        }

        /// <summary>
        /// Gets a specific FileBlock with the given Id
        /// </summary>
        /// <param name="blockId">The Id of the Block to retrieve</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets all the file blocks of the current FileUploadSession
        /// </summary>
        /// <returns></returns>
        internal List<FileBlock> GetAllBlocks()
        {
            return _fileBlocks.Values.ToList();
        }

        /// <summary>
        /// Given a CommitFileUploadSessionProperties, converts the current file upload session and their associated FileBlocks using the sequence provided
        /// in the properties into a FileAttachment object
        /// object, validating the the sequence of the 
        /// </summary>
        /// <param name="commitProperties">The commit file properties with the file blocks sequence</param>
        /// <returns></returns>
        internal FileAttachment ToFileAttachment(CommitFileUploadSessionProperties commitProperties)
        {
            List<byte> byteContents = new List<byte>();
            FileBlock fileBlock;
            foreach (var blockId in commitProperties.BlockIdsListSequence)
            {
                var exists = _fileBlocks.TryGetValue(blockId, out fileBlock);
                if (!exists)
                {
                    //Throw ex
                }
                byteContents.AddRange(fileBlock.Content);
            }

            return new FileAttachment()
            {
                Content = byteContents.ToArray(),
                MimeType = commitProperties.MimeType,
                FileName = commitProperties.FileName
            };
        }
    }
}