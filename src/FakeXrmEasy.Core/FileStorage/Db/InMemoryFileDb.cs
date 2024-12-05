using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeXrmEasy.Abstractions.FileStorage;
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
    internal class InMemoryFileDb : IInMemoryFileDbUploader, IInMemoryFileDbDownloader, IInMemoryFileDbInternal
    {
        private readonly ConcurrentDictionary<string, FileUploadSession> _uncommittedFileUploads;
        private readonly ConcurrentDictionary<string, IFileAttachment> _files;
        private readonly ConcurrentDictionary<string, FileDownloadSession> _fileDownloadSessions;
        private readonly InMemoryDb _db;

        internal const string FILE_ATTACHMENT_TABLE_NAME = "fileattachment";
        private const string ORGANIZATION_TABLE_NAME = "organization";
        
        private const string BLOCKED_ATTACHMENTS_FIELD_NAME = "blockedattachments";
        private const string BLOCKED_MIME_TYPES_FIELD_NAME = "blockedmimetypes";
        private const string ALLOWED_MIME_TYPES_FIELD_NAME = "allowedmimetypes";

        internal InMemoryFileDb(InMemoryDb db)
        {
            _db = db;
            _uncommittedFileUploads = new ConcurrentDictionary<string, FileUploadSession>();
            _files = new ConcurrentDictionary<string, IFileAttachment>();
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
        public IFileAttachment GetFileById(string fileId)
        {
            _files.TryGetValue(fileId, out var file);
            return file;
        }
        
        public List<IFileAttachment> GetAllFiles()
        {
            return _files.Values.ToList();
        }

        public void AddFile(IFileAttachment fileAttachment)
        {
            var orgFileSettings = GetOrganizationFileSettings();

            if (IsFileExtensionBlocked(fileAttachment.FileName, orgFileSettings))
            {
                throw new BlockedAttachmentException(fileAttachment.FileName);
            }

            if (!IsMimeTypeAllowed(fileAttachment.MimeType, orgFileSettings))
            {
                throw new BlockedMimeTypeException(fileAttachment.FileName, fileAttachment.MimeType);
            }
            
            var wasAdded = _files.TryAdd(fileAttachment.Id, fileAttachment);
            if (!wasAdded)
            {
                throw new CouldNotAddFileException();
            }
        }

        public void DeleteFile(string fileId)
        {
            var wasDeleted = _files.TryRemove(fileId, out var file);
            if (!wasDeleted)
            {
                throw new CouldNotDeleteFileException(fileId);
            }
            
            //Set file attribute to null
            var entityReference = file.Target;
            var table = _db.GetTable(entityReference.LogicalName);
            if (table.Contains(entityReference.Id))
            {
                var entity = table.GetById(entityReference.Id);
                entity[file.AttributeName] = null;
                table.Replace(entity);
            }
        }

        public List<IFileAttachment> GetFilesForTarget(EntityReference target)
        {
            return _files.Values.Where
                    (f => 
                        f.Target.LogicalName.Equals(target.LogicalName) && 
                        f.Target.Id.Equals(target.Id))
                .ToList();
        }

        public OrganizationFileSettings GetOrganizationFileSettings()
        {
            var orgFileSettings = new OrganizationFileSettings();

            var exists = GetOrgBlockedAttachments(out var orgBlockedAttachments);
            if (exists)
            {
                orgFileSettings.BlockedAttachments = orgBlockedAttachments;
            }

            exists = GetOrgAllowedMimeTypes(out var allowedMimeTypes);
            if (exists)
            {
                orgFileSettings.AllowedMimeTypes = allowedMimeTypes;
            }
            
            exists = GetOrgBlockedMimeTypes(out var blockedMimeTypes);
            if (exists)
            {
                orgFileSettings.BlockedMimeTypes = blockedMimeTypes;
            }
            return orgFileSettings;
        }

        private Entity GetOrganization()
        {
            if (!_db.ContainsTable(ORGANIZATION_TABLE_NAME))
            {
                return null;
            }

            var orgTable = _db.GetTable(ORGANIZATION_TABLE_NAME);
            return orgTable.Rows.FirstOrDefault();
        }
        
        private bool GetOrgBlockedAttachments(out string[] blockedAttachments)
        {
            return TryGetOrganizationFileSetting(BLOCKED_ATTACHMENTS_FIELD_NAME, out blockedAttachments);
        }

        private bool GetOrgAllowedMimeTypes(out string[] allowedMimeTypes)
        {
            return TryGetOrganizationFileSetting(ALLOWED_MIME_TYPES_FIELD_NAME, out allowedMimeTypes);
        }
        
        private bool GetOrgBlockedMimeTypes(out string[] blockedMimeTypes)
        {
            return TryGetOrganizationFileSetting(BLOCKED_MIME_TYPES_FIELD_NAME, out blockedMimeTypes);
        }
        
        private bool TryGetOrganizationFileSetting(string settingFieldName, out string[] fileSetting)
        {
            bool exists = false;
            var organization = GetOrganization();
            if (organization == null)
            {
                fileSetting = new string[] { };
                return exists;
            }

            if (organization.Contains(settingFieldName))
            {
                exists = true;
                var fileSettingAttributeValue = (string)organization[settingFieldName];
                if (string.IsNullOrWhiteSpace(fileSettingAttributeValue))
                {
                    fileSetting = new string[] { };
                    return exists;
                }
                fileSetting = OrganizationFileSettings.FromCommaSeparated(fileSettingAttributeValue);
            }
            else
            {
                fileSetting = new string[] { };
            }

            return exists;
        }

        public bool IsFileExtensionBlocked(string fileName, OrganizationFileSettings fileSettings)
        {
            var extension = new FileInfo(fileName).Extension.Substring(1); //ignore the dot
            return fileSettings.BlockedAttachments.Contains(extension);
        }
        
        public bool IsMimeTypeAllowed(string mimeType, OrganizationFileSettings fileSettings)
        {
            if (fileSettings.AllowedMimeTypes.Length > 0)
            {
                return fileSettings.AllowedMimeTypes.Contains(mimeType);
            }
            
            if (fileSettings.BlockedMimeTypes.Length > 0)
            {
                return !fileSettings.BlockedMimeTypes.Contains(mimeType);
            }
            
            return true;
        }
        
        #endregion
        
        
        /// <summary>
        /// Creates a query to query files by ay criteria
        /// </summary>
        /// <returns></returns>
        public IQueryable<IFileAttachment> CreateQuery()
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

            if (attributeMetadata is FileAttributeMetadata fileAttributeMetadata 
                && (decimal) fileAttachment.Content.Length / 1024 > fileAttributeMetadata.MaxSizeInKB)
            {
                throw new MaxSizeExceededException(tableLogicalName, fileUploadSession.Properties.FileAttributeName,
                    fileAttributeMetadata.MaxSizeInKB.Value);
            }
            
            if (attributeMetadata is ImageAttributeMetadata imageAttributeMetadata &&
                (decimal) fileAttachment.Content.Length / 1024 > imageAttributeMetadata.MaxSizeInKB)
            {

                throw new MaxSizeExceededException(tableLogicalName, fileUploadSession.Properties.FileAttributeName,
                    imageAttributeMetadata.MaxSizeInKB.Value);
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

        private IFileAttachment CheckFileExists(FileDownloadProperties fileDownloadProperties)
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

        public byte[] DownloadFileBlock(DownloadBlockProperties downloadBlockProperties)
        {
            var fileDownloadSession = GetFileDownloadSession(downloadBlockProperties.FileDownloadSessionId);
            if (fileDownloadSession == null)
            {
                throw new FileTokenContinuationNotFoundException(downloadBlockProperties.FileDownloadSessionId);
            }

            if (downloadBlockProperties.BlockLength <= 0)
            {
                throw new InvalidBlockLengthException();
            }

            if (downloadBlockProperties.Offset < 0)
            {
                throw new InvalidOffsetException(downloadBlockProperties, fileDownloadSession.File.Content.Length);
            }

            if (downloadBlockProperties.BlockLength + downloadBlockProperties.Offset >
                fileDownloadSession.File.Content.Length)
            {
                throw new InvalidOffsetException(downloadBlockProperties, fileDownloadSession.File.Content.Length);
            }

            var data = new byte[downloadBlockProperties.BlockLength];
            
            Array.Copy(fileDownloadSession.File.Content, downloadBlockProperties.Offset, data,
                0, downloadBlockProperties.BlockLength);

            return data;
        }
        #endregion
    }
}