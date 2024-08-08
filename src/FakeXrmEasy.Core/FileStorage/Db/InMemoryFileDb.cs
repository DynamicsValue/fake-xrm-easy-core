using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal class InMemoryFileDb : IInMemoryFileDbInternal
    {
        private readonly ConcurrentDictionary<string, FileUploadSession> _uncommittedFileUploads;
        private readonly ConcurrentDictionary<string, FileAttachment> _files;
        private readonly InMemoryDb _db;

        internal const string FILE_ATTACHMENT_TABLE_NAME = "fileattachment";
        
        internal InMemoryFileDb(InMemoryDb db)
        {
            _db = db;
            _uncommittedFileUploads = new ConcurrentDictionary<string, FileUploadSession>();
            _files = new ConcurrentDictionary<string, FileAttachment>();
        }

        public string InitFileUploadSession(FileUploadProperties fileUploadProperties)
        {
            ValidateEntityReference(fileUploadProperties);
            
            string fileContinuationToken = Guid.NewGuid().ToString();
            _uncommittedFileUploads.GetOrAdd(fileContinuationToken, new FileUploadSession()
            {
                FileUploadSessionId = fileContinuationToken,
                Properties = new FileUploadProperties(fileUploadProperties)
            });
            return fileContinuationToken;
        }

        private void ValidateEntityReference(FileUploadProperties fileUploadProperties)
        {
            if (!_db.ContainsEntityRecord(fileUploadProperties.Target.LogicalName, fileUploadProperties.Target.Id))
            {
                throw new RecordNotFoundException(fileUploadProperties.Target);
            }
        }
        
        public FileUploadSession GetFileUploadSession(string fileUploadSessionId)
        {
            FileUploadSession session = null;
            var exists = _uncommittedFileUploads.TryGetValue(fileUploadSessionId, out session);
            if (exists)
            {
                return session;
            }

            return null;
        }

        public List<FileUploadSession> GetAllFileUploadSessions()
        {
            return _uncommittedFileUploads.Values.ToList();
        }

        public List<FileAttachment> GetAllFiles()
        {
            return _files.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<FileAttachment> CreateQuery()
        {
            return _files.Values.AsQueryable();
        }

        public string CommitFileUploadSession(CommitFileUploadSessionProperties commitProperties)
        {
            FileUploadSession fileUploadSession;
            var exists =
                _uncommittedFileUploads.TryGetValue(commitProperties.FileUploadSessionId, out fileUploadSession);
            if (!exists)
            {
                throw new FileTokenContinuationNotFoundException(commitProperties.FileUploadSessionId);
            }
            
            lock(fileUploadSession._fileUploadSessionLock)
            {
                var fileAttachment = fileUploadSession.ToFileAttachment(commitProperties);
                fileAttachment.Id = Guid.NewGuid().ToString();

                var addedSuccessfully = _files.TryAdd(fileAttachment.Id, fileAttachment);
                if (!addedSuccessfully)
                {
                    throw new CouldNotCommitFileException(commitProperties.FileUploadSessionId);
                }
                
                var removedOk = _uncommittedFileUploads.TryRemove(fileUploadSession.FileUploadSessionId,
                    out var removedFileUploadSession);
                if (!removedOk)
                {
                    //could not be removed, then rollback the added file so it can be committed again
                    _files.TryRemove(fileAttachment.Id, out var removedFileAttachment);
                }

                SaveFileAttachment(fileUploadSession, fileAttachment);
                
                return fileAttachment.Id;
            }
        }

        private void SaveFileAttachment(FileUploadSession fileUploadSession, FileAttachment fileAttachment)
        {
            //Add file attachment
            if (!_db.ContainsTable(FILE_ATTACHMENT_TABLE_NAME))
            {
                InMemoryTable fileAttachmentTable;
                _db.AddTable(FILE_ATTACHMENT_TABLE_NAME, out fileAttachmentTable);
            }
                
            var filesTable = _db.GetTable(FILE_ATTACHMENT_TABLE_NAME);
            filesTable.Add(new Entity(FILE_ATTACHMENT_TABLE_NAME)
            {
                Id = new Guid(fileAttachment.Id)
            });
                
            //Update associated record
            var table = _db.GetTable(fileUploadSession.Properties.Target.LogicalName);
            var record = table.GetById(fileUploadSession.Properties.Target.Id);
            record[fileUploadSession.Properties.FileAttributeName] = fileAttachment.Id;
            record[$"{fileUploadSession.Properties.FileAttributeName}_name"] = fileAttachment.FileName;
        }
    }
}