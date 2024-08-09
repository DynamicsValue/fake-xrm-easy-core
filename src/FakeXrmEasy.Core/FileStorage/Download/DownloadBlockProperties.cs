namespace FakeXrmEasy.Core.FileStorage.Download
{
    internal class DownloadBlockProperties
    {
        internal string FileDownloadSessionId { get; set; }
        internal long BlockLength { get; set; }
        internal long Offset { get; set; }
    }
}