using FakeXrmEasy.Abstractions.FileStorage;
using FakeXrmEasy.Core.FileStorage.Download;

namespace FakeXrmEasy.Core.FileStorage.Db
{
    internal class FileDownloadSession
    {
        internal string FileDownloadSessionId { get; set; }
        internal FileDownloadProperties Properties { get; set; }
        internal IFileAttachment File { get; set; }
    }
}