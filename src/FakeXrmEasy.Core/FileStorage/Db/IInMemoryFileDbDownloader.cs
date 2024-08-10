using System.Collections.Generic;
using FakeXrmEasy.Core.FileStorage.Download;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal interface IInMemoryFileDbDownloader
    {
        string InitFileDownloadSession(FileDownloadProperties fileDownloadProperties);
        
        FileDownloadSession GetFileDownloadSession(string fileDownloadSessionId);
        
        List<FileDownloadSession> GetAllFileDownloadSessions();

        byte[] DownloadFileBlock(DownloadBlockProperties downloadBlockProperties);
    }
}