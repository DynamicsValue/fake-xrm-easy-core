using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;
using FakeXrmEasy.Core.FileStorage.Download;
using FakeXrmEasy.Core.FileStorage.Upload;
using Microsoft.Xrm.Sdk;
#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk.Metadata;
#endif

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal class InMemoryFileDb : IInMemoryFileDbUploader, IInMemoryFileDbDownloader
    {
        private readonly ConcurrentDictionary<string, FileUploadSession> _uncommittedFileUploads;
        private readonly ConcurrentDictionary<string, FileAttachment> _files;
        private readonly ConcurrentDictionary<string, FileDownloadSession> _fileDownloadSessions;
        private readonly InMemoryDb _db;

        internal const string FILE_ATTACHMENT_TABLE_NAME = "fileattachment";
        
        internal InMemoryFileDb(InMemoryDb db)
        {
            _db = db;
            _uncommittedFileUploads = new ConcurrentDictionary<string, FileUploadSession>();
            _files = new ConcurrentDictionary<string, FileAttachment>();
            _fileDownloadSessions = new ConcurrentDictionary<string, FileDownloadSession>();
        }

        public string InitFileUploadSession(FileUploadProperties fileUploadProperties)
        {
            ValidateEntityReference(fileUploadProperties.Target);
            
            string fileContinuationToken = Guid.NewGuid().ToString();
            _uncommittedFileUploads.GetOrAdd(fileContinuationToken, new FileUploadSession()
            {
                FileUploadSessionId = fileContinuationToken,
                Properties = new FileUploadProperties(fileUploadProperties)
            });
            return fileContinuationToken;
        }

        private void ValidateEntityReference(EntityReference entityReference)
        {
            if (!_db.ContainsEntityRecord(entityReference.LogicalName, entityReference.Id))
            {
                throw new RecordNotFoundException(entityReference);
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

        #region Internal File Manipulation
        internal List<FileAttachment> GetAllFiles()
        {
            return _files.Values.ToList();
        }

        internal void AddFile(FileAttachment fileAttachment)
        {
            var wasAdded = _files.TryAdd(fileAttachment.Id, fileAttachment);
            if (!wasAdded)
            {
                throw new CouldNotAddFileException();
            }
        }
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<FileAttachment> CreateQuery()
        {
            return _files.Values.AsQueryable();
        }

        #region Upload
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

                #if FAKE_XRM_EASY_9
                ValidateMaxFileSize(fileUploadSession, fileAttachment);
                #endif
                
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

        #if FAKE_XRM_EASY_9
        private void ValidateMaxFileSize(FileUploadSession fileUploadSession, FileAttachment fileAttachment)
        {
            var tableLogicalName = fileUploadSession.Properties.Target.LogicalName;
            var table = _db.GetTable(tableLogicalName);
            var entityMetadata = table.GetEntityMetadata();
            if (entityMetadata == null)
            {
                return;
            }

            var attributeMetadata =
                entityMetadata.Attributes.FirstOrDefault(a =>
                    a.LogicalName == fileUploadSession.Properties.FileAttributeName);

            if (attributeMetadata == null)
            {
                return;
            }

            var fileAttributeMetadata = attributeMetadata as FileAttributeMetadata;

            if ((decimal) fileAttachment.Content.Length / 1024 > fileAttributeMetadata.MaxSizeInKB)
            {
                throw new MaxSizeExceededException(tableLogicalName, fileUploadSession.Properties.FileAttributeName,
                    fileAttributeMetadata.MaxSizeInKB.Value);
            }
        }
        #endif
        
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

        #endregion
        
        #region Download
        public string InitFileDownloadSession(FileDownloadProperties fileDownloadProperties)
        {
            ValidateEntityReference(fileDownloadProperties.Target);
            var file = CheckFileExists(fileDownloadProperties);
            
            string fileContinuationToken = Guid.NewGuid().ToString();
            _fileDownloadSessions.GetOrAdd(fileContinuationToken, new FileDownloadSession()
            {
                FileDownloadSessionId = fileContinuationToken,
                Properties = new FileDownloadProperties(fileDownloadProperties),
                File = file
            });
            return fileContinuationToken;
        }

        private FileAttachment CheckFileExists(FileDownloadProperties fileDownloadProperties)
        {
            var entityReference = fileDownloadProperties.Target;
            var record = _db.GetTable(entityReference.LogicalName).GetById(entityReference.Id);
            if (!record.Attributes.ContainsKey(fileDownloadProperties.FileAttributeName))
            {
                throw new FileToDownloadNotFoundException(entityReference, fileDownloadProperties.FileAttributeName);
            }

            var fileId = (string)record[fileDownloadProperties.FileAttributeName];

            var exists = _files.TryGetValue(fileId, out var file);
            if (!exists)
            {
                throw new FileToDownloadNotFoundException(entityReference, fileDownloadProperties.FileAttributeName);
            }
            return file;
        }
        
        public FileDownloadSession GetFileDownloadSession(string fileDownloadSessionId)
        {
            FileDownloadSession session = null;
            var exists = _fileDownloadSessions.TryGetValue(fileDownloadSessionId, out session);
            if (exists)
            {
                return session;
            }

            return null;
        }

        public List<FileDownloadSession> GetAllFileDownloadSessions()
        {
            return _fileDownloadSessions.Values.ToList();
        }

        public byte[] DownloadFileBlock(DownloadBlockProperties uploadBlockProperties)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}