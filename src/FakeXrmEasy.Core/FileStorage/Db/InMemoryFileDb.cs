using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal class InMemoryFileDb
    {
        private readonly ConcurrentDictionary<string, FileUploadSession> _uncommittedFileUploads;

        internal InMemoryFileDb()
        {
            _uncommittedFileUploads = new ConcurrentDictionary<string, FileUploadSession>();
        }

        internal string InitFileUploadSession(FileUploadProperties fileUploadProperties)
        {
            string fileContinuationToken = Guid.NewGuid().ToString();
            _uncommittedFileUploads.GetOrAdd(fileContinuationToken, new FileUploadSession()
            {
                FileUploadSessionId = fileContinuationToken,
                Properties = new FileUploadProperties(fileUploadProperties)
            });
            return fileContinuationToken;
        }

        internal FileUploadSession GetFileUploadSession(string fileUploadSessionId)
        {
            FileUploadSession session = null;
            var exists = _uncommittedFileUploads.TryGetValue(fileUploadSessionId, out session);
            if (exists)
            {
                return session;
            }

            return null;
        }

        internal List<FileUploadSession> GetAllFileUploadSessions()
        {
            return _uncommittedFileUploads.Values.ToList();
        }
    }
}