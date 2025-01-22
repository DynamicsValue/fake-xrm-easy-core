using System.Collections.Generic;
using FakeXrmEasy.Core.FileStorage.Upload;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal interface IInMemoryFileDbUploader
    {
        /// <summary>
        /// Inits a file upload session against this InMemoryFileDb
        /// </summary>
        /// <param name="fileUploadProperties"></param>
        /// <returns></returns>
        string InitFileUploadSession(FileUploadProperties fileUploadProperties);
        
        /// <summary>
        /// Returns information about a current file upload session that is in progress
        /// </summary>
        /// <param name="fileUploadSessionId">The Id of the FileUploadSession to return</param>
        /// <returns></returns>
        FileUploadSession GetFileUploadSession(string fileUploadSessionId);
        
        /// <summary>
        /// Returns a list of all the current file upload sessions in the current InMemoryFileDb
        /// </summary>
        /// <returns></returns>
        List<FileUploadSession> GetAllFileUploadSessions();

        /// <summary>
        /// Commits a current file upload session to the InMemoryFileDb and returns the associated FileId
        /// </summary>
        /// <param name="commitProperties">Information about the file upload session to commit along with its block Id sequence</param>
        /// <returns>The id of the File that is committed to the InMemoryFileDb</returns>
        string CommitFileUploadSession(CommitFileUploadSessionProperties commitProperties);

    }
}